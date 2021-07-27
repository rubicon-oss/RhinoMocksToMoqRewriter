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
  public class SetupResultForRewriterTests
  {
    private SetupResultForRewriter _rewriter;

    private readonly Context _context =
        new Context()
        {
            //language=C#
            InterfaceContext =
                @"
int Number { get; set; }
int DoSomething (int a);
int DoSomething(Func<int> func);
int DoSomething(Action action);",
            //language=C#
            ClassContext =
                @"
private void DoSomething (string s, object o, Action a)
{
  throw new NotImplementedException();
}

private ITestInterface _mock = MockRepository.GenerateMock<ITestInterface>();"
        };

    [SetUp]
    public void SetUp ()
    {
      _rewriter = new SetupResultForRewriter();
    }

    [Test]
    [TestCase (
        //language=C#
        @"SetupResult.For (_mock.Number).Return (42);",
        //language=C#
        @"_mock.Stub (_ => _.Number).Return (42).Repeat.Any();")]
    [TestCase (
        //language=C#
        @"SetupResult.For (_mock.Number).IgnoreArguments().Return (42);",
        //language=C#
        @"_mock.Stub (_ => _.Number).IgnoreArguments().Return (42).Repeat.Any();")]
    public void Rewrite_SetupResultFor (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"
using (null)
{
  SetupResult.For (_mock.Number).Return (42);
}",
        //language=C#
        @"
using (null)
{
 _mock.Stub (_ => _.Number).Return (42).Repeat.Any();
}")]
    [TestCase (
        //language=C#
        @"
DoSomething(
    ""test"",
    null,
    delegate { SetupResult.For (_mock.DoSomething(() => 1)).Return (42); });",
        //language=C#
        @"
DoSomething(
    ""test"",
    null,
    delegate { _mock.Stub (_ => _.DoSomething (() => 1)).Return (42).Repeat.Any(); });")]
    [TestCase (
        //language=C#
        @"
_mock.DoSomething(
    delegate
    {
      SetupResult.For (_mock.DoSomething (
        () => { SetupResult.For (_mock.Number).Return (42); })).Return (1);
    });",
        //language=C#
        @"
_mock.DoSomething(
    delegate
    {
      _mock.Stub (_ => _.DoSomething (
        () => { _mock.Stub (_ => _.Number).Return (42).Repeat.Any(); })).Return (1).Repeat.Any();
    });")]
    public void Rewrite_NestedSetupResultFor (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (expected, _context);
      _rewriter.Model = model;
      _rewriter.RhinoMocksSymbols = new RhinoMocksSymbols (model.Compilation);
      var actualNode = _rewriter.Visit (node);

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