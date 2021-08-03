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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ArgumentStrategies
{
  public static class ArgumentRewriteStrategyFactory
  {
    public static IArgumentRewriteStrategy GetRewriteStrategy (ArgumentSyntax node, SemanticModel model, RhinoMocksSymbols rhinoMocksSymbols)
    {
      var symbol = model.GetSymbolInfo (node.Expression).Symbol?.OriginalDefinition;
      if (symbol == null)
      {
        return new DefaultArgumentRewriteStrategy();
      }

      return symbol switch
      {
          var s when rhinoMocksSymbols.ArgIsSymbols.Contains (s) => ArgIsArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgListEqualSymbols.Contains (s) => ArgListEqualArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgListIsInSymbols.Contains (s) => ArgListIsInArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgListContainsAll.Contains (s) => ArgListContainsAllArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgMatchesSymbols.Contains (s) => ArgMatchesArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgIsAnythingSymbols.Contains (s) => ArgIsAnythingArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgIsEqualSymbols.Contains (s) => ArgIsArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgIsNotEqualSymbols.Contains (s) => ArgIsNotEqualArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgIsSameSymbols.Contains (s) => ArgIsArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgIsNotSameSymbols.Contains (s) => ArgIsNotSameArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgIsNullSymbols.Contains (s) => ArgIsNullArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgIsNotNullSymbols.Contains (s) => ArgIsNotNullArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgIsGreaterThanSymbols.Contains (s) => ArgIsGreaterThanArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgIsGreaterThanOrEqualSymbols.Contains (s) => ArgIsGreaterThanOrEqualArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgIsLessThanSymbols.Contains (s) => ArgIsLessThanArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgIsLessThaOrEqualSymbols.Contains (s) => ArgIsLessThanOrEqualArgumentRewriteStrategy.Instance,
          var s when rhinoMocksSymbols.ArgTextLikeSymbols.Contains (s) => ArgIsArgumentRewriteStrategy.Instance,
          _ => DefaultArgumentRewriteStrategy.Instance
      };
    }
  }
}