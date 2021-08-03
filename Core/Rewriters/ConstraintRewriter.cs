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
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ConstraintStrategies;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class ConstraintRewriter : RewriterBase
  {
    public override SyntaxNode? VisitArgument (ArgumentSyntax node)
    {
      var baseCallNode = (ArgumentSyntax) base.VisitArgument (node)!;
      if (node.Expression is not InvocationExpressionSyntax invocationExpression)
      {
        return baseCallNode;
      }

      var symbol = Model.GetSymbolInfo (invocationExpression).Symbol?.OriginalDefinition;
      if (!RhinoMocksSymbols.ArgMatchesSymbols.Contains (symbol, SymbolEqualityComparer.Default))
      {
        return baseCallNode;
      }

      if (invocationExpression.ArgumentList.Arguments.First().Expression is LambdaExpressionSyntax)
      {
        return baseCallNode;
      }

      return MoqSyntaxFactory.SimpleArgument (
          invocationExpression.WithArgumentList (
              MoqSyntaxFactory.SimpleArgumentList (
                  MoqSyntaxFactory.SimpleLambdaExpression (
                      Formatter.MarkWithFormatAnnotation (
                          ConvertExpression (invocationExpression.ArgumentList.GetFirstArgument().Expression))))));
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

      var strategy = ConstraintRewriteStrategyFactory.GetRewriteStrategy (invocationExpression, Model, RhinoMocksSymbols);
      return strategy.Rewrite (invocationExpression);
    }
  }
}