﻿//  Copyright (c) rubicon IT GmbH
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RhinoMocksToMoqRewriter.Core.Extensions;
using RhinoMocksToMoqRewriter.Core.Rewriters;
using RhinoMocksToMoqRewriter.Core.Utilities;

namespace RhinoMocksToMoqRewriter.Core
{
  public static class RewriterOrchestrator
  {
    private static readonly List<RewriterBase> s_rewriters =
        new List<RewriterBase>
        {
            new MockInstantiationRewriter (new Formatter())
        };

    public static async void Rewrite (IEnumerable<CSharpCompilation> compilations)
    {
      var syntaxTrees = new List<SyntaxTree>();
      foreach (var compilation in compilations)
      {
        var currentCompilation = compilation;
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
          var currentTree = syntaxTree;
          foreach (var rewriter in s_rewriters)
          {
            var model = currentCompilation.GetSemanticModel (currentTree);
            rewriter.Model = model;

            var newRoot = rewriter.Visit (await currentTree.GetRootAsync());
            var newTree = currentTree.WithRootAndOptions (newRoot, currentTree.Options);

            currentCompilation = currentCompilation.ReplaceSyntaxTreeAndUpdateCompilation (currentTree, newTree);
            currentTree = newTree;
          }

          if (currentTree != syntaxTree)
          {
            syntaxTrees.Add (currentTree);
          }
        }
      }

      WriteBackChanges (syntaxTrees);
    }

    private static async void WriteBackChanges (IEnumerable<SyntaxTree> syntaxTrees)
    {
      foreach (var syntaxTree in syntaxTrees)
      {
        await File.WriteAllTextAsync (
            syntaxTree.FilePath!,
            (await syntaxTree.GetRootAsync()).ToFullString());
      }
    }
  }
}