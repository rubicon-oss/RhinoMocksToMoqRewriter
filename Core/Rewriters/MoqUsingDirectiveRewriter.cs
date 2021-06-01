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

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class MoqUsingDirectiveRewriter : RewriterBase
  {
    public override SyntaxNode? VisitCompilationUnit (CompilationUnitSyntax node)
    {
      var moqUsing = MoqSyntaxFactory.MoqUsingDirective();
      var mockRepositoryAlias = MoqSyntaxFactory.RhinoMocksRepositoryAlias();
      var moqProtectedUsing = MoqSyntaxFactory.MoqProtectedUsingDirective();
      var currentUsings = node.Usings;

      if (!currentUsings.Any (u => u.IsEquivalentTo (moqUsing, false)))
      {
        currentUsings = currentUsings.Add (Formatter.MarkWithFormatAnnotation (moqUsing));
      }

      if (!currentUsings.Any (u => u.IsEquivalentTo (moqProtectedUsing, false)))
      {
        currentUsings = currentUsings.Add (Formatter.MarkWithFormatAnnotation (moqProtectedUsing));
      }

      if (!currentUsings.Any (u => u.IsEquivalentTo (mockRepositoryAlias, false)))
      {
        currentUsings = currentUsings.Add (Formatter.MarkWithFormatAnnotation (mockRepositoryAlias));
      }

      var systemUsings = currentUsings.Where (u => IsSystemUsingDirective (u) && !HasAlias (u) && !IsStaticUsingDirective (u)).OrderBy (u => u.ToString());
      var otherUsings = currentUsings.Where (u => !IsSystemUsingDirective (u) && !HasAlias (u) && !IsStaticUsingDirective (u)).OrderBy (u => u.ToString());
      var aliasUsings = currentUsings.Where (HasAlias).OrderBy (u => u.ToString());
      var staticUsings = currentUsings.Where (u => !HasAlias (u) && IsStaticUsingDirective (u)).OrderBy (u => u.ToString());

      return node.WithUsings (
          new SyntaxList<UsingDirectiveSyntax> (
              systemUsings
                  .Concat (otherUsings)
                  .Concat (staticUsings)
                  .Concat (aliasUsings)));
    }

    private static bool IsStaticUsingDirective (UsingDirectiveSyntax usingDirective)
    {
      return usingDirective.StaticKeyword.Value?.Equals ("static") == true;
    }

    private static bool IsSystemUsingDirective (UsingDirectiveSyntax usingDirective)
    {
      return usingDirective.DescendantNodes().Any (u => u.IsEquivalentTo (MoqSyntaxFactory.SystemIdentifierName, false));
    }

    private static bool HasAlias (UsingDirectiveSyntax usingDirective)
    {
      return usingDirective.Alias is not null;
    }
  }
}