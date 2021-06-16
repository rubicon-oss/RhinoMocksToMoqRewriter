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
using RhinoMocksToMoqRewriter.Core;
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
            NamespaceContext =
                @"
public class A
{
  public A() {}
  public A (ITestInterface a) {}
  public A (ITestInterface[] a) {}
  public void RegisterSingle<TService> (Func<TService> instanceFactory) {}
}",
            //language=C#
            ClassContext =
                @"
private Mock<ITestInterface> _mock;
private ITestInterface _noMock;",
            //language=C#
            MethodContext =
                @"
_mock = new Mock<ITestInterface>();
var mock = new Mock<ITestInterface>();
var sequence = new MockSequence();
var aClass = new A();"
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
    [TestCase (
        //language=C#
        @"_mock.InSequence (sequence).Setup (m => m.DoSomething (1)).Returns (new[] {1});",
        //language=C#
        @"_mock.InSequence (sequence).Setup (m => m.DoSomething (1)).Returns (new[] {1});")]
    [TestCase (
        //language=C#
        @"_mock.Protected().Setup (""OnInit"", true, EventArgs.Empty).Verifiable();",
        //language=C#
        @"_mock.Protected().Setup (""OnInit"", true, EventArgs.Empty).Verifiable();")]
    public void Rewrite_MemberAccessExpression (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      _rewriter.MoqSymbols = new MoqSymbols (model.Compilation);
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
    [TestCase (
        //language=C#
        @"mock.DoSomething (new Mock<ITestInterface>());",
        //language=C#
        @"mock.Object.DoSomething (new Mock<ITestInterface>().Object);")]
    [TestCase (
        //language=C#
        @"aClass.RegisterSingle (() => _mock);",
        //language=C#
        @"aClass.RegisterSingle (() => _mock.Object);")]
    [TestCase (
        //language=C#
        @"aClass.RegisterSingle (() => _noMock);",
        //language=C#
        @"aClass.RegisterSingle (() => _noMock);")]
    public void Rewrite_SingleArgument (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      _rewriter.MoqSymbols = new MoqSymbols (model.Compilation);
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
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      _rewriter.MoqSymbols = new MoqSymbols (model.Compilation);
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"var array = new[] { _mock };",
        //language=C#
        @"var array = new[] { _mock.Object };")]
    [TestCase (
        //language=C#
        @"var array = new[] { _noMock };",
        //language=C#
        @"var array = new[] { _noMock };")]
    [TestCase (
        //language=C#
        @"var array = new[] { _mock, _noMock };",
        //language=C#
        @"var array = new[] { _mock.Object, _noMock };")]
    [TestCase (
        //language=C#
        @"var a = new A (_mock);",
        //language=C#
        @"var a = new A (_mock.Object);")]
    [TestCase (
        //language=C#
        @"var a = new A (_noMock);",
        //language=C#
        @"var a = new A (_noMock);")]
    [TestCase (
        //language=C#
        @"var list = new List<ITestInterface>() { _mock, _noMock };",
        //language=C#
        @"var list = new List<ITestInterface>() { _mock.Object, _noMock };")]
    [TestCase (
        //language=C#
        @"var array = new[] { _mock.DoSomething (mock) };",
        //language=C#
        @"var array = new[] { _mock.Object.DoSomething (mock.Object) };")]
    [TestCase (
        //language=C#
        @"var a = new A (new[] { mock, mock, mock });",
        //language=C#
        @"var a = new A (new[] { mock.Object, mock.Object, mock.Object });")]
    public void Rewrite_Initializer (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileLocalDeclarationStatementWithContext (source, _context, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileLocalDeclarationStatementWithContext (expected, _context);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      _rewriter.MoqSymbols = new MoqSymbols (model.Compilation);
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"var parentMock = new Mock<ITestInterface>();",
        //language=C#
        @"var parentMock = new Mock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"ITestInterface parentMock = new Mock<ITestInterface>();",
        //language=C#
        @"ITestInterface parentMock = new Mock<ITestInterface>().Object;")]
    [TestCase (
        //language=C#
        @"var maybeMock = _mock;",
        //language=C#
        @"var maybeMock = _mock;")]
    [TestCase (
        //language=C#
        @"Mock<ITestInterface> aMock = _mock;",
        //language=C#
        @"Mock<ITestInterface> aMock = _mock;")]
    [TestCase (
        //language=C#
        @"ITestInterface noMock = _mock;",
        //language=C#
        @"ITestInterface noMock = _mock.Object;")]
    [TestCase (
        //language=C#
        @"ITestInterface noMock = _noMock;",
        //language=C#
        @"ITestInterface noMock = _noMock;")]
    public void Rewrite_LocalDeclarationStatement (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileStatementWithContext (source, _context, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileStatementWithContext (expected, _context);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      _rewriter.MoqSymbols = new MoqSymbols (model.Compilation);
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"public Mock<ITestInterface> parentMock = new Mock<ITestInterface>();",
        //language=C#
        @"public Mock<ITestInterface> parentMock = new Mock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"public ITestInterface parentMock = new Mock<ITestInterface>();",
        //language=C#
        @"public ITestInterface parentMock = new Mock<ITestInterface>().Object;")]
    public void Rewrite_FieldDeclaration (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileFieldDeclarationWithContext (source, _context, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileFieldDeclarationWithContext (expected, _context);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      _rewriter.MoqSymbols = new MoqSymbols (model.Compilation);
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"_noMock = new Mock<ITestInterface>();",
        //language=C#
        @"_noMock = new Mock<ITestInterface>().Object;")]
    [TestCase (
        //language=C#
        @"_mock = new Mock<ITestInterface>();",
        //language=C#
        @"_mock = new Mock<ITestInterface>();")]
    public void Rewrite_AssignmentExpression (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileStatementWithContext (source, _context, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileStatementWithContext (expected, _context);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      _rewriter.MoqSymbols = new MoqSymbols (model.Compilation);
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }
  }
}