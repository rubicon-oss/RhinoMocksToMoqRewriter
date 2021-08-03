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
using System.Threading.Tasks;
using CommandLine;
using Microsoft.CodeAnalysis.CSharp;
using RhinoMocksToMoqRewriter.Core;

namespace RhinoMocksToMoqRewriter.Application
{
  public static class Program
  {
    public static async Task<int> Main (string[] args)
    {
      var compilationLoader = new CompilationLoader();
      var compilations = new List<CSharpCompilation>();
      try
      {
        await Parser.Default.ParseArguments<Options> (args)
            .WithParsedAsync (async opt => { compilations.AddRange (await ParseArgumentsAsync (compilationLoader, opt)); });
      }
      catch (System.IO.FileNotFoundException e)
      {
        await Console.Error.WriteLineAsync (e.Message);
        return 1;
      }

      await RewriterOrchestrator.RewriteAsync (compilations, compilationLoader.Generator, compilationLoader.Workspace);

      return 0;
    }

    private static async Task<IReadOnlyList<CSharpCompilation>> ParseArgumentsAsync (CompilationLoader compilationLoader, Options opt)
    {
      if (!string.IsNullOrEmpty (opt.SolutionPath))
      {
        var solution = await compilationLoader.LoadSolutionAsync (opt.SolutionPath);
        return await compilationLoader.LoadCompilationsAsync (solution.Projects);
      }

      if (!string.IsNullOrEmpty (opt.ProjectPath))
      {
        var project = await compilationLoader.LoadProjectAsync (opt.ProjectPath);
        return await compilationLoader.LoadCompilationsAsync (new[] { project });
      }

      throw new ArgumentException ("Unable to load documents!");
    }
  }
}