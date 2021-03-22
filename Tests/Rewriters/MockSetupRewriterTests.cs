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
using System.Linq;
using NUnit.Framework;
using RhinoMocksToMoqRewriter.Core.Rewriters;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters
{
  [TestFixture]
  public class MockSetupRewriterTests
  {
    private MockSetupRewriter _rewriter;

    private readonly Context _context =
        new Context
        {
            //language=C#
            InterfaceContext =
                @"
void DoSomething();
int DoSomething (int b);
T Do<T> (Func<T> func);",
            //language=C#
            ClassContext =
                @"
private ITestInterface _mock;",
            //language=C#
            MethodContext = @"
var stub = MockRepository.GenerateStub<ITestInterface>();
var mock = MockRepository.GenerateMock<ITestInterface>();"
        };

    [SetUp]
    public void SetUp ()
    {
      _rewriter = new MockSetupRewriter();
    }

    [Test]
    [TestCase (
        //language=C#
        @"stub.Stub (m => m.DoSomething());",
        //language=C#
        @"stub.Setup (m => m.DoSomething());")]
    [TestCase (
        //language=C#
        @"mock.Stub (m => m.DoSomething());",
        //language=C#
        @"mock.Setup (m => m.DoSomething());")]
    [TestCase (
        //language=C#
        @"
mock.Expect (m => m.DoSomething());",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething())
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
mock
  .Expect (m => m.DoSomething (1))
  .Return (2);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething (1))
  .Returns (2)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
stub
  .Expect (m => m.DoSomething (1))
  .Return (2);",
        //language=C#
        @"
stub
  .Setup (m => m.DoSomething (1))
  .Returns (2)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
stub
  .Stub (m => m.DoSomething (1))
  .Return (2);",
        //language=C#
        @"
stub
  .Setup (m => m.DoSomething (1))
  .Returns (2);")]
    [TestCase (
        //language=C#
        @"
mock
  .Expect (m => m.DoSomething (1))
  .Return (2)
  .Callback (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething (1))
  .Returns (2)
  .Callback (null)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
mock
  .Stub (m => m.DoSomething (1))
  .WhenCalled (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething (1))
  .Callback (null);")]
    [TestCase (
        //language=C#
        @"
mock
  .Expect (m => m.DoSomething (1))
  .Return (2)
  .WhenCalled (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething (1))
  .Returns (2)
  .Callback (null)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
mock
  .Expect (m => m.DoSomething())
  .WhenCalled (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething())
  .Callback (null)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
mock
  .Stub (m => m.DoSomething())
  .Callback (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething())
  .Callback (null);")]
    [TestCase (
        //language=C#
        @"
mock
  .Expect (m => m.DoSomething())
  .Callback (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething())
  .Callback (null)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.Stub (mock, m => m.DoSomething());",
        //language=C#
        @"mock.Setup (m => m.DoSomething());")]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.RhinoMocksExtensions.Expect (mock, m => m.DoSomething ());",
        //language=C#
        @"mock.Setup (m => m.DoSomething()).Verifiable();")]
    [TestCase (
        //language=C#
        @"
Rhino.Mocks.RhinoMocksExtensions.Expect (mock, m => m.DoSomething (1))
  .Return (2);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething (1))
  .Returns (2)
  .Verifiable();")]
    [TestCase (
        //language=C#
        @"
Rhino.Mocks.RhinoMocksExtensions.Stub (mock, m => m.DoSomething())
  .WhenCalled (null);",
        //language=C#
        @"
mock
  .Setup (m => m.DoSomething())
  .Callback (null);")]
    public void Rewrite_Setup (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context, true);
      _rewriter.Model = model;
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [TestCase (
        //language=C#
        @"
mock.Do(
  () =>
  {
    mock
      .Expect (m => m.DoSomething (1)).Return (2);
    return mock;
  });",
        //language=C#
        @"
mock.Do(
  () =>
  {
    mock
      .Setup (m => m.DoSomething (1)).Return (2);
    return mock;
  });")]
    public void Rewrite_SetupWithBaseCall (string source, string expected)
    {
      var (model, nodes) = CompiledSourceFileProvider.CompileExpressionStatementsWithContext (source, _context);
      var (_, expectedNodes) = CompiledSourceFileProvider.CompileExpressionStatementsWithContext (expected, _context, true);
      var node = nodes.ToArray()[^2];
      var expectedNode = expectedNodes.ToArray()[^2];

      _rewriter.Model = model;
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }
  }
}