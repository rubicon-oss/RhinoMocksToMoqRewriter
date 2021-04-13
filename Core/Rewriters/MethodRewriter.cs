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
  public class MethodRewriter : RewriterBase
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

      var expressionStatements = GetAllExpressionStatements (node);
      var rhinoMocksExpressionStatements = GetAllRhinoMocksMethods (expressionStatements, verifyAllMethodSymbol, verifyAllExpectationsMethodSymbol).ToList();
      if (rhinoMocksExpressionStatements.Count == 0)
      {
        return node;
      }

      var treeWithTrackedNodes = node.TrackNodes (rhinoMocksExpressionStatements);
      foreach (var expressionStatement in rhinoMocksExpressionStatements)
      {
        var trackedExpressionStatement = treeWithTrackedNodes.GetCurrentNode (expressionStatement);
        treeWithTrackedNodes = treeWithTrackedNodes
            .ReplaceNode (
                trackedExpressionStatement,
                ComputeReplacementNode (
                    expressionStatement,
                    verifyAllMethodSymbol,
                    verifyAllExpectationsMethodSymbol,
                    mockRepositoryCompilationSymbol));
      }

      return treeWithTrackedNodes;
    }

    private IEnumerable<ExpressionStatementSyntax> ComputeReplacementNode (
        ExpressionStatementSyntax originalNode,
        IMethodSymbol verifyAllMethodSymbol,
        IMethodSymbol verifyAllExpectationsMethodSymbol,
        INamedTypeSymbol mockRepositoryCompilationSymbol)
    {
      var symbol = Model.GetSymbolInfo (originalNode.Expression).Symbol as IMethodSymbol;
      if (verifyAllMethodSymbol.Equals (symbol, SymbolEqualityComparer.Default))
      {
        return RewriteVerifyAllExpression (originalNode, mockRepositoryCompilationSymbol).ToList();
      }

      if (verifyAllExpectationsMethodSymbol.Equals (symbol?.ReducedFrom ?? symbol, SymbolEqualityComparer.Default))
      {
        return new[] { RewriteVerifyAllExpectationsExpression (originalNode) };
      }

      throw new InvalidOperationException ("Cannot resolve MethodSymbol from RhinoMocks Method");
    }

    private IEnumerable<ExpressionStatementSyntax> RewriteVerifyAllExpression (
        ExpressionStatementSyntax node,
        INamedTypeSymbol mockRepositoryCompilationSymbol)
    {
      var rootNode = node.SyntaxTree.GetRoot();
      var assignmentExpressions = GetAllAssignmentExpressionsWithInvocationExpression (rootNode);
      var mockIdentifierNames = GetMockIdentifierNames (assignmentExpressions, node, mockRepositoryCompilationSymbol);

      return mockIdentifierNames.Select (
              identifierName => MoqSyntaxFactory.VerifyStatement (identifierName.WithoutTrivia())
                  .WithTrailingTrivia (SyntaxFactory.Whitespace (Environment.NewLine))
                  .WithLeadingTrivia (node.GetLeadingTrivia()))
          .ToList();
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

      return MoqSyntaxFactory.VerifyStatement (identifierName.Identifier)
          .WithTrailingTrivia (SyntaxFactory.Whitespace (Environment.NewLine))
          .WithLeadingTrivia (node.GetLeadingTrivia());
    }

    private static IEnumerable<ExpressionStatementSyntax> GetAllExpressionStatements (SyntaxNode node)
    {
      return node.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s);
    }

    private IEnumerable<ExpressionStatementSyntax> GetAllRhinoMocksMethods (
        IEnumerable<ExpressionStatementSyntax> expressionStatements,
        IMethodSymbol verifyAllMethodSymbol,
        IMethodSymbol verifyAllExpectationsMethodSymbol)
    {
      return expressionStatements
          .Where (
              s => Model.GetSymbolInfo (s.Expression).Symbol is IMethodSymbol methodSymbol
                   && (verifyAllMethodSymbol.Equals (methodSymbol, SymbolEqualityComparer.Default)
                       || verifyAllExpectationsMethodSymbol.Equals (methodSymbol.ReducedFrom ?? methodSymbol, SymbolEqualityComparer.Default)));
    }

    private IEnumerable<SyntaxToken> GetMockIdentifierNames (
        IEnumerable<AssignmentExpressionSyntax> assignmentExpressions,
        SyntaxNode node,
        INamedTypeSymbol mockRepositoryCompilationSymbol)
    {
      var mockRepositoryIdentifierName = node.GetFirstIdentifierName();
      var mockSymbols = GetMockSymbols (mockRepositoryCompilationSymbol);

      return assignmentExpressions
          .Where (s => mockRepositoryIdentifierName.IsEquivalentTo (s.Right.GetFirstIdentifierName(), false))
          .Where (
              s => mockSymbols.Contains (
                  Model.GetSymbolInfo (((InvocationExpressionSyntax) s.Right).Expression).Symbol!.OriginalDefinition,
                  SymbolEqualityComparer.Default))
          .Where (s => s.Left.IsKind (SyntaxKind.IdentifierName))
          .Select (s => ((IdentifierNameSyntax) s.Left).Identifier)
          .ToList();
    }

    private static IEnumerable<ISymbol> GetMockSymbols (INamedTypeSymbol mockRepositoryCompilationSymbol)
    {
      var generateMockMethodSymbols = new List<ISymbol>();
      generateMockMethodSymbols.AddRange (mockRepositoryCompilationSymbol.GetMembers ("DynamicMock"));
      generateMockMethodSymbols.AddRange (mockRepositoryCompilationSymbol.GetMembers ("StrictMock"));
      generateMockMethodSymbols.AddRange (mockRepositoryCompilationSymbol.GetMembers ("PartialMock"));
      generateMockMethodSymbols.AddRange (mockRepositoryCompilationSymbol.GetMembers ("PartialMultiMock"));

      return generateMockMethodSymbols;
    }

    private static IEnumerable<AssignmentExpressionSyntax> GetAllAssignmentExpressionsWithInvocationExpression (SyntaxNode rootNode)
    {
      return rootNode.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Where (s => s.Expression.IsKind (SyntaxKind.SimpleAssignmentExpression))
          .Select (s => (AssignmentExpressionSyntax) s.Expression)
          .Where (s => s.Right.IsKind (SyntaxKind.InvocationExpression));
    }
  }
}