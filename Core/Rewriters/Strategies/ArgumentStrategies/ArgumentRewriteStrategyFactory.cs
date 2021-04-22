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

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ArgumentStrategies
{
  public static class ArgumentRewriteStrategyFactory
  {
    public static IArgumentRewriteStrategy GetRewriteStrategy (ArgumentSyntax node, SemanticModel model)
    {
      var symbol = model.GetSymbolInfo (node.Expression).Symbol?.OriginalDefinition;
      if (symbol == null)
      {
        return new DefaultArgumentRewriteStrategy();
      }

      var rhinoMocksArgSymbol = model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Arg");
      var rhinoMocksConstraintsListArgSymbol = model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.ListArg`1");
      var rhinoMocksGenericArg = model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Arg`1");
      var rhinoMocksConstraintsIsArgSymbol = model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.IsArg`1");
      var rhinoMocksArgTextSymbol = model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.TextArg");
      if (rhinoMocksArgSymbol == null || rhinoMocksConstraintsListArgSymbol == null || rhinoMocksGenericArg == null || rhinoMocksConstraintsIsArgSymbol == null
          || rhinoMocksArgTextSymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found!");
      }

      return symbol switch
      {
          var s when rhinoMocksArgSymbol.GetMembers ("Is").Contains (s) => ArgIsArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsListArgSymbol.GetMembers ("Equal").Contains (s) => ArgListEqualArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsListArgSymbol.GetMembers ("IsIn").Contains (s) => ArgListIsInArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsListArgSymbol.GetMembers ("ContainsAll").Contains (s) => ArgListContainsAllArgumentRewriteStrategy.Instance,
          var s when rhinoMocksGenericArg.GetMembers ("Matches").Contains (s) => ArgMatchesArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsArgSymbol.GetMembers ("Anything").Contains (s) => ArgIsAnythingArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsArgSymbol.GetMembers ("Equal").Contains (s) => ArgIsArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsArgSymbol.GetMembers ("NotEqual").Contains (s) => ArgIsNotEqualOrSameArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsArgSymbol.GetMembers ("Same").Contains (s) => ArgIsArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsArgSymbol.GetMembers ("NotSame").Contains (s) => ArgIsNotEqualOrSameArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsArgSymbol.GetMembers ("Null").Contains (s) => ArgIsNullArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsArgSymbol.GetMembers ("NotNull").Contains (s) => ArgIsNotNullArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsArgSymbol.GetMembers ("GreaterThan").Contains (s) => ArgIsGreaterThanArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsArgSymbol.GetMembers ("GreaterThanOrEqual").Contains (s) => ArgIsGreaterThanOrEqualArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsArgSymbol.GetMembers ("LessThan").Contains (s) => ArgIsLessThanArgumentRewriteStrategy.Instance,
          var s when rhinoMocksConstraintsIsArgSymbol.GetMembers ("LessThanOrEqual").Contains (s) => ArgIsLessThanOrEqualArgumentRewriteStrategy.Instance,
          var s when rhinoMocksArgTextSymbol.GetMembers ("Like").Contains (s) => ArgIsArgumentRewriteStrategy.Instance,
          _ => DefaultArgumentRewriteStrategy.Instance
      };
    }
  }
}