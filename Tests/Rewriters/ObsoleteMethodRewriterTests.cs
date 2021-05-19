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
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Moq;
using NUnit.Framework;
using RhinoMocksToMoqRewriter.Core;
using RhinoMocksToMoqRewriter.Core.Rewriters;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters
{
  [TestFixture]
  public class ObsoleteMethodRewriterTests
  {
    private ObsoleteMethodRewriter _rewriter;
    private Mock<IFormatter> _formatter;

    private readonly Context _context =
        new Context
        {
            InterfaceContext =
                //language=C#
                @"
void DoSomething (string a);",
            ClassContext =
                //language=C#
                @"
private static MockRepository _a;
private static MockRepository _b = new MockRepository();
private ITestInterface _fieldMock;
private MockRepository _fieldMockRepository;"
        };

    [SetUp]
    public void SetUp ()
    {
      _formatter = new Mock<IFormatter>();
      _formatter.Setup (f => f.Format (It.IsAny<SyntaxNode>())).Returns<SyntaxNode> (s => s);
      _rewriter = new ObsoleteMethodRewriter (_formatter.Object);
    }

    [TestCase (
        //language=C#
        @"private MockRepository _mockRepository = new MockRepository();",
        null)]
    [TestCase (
        //language=C#
        @"private MockRepository _mockRepository = default;",
        null)]
    [TestCase (
        //language=C#
        @"private MockRepository _mockRepository;",
        null)]
    [TestCase (
        //language=C#
        @"private MockRepository _mockRepository = _a = new MockRepository();",
        null)]
    [TestCase (
        //language=C#
        @"private MockRepository _mockRepository = _b;",
        null)]
    [TestCase (
        //language=C#
        @"private int _temp;",
        //language=C#
        @"private int _temp;")]
    [TestCase (
        //language=C#
        @"private int _temp = 1;",
        //language=C#
        @"private int _temp = 1;")]
    public void Rewrite_FieldDeclaration (string source, [CanBeNull] string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileFieldDeclarationWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileFieldDeclarationWithContext (expected, _context);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      var actualNode = node.Accept (_rewriter);

      if (expected is null)
      {
        Assert.IsNull (actualNode);
      }
      else
      {
        Assert.IsNotNull (actualNode);
        Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
      }
    }

    [TestCase (
        //language=C#
        @"
MockRepository mockRepository = null;",
        null)]
    [TestCase (
        //language=C#
        @"
MockRepository mockRepository = default;",
        null)]
    [TestCase (
        //language=C#
        @"
var mockRepository = new MockRepository();",
        null)]
    [TestCase (
        //language=C#
        @"
var mockRepository = _fieldMockRepository;",
        null)]
    [TestCase (
        //language=C#
        @"
MockRepository a = default;
var mockRepository = a = _fieldMockRepository;",
        null)]
    [TestCase (
        //language=C#
        @"
var mockRepository = new MockRepository();
var mock = MockRepository.GenerateMock<ITestInterface>();
var partialMock = mockRepository.PartialMock<ITestInterface>();",
        //language=C#
        @"
var mock = MockRepository.GenerateMock<ITestInterface>();
var partialMock = mockRepository.PartialMock<ITestInterface>();")]
    [TestCase (
        //language=C#
        @"_fieldMockRepository.ReplayAll();",
        null)]
    [TestCase (
        //language=C#
        @"_fieldMock.Replay();",
        null)]
    [TestCase (
        //language=C#
        @"
_fieldMockRepository.ReplayAll();
_fieldMock.Replay();",
        null)]
    [TestCase (
        //language=C#
        @"
int i = 10;
var mockRepository = new MockRepository();
_fieldMockRepository = new MockRepository();
mockRepository = _fieldMockRepository;
Console.WriteLine(10);
_fieldMock.Replay();
mockRepository.ReplayAll();",
        //language=C#
        @"
int i = 10;
Console.WriteLine(10);")]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.Replay (_fieldMock);",
        null)]
    [TestCase (
        //language=C#
        @"
Rhino.Mocks.RhinoMocksExtensions.Replay (_fieldMock);
_fieldMock.Replay();",
        null)]
    [TestCase (
        //language=C#
        @"_fieldMockRepository.BackToRecordAll();",
        null)]
    [TestCase (
        //language=C#
        @"_fieldMock.BackToRecord();",
        null)]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.BackToRecord(_fieldMock);",
        null)]
    [Test]
    public void Rewrite_MethodDeclaration (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (expected, _context, true);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }
  }
}