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
  public class MockInstantiationRewriter : RewriterBase
  {
    private readonly IFormatter _formatter;

    public MockInstantiationRewriter (IFormatter formatter)
    {
      _formatter = formatter;
    }

    public override SyntaxNode? VisitInvocationExpression (InvocationExpressionSyntax node)
    {
      var rhinoMocksMockRepositorySymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.MockRepository");
      if (rhinoMocksMockRepositorySymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var mockOrStubSymbols = GetAllMockOrStubSymbols (rhinoMocksMockRepositorySymbol);
      var strictMockSymbols = GetAllStrictMockSymbols (rhinoMocksMockRepositorySymbol);
      var partialMockSymbols = GetAllPartialMockSymbols (rhinoMocksMockRepositorySymbol);
      var methodSymbol = (Model.GetSymbolInfo (node).Symbol as IMethodSymbol)?.OriginalDefinition;
      if (methodSymbol == null)
      {
        return node;
      }

      var rhinoMethodGenericName = node.GetFirstGenericNameOrDefault();
      if (rhinoMethodGenericName == null)
      {
        return node;
      }

      var nodeWithRewrittenDescendants = (InvocationExpressionSyntax) base.VisitInvocationExpression (node)!;
      var moqMockTypeArgumentList = SyntaxFactory.TypeArgumentList().AddArguments (rhinoMethodGenericName.TypeArgumentList.Arguments.First());
      var moqMockArgumentSyntaxList = nodeWithRewrittenDescendants.ArgumentList;

      return methodSymbol switch
      {
          var s when mockOrStubSymbols.Contains (methodSymbol, SymbolEqualityComparer.Default) => _formatter.Format (
              MoqSyntaxFactory.MockCreationExpression (moqMockTypeArgumentList, moqMockArgumentSyntaxList)),
          var s when partialMockSymbols.Contains (methodSymbol, SymbolEqualityComparer.Default) => _formatter.Format (
              MoqSyntaxFactory.PartialMockCreationExpression (moqMockTypeArgumentList, moqMockArgumentSyntaxList)),
          var s when strictMockSymbols.Contains (methodSymbol, SymbolEqualityComparer.Default) => _formatter.Format (
              MoqSyntaxFactory.StrictMockCreationExpression (moqMockTypeArgumentList, moqMockArgumentSyntaxList)),
          _ => node
      };
    }

    private static IEnumerable<ISymbol> GetAllPartialMockSymbols (INamedTypeSymbol rhinoMocksMockRepositorySymbol)
    {
      return rhinoMocksMockRepositorySymbol.GetMembers ("PartialMock")
          .Concat (rhinoMocksMockRepositorySymbol.GetMembers ("PartialMultiMock"))
          .Concat (rhinoMocksMockRepositorySymbol.GetMembers ("GeneratePartialMock"));
    }

    private static IEnumerable<ISymbol> GetAllStrictMockSymbols (INamedTypeSymbol rhinoMocksMockRepositorySymbol)
    {
      return rhinoMocksMockRepositorySymbol.GetMembers ("StrictMock")
          .Concat (rhinoMocksMockRepositorySymbol.GetMembers ("StrictMultiMock"))
          .Concat (rhinoMocksMockRepositorySymbol.GetMembers ("GenerateStrictMock"));
    }

    private static IEnumerable<ISymbol> GetAllMockOrStubSymbols (INamedTypeSymbol rhinoMocksMockRepositorySymbol)
    {
      return rhinoMocksMockRepositorySymbol.GetMembers ("GenerateMock")
          .Concat (rhinoMocksMockRepositorySymbol.GetMembers ("DynamicMock"))
          .Concat (rhinoMocksMockRepositorySymbol.GetMembers ("DynamicMultiMock"))
          .Concat (rhinoMocksMockRepositorySymbol.GetMembers ("Stub"))
          .Concat (rhinoMocksMockRepositorySymbol.GetMembers ("GenerateStub"));
    }
  }
}