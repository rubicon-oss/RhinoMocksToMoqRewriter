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
using RhinoMocksToMoqRewriter.Core.Extensions;
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies;

namespace RhinoMocksToMoqRewriter.Core.Utilities
{
  public static class ArgumentRewriterStrategyFactory
  {
    public static IArgumentRewriteStrategy GetRewriterStrategy (ArgumentSyntax node, SemanticModel model)
    {
      var symbol = model.GetSymbolInfo (node.Expression).GetFirstOverloadOrDefault();
      if (symbol == null)
      {
        return new DefaultArgumentRewriteStrategy();
      }

      var compilationSymbol = model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Arg");
      var isRhinoMocksMethod = SymbolEqualityComparer.Default.Equals (symbol.ContainingType.OriginalDefinition, compilationSymbol);
      if (isRhinoMocksMethod && symbol.Name == "Is")
      {
        return new ArgIsArgumentArgumentRewriteStrategy();
      }

      compilationSymbol = model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.ListArg`1");
      isRhinoMocksMethod = SymbolEqualityComparer.Default.Equals (symbol.ContainingType.OriginalDefinition, compilationSymbol);
      if (isRhinoMocksMethod)
      {
        return symbol.Name switch
        {
            "Equal" => new ArgListEqualArgumentArgumentRewriteStrategy(),
            "IsIn" => new ArgListIsInArgumentArgumentRewriteStrategy(),
            "ContainsAll" => new ArgListContainsAllArgumentArgumentRewriteStrategy(),
            _ => new DefaultArgumentRewriteStrategy()
        };
      }

      compilationSymbol = model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Arg`1");
      isRhinoMocksMethod = SymbolEqualityComparer.Default.Equals (symbol.ContainingType.OriginalDefinition, compilationSymbol);
      if (isRhinoMocksMethod && symbol.Name == "Matches")
      {
        return new ArgMatchesArgumentArgumentRewriteStrategy();
      }

      compilationSymbol = model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.IsArg`1");
      isRhinoMocksMethod = SymbolEqualityComparer.Default.Equals (symbol.ContainingType.OriginalDefinition, compilationSymbol);
      if (isRhinoMocksMethod)
      {
        return symbol.Name switch
        {
            "Anything" => new ArgIsAnythingArgumentArgumentRewriteStrategy(),
            "Equal" => new ArgIsArgumentArgumentRewriteStrategy(),
            "Same" => new ArgIsArgumentArgumentRewriteStrategy(),
            "NotSame" => new ArgIsNotSameArgumentArgumentRewriteStrategy(),
            "Null" => new ArgIsNullArgumentArgumentRewriteStrategy(),
            "NotNull" => new ArgIsNotNullArgumentArgumentRewriteStrategy(),
            "GreaterThan" => new ArgIsGreaterThanArgumentArgumentRewriteStrategy(),
            "GreaterThanOrEqual" => new ArgIsGreaterThanOrEqualArgumentArgumentRewriteStrategy(),
            "LessThan" => new ArgIsLessThanArgumentArgumentRewriteStrategy(),
            "LessThanOrEqual" => new ArgIsLessThanOrEqualArgumentArgumentRewriteStrategy(),
            _ => new DefaultArgumentRewriteStrategy()
        };
      }

      return new DefaultArgumentRewriteStrategy();
    }
  }
}