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
  public class ArgumentRewriterTests
  {
    private ArgumentRewriter _rewriter;
    private Mock<IFormatter> _formatter;

    private readonly Context _context =
        new Context
        {
            InterfaceContext =
                //language=C#
                @"
void DoSomething (string a); 
void DoSomething (int b);
void DoSomething (int b, bool c);
void DoSomething (string a, int b, bool c);
void DoSomething (IEnumerable<int> b);
void DoSomething (IEnumerable<int> b, IEnumerable<string> d);
void DoSomethingWithDateTime (DateTime? dateTime);",
            ClassContext =
                //language=C#
                @"
private string _aString; 
private int _bInt; 
private bool _cBool;",
            MethodContext =
                //language=C#
                @"
var mock = MockRepository.GenerateMock<ITestInterface>();
int a = new[] { 1, 2, 3, 4 };
int b = new[] { 1, 2 };"
        };

    [SetUp]
    public void SetUp ()
    {
      _formatter = new Mock<IFormatter>();
      _formatter.Setup (f => f.Format (It.IsAny<SyntaxNode>()))
          .Returns<SyntaxNode> (s => s);
      _rewriter = new ArgumentRewriter (_formatter.Object);
    }

    #region Rhino.Mocks.Arg

    [Test]
    [TestCase ("mock.DoSomething (Arg.Is (_aString));", "mock.DoSomething (_aString);")]
    [TestCase ("mock.DoSomething (Arg.Is (_bInt));", "mock.DoSomething (_bInt);")]
    [TestCase (
        "mock.DoSomething (Arg.Is (_bInt), Arg.Is (_cBool));",
        "mock.DoSomething (_bInt, _cBool);")]
    [TestCase (
        @"mock.DoSomething (
    Arg.Is (_aString), 
    Arg.Is (_bInt), 
    Arg.Is (_cBool));",
        @"mock.DoSomething (
    _aString,
    _bInt,
    _cBool);")]
    public void Rewrite_ArgIs (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    #endregion

    #region Rhino.Mocks.Constraints.IsArg`1

    [Test]
    [TestCase ("mock.DoSomething (Arg<int>.Is.Anything);", "mock.DoSomething (It.IsAny<int>());")]
    [TestCase (
        "mock.DoSomething (Arg<int>.Is.Anything, Arg<bool>.Is.Anything);",
        "mock.DoSomething (It.IsAny<int>(), It.IsAny<bool>());")]
    public void Rewrite_ArgIsAnything (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase ("mock.DoSomethingWithDateTime (Arg<DateTime?>.Is.NotNull);", "mock.DoSomethingWithDateTime (It.IsNotNull<DateTime?>());")]
    public void Rewrite_ArgIsNotNull (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase ("mock.DoSomethingWithDateTime (Arg<DateTime?>.Is.Null);", "mock.DoSomethingWithDateTime (null);")]
    public void Rewrite_ArgIsNull (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase ("mock.DoSomething (Arg<int>.Is.Equal(1));", "mock.DoSomething (1);")]
    public void Rewrite_ArgIsEqual (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase ("mock.DoSomething (Arg<int>.Is.Same(1));", "mock.DoSomething (1);")]
    [TestCase ("mock.DoSomething (Arg<int>.Is.Same(a));", "mock.DoSomething (a);")]
    public void Rewrite_ArgIsSame (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase ("mock.DoSomething (Arg<int>.Is.NotSame(1));", "mock.DoSomething (It.Is<int> (param => param != 1));")]
    [TestCase ("mock.DoSomething (Arg<int>.Is.NotSame(a));", "mock.DoSomething (It.Is<int> (param => param != a));")]
    public void Rewrite_ArgIsNotSame (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase ("mock.DoSomething (Arg<int>.Is.GreaterThan (2));", "mock.DoSomething (It.Is<int> (param => param > 2));")]
    [TestCase (
        "mock.DoSomethingWithDateTime (Arg<DateTime?>.Is.GreaterThan (DateTime.Now));",
        "mock.DoSomethingWithDateTime (It.Is<DateTime?> (param => param > DateTime.Now));")]
    public void Rewrite_ArgIsGreaterThan (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase ("mock.DoSomething (Arg<int>.Is.GreaterThanOrEqual (2));", "mock.DoSomething (It.Is<int> (param => param >= 2));")]
    public void Rewrite_ArgIsGreaterThanOrEqual (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase ("mock.DoSomething (Arg<int>.Is.LessThan (2));", "mock.DoSomething (It.Is<int> (param => param < 2));")]
    public void Rewrite_ArgIsLessThan (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase ("mock.DoSomething (Arg<int>.Is.LessThanOrEqual (2));", "mock.DoSomething (It.Is<int> (param => param <= 2));")]
    public void Rewrite_ArgIsLessThanOrEqual (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    #endregion

    #region Rhino.Mocks.Arg`1

    [Test]
    [TestCase ("mock.DoSomething (Arg<int[]>.Matches (i => i.Length == 2));", "mock.DoSomething (It.Is<int[]> (i => i.Length == 2));")]
    [TestCase (
        "mock.DoSomething (Arg<int[]>.Matches (i => i.Length == 2), Arg<string[]>.Matches (s => s.Length == 2));",
        "mock.DoSomething (It.Is<int[]> (i => i.Length == 2), It.Is<string[]> (s => s.Length == 2));")]
    public void Rewrite_ArgMatches (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    #endregion

    #region Rhino.Mocks.Constraints.ListArg`1

    [Test]
    [TestCase ("mock.DoSomething (Arg<int[]>.List.Equal (new [] {1, 2}));", "mock.DoSomething (new [] {1, 2});")]
    [TestCase (
        "mock.DoSomething (Arg<int[]>.List.Equal (new [] {1, 2}), Arg<string[]>.List.Equal(new [] {\"a\", \"b\"}));",
        "mock.DoSomething (new [] {1, 2}, new [] {\"a\", \"b\"});")]
    public void Rewrite_ArgListEquals (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase ("mock.DoSomething (Arg<int[]>.List.IsIn (3));", "mock.DoSomething (It.Is<int[]> (param => param.Contains(3)));")]
    public void Rewrite_ArgListIsIn (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    [Test]
    [TestCase ("mock.DoSomething (Arg<int[]>.List.ContainsAll (a));", "mock.DoSomething (It.Is<int[]> (param => a.All(param.Contains)));")]
    [TestCase ("mock.DoSomething (Arg<int[]>.List.ContainsAll (new[] {1, 2, 3}));", "mock.DoSomething (It.Is<int[]> (param => new[] {1, 2, 3}.All(param.Contains)));")]
    public void Rewrite_ArgListContainsAll (string source, string expected)
    {
      var (model, actualNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var newNode = actualNode.Accept (_rewriter);

      Assert.NotNull (newNode);
      Assert.That (expectedNode.IsEquivalentTo (newNode, false));
    }

    #endregion
  }
}