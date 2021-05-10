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
            ClassContext =
                @"
private ITestInterface _mock;
private MockRepository _mockRepository = new MockRepository();",
            //language=C#
            MethodContext =
                @"
_mock = MockRepository.GenerateMock<ITestInterface>();
_mockA = MockRepository.GenerateMock<ITestInterface>();
_mockB = MockRepository.GenerateMock<ITestInterface>();
_mockC = MockRepository.GenerateMock<ITestInterface>();
_mockD = MockRepository.GenerateMock<ITestInterface>();"
        };

    [SetUp]
    public void SetUp ()
    {
      _formatter = new Mock<IFormatter>();
      _formatter.Setup (f => f.Format (It.IsAny<SyntaxNode>())).Returns<SyntaxNode> (s => s);
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
        @"private ITestInterface _mockA = null;",
        //language=C#
        @"private Mock<ITestInterface> _mockA = null;")]
    [TestCase (
        //language=C#
        @"private ITestInterface _noMock;",
        //language=C#
        @"private ITestInterface _noMock;")]
    [TestCase (
        //language=C#
        @"private ITestInterface _mockA, _mockB, _mockC;",
        //language=C#
        @"private Mock<ITestInterface> _mockA, _mockB, _mockC;")]
    [TestCase (
        //language=C#
        @"private ITestInterface _mockD = MockRepository.GenerateMock<ITestInterface>();",
        //language=C#
        @"private Mock<ITestInterface> _mockD = MockRepository.GenerateMock<ITestInterface>();")]
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
  }
}