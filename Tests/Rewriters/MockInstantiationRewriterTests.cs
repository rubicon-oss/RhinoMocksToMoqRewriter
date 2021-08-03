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
using Moq;
using NUnit.Framework;
using RhinoMocksToMoqRewriter.Core;
using RhinoMocksToMoqRewriter.Core.Rewriters;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters
{
  [TestFixture]
  public class MockInstantiationRewriterTests
  {
    private MockInstantiationRewriter _rewriter;
    private Mock<IFormatter> _formatter;

    private readonly Context _context =
        new Context
        {
            //language=C#
            ClassContext =
                @"
private ITestInterface _mock;
private MockRepository _mockRepository = new MockRepository();
public delegate void AnyDelegate();
private static void DoSomething (Func<int> func) => throw new NotImplementedException();
private static void DoSomething (AnyDelegate d) => throw new NotImplementedException();",
        };

    [SetUp]
    public void SetUp ()
    {
      _formatter = new Mock<IFormatter>();
      _formatter.Setup (f => f.Format (It.IsAny<SyntaxNode>())).Returns<SyntaxNode> (s => s);
      _rewriter = new MockInstantiationRewriter (_formatter.Object);
    }

    [Test]
    [TestCase (
        //language=C#
        @"_mock = MockRepository.GenerateMock<ITestInterface>();",
        //language=C#
        @"_mock = new Mock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"_mock = MockRepository.GenerateMock<ITestInterface, IDisposable>();",
        //language=C#
        @"_mock = new Mock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"_mock = MockRepository.GenerateMock<ITestInterface, IDisposable, IConvertible>();",
        //language=C#
        @"_mock = new Mock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"_mock = MockRepository.GenerateStrictMock<ITestInterface>();",
        //language=C#
        @"_mock = new Mock<ITestInterface> (MockBehavior.Strict);")]
    [TestCase (
        //language=C#
        @"_mock = MockRepository.GenerateStrictMock<ITestInterface> (42);",
        //language=C#
        @"_mock = new Mock<ITestInterface> (MockBehavior.Strict, 42);")]
    [TestCase (
        //language=C#
        @"_mock = MockRepository.GenerateStrictMock<ITestInterface, IDisposable>();",
        //language=C#
        @"_mock = new Mock<ITestInterface> (MockBehavior.Strict);")]
    [TestCase (
        //language=C#
        @"_mock = MockRepository.GenerateStub<ITestInterface>();",
        //language=C#
        @"_mock = new Mock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"Console.WriteLine(1);",
        //language=C#
        @"Console.WriteLine(1);")]
    [TestCase (
        //language=C#
        @"_mock = MockRepository.GenerateMock<ITestInterface>();",
        //language=C#
        @"_mock = new Mock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"_mock = MockRepository.GenerateMock<ITestInterface> (42, 32);",
        //language=C#
        @"_mock = new Mock<ITestInterface> (42, 32);")]
    [TestCase (
        //language=C#
        @"
_mock = MockRepository.GenerateMock<ITestInterface> (
    42,
    32,
    43);",
        //language=C#
        @"
_mock = new Mock<ITestInterface> (
    42,
    32,
    43);")]
    [TestCase (
        //language=C#
        @"
_mock = MockRepository.GenerateStrictMock<ITestInterface> (
    42,
    32,
    43);",
        //language=C#
        @"
_mock = new Mock<ITestInterface> (
    MockBehavior.Strict,
    42,
    32,
    43);")]
    [TestCase (
        //language=C#
        @"
_mock = MockRepository.GenerateStrictMock<ITestInterface> (
    42,
    32,
    43);",
        //language=C#
        @"
_mock = new Mock<ITestInterface> (
    MockBehavior.Strict,
    42,
    32,
    43);")]
    [TestCase (
        //language=C#
        @"_mock = MockRepository.GenerateMock<ITestInterface> (MockRepository.GenerateStrictMock<ITestInterface>());",
        //language=C#
        @"_mock = new Mock<ITestInterface> (new Mock<ITestInterface> (MockBehavior.Strict));")]
    [TestCase (
        //language=C#
        @"_mock = MockRepository.GeneratePartialMock<ITestInterface>();",
        //language=C#
        @"_mock = new Mock<ITestInterface>(){ CallBase = true };")]
    [TestCase (
        //language=C#
        @"_mock = MockRepository.GeneratePartialMock<ITestInterface> (1, 2);",
        //language=C#
        @"_mock = new Mock<ITestInterface>(1, 2){ CallBase = true };")]
    [TestCase (
        //language=C#
        @"_mock = _mockRepository.DynamicMock<ITestInterface>();",
        //language=C#
        @"_mock = new Mock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"_mock = _mockRepository.DynamicMultiMock<ITestInterface>();",
        //language=C#
        @"_mock = new Mock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"_mock = _mockRepository.PartialMock<ITestInterface>();",
        //language=C#
        @"_mock = new Mock<ITestInterface>() { CallBase = true };")]
    [TestCase (
        //language=C#
        @"_mock = _mockRepository.PartialMultiMock<ITestInterface>();",
        //language=C#
        @"_mock = new Mock<ITestInterface>() { CallBase = true };")]
    [TestCase (
        //language=C#
        @"_mock = _mockRepository.StrictMock<ITestInterface>();",
        //language=C#
        @"_mock = new Mock<ITestInterface> (MockBehavior.Strict);")]
    [TestCase (
        //language=C#
        @"_mock = _mockRepository.StrictMultiMock<ITestInterface>();",
        //language=C#
        @"_mock = new Mock<ITestInterface> (MockBehavior.Strict);")]
    [TestCase (
        //language=C#
        @"_mockRepository.VerifyAll();",
        //language=C#
        @"_mockRepository.VerifyAll();")]
    [TestCase (
        //language=C#
        @"_mock = (ITestInterface) _mockRepository.StrictMock (typeof (ITestInterface));",
        //language=C#
        @"_mock = (ITestInterface) new Mock<ITestInterface> (MockBehavior.Strict);")]
    [TestCase (
        //language=C#
        @"_mock = (ITestInterface) _mockRepository.StrictMock (typeof (ITestInterface), 1, 2);",
        //language=C#
        @"_mock = (ITestInterface) new Mock<ITestInterface> (MockBehavior.Strict, 1, 2);")]
    public void Rewrite_ExpressionStatement (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context, true);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"var anotherMock = MockRepository.GenerateStub<ITestInterface>();",
        //language=C#
        @"var anotherMock = new Mock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"
var anotherMock = MockRepository.GenerateStrictMock<ITestInterface> (
    42,
    32,
    43);",
        //language=C#
        @"
var anotherMock = new Mock<ITestInterface> (
    MockBehavior.Strict,
    42,
    32,
    43);")]
    [TestCase (
        //language=C#
        @"var anotherMock = MockRepository.GeneratePartialMock<ITestInterface>();",
        //language=C#
        @"var anotherMock = new Mock<ITestInterface>(){ CallBase = true };")]
    [TestCase (
        //language=C#
        @"var anotherMock = MockRepository.GeneratePartialMock<ITestInterface> (1, 2);",
        //language=C#
        @"var anotherMock = new Mock<ITestInterface>(1, 2) { CallBase = true };")]
    [TestCase (
        //language=C#
        @"
var anotherMock = MockRepository.GeneratePartialMock<ITestInterface>(
    1,
    2);",
        //language=C#
        @"
var anotherMock = new Mock<ITestInterface>(
    1,
    2) { CallBase = true };")]
    [TestCase (
        //language=C#
        @"ITestInterface anotherMock = MockRepository.GenerateStub<ITestInterface>();",
        //language=C#
        @"var anotherMock = new Mock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"ITestInterface anotherMock = _mockRepository.StrictMock<ITestInterface>();",
        //language=C#
        @"var anotherMock = new Mock<ITestInterface> (MockBehavior.Strict);")]
    [TestCase (
        //language=C#
        @"string anotherMock = ""abc"";",
        //language=C#
        @"string anotherMock = ""abc"";")]
    public void Rewrite_LocalDeclarationStatement (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileLocalDeclarationStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileLocalDeclarationStatementWithContext (expected, _context, true);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"private ITestInterface _anotherMock = MockRepository.GenerateMock<ITestInterface>();",
        //language=C#
        @"private ITestInterface _anotherMock = new Mock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"
public ITestInterface Mock = MockRepository.GenerateMock<ITestInterface> (
    42,
    32,
    43);",
        //language=C#
        @"
public ITestInterface Mock = new Mock<ITestInterface> (
    42,
    32,
    43);")]
    [TestCase (
        //language=C#
        @"private ITestInterface _anotherMock = MockRepository.GeneratePartialMock<ITestInterface>();",
        //language=C#
        @"private ITestInterface _anotherMock = new Mock<ITestInterface>(){ CallBase = true };")]
    public void Rewrite_FieldDeclarationStatement (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileFieldDeclarationWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileFieldDeclarationWithContext (expected, _context, true);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"
DoSomething (delegate
  {
    var mock = MockRepository.GenerateMock<ITestInterface>();
    return 1;
  });",
        //language=C#
        @"
DoSomething (delegate
  {
    var mock = new Mock<ITestInterface>();
    return 1;
  });")]
    [TestCase (
        //language=C#
        @"
DoSomething (delegate
  {
    var mock = MockRepository.GenerateStub<ITestInterface>();
  });",
        //language=C#
        @"
DoSomething (delegate
  {
    var mock = new Mock<ITestInterface>();
  });")]
    public void Rewrite_NestedStatements (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context, true);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }
  }
}