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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using RhinoMocksToMoqRewriter.Core.Rewriters;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters
{
  [TestFixture]
  public class ExpectCallRewriterTests
  {
    private ExpectCallRewriter _rewriter;

    private readonly Context _context =
        new Context()
        {
            //language=C#
            NamespaceContext =
                @"
public interface IA
{
  int DoSomething();
}

public class A : IA
{
  public int DoSomething()
  {
    throw new System.NotImplementedException();
  }
}",
            //language=C#
            InterfaceContext =
                @"
IA A { get; set; }
string FullName { get; set; }
bool IsInvalid { get; set; }
int DoSomething();
int DoSomething (int a, int b, int c);
ITestInterface DoSomething (int a);
bool DoSomething (IA a);
int DoSomething<T>(Func<T> func);",
            //language=C#
            ClassContext =
                @"
private IA _a = new A();
private ITestInterface _mock = MockRepository.GenerateMock<ITestInterface>();"
        };

    [SetUp]
    public void SetUp ()
    {
      _rewriter = new ExpectCallRewriter();
      _rewriter.Generator = SyntaxGenerator.GetGenerator (new AdhocWorkspace(), "C#");
    }

    [Test]
    [TestCase (
        //language=C#
        @"Expect.Call (_mock.DoSomething()).Return (1);",
        //language=C#
        @"_mock.Expect (_ => _.DoSomething()).Return (1);")]
    [TestCase (
        //language=C#
        @"Expect.Call (_mock.DoSomething());",
        //language=C#
        @"_mock.Expect (_ => _.DoSomething());")]
    [TestCase (
        //language=C#
        @"Expect.Call (_mock.DoSomething (1, 2, 3)).Callback (null).Return (1);",
        //language=C#
        @"_mock.Expect (_ => _.DoSomething (1, 2, 3)).Callback (null).Return (1);")]
    [TestCase (
        //language=C#
        @"Expect.Call (_mock.DoSomething (1).DoSomething()).Callback (null).Return (1);",
        //language=C#
        @"_mock.Expect (_ => _.DoSomething (1).DoSomething()).Callback (null).Return (1);")]
    [TestCase (
        //language=C#
        @"Expect.Call (_mock.A.DoSomething()).Return (1);",
        //language=C#
        @"_mock.Expect (_ => _.A.DoSomething()).Return (1);")]
    [TestCase (
        //language=C#
        @"
Expect.Call(_mock.DoSomething (_a))
  .Constraints(Rhino.Mocks.Constraints.Property.Value(""FullName"", ""abc""))
  .Return(true);",
        //language=C#
        @"
_mock.Expect(
    _ => _.DoSomething (
      Arg<IA>.Matches (Rhino.Mocks.Constraints.Property.Value(""FullName"", ""abc""))))
  .Return(true);")]
    [TestCase (
        //language=C#
        @"Expect.Call (_mock.IsInvalid).Return (false);",
        //language=C#
        @" _mock.Expect(_ => _.IsInvalid).Return (false);")]
    [TestCase (
        //language=C#
        @"Expect.Call (_mock.DoSomething (Arg.Is (1), Arg.Is (4), Arg.Is (9))).Return (4);",
        //language=C#
        @"_mock.Expect (_ => _.DoSomething (Arg.Is (1), Arg.Is (4), Arg.Is (9))).Return (4);")]
    [TestCase (
        //language=C#
        @"
Expect.Call(_mock.DoSomething(1, 2, 3))
  .Constraints(
    Rhino.Mocks.Constraints.Is.Equal (1), 
    Rhino.Mocks.Constraints.Is.Equal (2), 
    Rhino.Mocks.Constraints.Is.Equal (3))
  .Return (5);",
        //language=C#
        @"
_mock.Expect(
    _ => _.DoSomething (
      Arg<int>.Matches(Rhino.Mocks.Constraints.Is.Equal (1)),
      Arg<int>.Matches(Rhino.Mocks.Constraints.Is.Equal (2)),
      Arg<int>.Matches(Rhino.Mocks.Constraints.Is.Equal (3))))
  .Return (5);")]
    [TestCase (
        //language=C#
        @"
Expect.Call(_mock.DoSomething(1, 2, 3))
  .Constraints(
    Rhino.Mocks.Constraints.Is.GreaterThan (0) & Rhino.Mocks.Constraints.Is.LessThan (2), 
    Rhino.Mocks.Constraints.Is.GreaterThanOrEqual (2) & Rhino.Mocks.Constraints.Is.NotEqual (3), 
    Rhino.Mocks.Constraints.Is.Equal (3))
  .Return (5);",
        //language=C#
        @"
_mock.Expect(
    _ => _.DoSomething (
      Arg<int>.Matches (Rhino.Mocks.Constraints.Is.GreaterThan (0) & Rhino.Mocks.Constraints.Is.LessThan (2)),
      Arg<int>.Matches (Rhino.Mocks.Constraints.Is.GreaterThanOrEqual (2) & Rhino.Mocks.Constraints.Is.NotEqual (3)),
      Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Equal (3))))
  .Return (5);")]
    [TestCase (
        //language=C#
        @"Expect.Call (_mock.DoSomething (Arg.Is (1), Arg.Is (4), Arg.Is (9))).Return (4).IgnoreArguments();",
        //language=C#
        @"_mock.Expect (_ => _.DoSomething (Arg.Is (1), Arg.Is (4), Arg.Is (9))).Return (4).IgnoreArguments();")]
    public void Rewrite_ExpectCall (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"
_mock.DoSomething(() =>
{
    Expect.Call(_mock.IsInvalid).Return (true);
    return 1;
});")]
    [TestCase (
        //language=C#
        @"
Expect.Call (_mock.DoSomething(() =>
{
    Expect.Call(_mock.IsInvalid).Return (true);
    return 1;
})).Return (1);")]
    [TestCase (
        //language=C#
        @"
Expect.Call(_mock.DoSomething(() =>
{
    Expect.Call(_mock.IsInvalid)
      .Return (true)
      .IgnoreArguments()
      .WhenCalled (null);
    return 1;
})).Return(3)
.IgnoreArguments()
.Callback (null);")]
    public void Throws_ArgumentException_On_Nested_ExpectCall (string source)
    {
      var (model, node) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (source, _context);
      _rewriter.Model = model;
      Assert.Throws<ArgumentException> (() => node.Accept (_rewriter));
    }
  }
}