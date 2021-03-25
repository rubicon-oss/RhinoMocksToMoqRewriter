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
using RhinoMocksToMoqRewriter.Core.Rewriters;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters
{
  [TestFixture]
  public class FieldRewriterTests
  {
    private FieldRewriter _rewriter;
    private Mock<IFormatter> _formatter;

    private readonly Context _context =
        new Context
        {
            //language=C#
            InterfaceContext =
                @"
void DoSomething();
int DoSomething(string a);
void DoSomething (int b);
List<ITestInterface> DoSomething (int b, int c);
void DoSomething (int b, ITestInterface c);
ITestInterface DoSomething (string b);
ITestInterface DoSomething (bool b);
ITestInterface DoSomething (ITestInterface b);",
            //language=C#
            ClassContext =
                @"
private ITestInterface _mock;
private MockRepository _mockRepository = new MockRepository();",
            //language=C#
            MethodContext =
                @"
var rhinoMock = MockRepository.GenerateMock<ITestInterface>();
_mock = MockRepository.GenerateMock<ITestInterface>();
_mockA = MockRepository.GenerateMock<ITestInterface>();
_mockB = MockRepository.GenerateMock<ITestInterface>();
_mockC = MockRepository.GenerateMock<ITestInterface>();
_mockD = MockRepository.GenerateMock<ITestInterface>();
_mockE = MockRepository.GenerateMock<ITestInterface>();
_mockI = MockRepository.GenerateMock<ITestInterface>();"
        };

    [SetUp]
    public void SetUp ()
    {
      _formatter = new Mock<IFormatter>();
      _formatter.Setup (f => f.Format (It.IsAny<SyntaxNode>()))
          .Returns<SyntaxNode> (s => s);
      _rewriter = new FieldRewriter (_formatter.Object);
    }

    [Test]
    [TestCase (
        //language=C#
        @"private ITestInterface _mockA;",
        //language=C#
        @"private Mock<ITestInterface> _mockA;")]
    [TestCase (
        //language=C#
        @"private ITestInterface _mockI = null;",
        //language=C#
        @"private Mock<ITestInterface> _mockI = null;")]
    [TestCase (
        //language=C#
        @"private ITestInterface _noMock;",
        //language=C#
        @"private ITestInterface _noMock;")]
    [TestCase (
        //language=C#
        @"private ITestInterface _mockB, _mockC, _mockD;",
        //language=C#
        @"private Mock<ITestInterface> _mockB, _mockC, _mockD;")]
    [TestCase (
        //language=C#
        @"private ITestInterface _mockE = MockRepository.GenerateMock<ITestInterface>();",
        //language=C#
        @"private Mock<ITestInterface> _mockE = MockRepository.GenerateMock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"private ITestInterface _mockX = _mockRepository.StrictMock<ITestInterface>();",
        //language=C#
        @"private Mock<ITestInterface> _mockX = _mockRepository.StrictMock<ITestInterface>();")]
    public void Rewrite_MockFieldDeclaration (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileFieldDeclarationWithContext (source, _context, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileFieldDeclarationWithContext (expected, _context, true);
      _rewriter.Model = model;
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"rhinoMock.DoSomething (_mock);",
        //language=C#
        @"rhinoMock.DoSomething (_mock.Object);")]
    [TestCase (
        //language=C#
        @"rhinoMock.DoSomething (1);",
        //language=C#
        @"rhinoMock.DoSomething (1);")]
    [TestCase (
        //language=C#
        @"rhinoMock.DoSomething (1, _mock);",
        //language=C#
        @"rhinoMock.DoSomething (1, _mock.Object);")]
    [TestCase (
        //language=C#
        @"rhinoMock.DoSomething (1, _mock.DoSomething(""anyString""));",
        //language=C#
        @"rhinoMock.DoSomething (1, _mock.Object.DoSomething(""anyString"");")]
    [TestCase (
        //language=C#
        @"rhinoMock.DoSomething (1, _mock.DoSomething(_mock));",
        //language=C#
        @"rhinoMock.DoSomething (1, _mock.Object.DoSomething(_mock.Object));")]
    public void Rewrite_MockArgument (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileArgumentListWithContext (source, _context, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileArgumentListWithContext (expected, _context, true);
      _rewriter.Model = model;
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"_mock.DoSomething();",
        //language=C#
        @"_mock.Object.DoSomething();")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (1);",
        //language=C#
        @"_mock.Object.DoSomething (1);")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (1, 4).First();",
        //language=C#
        @"_mock.Object.DoSomething (1, 4).First();")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (false).DoSomething();",
        //language=C#
        @"_mock.Object.DoSomething (false).DoSomething();")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (1, 4).First().DoSomething();",
        //language=C#
        @"_mock.Object.DoSomething (1, 4).First().DoSomething();")]
    [TestCase (
        //language=C#
        @"_mock.Verify();",
        //language=C#
        @"_mock.Verify();")]
    [TestCase (
        //language=C#
        @"Console.WriteLine (1);",
        //language=C#
        @"Console.WriteLine (1);")]
    [TestCase (
        //language=C#
        @"_mock.Expect (m => m.DoSomething());",
        //language=C#
        @"_mock.Expect (m => m.DoSomething());")]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.Expect (rhinoMock, m => m.DoSomething());",
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.Expect (rhinoMock, m => m.DoSomething());")]
    [TestCase (
        //language=C#
        @"_mock.Stub (m => m.DoSomething());",
        //language=C#
        @"_mock.Stub (m => m.DoSomething());")]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.Stub (rhinoMock, m => m.DoSomething (""anyString"")).Return (1);",
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.Stub (rhinoMock, m => m.DoSomething (""anyString"")).Return (1);")]
    [TestCase (
        //language=C#
        @"_mock.VerifyAllExpectations();",
        //language=C#
        @"_mock.VerifyAllExpectations();")]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.VerifyAllExpectations (rhinoMock);",
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.VerifyAllExpectations (rhinoMock);")]
    [TestCase (
        //language=C#
        @"rhinoMock.Replay();",
        //language=C#
        @"rhinoMock.Replay();")]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.Replay (rhinoMock);",
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.Replay (rhinoMock);")]
    [TestCase (
        //language=C#
        @"_mock.Verify();",
        //language=C#
        @"_mock.Verify();")]
    public void Rewrite_MockInvocationExpression (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context, true);
      _rewriter.Model = model;
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }
  }
}