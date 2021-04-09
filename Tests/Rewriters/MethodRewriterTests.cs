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
  public class MethodRewriterTests
  {
    private MethodRewriter _rewriter;

    private readonly Context _context =
        new Context
        {
            //language=C#
            InterfaceContext = @"void DoSomething();",
            //language=C#
            ClassContext =
                @"
private ITestInterface _mock1;
private ITestInterface _mock2;
private ITestInterface _mock3;
private ITestInterface _mock4;
private ITestInterface _mock5;
private ITestInterface _mock6;
private MockRepository _mockRepository1;
private MockRepository _mockRepository2;",
            //language=C#
            MethodContext =
                @"
_mockRepository1 = new MockRepository();
_mockRepository2 = new MockRepository();
var mock = MockRepository.GenerateMock<ITestInterface>();
_mock1 = _mockRepository1.StrictMock<ITestInterface>();
_mock2 = _mockRepository1.PartialMock<ITestInterface>();
_mock3 = _mockRepository1.PartialMultiMock<ITestInterface>();
_mock4 = _mockRepository1.DynamicMock<ITestInterface>();
_mock5 = _mockRepository2.StrictMock<ITestInterface>();
_mock6 = _mockRepository2.StrictMock<ITestInterface>();"
        };

    [SetUp]
    public void SetUp ()
    {
      _rewriter = new MethodRewriter();
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
    public void Rewrite_MethodDeclaration (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (expected, _context, true);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }
  }
}