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

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ConstraintsStrategies
{
  public static class ConstraintsRewriteStrategyFactory
  {
    public static IConstraintsRewriteStrategy GetRewriteStrategy (ExpressionSyntax node, SemanticModel model)
    {
      var symbol = model.GetSymbolInfo (node).Symbol;
      if (symbol == null)
      {
        return DefaultConstraintsRewriteStrategy.Instance;
      }

      var rhinoMocksConstraintsIsSymbol = model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.Is");
      var rhinoMocksConstraintsListSymbol = model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.List");
      var rhinoMocksConstraintsPropertySymbol = model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.Property");
      if (rhinoMocksConstraintsIsSymbol == null || rhinoMocksConstraintsListSymbol == null || rhinoMocksConstraintsPropertySymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found!");
      }

      if (IsNotAutomaticallyRewritable (node, symbol, model, rhinoMocksConstraintsIsSymbol, rhinoMocksConstraintsPropertySymbol))
      {
        Console.Error.WriteLine (
            $"  WARNING: Unable to convert Rhino.Mocks.Constraints.Property.Value or Rhino.Mocks.Constraints.Is.NotNull\r\n"
            + $"  {node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}");

        return DefaultConstraintsRewriteStrategy.Instance;
      }

      return symbol switch
      {
          _ when rhinoMocksConstraintsIsSymbol.GetMembers ("Equal").Contains (symbol) => IsEqualConstraintsRewriteStrategy.Instance,
          _ when rhinoMocksConstraintsIsSymbol.GetMembers ("NotEqual").Contains (symbol) => IsNotEqualConstraintsRewriteStrategy.Instance,
          _ when rhinoMocksConstraintsIsSymbol.GetMembers ("Same").Contains (symbol) => IsSameConstraintsRewriteStrategy.Instance,
          _ when rhinoMocksConstraintsIsSymbol.GetMembers ("NotSame").Contains (symbol) => IsNotSameConstraintsRewriteStrategy.Instance,
          _ when rhinoMocksConstraintsIsSymbol.GetMembers ("Null").Contains (symbol) => IsNullConstraintsRewriteStrategy.Instance,
          _ when rhinoMocksConstraintsIsSymbol.GetMembers ("NotNull").Contains (symbol) => IsNotNullConstraintsRewriteStrategy.Instance,
          _ when rhinoMocksConstraintsIsSymbol.GetMembers ("GreaterThan").Contains (symbol) => IsGreaterThanConstraintsRewriteStrategy.Instance,
          _ when rhinoMocksConstraintsIsSymbol.GetMembers ("GreaterThanOrEqual").Contains (symbol) => IsGreaterThanOrEqualConstraintsRewriteStrategy.Instance,
          _ when rhinoMocksConstraintsIsSymbol.GetMembers ("LessThan").Contains (symbol) => IsLessThanConstraintsRewriteStrategy.Instance,
          _ when rhinoMocksConstraintsIsSymbol.GetMembers ("LessThanOrEqual").Contains (symbol) => IsLessThanOrEqualConstraintsRewriteStrategy.Instance,
          _ when rhinoMocksConstraintsListSymbol.GetMembers ("IsIn").Contains (symbol) => ListIsInConstraintsRewriteStrategy.Instance,
          _ when rhinoMocksConstraintsListSymbol.GetMembers ("ContainsAll").Contains (symbol) => ListContainsAllConstraintsRewriteStrategy.Instance,
          _ when rhinoMocksConstraintsPropertySymbol.GetMembers ("Value").Contains (symbol) => PropertyValueConstraintsRewriteStrategy.Instance,
          _ => DefaultConstraintsRewriteStrategy.Instance
      };
    }

    private static bool IsNotAutomaticallyRewritable (
        ExpressionSyntax node,
        ISymbol symbol,
        SemanticModel model,
        INamedTypeSymbol rhinoMocksConstraintsIsSymbol,
        INamedTypeSymbol rhinoMocksConstraintsPropertySymbol)
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

      return (rhinoMocksConstraintsPropertySymbol.GetMembers ("Value").Contains (symbol, SymbolEqualityComparer.Default)
              || rhinoMocksConstraintsIsSymbol.GetMembers ("NotNull").Contains (symbol, SymbolEqualityComparer.Default))
             && (rhinoMocksConstraintsPropertySymbol.GetMembers ("Value").Contains (nextExpressionSymbol, SymbolEqualityComparer.Default)
                 || rhinoMocksConstraintsIsSymbol.GetMembers ("NotNull").Contains (nextExpressionSymbol, SymbolEqualityComparer.Default));
    }
  }
}