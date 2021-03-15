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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Extensions;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class MockSetupRewriter : RewriterBase
  {
    public override SyntaxNode? VisitExpressionStatement (ExpressionStatementSyntax node)
    {
      if (Model == null)
      {
        throw new InvalidOperationException ("SemanticModel must not be null!");
      }

      var rhinoMocksExtensionsCompilationSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.RhinoMocksExtensions");
      var rhinoMocksIMethodOptionsSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Interfaces.IMethodOptions`1");
      if (rhinoMocksExtensionsCompilationSymbol == null || rhinoMocksIMethodOptionsSymbol == null)
      {
        throw new ArgumentException ("Rhino.Mocks cannot be found.");
      }

      var expectSymbols = rhinoMocksExtensionsCompilationSymbol.GetMembers ("Expect");
      var stubSymbols = rhinoMocksExtensionsCompilationSymbol.GetMembers ("Stub");
      var returnIdentifierName = SyntaxFactory.IdentifierName ("Return");
      var whenCalledIdentifierName = SyntaxFactory.IdentifierName ("WhenCalled");

      var identifierNames = GetAllRhinoMocksIdentifierNames (node, expectSymbols, stubSymbols, returnIdentifierName, whenCalledIdentifierName).ToList();

      var treeWithTrackedNodes = node.TrackNodes (identifierNames);
      foreach (var identifierName in identifierNames)
      {
        var trackedIdentifierName = treeWithTrackedNodes.GetCurrentNode (identifierName);
        treeWithTrackedNodes = treeWithTrackedNodes.ReplaceNode (
            trackedIdentifierName,
            ComputeReplacementNode (
                identifierName,
                expectSymbols,
                stubSymbols,
                returnIdentifierName,
                whenCalledIdentifierName));
      }

      if (ContainsExceptMethodSymbol (identifierNames, expectSymbols))
      {
        return MoqSyntaxFactory.VerifiableMock (treeWithTrackedNodes.Expression)
            .WithTrailingTrivia (SyntaxFactory.Whitespace (Environment.NewLine));
      }

      return treeWithTrackedNodes;
    }

    private bool ContainsExceptMethodSymbol (List<IdentifierNameSyntax> identifierNames, IEnumerable<ISymbol> expectSymbols)
    {
      return identifierNames
          .Select (s => Model!.GetSymbolInfo (s).GetFirstOverloadOrDefault())
          .Any (s => expectSymbols.Contains ((s as IMethodSymbol)?.ReducedFrom ?? s, SymbolEqualityComparer.Default));
    }

    private IdentifierNameSyntax ComputeReplacementNode (
        IdentifierNameSyntax originalNode,
        IEnumerable<ISymbol> expectSymbols,
        IEnumerable<ISymbol> stubSymbols,
        IdentifierNameSyntax returnIdentifierName,
        IdentifierNameSyntax whenCalledIdentifierName)
    {
      var symbol = Model!.GetSymbolInfo (originalNode).GetFirstOverloadOrDefault() as IMethodSymbol;
      if (stubSymbols.Contains (symbol?.ReducedFrom, SymbolEqualityComparer.Default))
      {
        return MoqSyntaxFactory.SetupIdentifierName();
      }

      if (expectSymbols.Contains (symbol?.ReducedFrom, SymbolEqualityComparer.Default))
      {
        return MoqSyntaxFactory.SetupIdentifierName().WithTrailingTrivia (SyntaxFactory.Space);
      }

      if (returnIdentifierName.IsEquivalentTo (originalNode, true))
      {
        return MoqSyntaxFactory.ReturnsIdentifierName().WithTrailingTrivia (SyntaxFactory.Space);
      }

      if (whenCalledIdentifierName.IsEquivalentTo (originalNode, true))
      {
        return MoqSyntaxFactory.CallbackIdentifierName().WithTrailingTrivia (SyntaxFactory.Space);
      }

      throw new InvalidOperationException ("Cannot resolve MethodSymbol from RhinoMocks Method");
    }

    private IEnumerable<IdentifierNameSyntax> GetAllRhinoMocksIdentifierNames (
        ExpressionStatementSyntax node,
        ImmutableArray<ISymbol> expectSymbols,
        ImmutableArray<ISymbol> stubSymbols,
        IdentifierNameSyntax returnsIdentifierName,
        IdentifierNameSyntax whenCalledIdentifierName)
    {
      return node.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.IdentifierName))
          .Where (
              s => (Model!.GetSymbolInfo (s).GetFirstOverloadOrDefault() is IMethodSymbol methodSymbol
                    && (expectSymbols.Contains (methodSymbol.ReducedFrom ?? methodSymbol, SymbolEqualityComparer.Default)
                        || stubSymbols.Contains (methodSymbol.ReducedFrom ?? methodSymbol, SymbolEqualityComparer.Default))
                    || returnsIdentifierName.IsEquivalentTo (s, true)
                    || whenCalledIdentifierName.IsEquivalentTo (s, true)))
          .Select (s => (IdentifierNameSyntax) s);
    }
  }
}