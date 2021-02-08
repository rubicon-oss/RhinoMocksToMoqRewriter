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
using Microsoft.CodeAnalysis;

namespace RhinoMocksToMoqRewriter.Application
{
  public static class Program
  {
    public static async Task<int> Main (string[] args)
    {
      var documents = new List<Document>();
      try
      {
        await Parser.Default.ParseArguments<Options> (args)
            .WithParsedAsync (async opt => { documents.AddRange (await ParseArguments (opt)); });
      }
      catch (System.IO.FileNotFoundException e)
      {
        await Console.Error.WriteLineAsync (e.Message);
        return 1;
      }

      foreach (var document in documents)
      {
        // TODO: Create Syntax Tree from document
        // TODO: Create Semantic Model from document
        Console.WriteLine (document.Name);
      }

      return 0;
    }

    private static async Task<IReadOnlyList<Document>> ParseArguments (Options opt)
    {
      var documentLoader = new DocumentLoader();
      if (!string.IsNullOrEmpty (opt.SolutionPath))
      {
        var solution = await documentLoader.LoadSolution (opt.SolutionPath);
        return await documentLoader.LoadDocuments (solution.Projects);
      }

      if (!string.IsNullOrEmpty (opt.ProjectPath))
      {
        var project = await documentLoader.LoadProject (opt.ProjectPath);
        return await documentLoader.LoadDocuments (new[] { project });
      }

      throw new ArgumentException ("Unable to load documents!");
    }
  }
}