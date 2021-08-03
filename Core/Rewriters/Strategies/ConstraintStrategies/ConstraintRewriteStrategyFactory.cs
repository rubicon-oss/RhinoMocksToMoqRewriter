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
namespace RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ConstraintStrategies
{
  public static class ConstraintRewriteStrategyFactory
  {
    public static IConstraintRewriteStrategy GetRewriteStrategy (ExpressionSyntax node, SemanticModel model, RhinoMocksSymbols rhinoMocksSymbols)
    {
      var symbol = model.GetSymbolInfo (node).Symbol;
      if (symbol == null)
      {
        return DefaultConstraintRewriteStrategy.Instance;
      }

      if (IsNotAutomaticallyRewritable (node, symbol, model, rhinoMocksSymbols))
      {
        Console.Error.WriteLine (
            $"  WARNING: Unable to convert Rhino.Mocks.Constraints.Property.Value or Rhino.Mocks.Constraints.Is.NotNull\r\n"
            + $"  {node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}");

        return DefaultConstraintRewriteStrategy.Instance;
      }

      return symbol switch
      {
          _ when rhinoMocksSymbols.ConstraintIsEqualSymbols.Contains (symbol) => IsEqualConstraintRewriteStrategy.Instance,
          _ when rhinoMocksSymbols.ConstraintIsNotEqualSymbols.Contains (symbol) => IsNotEqualConstraintRewriteStrategy.Instance,
          _ when rhinoMocksSymbols.ConstraintIsSameSymbols.Contains (symbol) => IsSameConstraintRewriteStrategy.Instance,
          _ when rhinoMocksSymbols.ConstraintIsNotSameSymbols.Contains (symbol) => IsNotSameConstraintRewriteStrategy.Instance,
          _ when rhinoMocksSymbols.ConstraintIsNullSymbols.Contains (symbol) => IsNullConstraintRewriteStrategy.Instance,
          _ when rhinoMocksSymbols.ConstraintIsNotNullSymbols.Contains (symbol) => IsNotNullConstraintRewriteStrategy.Instance,
          _ when rhinoMocksSymbols.ConstraintIsGreaterThanSymbols.Contains (symbol) => IsGreaterThanConstraintRewriteStrategy.Instance,
          _ when rhinoMocksSymbols.ConstraintIsGreaterThanOrEqualSymbols.Contains (symbol) => IsGreaterThanOrEqualConstraintRewriteStrategy.Instance,
          _ when rhinoMocksSymbols.ConstraintIsLessThanSymbols.Contains (symbol) => IsLessThanConstraintRewriteStrategy.Instance,
          _ when rhinoMocksSymbols.ConstraintIsLessThanOrEqualSymbols.Contains (symbol) => IsLessThanOrEqualConstraintRewriteStrategy.Instance,
          _ when rhinoMocksSymbols.ConstraintListIsInSymbols.Contains (symbol) => ListIsInConstraintRewriteStrategy.Instance,
          _ when rhinoMocksSymbols.ConstraintListContainsAllSymbols.Contains (symbol) => ListContainsAllConstraintRewriteStrategy.Instance,
          _ when rhinoMocksSymbols.ConstraintPropertyValueSymbols.Contains (symbol) => PropertyValueConstraintRewriteStrategy.Instance,
          _ => DefaultConstraintRewriteStrategy.Instance
      };
    }

    private static bool IsNotAutomaticallyRewritable (ExpressionSyntax node, ISymbol symbol, SemanticModel model, RhinoMocksSymbols rhinoMocksSymbols)
    {
      if (node.Parent is not BinaryExpressionSyntax parentBinaryExpression)
      {
        return false;
      }

      var nextExpressionSymbol = model.GetSymbolInfo (parentBinaryExpression.Right).Symbol;
      if (node.IsEquivalentTo (parentBinaryExpression.Right, false))
      {
        nextExpressionSymbol = model.GetSymbolInfo (parentBinaryExpression.Left).Symbol;
      }

      return (rhinoMocksSymbols.ConstraintPropertyValueSymbols.Contains (symbol, SymbolEqualityComparer.Default)
              || rhinoMocksSymbols.ConstraintIsNotNullSymbols.Contains (symbol, SymbolEqualityComparer.Default))
             && (rhinoMocksSymbols.ConstraintPropertyValueSymbols.Contains (nextExpressionSymbol, SymbolEqualityComparer.Default)
                 || rhinoMocksSymbols.ConstraintIsNotNullSymbols.Contains (nextExpressionSymbol, SymbolEqualityComparer.Default));
    }
  }
}