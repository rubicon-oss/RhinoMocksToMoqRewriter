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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class MockInstantiationRewriter : RewriterBase
  {
    private const string c_generateStrictMock = "GenerateStrictMock";
    private const string c_generatePartialMock = "GeneratePartialMock";
    private readonly IFormatter _formatter;

    public MockInstantiationRewriter (IFormatter formatter)
    {
      _formatter = formatter;
    }

    public override SyntaxNode? VisitInvocationExpression (InvocationExpressionSyntax node)
    {
      if (Model == null)
      {
        throw new InvalidOperationException ("SemanticModel must not be null!");
      }

      var compilationSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.MockRepository");
      if (compilationSymbol == null)
      {
        throw new ArgumentException ("Rhino.Mocks cannot be found.");
      }

      var methodSymbol = Model.GetSymbolInfo (node).Symbol as IMethodSymbol;
      if (methodSymbol == null)
      {
        return node;
      }

      var isRhinoMocksMethod = SymbolEqualityComparer.Default.Equals (methodSymbol.ContainingType, compilationSymbol);
      if (!isRhinoMocksMethod)
      {
        return node;
      }

      var rhinoMethodGenericName = node.DescendantNodes().FirstOrDefault (n => n.IsKind (SyntaxKind.GenericName)) as GenericNameSyntax;
      if (rhinoMethodGenericName == null || rhinoMethodGenericName.Identifier.Text != methodSymbol.Name)
      {
        return node;
      }

      var nodeWithRewrittenDescendants = (InvocationExpressionSyntax) base.VisitInvocationExpression (node)!;
      var moqMockTypeArgumentList = SyntaxFactory.TypeArgumentList().AddArguments (rhinoMethodGenericName.TypeArgumentList.Arguments.First());
      var moqMockArgumentSyntaxList = nodeWithRewrittenDescendants.ArgumentList;
      if (methodSymbol.Name == c_generateStrictMock)
      {
        moqMockArgumentSyntaxList = moqMockArgumentSyntaxList.WithArguments (
            moqMockArgumentSyntaxList.Arguments.Insert (
                0,
                MoqSyntaxFactory.MockBehaviorStrictArgument()));
      }

      InitializerExpressionSyntax? initializer = null;
      if (methodSymbol.Name == c_generatePartialMock)
      {
        initializer = MoqSyntaxFactory.CallBaseInitializer();
      }

      var newNode =
          MoqSyntaxFactory.MockCreationExpression (
              moqMockTypeArgumentList,
              moqMockArgumentSyntaxList,
              initializer);

      var formattedNode = _formatter.Format (newNode);
      return formattedNode;
    }
  }
}