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
using Microsoft.CodeAnalysis;
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

      return symbol switch
      {
          var s when rhinoMocksConstraintsIsSymbol.GetMembers ("Equal").Contains (s) => IsEqualOrSameConstraintsRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsSymbol.GetMembers ("NotEqual").Contains (s) => IsNotEqualOrSameConstraintsRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsSymbol.GetMembers ("Same").Contains (s) => IsEqualOrSameConstraintsRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsSymbol.GetMembers ("NotSame").Contains (s) => IsNotEqualOrSameConstraintsRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsSymbol.GetMembers ("Null").Contains (s) => IsNullConstraintsRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsSymbol.GetMembers ("NotNull").Contains (s) => IsNotNullConstraintsRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsSymbol.GetMembers ("GreaterThan").Contains (s) => IsGreaterThanConstraintsRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsSymbol.GetMembers ("GreaterThanOrEqual").Contains (s) => IsGreaterThanOrEqualConstraintsRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsSymbol.GetMembers ("LessThan").Contains (s) => IsLessThanConstraintsRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsSymbol.GetMembers ("LessThanOrEqual").Contains (s) => IsLessThanOrEqualConstraintsRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsListSymbol.GetMembers ("IsIn").Contains (s) => ListIsInConstraintsRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsListSymbol.GetMembers ("ContainsAll").Contains (s) => ListContainsAllConstraintsRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsPropertySymbol.GetMembers ("Value").Contains (s) => PropertyValueConstraintsRewriteStrategy.Instance,
          _ => DefaultConstraintsRewriteStrategy.Instance
      };
    }
  }
}