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
using RhinoMocksToMoqRewriter.Core.Extensions;
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ConstraintsStrategies;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class ConstraintsRewriter : RewriterBase
  {
    public override SyntaxNode? VisitArgument (ArgumentSyntax node)
    {
      var baseCallNode = (ArgumentSyntax) base.VisitArgument (node)!;

      var rhinoMocksConstraintsIsSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.Is");
      var rhinoMocksArgSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Arg`1");
      if (rhinoMocksConstraintsIsSymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var rhinoMocksArgMatchesSymbol = rhinoMocksArgSymbol!.GetMembers ("Matches");

      if (node.Expression is InvocationExpressionSyntax invocationExpression)
      {
        if (rhinoMocksArgMatchesSymbol.Contains (Model.GetSymbolInfo (invocationExpression).Symbol?.OriginalDefinition, SymbolEqualityComparer.Default))
        {
          var lambda = MoqSyntaxFactory.SimpleLambdaExpression (ConvertExpression (invocationExpression.ArgumentList.GetFirstArgument().Expression));

          return MoqSyntaxFactory.SimpleArgument (
              invocationExpression.WithArgumentList (
                  SyntaxFactory.ArgumentList (
                      SyntaxFactory.SingletonSeparatedList (
                          SyntaxFactory.Argument (lambda)))));
        }
      }

      return baseCallNode;
    }

    private ExpressionSyntax ConvertExpression (ExpressionSyntax expression)
    {
      if (expression is BinaryExpressionSyntax binaryExpression)
      {
        var left = ConvertExpression (binaryExpression.Left);
        var right = ConvertExpression (binaryExpression.Right);
        if (binaryExpression.OperatorToken.IsKind (SyntaxKind.AmpersandToken))
        {
          return MoqSyntaxFactory.LogicalAndBinaryExpression (left, right);
        }

        if (binaryExpression.OperatorToken.IsKind (SyntaxKind.BarToken))
        {
          return MoqSyntaxFactory.LogicalOrBinaryExpression (left, right);
        }
      }

      if (expression is not InvocationExpressionSyntax invocationExpression)
      {
        return expression;
      }

      var strategy = ConstraintsRewriteStrategyFactory.GetRewriteStrategy (invocationExpression, Model);
      return strategy.Rewrite (invocationExpression);
    }
  }
}