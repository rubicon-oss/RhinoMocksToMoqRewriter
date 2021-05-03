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
  public class ArgumentRewriterTests
  {
    private ArgumentRewriter _rewriter;

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
int[] aArray = new[] { 1, 2, 3, 4 };
int[] bArray = new[] { 1, 2 };
int a = 1;
int b = 2;"
        };

    [SetUp]
    public void SetUp ()
    {
      _rewriter = new ArgumentRewriter();
    }

    #region Rhino.Mocks.Arg

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg.Is (_aString));",
        //language=C#
        @"mock.DoSomething (_aString);")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg.Is (_bInt));",
        //language=C#
        @"mock.DoSomething (_bInt);")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg.Is (_bInt), Arg.Is (_cBool));",
        //language=C#
        @"mock.DoSomething (_bInt, _cBool);")]
    [TestCase (
        //language=C#
        @"
mock.DoSomething (
  Arg.Is (_aString), 
  Arg.Is (_bInt), 
  Arg.Is (_cBool));",
        //language=C#
        @"
mock.DoSomething (
  _aString,
  _bInt,
  _cBool);")]
    [TestCase (
        //language=C#
        "mock.Expect (_ => _.DoSomething (Arg.Is (_bInt)));",
        //language=C#
        "mock.Expect (_ => _.DoSomething (_bInt));")]
    public void Rewrite_ArgIs (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    #endregion

    #region Rhino.Mocks.Constraints.IsArg`1

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.Anything);",
        //language=C#
        @"mock.DoSomething (It.IsAny<int>());")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.Anything, Arg<bool>.Is.Anything);",
        //language=C#
        @"mock.DoSomething (It.IsAny<int>(), It.IsAny<bool>());")]
    public void Rewrite_ArgIsAnything (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomethingWithDateTime (Arg<DateTime?>.Is.NotNull);",
        //language=C#
        @"mock.DoSomethingWithDateTime (It.IsNotNull<DateTime?>());")]
    public void Rewrite_ArgIsNotNull (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomethingWithDateTime (Arg<DateTime?>.Is.Null);",
        //language=C#
        @"mock.DoSomethingWithDateTime (null);")]
    public void Rewrite_ArgIsNull (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.Equal (1));",
        //language=C#
        @"mock.DoSomething (1);")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.Equal (a));",
        //language=C#
        @"mock.DoSomething (a);")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.Same (1));",
        //language=C#
        @"mock.DoSomething (1);")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.Same (a));",
        //language=C#
        @"mock.DoSomething (a);")]
    public void Rewrite_ArgIsEqualOrSame (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.NotEqual (1));",
        //language=C#
        @"mock.DoSomething (It.Is<int> (_ => !object.Equals (_, 1)));")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.NotEqual (a));",
        //language=C#
        @"mock.DoSomething (It.Is<int> (_ => !object.Equals (_, a)));")]
    public void Rewrite_ArgIsNotEqual (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.NotSame (1));",
        //language=C#
        @"mock.DoSomething (It.Is<int> (_ => !object.ReferenceEquals (_, 1)));")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.NotSame (a));",
        //language=C#
        @"mock.DoSomething (It.Is<int> (_ => !object.ReferenceEquals (_, a)));")]
    public void Rewrite_ArgIsNotSame (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.GreaterThan (2));",
        //language=C#
        @"mock.DoSomething (It.Is<int> (_ => _ > 2));")]
    [TestCase (
        //language=C#
        @"mock.DoSomethingWithDateTime (Arg<DateTime?>.Is.GreaterThan (DateTime.Now));",
        //language=C#
        @"mock.DoSomethingWithDateTime (It.Is<DateTime?> (_ => _ > DateTime.Now));")]
    public void Rewrite_ArgIsGreaterThan (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.GreaterThanOrEqual (2));",
        //language=C#
        @"mock.DoSomething (It.Is<int> (_ => _ >= 2));")]
    public void Rewrite_ArgIsGreaterThanOrEqual (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.LessThan (2));",
        //language=C#
        @"mock.DoSomething (It.Is<int> (_ => _ < 2));")]
    public void Rewrite_ArgIsLessThan (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.LessThanOrEqual (2));",
        //language=C#
        @"mock.DoSomething (It.Is<int> (_ => _ <= 2));")]
    public void Rewrite_ArgIsLessThanOrEqual (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    #endregion

    #region Rhino.Mocks.Arg`1

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int[]>.Matches (i => i.Length == 2));",
        //language=C#
        @"mock.DoSomething (It.Is<int[]> (i => i.Length == 2));")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int[]>.Matches (i => i.Length == 2), Arg<string[]>.Matches (s => s.Length == 2));",
        //language=C#
        @"mock.DoSomething (It.Is<int[]> (i => i.Length == 2), It.Is<string[]> (s => s.Length == 2));")]
    public void Rewrite_ArgMatches (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    #endregion

    #region Rhino.Mocks.Constraints.ListArg`1

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int[]>.List.Equal (new [] {1, 2}));",
        //language=C#
        @"mock.DoSomething (new [] {1, 2});")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int[]>.List.Equal (new [] {1, 2}), Arg<string[]>.List.Equal (new [] {""a"", ""b""}));",
        //language=C#
        @"mock.DoSomething (new [] {1, 2}, new [] {""a"", ""b""});")]
    public void Rewrite_ArgListEquals (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int[]>.List.IsIn (3));",
        //language=C#
        @"mock.DoSomething (It.Is<int[]> (_ => _.Contains (3)));")]
    public void Rewrite_ArgListIsIn (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int[]>.List.ContainsAll (aArray));",
        //language=C#
        @"mock.DoSomething (It.Is<int[]> (_ => aArray.All (_.Contains)));")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int[]>.List.ContainsAll (new[] {1, 2, 3}));",
        //language=C#
        @"mock.DoSomething (It.Is<int[]> (_ => new[] {1, 2, 3}.All (_.Contains)));")]
    public void Rewrite_ArgListContainsAll (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    #endregion
  }
}