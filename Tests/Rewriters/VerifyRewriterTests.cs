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
  public class VerifyRewriterTests
  {
    private VerifyRewriter _rewriter;

    private readonly Context _context =
        new Context
        {
            //language=C#
            InterfaceContext = @"void DoSomething();",
            //language=C#
            ClassContext =
                @"
private ITestInterface _stub1; 
private ITestInterface _mock1; 
private ITestInterface _mock2;
private ITestInterface _mock3;
private ITestInterface _mock4;
private ITestInterface _mock5;
private ITestInterface _mock6;
private ITestInterface _mock7;
private ITestInterface _mock8;
private MockRepository _mockRepository1;
private MockRepository _mockRepository2;
private MockRepository _mockRepository3;
private MockRepository _mockRepository4;",
            //language=C#
            MethodContext =
                @"
_mockRepository1 = new MockRepository();
_mockRepository2 = new MockRepository();
_mockRepository3 = new MockRepository();
_mockRepository4 = new MockRepository();
var mock = MockRepository.GenerateMock<ITestInterface>();
_mock1 = _mockRepository1.StrictMock<ITestInterface>();
_mock2 = _mockRepository1.PartialMock<ITestInterface>();
_mock3 = _mockRepository1.PartialMultiMock<ITestInterface>();
_mock4 = _mockRepository1.DynamicMock<ITestInterface>();
_stub1 = _mockRepository3.Stub<ITestInterface>();
_mock5 = _mockRepository2.DynamicMultiMock<ITestInterface>();
_mock6 = _mockRepository2.StrictMock<ITestInterface>();
_mock7 = _mockRepository3.StrictMock<ITestInterface>();
_mock8 = _mockRepository3.StrictMock<ITestInterface>();
var stub2 = _mockRepository3.Stub<ITestInterface>();
var localMock1 = _mockRepository4.StrictMock<ITestInterface>();
var localMock2 = _mockRepository4.StrictMock<ITestInterface>();
ITestInterface noMock;"
        };

    [SetUp]
    public void SetUp ()
    {
      _rewriter = new VerifyRewriter();
    }

    [Test]
    [TestCase (
        //language=C#
        @"
_mockRepository1.VerifyAll();",
        //language=C#
        @"
_mock1.Verify();
_mock2.Verify();
_mock3.Verify();
_mock4.Verify();")]
    [TestCase (
        //language=C#
        @"mock.VerifyAllExpectations();",
        //language=C#
        @"mock.Verify();")]
    [TestCase (
        //language=C#
        @"mock.DoSomething();",
        //language=C#
        @"mock.DoSomething();")]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.VerifyAllExpectations(mock);",
        //language=C#
        @"mock.Verify();")]
    [TestCase (
        //language=C#
        @"
_mockRepository1.VerifyAll();
mock.VerifyAllExpectations();",
        //language=C#
        @"
_mock1.Verify();
_mock2.Verify();
_mock3.Verify();
_mock4.Verify();
mock.Verify();")]
    [TestCase (
        //language=C#
        @"
_mockRepository2.VerifyAll();",
        //language=C#
        @"
_mock5.Verify();
_mock6.Verify();")]
    [TestCase (
        //language=C#
        @"
_mockRepository1.VerifyAll();
_mockRepository2.VerifyAll();",
        //language=C#
        @"
_mock1.Verify();
_mock2.Verify();
_mock3.Verify();
_mock4.Verify();
_mock5.Verify();
_mock6.Verify();")]
    [TestCase (
        //language=C#
        @"_mock1.AssertWasNotCalled (_ => _.DoSomething());",
        //language=C#
        @"_mock1.Verify (_ => _.DoSomething(), Times.Never());")]
    [TestCase (
        //language=C#
        @"_mock1.AssertWasNotCalled (_ => _.DoSomething(), o => o.IgnoreArguments());",
        //language=C#
        @"_mock1.Verify (_ => _.DoSomething(), Times.Never(), o => o.IgnoreArguments());")]
    [TestCase (
        //language=C#
        @"_mock1.AssertWasCalled (_ => _.DoSomething(), o => o.Repeat.Once());",
        //language=C#
        @"_mock1.Verify (_ => _.DoSomething(), Times.AtLeastOnce(), o => o.Repeat.Once());")]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.AssertWasNotCalled (_mock1, _ => _.DoSomething());",
        //language=C#
        @"_mock1.Verify (_ => _.DoSomething(), Times.Never());")]
    [TestCase (
        //language=C#
        @"mock.AssertWasCalled (m => m.DoSomething());",
        //language=C#
        @"mock.Verify (m => m.DoSomething(), Times.AtLeastOnce());")]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.AssertWasCalled (mock, m => m.DoSomething());",
        //language=C#
        @"mock.Verify (m => m.DoSomething(), Times.AtLeastOnce());")]
    [TestCase (
        //language=C#
        @"
using (null)
{
  mock.VerifyAllExpectations();
}",
        //language=C#
        @"
using (null)
{
  mock.Verify();
}")]
    [TestCase (
        //language=C#
        @"
using (null)
{
  mock.VerifyAllExpectations();
}

mock.VerifyAllExpectations();",
        //language=C#
        @"
using (null)
{
  mock.Verify();
}

mock.Verify();")]
    [TestCase (
        //language=C#
        @"
_mockRepository4.VerifyAll();",
        //language=C#
        @"
localMock1.Verify();
localMock2.Verify();")]
    public void Rewrite_MethodDeclaration (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (expected, _context, true);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [TestCase (
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.VerifyAllExpectations();",
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.Verify (_ => _.DoSomething(), Times.Never());",
        //language=C#
        @"
0")]
    [TestCase (
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.VerifyAllExpectations();",
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.Verify (_ => _.DoSomething(), Times.Once());",
        //language=C#
        @"
1")]
    [TestCase (
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.VerifyAllExpectations();",
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.Verify (_ => _.DoSomething(), Times.Exactly (2));",
        //language=C#
        @"
2")]
    [TestCase (
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.VerifyAllExpectations();",
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.Verify (_ => _.DoSomething(), Times.Exactly (3));",
        //language=C#
        @"
3")]
    [TestCase (
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.VerifyAllExpectations();",
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.Verify (_ => _.DoSomething(), Times.AtLeastOnce());",
        //language=C#
        @"
-1")]
    [TestCase (
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.VerifyAllExpectations();",
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.Verify (_ => _.DoSomething(), Times.Between (2, 8));",
        //language=C#
        @"
2:8")]
    [TestCase (
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.VerifyAllExpectations();",
        //language=C#
        @"
mock.Setup (_ => _.DoSomething());
mock.Verify (_ => _.DoSomething(), Times.Exactly (""Unable to convert times expression""));",
        //language=C#
        @"
-3")]
    [TestCase (
        //language=C#
        @"
_mock7.Setup (_ => _.DoSomething());
_mock8.Setup (_ => _.DoSomething());
_mockRepository3.VerifyAll();
Console.WriteLine (1);",
        //language=C#
        @"
_mock7.Setup (_ => _.DoSomething());
_mock8.Setup (_ => _.DoSomething());
_mock7.Verify (_ => _.DoSomething(), Times.Exactly (2));
_mock8.Verify (_ => _.DoSomething(), Times.Once());
Console.WriteLine (1);",
        //language=C#
        @"
2
1")]
    public void Rewrite_VerifyWithAnnotation (string source, string expected, string annotationData)
    {
      var (model, node) = CompiledSourceFileProvider.CompileMethodDeclarationWithContextAndAdditionalAnnotations (source, _context, annotationData, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (expected, _context, true);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.IsNotEmpty (actualNode.GetAnnotatedNodes (MoqSyntaxFactory.VerifyAnnotationKind));
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }
  }
}