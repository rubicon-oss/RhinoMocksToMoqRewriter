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
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NUnit.Framework;

namespace RhinoMocksToMoqRewriter.Tests
{
  [TestFixture]
  public class IntegrationTests
  {
    private MSBuildWorkspace _workspace;
    private const string c_pathToExecutable = @"../../../../Application/bin/Release/net5.0/RhinoMocksToMoqRewriter.dll";
    private const string c_pathToProject = @"../../../../TestProject/TestProject.csproj";
    private const string c_pathToApplication = @"../../../../Application/Application.csproj";
    private const string c_pathToCore = @"../../../../Core/Core.csproj";
    private List<(string FilePath, string Text)> _backupDocuments;
    private Project _project;

    [SetUp]
    public void SetUp ()
    {
      Initialize();
    }

    [TearDown]
    public void TearDown ()
    {
      Rollback();
    }

    [Test]
    public void Rewrite_Files ()
    {
      Assert.IsNotEmpty (_backupDocuments);

      var applicationStartInfo = new ProcessStartInfo ("dotnet") { Arguments = $"{c_pathToExecutable} -- -P {c_pathToProject}" };
      var process = Process.Start (applicationStartInfo);
      Assert.IsNotNull (process);

      process.WaitForExit();

      Assert.IsTrue (process.HasExited);
      Assert.AreEqual (0, process.ExitCode);

      var expectedTexts = _backupDocuments.Select (d => File.ReadAllText (d.FilePath! + ".expected")).ToList();
      var actualTexts = _backupDocuments.Select (d => File.ReadAllText (d.FilePath!)).ToList();

      Assert.IsNotEmpty (actualTexts);
      Assert.IsNotEmpty (expectedTexts);
      Assert.AreEqual (expectedTexts.Count, actualTexts.Count);

      CompareDocuments (actualTexts, expectedTexts);
    }

    private static void CompareDocuments (List<string> actualTexts, List<string> expectedTexts)
    {
      for (var i = 0; i < expectedTexts.Count; i++)
      {
        var expectedText = expectedTexts[i];
        var actualText = actualTexts[i];

        Assert.IsNotEmpty (expectedText);
        Assert.IsNotEmpty (actualText);

        Assert.AreEqual (expectedText, actualText);
      }
    }

    private void Initialize ()
    {
      var instance = MSBuildLocator.QueryVisualStudioInstances().First();
      MSBuildLocator.RegisterInstance (instance);
      _workspace = MSBuildWorkspace.Create();

      _project = _workspace.OpenProjectAsync (c_pathToProject).GetAwaiter().GetResult();
      _backupDocuments = _project.Documents
          .Select (d => (d.FilePath, File.ReadAllText (d.FilePath!)))
          .Where (d => d.FilePath!.EndsWith ("Tests.cs"))
          .ToList();

      Build (c_pathToApplication);
      Build (c_pathToCore);
    }

    private static void Build (string path)
    {
      var applicationStartInfo = new ProcessStartInfo ("dotnet") { Arguments = $"build {path} --configuration Release --no-restore" };
      var process = Process.Start (applicationStartInfo);
      Assert.IsNotNull (process);

      process.WaitForExit();

      Assert.IsTrue (process.HasExited);
      Assert.AreEqual (0, process.ExitCode);
    }

    private void Rollback ()
    {
      foreach (var (filePath, text) in _backupDocuments)
      {
        File.WriteAllText (filePath, text);
      }
    }
  }
}