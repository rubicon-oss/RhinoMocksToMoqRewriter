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
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;

namespace RhinoMocksToMoqRewriter.Application
{
  public class CompilationLoader
  {
    private readonly MSBuildWorkspace _msBuildWorkspace;

    private AnalyzerManager _analyzerManager;

    public SyntaxGenerator Generator { get; }

    public Workspace Workspace { get; private set; } = null!;

    public CompilationLoader ()
    {
      var instance = MSBuildLocator.QueryVisualStudioInstances().First();
      MSBuildLocator.RegisterInstance (instance);
      _msBuildWorkspace = MSBuildWorkspace.Create();
      _analyzerManager = new AnalyzerManager();
      Generator = SyntaxGenerator.GetGenerator (_msBuildWorkspace, "C#");
    }

    public async Task<Solution> LoadSolutionAsync (string pathToSolution)
    {
      var solution = await _msBuildWorkspace.OpenSolutionAsync (pathToSolution);
      Workspace = solution.Workspace;
      var m = new AnalyzerManager (pathToSolution);
      var r = m.Projects.Where (s => s.Key.Contains ("ObjectBinding.Web.UnitTests")).First().Value.Build();
      var f = (await r.First().GetWorkspace().CurrentSolution.Projects.First().GetCompilationAsync())!.GetDiagnostics().ToList();
      var w = r.First().GetWorkspace();
      w.ClearSolution();
      var e = w.AddSolution (SolutionInfo.Create (solution.Id, solution.Version, pathToSolution));
      return solution;
    }

    public async Task<Project> LoadProjectAsync (string pathToProject)
    {
      //var project = await _msBuildWorkspace.OpenProjectAsync (pathToProject);
      //Workspace = project.Solution.Workspace;
      var adhocWorkspace = _analyzerManager.GetProject (pathToProject).Build().Results.First().GetWorkspace();
      var project = adhocWorkspace.CurrentSolution.Projects.First();
      Workspace = adhocWorkspace;
      return await Task.FromResult(project);
    }

    public async Task<IReadOnlyList<CSharpCompilation>> LoadCompilationsAsync (IEnumerable<Project> projects)
    {
      var allCompilations = await Task
          .WhenAll (projects.Select (async p => await p.GetCompilationAsync()));
      var rhinoMocksCompilations = allCompilations
          .Select (c => c as CSharpCompilation)
          .Where (c => c?.ReferencedAssemblyNames.Any (a => a.Name.Contains ("Rhino.Mocks")) == true)
          .ToList();
      var moqCompilations = allCompilations
          .Select (c => c as CSharpCompilation)
          .Where (c => c?.ReferencedAssemblyNames.Any (a => a.Name.Contains ("Moq")) == true)
          .ToList();

      AssertThatAllRhinoMocksCompilationsReferenceMoq (rhinoMocksCompilations!, moqCompilations!);

      return rhinoMocksCompilations.AsReadOnly()!;
    }

    private static void AssertThatAllRhinoMocksCompilationsReferenceMoq (IReadOnlyList<CSharpCompilation> rhinoMocksCompilations, IReadOnlyList<CSharpCompilation> moqCompilations)
    {
      var projectsWithMissingMoqReference = rhinoMocksCompilations.Except (moqCompilations).Select (c => c.AssemblyName).ToList();
      if (projectsWithMissingMoqReference.Any())
      {
        var message =
            $"Moq is missing in the following projects where Rhino.Mocks IS installed: {Environment.NewLine}"
            + string.Join (Environment.NewLine, projectsWithMissingMoqReference);

        throw new InvalidOperationException (message);
      }
    }
  }
}