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

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class MockSetupRewriter : RewriterBase
  {
    public override SyntaxNode? VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      var rhinoMocksExtensionsCompilationSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.RhinoMocksExtensions");
      var rhinoMocksIMethodOptionsSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Interfaces.IMethodOptions`1");
      if (rhinoMocksExtensionsCompilationSymbol == null || rhinoMocksIMethodOptionsSymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var expectSymbols = rhinoMocksExtensionsCompilationSymbol.GetMembers ("Expect");
      var stubSymbols = rhinoMocksExtensionsCompilationSymbol.GetMembers ("Stub");
      var returnSymbols = rhinoMocksIMethodOptionsSymbol.GetMembers ("Return");
      var whenCalledSymbols = rhinoMocksIMethodOptionsSymbol.GetMembers ("WhenCalled");
      var callbackSymbols = rhinoMocksIMethodOptionsSymbol.GetMembers ("Callback");

      var rhinoMocksExpressionStatements = GetAllRhinoMocksExpressionStatements (node, expectSymbols, stubSymbols, returnSymbols, whenCalledSymbols, callbackSymbols)
          .ToList();

      var trackedNodes = node.TrackNodes (rhinoMocksExpressionStatements);
      foreach (var expressionStatement in rhinoMocksExpressionStatements)
      {
        var newExpressionStatement =
            RewriteMockSetupExpression (expressionStatement, expectSymbols, stubSymbols, returnSymbols, whenCalledSymbols);

        trackedNodes = trackedNodes.ReplaceNode (
            trackedNodes.GetCurrentNode (expressionStatement)!,
            newExpressionStatement);
      }

      return trackedNodes;
    }

    private SyntaxNode RewriteMockSetupExpression (
        SyntaxNode originalNode,
        IReadOnlyCollection<ISymbol> expectSymbols,
        IReadOnlyCollection<ISymbol> stubSymbols,
        IReadOnlyCollection<ISymbol> returnSymbols,
        IReadOnlyCollection<ISymbol> whenCalledSymbols)
    {
      var nodesToBeReplacedInOriginalNode = GetAllNodesToBeReplaced (originalNode, expectSymbols, stubSymbols, returnSymbols, whenCalledSymbols).ToList();
      var trackedNodesToBeReplacedInOriginalNode = originalNode.TrackNodes (nodesToBeReplacedInOriginalNode);
      foreach (var currentNode in nodesToBeReplacedInOriginalNode)
      {
        var trackedNode = trackedNodesToBeReplacedInOriginalNode.GetCurrentNode (currentNode);
        trackedNodesToBeReplacedInOriginalNode = trackedNodesToBeReplacedInOriginalNode.ReplaceNode (
            trackedNode!,
            ComputeReplacementNode (
                currentNode,
                expectSymbols,
                stubSymbols,
                returnSymbols,
                whenCalledSymbols));
      }

      if (ContainsExceptMethodSymbol (nodesToBeReplacedInOriginalNode, expectSymbols))
      {
        return MoqSyntaxFactory.VerifiableMock (((ExpressionStatementSyntax) trackedNodesToBeReplacedInOriginalNode).Expression)
            .WithTrailingTrivia (SyntaxFactory.Whitespace (Environment.NewLine));
      }

      return trackedNodesToBeReplacedInOriginalNode;
    }

    private IEnumerable<ExpressionStatementSyntax> GetAllRhinoMocksExpressionStatements (
        MethodDeclarationSyntax node,
        IReadOnlyCollection<ISymbol> expectSymbols,
        IReadOnlyCollection<ISymbol> stubSymbols,
        IReadOnlyCollection<ISymbol> returnSymbols,
        IReadOnlyCollection<ISymbol> whenCalledSymbols,
        IReadOnlyCollection<ISymbol> callbackSymbols)
    {
      return node.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Where (
              s => Model.GetSymbolInfo (s.Expression).Symbol is IMethodSymbol methodSymbol
                   && (expectSymbols.Contains (methodSymbol.ReducedFrom ?? methodSymbol.OriginalDefinition, SymbolEqualityComparer.Default)
                       || stubSymbols.Contains (methodSymbol.ReducedFrom ?? methodSymbol.OriginalDefinition, SymbolEqualityComparer.Default)
                       || returnSymbols.Contains (methodSymbol.OriginalDefinition, SymbolEqualityComparer.Default)
                       || whenCalledSymbols.Contains (methodSymbol.OriginalDefinition, SymbolEqualityComparer.Default)
                       || callbackSymbols.Contains (methodSymbol.OriginalDefinition, SymbolEqualityComparer.Default)));
    }

    private bool ContainsExceptMethodSymbol (IEnumerable<SyntaxNode> nodes, IEnumerable<ISymbol> expectSymbols)
    {
      return nodes
          .Select (s => Model.GetSymbolInfo (s).Symbol)
          .Any (s => expectSymbols.Contains ((s as IMethodSymbol)?.ReducedFrom ?? s?.OriginalDefinition, SymbolEqualityComparer.Default));
    }

    private SyntaxNode ComputeReplacementNode (
        SyntaxNode originalNode,
        IReadOnlyCollection<ISymbol> expectSymbols,
        IReadOnlyCollection<ISymbol> stubSymbols,
        IReadOnlyCollection<ISymbol> returnSymbols,
        IReadOnlyCollection<ISymbol> whenCalledSymbols)
    {
      var symbol = Model.GetSymbolInfo (originalNode).Symbol as IMethodSymbol;
      if (stubSymbols.Contains (symbol?.ReducedFrom, SymbolEqualityComparer.Default)
          || expectSymbols.Contains (symbol?.ReducedFrom, SymbolEqualityComparer.Default))
      {
        return MoqSyntaxFactory.SetupIdentifierName().WithTrailingTrivia (SyntaxFactory.Space);
      }

      if (stubSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
          || expectSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default))
      {
        var (identifierName, lambdaExpression) = GetIdentifierNameAndLambdaExpression (originalNode);
        return MoqSyntaxFactory.SetupExpression (identifierName, lambdaExpression);
      }

      if (returnSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default))
      {
        return MoqSyntaxFactory.ReturnsIdentifierName().WithTrailingTrivia (SyntaxFactory.Space);
      }

      if (whenCalledSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default))
      {
        return MoqSyntaxFactory.CallbackIdentifierName().WithTrailingTrivia (SyntaxFactory.Space);
      }

      throw new InvalidOperationException ("Cannot resolve MethodSymbol from RhinoMocks Method");
    }

    private static (IdentifierNameSyntax, LambdaExpressionSyntax) GetIdentifierNameAndLambdaExpression (SyntaxNode originalNode)
    {
      if (originalNode is not InvocationExpressionSyntax invocationExpression)
      {
        throw new InvalidOperationException ("Node must be an InvocationExpression");
      }

      var identifierName = (IdentifierNameSyntax) invocationExpression.ArgumentList.Arguments.First()!.Expression;
      var lambdaExpression = (LambdaExpressionSyntax) invocationExpression.ArgumentList.Arguments.Last()!.Expression;

      return (identifierName, lambdaExpression)!;
    }

    private IEnumerable<SyntaxNode> GetAllNodesToBeReplaced (
        SyntaxNode node,
        IReadOnlyCollection<ISymbol> expectSymbols,
        IReadOnlyCollection<ISymbol> stubSymbols,
        IReadOnlyCollection<ISymbol> returnSymbols,
        IReadOnlyCollection<ISymbol> whenCalledSymbols)

    {
      return GetAllRhinoMocksIdentifierNames (node, expectSymbols, stubSymbols, returnSymbols, whenCalledSymbols)
          .Concat (
              node.DescendantNodesAndSelf()
                  .Where (s => s.IsKind (SyntaxKind.InvocationExpression))
                  .Where (
                      s => Model.GetSymbolInfo (s).Symbol?.OriginalDefinition is IMethodSymbol methodSymbol
                           && (stubSymbols.Contains (methodSymbol, SymbolEqualityComparer.Default)
                               || expectSymbols.Contains (methodSymbol, SymbolEqualityComparer.Default))));
    }

    private IEnumerable<SyntaxNode> GetAllRhinoMocksIdentifierNames (
        SyntaxNode node,
        IReadOnlyCollection<ISymbol> expectSymbols,
        IReadOnlyCollection<ISymbol> stubSymbols,
        IReadOnlyCollection<ISymbol> returnSymbols,
        IReadOnlyCollection<ISymbol> whenCalledSymbols)
    {
      return node.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.IdentifierName))
          .Where (
              s => Model.GetSymbolInfo (s).Symbol is IMethodSymbol methodSymbol
                   && (expectSymbols.Contains (methodSymbol.ReducedFrom, SymbolEqualityComparer.Default)
                       || stubSymbols.Contains (methodSymbol.ReducedFrom, SymbolEqualityComparer.Default)
                       || returnSymbols.Contains (methodSymbol.OriginalDefinition, SymbolEqualityComparer.Default)
                       || whenCalledSymbols.Contains (methodSymbol.OriginalDefinition, SymbolEqualityComparer.Default)))
          .Select (s => (IdentifierNameSyntax) s);
    }
  }
}