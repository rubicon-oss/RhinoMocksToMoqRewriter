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
      var mockRepositoryCompilationSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.MockRepository");
      var mockExtensionsCompilationSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.RhinoMocksExtensions");
      if (mockRepositoryCompilationSymbol == null || mockExtensionsCompilationSymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var verifyAllMethodSymbol = (IMethodSymbol) mockRepositoryCompilationSymbol.GetMembers ("VerifyAll").Single();
      var verifyAllExpectationsMethodSymbol = (IMethodSymbol) mockExtensionsCompilationSymbol.GetMembers ("VerifyAllExpectations").Single();
      var assertWasNotCalledMethodSymbols = mockExtensionsCompilationSymbol.GetMembers ("AssertWasNotCalled");
      var assertWasCalledMethodSymbols = mockExtensionsCompilationSymbol.GetMembers ("AssertWasCalled");

      var rhinoMocksVerifySymbols = GetRhinoMocksVerifySymbols (mockRepositoryCompilationSymbol, mockExtensionsCompilationSymbol).ToList();

      var rhinoMocksVerifyExpressionStatements = GetRhinoMocksVerifyExpressionStatements (node, rhinoMocksVerifySymbols).ToList();
      var annotatedSetupExpressionStatements =
          node.GetAnnotatedNodes (MoqSyntaxFactory.VerifyAnnotationKind).Select (s => (ExpressionStatementSyntax) s).ToList();
      if (rhinoMocksVerifyExpressionStatements.Count == 0 && annotatedSetupExpressionStatements.Count == 0)
      {
        return node;
      }

      var trackedNodes = node.TrackNodes (node.DescendantNodesAndSelf(), CompilationId)!;
      trackedNodes = ReplaceRhinoMocksVerifyExpressions (
          trackedNodes,
          rhinoMocksVerifyExpressionStatements,
          annotatedSetupExpressionStatements,
          verifyAllMethodSymbol,
          verifyAllExpectationsMethodSymbol,
          mockRepositoryCompilationSymbol,
          assertWasNotCalledMethodSymbols,
          assertWasCalledMethodSymbols);

      return trackedNodes;
    }

    private MethodDeclarationSyntax ReplaceRhinoMocksVerifyExpressions (
        MethodDeclarationSyntax node,
        IReadOnlyList<ExpressionStatementSyntax> rhinoMocksVerifyExpressionStatements,
        IReadOnlyList<ExpressionStatementSyntax> annotatedSetupExpressionStatements,
        IMethodSymbol verifyAllMethodSymbol,
        IMethodSymbol verifyAllExpectationsMethodSymbol,
        INamedTypeSymbol mockRepositoryCompilationSymbol,
        IReadOnlyList<ISymbol> assertWasNotCalledMethodSymbols,
        IReadOnlyList<ISymbol> assertWasCalledMethodSymbols)
    {
      foreach (var expressionStatement in rhinoMocksVerifyExpressionStatements)
      {
        var currentNode = node.GetCurrentNode (expressionStatement, CompilationId);
        var replacementNodes = ComputeReplacementNode (
            expressionStatement,
            verifyAllMethodSymbol,
            verifyAllExpectationsMethodSymbol,
            mockRepositoryCompilationSymbol,
            assertWasNotCalledMethodSymbols,
            assertWasCalledMethodSymbols).ToList();

        if (NeedsTimesExpression (replacementNodes, annotatedSetupExpressionStatements))
        {
          replacementNodes = InsertTimesExpression (replacementNodes, annotatedSetupExpressionStatements).ToList();
        }

        node = node.ReplaceNode (
            currentNode!,
            replacementNodes);
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

    private IEnumerable<ExpressionStatementSyntax> ComputeReplacementNode (
        ExpressionStatementSyntax originalNode,
        IMethodSymbol verifyAllMethodSymbol,
        IMethodSymbol verifyAllExpectationsMethodSymbol,
        INamedTypeSymbol mockRepositoryCompilationSymbol,
        IReadOnlyList<ISymbol> assertWasNotCalledMethodSymbols,
        IReadOnlyList<ISymbol> assertWasCalledMethodSymbols)
    {
      var symbol = Model.GetSymbolInfo (originalNode.Expression).Symbol as IMethodSymbol;
      return symbol switch
      {
          _ when verifyAllMethodSymbol.Equals (symbol, SymbolEqualityComparer.Default)
              => RewriteVerifyAllExpression (originalNode, mockRepositoryCompilationSymbol),
          _ when verifyAllExpectationsMethodSymbol.Equals (symbol?.ReducedFrom ?? symbol, SymbolEqualityComparer.Default)
              => new[] { RewriteVerifyAllExpectationsExpression (originalNode) },
          _ when assertWasNotCalledMethodSymbols.Contains (symbol?.ReducedFrom, SymbolEqualityComparer.Default)
              => new[] { ConvertExpression (originalNode, 0) },
          _ when assertWasNotCalledMethodSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => new[] { ConvertStaticExpression (originalNode, 0) },
          _ when assertWasCalledMethodSymbols.Contains (symbol?.ReducedFrom, SymbolEqualityComparer.Default)
              => new[] { ConvertExpression (originalNode, -1) },
          _ when assertWasCalledMethodSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
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

      var identifierName = invocationExpression.GetFirstIdentifierName();
      var mockedMethodExpression = invocationExpression.ArgumentList.GetFirstArgument().Expression;

      return node.WithExpression (MoqSyntaxFactory.VerifyExpression (identifierName, mockedMethodExpression, times));
    }

    private IEnumerable<ExpressionStatementSyntax> RewriteVerifyAllExpression (
        ExpressionStatementSyntax node,
        INamedTypeSymbol mockRepositoryCompilationSymbol)
    {
      var rootNode = node.SyntaxTree.GetRoot();
      var mockSymbols = GetMockSymbols (mockRepositoryCompilationSymbol).ToList();
      var mockRepositoryIdentifierName = node.GetFirstIdentifierName();
      var mockIdentifierNames = GetAllMockIdentifierNames (node, rootNode, mockRepositoryIdentifierName, mockSymbols);

      return mockIdentifierNames.Select (
          identifierName => MoqSyntaxFactory.VerifyExpressionStatement (identifierName.WithoutTrivia())
              .WithTrailingTrivia (SyntaxFactory.Whitespace (Environment.NewLine))
              .WithLeadingTrivia (node.GetLeadingTrivia()));
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

    private IEnumerable<ExpressionStatementSyntax> GetRhinoMocksVerifyExpressionStatements (SyntaxNode node, IReadOnlyList<ISymbol> rhinoMocksVerifySymbols)
    {
      return node.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Where (s => IsRhinoMocksMethod (s.Expression, rhinoMocksVerifySymbols));
    }

    private bool IsRhinoMocksMethod (SyntaxNode node, IReadOnlyList<ISymbol> rhinoMocksVerifySymbols)
    {
      return Model.GetSymbolInfo (node).Symbol is IMethodSymbol methodSymbol
             && rhinoMocksVerifySymbols.Contains (methodSymbol.ReducedFrom ?? methodSymbol.OriginalDefinition, SymbolEqualityComparer.Default);
    }

    private static IEnumerable<ISymbol> GetRhinoMocksVerifySymbols (INamedTypeSymbol mockRepositoryCompilationSymbol, INamedTypeSymbol mockExtensionsCompilationSymbol)
    {
      return mockRepositoryCompilationSymbol.GetMembers ("VerifyAll")
          .Concat (mockExtensionsCompilationSymbol.GetMembers ("VerifyAllExpectations"))
          .Concat (mockExtensionsCompilationSymbol.GetMembers ("AssertWasNotCalled"))
          .Concat (mockExtensionsCompilationSymbol.GetMembers ("AssertWasCalled"));
    }

    private static IEnumerable<ISymbol> GetMockSymbols (INamedTypeSymbol mockRepositoryCompilationSymbol)
    {
      return mockRepositoryCompilationSymbol.GetMembers ("DynamicMock")
          .Concat (mockRepositoryCompilationSymbol.GetMembers ("DynamicMultiMock"))
          .Concat (mockRepositoryCompilationSymbol.GetMembers ("StrictMock"))
          .Concat (mockRepositoryCompilationSymbol.GetMembers ("PartialMock"))
          .Concat (mockRepositoryCompilationSymbol.GetMembers ("PartialMultiMock"));
    }

    private IEnumerable<IdentifierNameSyntax> GetMockIdentifierNamesFromAssignmentExpressions (
        SyntaxNode rootNode,
        IdentifierNameSyntax mockRepositoryIdentifierName,
        IReadOnlyList<ISymbol> mockSymbols)
    {
      return rootNode.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Where (s => s.Expression.IsKind (SyntaxKind.SimpleAssignmentExpression))
          .Select (s => (AssignmentExpressionSyntax) s.Expression)
          .Where (s => s.Right.IsKind (SyntaxKind.InvocationExpression))
          .Where (s => IsMockFromCurrentMockRepository (mockRepositoryIdentifierName, (InvocationExpressionSyntax) s.Right, mockSymbols))
          .Where (s => s.Left.IsKind (SyntaxKind.IdentifierName))
          .Select (s => ((IdentifierNameSyntax) s.Left));
    }

    private IEnumerable<IdentifierNameSyntax> GetMockIdentifierNamesFromLocalDeclarationStatements (
        SyntaxNode node,
        IdentifierNameSyntax mockRepositoryIdentifierName,
        IReadOnlyList<ISymbol> mockSymbols)
    {
      return node.Ancestors().First (a => a.IsKind (SyntaxKind.MethodDeclaration))
          .DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.LocalDeclarationStatement))
          .Select (s => (LocalDeclarationStatementSyntax) s)
          .Where (s => s.Declaration.Variables.Any (v => v.Initializer is not null))
          .SelectMany (s => s.Declaration.Variables)
          .Where (
              s => s.Initializer is { Value: InvocationExpressionSyntax invocationExpression }
                   && IsMockFromCurrentMockRepository (mockRepositoryIdentifierName, invocationExpression, mockSymbols))
          .Select (s => SyntaxFactory.IdentifierName (s.Identifier));
    }

    private bool IsMockFromCurrentMockRepository (IdentifierNameSyntax mockRepositoryIdentifierName, InvocationExpressionSyntax generateMockExpression, IReadOnlyList<ISymbol> mockSymbols)
    {
      return mockRepositoryIdentifierName.IsEquivalentTo (generateMockExpression.GetFirstIdentifierName(), false)
             && mockSymbols.Contains (
                 Model.GetSymbolInfo (generateMockExpression.Expression).Symbol!.OriginalDefinition,
                 SymbolEqualityComparer.Default);
    }

    private IEnumerable<IdentifierNameSyntax> GetAllMockIdentifierNames (ExpressionStatementSyntax node, SyntaxNode rootNode, IdentifierNameSyntax mockRepositoryIdentifierName, List<ISymbol> mockSymbols)
    {
      return GetMockIdentifierNamesFromAssignmentExpressions (rootNode, mockRepositoryIdentifierName, mockSymbols)
          .Concat (GetMockIdentifierNamesFromLocalDeclarationStatements (node, mockRepositoryIdentifierName, mockSymbols));
    }
  }
}