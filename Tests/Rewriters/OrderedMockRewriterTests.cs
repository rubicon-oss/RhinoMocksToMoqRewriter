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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using RhinoMocksToMoqRewriter.Core;
using RhinoMocksToMoqRewriter.Core.Rewriters;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters
{
  [TestFixture]
  public class OrderedMockRewriterTests
  {
    private OrderedMockRewriter _rewriter;

    private readonly Context _context =
        new Context
        {
            //language=C#
            InterfaceContext =
                @"
void Begin();
void Perform();
int Perform (int i);
void End();",
            //language=C#
            ClassContext =
                @"
private static MockRepository _mockRepository = new MockRepository();
private Mock<ITestInterface> _mock1 = new Mock<ITestInterface>();
private Mock<ITestInterface> _mock2 = new Mock<ITestInterface>();"
        };

    [SetUp]
    public void SetUp ()
    {
      _rewriter = new OrderedMockRewriter();
    }

    [Test]
    [TestCase (
        //language=C#
        @"
using (_mockRepository.Ordered())
{
  _mock1.Setup (mock => mock.Begin());
  _mock2.Setup (mock => mock.Begin());
  _mock1.Setup (mock => mock.Perform());
  _mock2.Setup (mock => mock.Perform());
  _mock1.Setup (mock => mock.End());
  _mock2.Setup (mock => mock.End());
}",
        //language=C#
        @"
var sequence = new MockSequence();
_mock1.InSequence (sequence).Setup (mock => mock.Begin());
_mock2.InSequence (sequence).Setup (mock => mock.Begin());
_mock1.InSequence (sequence).Setup (mock => mock.Perform());
_mock2.InSequence (sequence).Setup (mock => mock.Perform());
_mock1.InSequence (sequence).Setup (mock => mock.End());
_mock2.InSequence (sequence).Setup (mock => mock.End());")]
    [TestCase (
        //language=C#
        @"
using (_mockRepository.Ordered())
{
  _mock1.Setup (mock => mock.Begin()).Callback (null);
  _mock1.Setup (mock => mock.Perform (1)).Returns (2);
  _mock1.Setup (mock => mock.End());
}",
        //language=C#
        @"
var sequence = new MockSequence();
_mock1.InSequence (sequence).Setup (mock => mock.Begin()).Callback (null);
_mock1.InSequence (sequence).Setup (mock => mock.Perform (1)).Returns (2);
_mock1.InSequence (sequence).Setup (mock => mock.End());")]
    [TestCase (
        //language=C#
        @"
using (_mock1.GetMockRepository().Ordered())
{
  _mock1.Setup (mock => mock.Begin()).Callback (null);
  _mock1.Setup (mock => mock.Perform());
  _mock1.Setup (mock => mock.End());
  Console.WriteLine (1);
}",
        //language=C#
        @"
var sequence = new MockSequence();
_mock1.InSequence (sequence).Setup (mock => mock.Begin()).Callback (null);
_mock1.InSequence (sequence).Setup (mock => mock.Perform());
_mock1.InSequence (sequence).Setup (mock => mock.End());
Console.WriteLine (1);")]
    [TestCase (
        //language=C#
        @"
using (null)
{
  _mock1.Setup (mock => mock.Begin());
  _mock1.Setup (mock => mock.Perform());
  _mock1.Setup (mock => mock.End());
}",
        //language=C#
        @"
using (null)
{
  _mock1.Setup (mock => mock.Begin());
  _mock1.Setup (mock => mock.Perform());
  _mock1.Setup (mock => mock.End());
}")]
    [TestCase (
        //language=C#
        @"
using (null)
{
  using (_mock1.GetMockRepository().Ordered())
  {
    _mock1.Setup (mock => mock.Begin()).Callback (null);
    _mock1.Setup (mock => mock.Perform());
    _mock1.Setup (mock => mock.End());
  }
  
  Console.WriteLine (1);
}",
        //language=C#
        @"
using (null)
{
  var sequence = new MockSequence();
  _mock1.InSequence (sequence).Setup (mock => mock.Begin()).Callback (null);
  _mock1.InSequence (sequence).Setup (mock => mock.Perform());
  _mock1.InSequence (sequence).Setup (mock => mock.End());
  
  Console.WriteLine (1);
}")]
    [TestCase (
        //language=C#
        @"
using (_mock1.GetMockRepository().Ordered())
{
  _mock1.Setup (mock => mock.Begin()).Callback (null);
  _mock1.Setup (mock => mock.Perform());
  _mock1.Setup (mock => mock.End());
}
  
using (_mockRepository.Ordered())
{
  _mock2.Setup (mock => mock.Begin());
  _mock2.Setup (mock => mock.Perform());
  _mock2.Setup (mock => mock.End());
}",
        //language=C#
        @"
var sequence1 = new MockSequence();
_mock1.InSequence (sequence1).Setup (mock => mock.Begin()).Callback (null);
_mock1.InSequence (sequence1).Setup (mock => mock.Perform());
_mock1.InSequence (sequence1).Setup (mock => mock.End());

var sequence2 = new MockSequence();
_mock2.InSequence (sequence2).Setup (mock => mock.Begin());
_mock2.InSequence (sequence2).Setup (mock => mock.Perform());
_mock2.InSequence (sequence2).Setup (mock => mock.End());")]
    [TestCase (
        //language=C#
        @"
using (_mockRepository.Ordered())
{
  _mock1.Protected().Setup (""OnInit"", true, EventArgs.Empty);
}",
        //language=C#
        @"
var sequence = new MockSequence();
_mock1.InSequence (sequence).Protected().Setup (""OnInit"", true, EventArgs.Empty);")]
    public void Rewrite_UsingStatement (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (expected, _context, true);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      _rewriter.MoqSymbols = new MoqSymbols (model.Compilation);
      var actualNode = node.Accept (_rewriter);

      var expectedExpressionStatements = expectedNode.DescendantNodes().Where (s => s.IsKind (SyntaxKind.ExpressionStatement)).ToList();
      var actualExpressionStatements = actualNode.DescendantNodes().Where (s => s.IsKind (SyntaxKind.ExpressionStatement)).ToList();

      Assert.AreEqual (expectedExpressionStatements.Count, actualExpressionStatements.Count);
      for (var i = 0; i < expectedExpressionStatements.Count; i++)
      {
        Assert.That (expectedExpressionStatements[i].IsEquivalentTo (actualExpressionStatements[i], false));
      }
    }
  }
}