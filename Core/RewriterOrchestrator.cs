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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using RhinoMocksToMoqRewriter.Core.Extensions;
using RhinoMocksToMoqRewriter.Core.Rewriters;

namespace RhinoMocksToMoqRewriter.Core
{
  public static class RewriterOrchestrator
  {
    private static IFormatter Formatter { get; } = new Formatter();

    private static readonly List<RewriterBase> s_rewriters =
        new List<RewriterBase>
        {
            new LastCallRewriter(),
            new SetupResultForRewriter(),
            new ExpectCallRewriter(),
            new ConstraintRewriter(),
            new MoqUsingDirectiveRewriter(),
            new IgnoreArgumentsRewriter(),
            new MockSetupRewriter (Formatter),
            new VerifyRewriter(),
            new FieldTypeRewriter (Formatter),
            new MockInstantiationRewriter (Formatter),
            new OrderedMockRewriter(),
            new ArgumentRewriter(),
            new ObsoleteMethodRewriter (Formatter),
            new ObjectRewriter(),
            new RhinoMocksUsingDirectiveRewriter(),
        };

    public static async Task RewriteAsync (IEnumerable<CSharpCompilation> compilations, SyntaxGenerator generator, Workspace workspace)
    {
      var syntaxTrees = new List<SyntaxTree>();
      foreach (var compilation in compilations)
      {
        await Console.Error.WriteLineAsync (compilation.AssemblyName);
        var currentCompilation = compilation;
        var rhinoMocksSymbols = new RhinoMocksSymbols (currentCompilation);
        var moqSymbols = new MoqSymbols (currentCompilation);
        foreach (var syntaxTree in compilation.SyntaxTrees.Where (s => s.ContainsRhinoMocksUsingDirective()))
        {
          await Console.Error.WriteLineAsync ($"- {syntaxTree.FilePath}");
          var currentTree = syntaxTree;
          foreach (var rewriter in s_rewriters)
          {
            var model = currentCompilation.GetSemanticModel (currentTree);
            rewriter.Model = model;
            rewriter.CompilationId = Guid.NewGuid();
            rewriter.Generator = generator;
            rewriter.RhinoMocksSymbols ??= rhinoMocksSymbols;
            rewriter.MoqSymbols ??= moqSymbols;

            var newRoot = rewriter.Visit (await currentTree.GetRootAsync());
            var newTree = currentTree.WithRootAndOptions (newRoot, currentTree.Options);

            currentCompilation = currentCompilation.ReplaceSyntaxTreeAndUpdateCompilation (currentTree, newTree);
            currentTree = newTree;
          }

          if (currentTree != syntaxTree)
          {
            syntaxTrees.Add (Rewriters.Formatter.FormatAnnotatedNodes (currentTree, workspace));
          }

          SyntaxNodeTrackingExtensions.ClearLookup();
        }
      }

      await WriteBackChangesAsync (syntaxTrees);
    }

    private static async Task WriteBackChangesAsync (IEnumerable<SyntaxTree> syntaxTrees)
    {
      foreach (var syntaxTree in syntaxTrees)
      {
        File.WriteAllText (
            syntaxTree.FilePath!,
            (await syntaxTree.GetRootAsync()).ToFullString(),
            syntaxTree.Encoding!);
      }
    }
  }
}