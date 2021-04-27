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
using NUnit.Framework;
using RhinoMocksToMoqRewriter.Core.Rewriters;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters
{
  [TestFixture]
  public class ObjectRewriterTests
  {
    private ObjectRewriter _rewriter;

    private readonly Context _context =
        new Context
        {
            //language=C#
            InterfaceContext =
                @"
void DoSomething();
int[] DoSomething (int a);
ITestInterface DoSomething (ITestInterface m);
ITestInterface DoSomething (int i, ITestInterface m);",
            //language=C#
            ClassContext =
                @"
private Mock<ITestInterface> _mock;
private ITestInterface _noMock;",
            //language=C#
            MethodContext =
                @"
_mock = new Mock<ITestInterface>();
var mock = new Mock<ITestInterface>();"
        };

    [SetUp]
    public void SetUp ()
    {
      _rewriter = new ObjectRewriter();
    }

    [Test]
    [TestCase (
        //language=C#
        @"_mock.DoSomething();",
        //language=C#
        @"_mock.Object.DoSomething();")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (1).First();",
        //language=C#
        @"_mock.Object.DoSomething (1).First();")]
    [TestCase (
        //language=C#
        @"mock.Verify();",
        //language=C#
        @"mock.Verify();")]
    [TestCase (
        //language=C#
        @"Console.WriteLine (1);",
        //language=C#
        @"Console.WriteLine (1);")]
    [TestCase (
        //language=C#
        @"mock.Setup (m => m.DoSomething());",
        //language=C#
        @"mock.Setup (m => m.DoSomething());")]
    [TestCase (
        //language=C#
        @"_mock.Setup (m => m.DoSomething (1)).Returns (new[] {1}).Callback (null).Verifiable();",
        //language=C#
        @"_mock.Setup (m => m.DoSomething (1)).Returns (new[] {1}).Callback (null).Verifiable();")]
    public void Rewrite_MemberAccessExpression (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"_noMock.DoSomething (_mock);",
        //language=C#
        @"_noMock.DoSomething (_mock.Object);")]
    [TestCase (
        //language=C#
        @"_noMock.DoSomething (1);",
        //language=C#
        @"_noMock.DoSomething (1);")]
    [TestCase (
        //language=C#
        @"_noMock.DoSomething (1, _mock);",
        //language=C#
        @"_noMock.DoSomething (1, _mock.Object);")]
    [TestCase (
        //language=C#
        @"_noMock.DoSomething (1, _mock.DoSomething (_mock));",
        //language=C#
        @"_noMock.DoSomething (1, _mock.Object.DoSomething (_mock.Object));")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (_mock.DoSomething (_mock));",
        //language=C#
        @"mock.Object.DoSomething (_mock.Object.DoSomething (_mock.Object));")]
    public void Rewrite_SingleArgument (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"return _noMock;",
        //language=C#
        @"return _noMock;")]
    [TestCase (
        //language=C#
        @"return mock;",
        //language=C#
        @"return mock.Object;")]
    [TestCase (
        //language=C#
        @"return _noMock.DoSomething (_mock.DoSomething (_mock));",
        //language=C#
        @"return _noMock.DoSomething (_mock.Object.DoSomething (_mock.Object));")]
    [TestCase (
        //language=C#
        @"return;",
        //language=C#
        @"return;")]
    public void Rewrite_ReturnStatement (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileReturnStatementWithContext (source, _context, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileReturnStatementWithContext (expected, _context, true);
      _rewriter.Model = model;
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }
  }
}