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
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using RhinoMocksToMoqRewriter.Core.Rewriters;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters
{
  [TestFixture]
  public class LastCallRewriterTests
  {
    private LastCallRewriter _rewriter;

    private readonly Context _context =
        new Context
        {
            //language=C#
            UsingContext =
                @"
#nullable enable",
            //language=C#
            InterfaceContext =
                @"
void DoSomething (Func<int> func);
void DoSomething (int? a, string b);",
            //language=C#
            ClassContext =
                @"
private static ITestInterface _mock1, _mock2, _mock3 = MockRepository.GenerateMock<ITestInterface>();
private static ITestInterface _mock4 = MockRepository.GenerateMock<ITestInterface>();
private static ITestInterface _mock5 = MockRepository.GenerateMock<ITestInterface>();
private static ITestInterface _mock6;
private static MockRepository _mockRepository = new MockRepository();",
            //language=C#
            MethodContext =
                @"
_mock6 = MockRepository.GenerateMock<ITestInterface>();
ITestInterface mock1, mock2, mock3 = MockRepository.GenerateMock<ITestInterface>();
var mock4 = MockRepository.GenerateMock<ITestInterface>();
var mock5 = MockRepository.GenerateMock<ITestInterface>();",
        };

    [SetUp]
    public void Setup ()
    {
      _rewriter = new LastCallRewriter();
      _rewriter.Generator = SyntaxGenerator.GetGenerator (new AdhocWorkspace(), "C#");
    }

    [Test]
    [TestCase (
        //language=C#
        @"
_mock1.DoSomething (null);
LastCall.Constraints (Rhino.Mocks.Constraints.Is.NotNull());",
        //language=C#
        @"
Expect.Call (() => _mock1.DoSomething (null)).Constraints(Rhino.Mocks.Constraints.Is.NotNull());")]
    [TestCase (
        //language=C#
        @"
_mock1.DoSomething (null);
LastCall.IgnoreArguments();",
        //language=C#
        @"
Expect.Call (() => _mock1.DoSomething (null)).IgnoreArguments();")]
    [TestCase (
        //language=C#
        @"
_mock1.DoSomething (null, """");
LastCall.Constraints (
  Rhino.Mocks.Constraints.Is.Equal (2) & Rhino.Mocks.Constraints.Is.NotNull(), 
  Rhino.Mocks.Constraints.Is.Equal (""abc""));",
        //language=C#
        @"
Expect.Call (() => _mock1.DoSomething (null, """"))
  .Constraints (
    Rhino.Mocks.Constraints.Is.Equal (2) & Rhino.Mocks.Constraints.Is.NotNull(), 
    Rhino.Mocks.Constraints.Is.Equal (""abc""));")]
    [TestCase (
        //language=C#
        @"
_mock1.DoSomething (null);
Console.WriteLine (1);
LastCall.Constraints (Rhino.Mocks.Constraints.Is.NotNull());",
        //language=C#
        @"
Console.WriteLine (1);
Expect.Call (() => _mock1.DoSomething (null)).Constraints(Rhino.Mocks.Constraints.Is.NotNull());")]
    [TestCase (
        //language=C#
        @"
_mock1.DoSomething (null);
_mock2.DoSomething (null);
_mock3.DoSomething (null);
_mock4.DoSomething (null);
LastCall.Constraints (Rhino.Mocks.Constraints.Is.NotNull());",
        //language=C#
        @"
_mock1.DoSomething (null);
_mock2.DoSomething (null);
_mock3.DoSomething (null);
Expect.Call (() => _mock4.DoSomething (null)).Constraints(Rhino.Mocks.Constraints.Is.NotNull());")]
    [TestCase (
        //language=C#
        @"
_mock6.DoSomething (null);
LastCall.IgnoreArguments();",
        //language=C#
        @"
Expect.Call (() => _mock6.DoSomething (null)).IgnoreArguments();")]
    [TestCase (
        //language=C#
        @"
_mock6.DoSomething (null);
LastCall.IgnoreArguments();
_mock3.DoSomething (null);
Console.WriteLine (1);",
        //language=C#
        @"
Expect.Call (() => _mock6.DoSomething (null)).IgnoreArguments();
_mock3.DoSomething (null);
Console.WriteLine (1);")]
    [TestCase (
        //language=C#
        @"
_mock1.DoSomething (null);
LastCall.IgnoreArguments();
_mock2.DoSomething (null);
LastCall.IgnoreArguments();
_mock3.DoSomething (null);
Console.WriteLine (1);
LastCall.IgnoreArguments();
Console.WriteLine (1);",
        //language=C#
        @"
Expect.Call (() => _mock1.DoSomething (null)).IgnoreArguments();
Expect.Call (() => _mock2.DoSomething (null)).IgnoreArguments();
Console.WriteLine (1);
Expect.Call (() => _mock3.DoSomething (null)).IgnoreArguments();
Console.WriteLine (1);")]
    [TestCase (
        //language=C#
        @"
using(null)
{
  _mock6.DoSomething (null);
  LastCall.IgnoreArguments();
}",
        //language=C#
        @"
using(null)
{
  Expect.Call (() => _mock6.DoSomething (null)).IgnoreArguments();
}")]
    [TestCase (
        //language=C#
        @"
using(null)
{
  _mock6.DoSomething (null);
  LastCall.IgnoreArguments();
}

_mock4.DoSomething (null);
LastCall.IgnoreArguments();",
        //language=C#
        @"
using(null)
{
  Expect.Call (() => _mock6.DoSomething (null)).IgnoreArguments();
}

Expect.Call (() => _mock4.DoSomething (null)).IgnoreArguments();")]
    [TestCase (
        //language=C#
        @"
LastCall.Constraints (Rhino.Mocks.Constraints.Is.NotNull());",
        //language=C#
        @"
LastCall.Constraints (Rhino.Mocks.Constraints.Is.NotNull());")]
    [TestCase (
        //language=C#
        @"
_mock4.DoSomething (null);
_mock6.DoSomething (null);
LastCall.IgnoreArguments();
LastCall.IgnoreArguments();",
        //language=C#
        @"
_mock4.DoSomething (null);
Expect.Call (() => _mock6.DoSomething (null)).IgnoreArguments();
Expect.Call (() => _mock6.DoSomething (null)).IgnoreArguments();")]
    [TestCase (
        //language=C#
        @"
var mock7 = _mockRepository.StrictMock<ITestInterface>();
using (null)
{
  mock7.DoSomething (null);
  LastCall.IgnoreArguments();
}",
        //language=C#
        @"
var mock7 = _mockRepository.StrictMock<ITestInterface>();
using (null)
{
  Expect.Call (() => mock7.DoSomething (null)).IgnoreArguments();
}")]
    [TestCase (
        //language=C#
        @"
var mock7 = _mockRepository.StrictMock<ITestInterface>();
using (null)
{
  var mock8 = _mockRepository.StrictMock<ITestInterface>();
  using (null)
  {
    mock7.DoSomething (null);
    LastCall.IgnoreArguments();
  }
  
  mock8.DoSomething (null);
  LastCall.IgnoreArguments();
}",
        //language=C#
        @"
var mock7 = _mockRepository.StrictMock<ITestInterface>();
using (null)
{
  var mock8 = _mockRepository.StrictMock<ITestInterface>();
  using (null)
  {
    Expect.Call (() => mock7.DoSomething (null)).IgnoreArguments();
  }
  
  Expect.Call (() => mock8.DoSomething (null)).IgnoreArguments();
}")]
    [TestCase (
        //language=C#
        @"
_mock1.DoSomething (null);
LastCall.Do (null);",
        //language=C#
        @"
Expect.Call (() => _mock1.DoSomething (null)).Do (null);")]
    public void Rewrite_LastCall (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }
  }
}