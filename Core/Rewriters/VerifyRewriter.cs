//  Copyright (c) rubicon IT GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Extensions;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class VerifyRewriter : RewriterBase
  {
    public override SyntaxNode? VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      var rhinoMocksVerifyExpressionStatements = GetRhinoMocksVerifyExpressionStatements (node).ToList();
      var annotatedSetupExpressionStatements =
          node.GetAnnotatedNodes (MoqSyntaxFactory.VerifyAnnotationKind).Select (s => (ExpressionStatementSyntax) s).ToList();
      if (rhinoMocksVerifyExpressionStatements.Count == 0 && annotatedSetupExpressionStatements.Count == 0)
      {
        return node;
      }

      var trackedNodes = node.TrackNodes (node.DescendantNodesAndSelf(), CompilationId)!;
      trackedNodes = ReplaceRhinoMocksVerifyExpressions (trackedNodes, rhinoMocksVerifyExpressionStatements, annotatedSetupExpressionStatements);

      return trackedNodes;
    }

    private MethodDeclarationSyntax ReplaceRhinoMocksVerifyExpressions (
        MethodDeclarationSyntax node,
        IReadOnlyList<ExpressionStatementSyntax> rhinoMocksVerifyExpressionStatements,
        IReadOnlyList<ExpressionStatementSyntax> annotatedSetupExpressionStatements)
    {
      foreach (var expressionStatement in rhinoMocksVerifyExpressionStatements)
      {
        var currentNode = node.GetCurrentNode (expressionStatement, CompilationId);
        var replacementNodes = ComputeReplacementNode (expressionStatement).ToList();

        if (NeedsTimesExpression (replacementNodes, annotatedSetupExpressionStatements))
        {
          replacementNodes = InsertTimesExpression (replacementNodes, annotatedSetupExpressionStatements).ToList();
        }

        try
        {
          node = node.ReplaceNode (
              currentNode!,
              replacementNodes);
        }
        catch (Exception)
        {
          Console.Error.WriteLine (
              $"  WARNING: Unable to convert Ordered using statement"
              + $"\r\n  {node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}");
        }
      }

      return node;
    }

    private static bool NeedsTimesExpression (
        IEnumerable<ExpressionStatementSyntax> replacementNodes,
        IReadOnlyList<ExpressionStatementSyntax> annotatedSetupExpressionStatements)
    {
      foreach (var replacementNode in replacementNodes)
      {
        var mockIdentifierName = replacementNode.GetFirstIdentifierName();
        if (annotatedSetupExpressionStatements.Select (s => s.GetFirstIdentifierName()).Any (s => s.IsEquivalentTo (mockIdentifierName, false)))
        {
          return true;
        }
      }

      return false;
    }

    private static IEnumerable<ExpressionStatementSyntax> InsertTimesExpression (
        List<ExpressionStatementSyntax> replacementNodes,
        IReadOnlyList<ExpressionStatementSyntax> annotatedSetupExpressionStatements)
    {
      for (var i = 0; i < replacementNodes.Count; i++)
      {
        var replacementNode = replacementNodes[i];
        foreach (var setupExpression in annotatedSetupExpressionStatements)
        {
          var replacementNodeIdentifierName = replacementNode.GetFirstIdentifierName();
          var setupIdentifierName = setupExpression.GetFirstIdentifierName();
          var timeData = setupExpression.GetAnnotations (MoqSyntaxFactory.VerifyAnnotationKind).Single().Data;

          if (!setupIdentifierName.IsEquivalentTo (replacementNodeIdentifierName, false))
          {
            continue;
          }

          replacementNodes[i] = CreateTimesExpression (replacementNode, replacementNodeIdentifierName, setupExpression, timeData!);
        }
      }

      return replacementNodes;
    }

    private static ExpressionStatementSyntax CreateTimesExpression (
        ExpressionStatementSyntax replacementNode,
        IdentifierNameSyntax replacementNodeIdentifierName,
        ExpressionStatementSyntax setupExpression,
        string timeData)
    {
      if (!int.TryParse (timeData, out var times))
      {
        var minMax = GetDataFromString (timeData);
        return replacementNode.WithExpression (
            MoqSyntaxFactory.VerifyExpression (
                replacementNodeIdentifierName,
                setupExpression.GetFirstArgument().Expression!,
                minMax));
      }

      return replacementNode.WithExpression (
          MoqSyntaxFactory.VerifyExpression (
              replacementNodeIdentifierName,
              setupExpression.GetFirstArgument().Expression,
              times));
    }

    private static (int Min, int Max) GetDataFromString (string? annotationData)
    {
      var data = annotationData!.Split (":");
      return (int.Parse (data.First()), int.Parse (data.Last()));
    }

    private IEnumerable<ExpressionStatementSyntax> ComputeReplacementNode (ExpressionStatementSyntax originalNode)
    {
      var symbol = Model.GetSymbolInfo (originalNode.Expression).Symbol as IMethodSymbol;
      return symbol switch
      {
          _ when RhinoMocksSymbols.VerifyAllSymbols.Contains (symbol, SymbolEqualityComparer.Default)
              => RewriteVerifyAllExpression (originalNode),
          _ when RhinoMocksSymbols.VerifyAllExpectationsSymbols.Contains (symbol?.ReducedFrom ?? symbol, SymbolEqualityComparer.Default)
              => new[] { RewriteVerifyAllExpectationsExpression (originalNode) },
          _ when RhinoMocksSymbols.AssertWasNotCalledSymbols.Contains (symbol?.ReducedFrom, SymbolEqualityComparer.Default)
              => new[] { ConvertExpression (originalNode, 0) },
          _ when RhinoMocksSymbols.AssertWasNotCalledSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => new[] { ConvertStaticExpression (originalNode, 0) },
          _ when RhinoMocksSymbols.AssertWasCalledSymbols.Contains (symbol?.ReducedFrom, SymbolEqualityComparer.Default)
              => new[] { ConvertExpression (originalNode, -1) },
          _ when RhinoMocksSymbols.AssertWasCalledSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => new[] { ConvertStaticExpression (originalNode, -1) },
          _ => throw new InvalidOperationException ("Cannot resolve MethodSymbol from RhinoMocks Method")
      };
    }

    private static ExpressionStatementSyntax ConvertStaticExpression (ExpressionStatementSyntax node, int times = 0)
    {
      if (node.Expression is not InvocationExpressionSyntax invocationExpression)
      {
        throw new InvalidOperationException ("Expression must be of type InvocationExpressionSyntax");
      }

      var identifierName = invocationExpression.ArgumentList.GetFirstArgument().Expression as IdentifierNameSyntax;
      if (identifierName == null)
      {
        throw new InvalidOperationException ("Node must have an IdentifierName");
      }

      var mockedMethodExpression = invocationExpression.ArgumentList.Arguments.Last().Expression;
      return node.WithExpression (MoqSyntaxFactory.VerifyExpression (identifierName, mockedMethodExpression, times));
    }

    private static ExpressionStatementSyntax ConvertExpression (ExpressionStatementSyntax node, int times = 0)
    {
      if (node.Expression is not InvocationExpressionSyntax invocationExpression)
      {
        throw new InvalidOperationException ("Expression must be of type InvocationExpressionSyntax");
      }

      ArgumentSyntax? optionsArgument = null;
      if (invocationExpression.ArgumentList.Arguments.Count == 2)
      {
        optionsArgument = invocationExpression.ArgumentList.Arguments.Last();
      }

      var identifierName = invocationExpression.GetFirstIdentifierName();
      var mockedMethodExpression = invocationExpression.ArgumentList.GetFirstArgument().Expression;

      return node.WithExpression (MoqSyntaxFactory.VerifyExpression (identifierName, mockedMethodExpression, times, optionsArgument));
    }

    private IEnumerable<ExpressionStatementSyntax> RewriteVerifyAllExpression (ExpressionStatementSyntax node)
    {
      var rootNode = node.SyntaxTree.GetRoot();
      var mockRepositoryIdentifierName = node.GetFirstIdentifierName();
      var mockIdentifierNames = GetAllMockIdentifierNames (node, rootNode, mockRepositoryIdentifierName).ToList();

      if (mockIdentifierNames.Count == 0)
      {
        return new[] { node };
      }

      var verifyStatements = new List<ExpressionStatementSyntax>();
      for (var i = 0; i < mockIdentifierNames.Count; i++)
      {
        var currentIdentifierName = mockIdentifierNames[i];
        if (i == mockIdentifierNames.Count - 1)
        {
          verifyStatements.Add (
              MoqSyntaxFactory.VerifyExpressionStatement (currentIdentifierName.WithoutTrivia())
                  .WithLeadingAndTrailingTriviaOfNode (node));
          continue;
        }

        verifyStatements.Add (
            MoqSyntaxFactory.VerifyExpressionStatement (currentIdentifierName.WithoutTrivia())
                .WithLeadingTrivia (node.GetLeadingTrivia())
                .WithTrailingTrivia (
                    SyntaxFactory.Whitespace (
                        node.GetLeadingTrivia().ToFullString().Contains (Environment.NewLine)
                            ? string.Empty
                            : Environment.NewLine)));
      }

      return verifyStatements;
    }

    private static ExpressionStatementSyntax RewriteVerifyAllExpectationsExpression (ExpressionStatementSyntax node)
    {
      if (node.Expression is not InvocationExpressionSyntax invocationExpression)
      {
        throw new InvalidOperationException ("Expression must be of type InvocationExpressionSyntax");
      }

      var identifierName = invocationExpression.ArgumentList.IsEmpty()
          ? node.Expression.GetFirstIdentifierName()
          : invocationExpression.ArgumentList.Arguments.First().Expression as IdentifierNameSyntax;

      if (identifierName == null)
      {
        throw new InvalidOperationException ("Node must have an IdentifierName");
      }

      return node.WithExpression (MoqSyntaxFactory.VerifyExpression (identifierName));
    }

    private IEnumerable<ExpressionStatementSyntax> GetRhinoMocksVerifyExpressionStatements (SyntaxNode node)
    {
      return node.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Where (s => IsRhinoMocksMethod (s.Expression));
    }

    private bool IsRhinoMocksMethod (SyntaxNode node)
    {
      return Model.GetSymbolInfo (node).Symbol is IMethodSymbol methodSymbol
             && RhinoMocksSymbols.AllVerifySymbols.Contains (methodSymbol.ReducedFrom ?? methodSymbol.OriginalDefinition, SymbolEqualityComparer.Default);
    }

    private IEnumerable<IdentifierNameSyntax> GetMockIdentifierNamesFromAssignmentExpressions (SyntaxNode rootNode, IdentifierNameSyntax mockRepositoryIdentifierName)
    {
      return rootNode.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Where (s => s.Expression.IsKind (SyntaxKind.SimpleAssignmentExpression))
          .Select (s => (AssignmentExpressionSyntax) s.Expression)
          .Where (s => s.Right.IsKind (SyntaxKind.InvocationExpression))
          .Where (s => IsMockFromCurrentMockRepository (mockRepositoryIdentifierName, (InvocationExpressionSyntax) s.Right))
          .Where (s => s.Left.IsKind (SyntaxKind.IdentifierName))
          .Select (s => ((IdentifierNameSyntax) s.Left));
    }

    private IEnumerable<IdentifierNameSyntax> GetMockIdentifierNamesFromLocalDeclarationStatements (SyntaxNode node, IdentifierNameSyntax mockRepositoryIdentifierName)
    {
      return node.Ancestors().First (a => a.IsKind (SyntaxKind.MethodDeclaration))
          .DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.LocalDeclarationStatement))
          .Select (s => (LocalDeclarationStatementSyntax) s)
          .Where (s => s.Declaration.Variables.Any (v => v.Initializer is not null))
          .SelectMany (s => s.Declaration.Variables)
          .Where (
              s => s.Initializer is { Value: InvocationExpressionSyntax invocationExpression }
                   && IsMockFromCurrentMockRepository (mockRepositoryIdentifierName, invocationExpression))
          .Select (s => SyntaxFactory.IdentifierName (s.Identifier));
    }

    private bool IsMockFromCurrentMockRepository (IdentifierNameSyntax mockRepositoryIdentifierName, InvocationExpressionSyntax generateMockExpression)
    {
      return mockRepositoryIdentifierName.IsEquivalentTo (generateMockExpression.GetFirstIdentifierName(), false)
             && RhinoMocksSymbols.AllMockSymbols.Contains (
                 Model.GetSymbolInfo (generateMockExpression.Expression).Symbol!.OriginalDefinition,
                 SymbolEqualityComparer.Default);
    }

    private IEnumerable<IdentifierNameSyntax> GetAllMockIdentifierNames (ExpressionStatementSyntax node, SyntaxNode rootNode, IdentifierNameSyntax mockRepositoryIdentifierName)
    {
      return GetMockIdentifierNamesFromAssignmentExpressions (rootNode, mockRepositoryIdentifierName)
          .Concat (GetMockIdentifierNamesFromLocalDeclarationStatements (node, mockRepositoryIdentifierName));
    }
  }
}