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
  public class ConstraintsRewriterTests
  {
    private ConstraintsRewriter _rewriter;

    private readonly Context _context =
        new Context()
        {
            //language=C#
            UsingContext =
                @"
using Rhino.Mocks.Constraints;",
            //language=C#
            InterfaceContext =
                @"
public int A { get; set; }
public int B { get; set; }
void DoSomething (int a);
void DoSomething (int[] b);
void DoSomething (List<int> b);
void DoSomething (ITestInterface b);
void DoSomething (ITestInterface b, int i);
void DoSomething (ITestInterface a, ITestInterface b);",
            //language=C#
            ClassContext =
                @"
private ITestInterface _mock = MockRepository.GenerateMock<ITestInterface>();"
        };

    [SetUp]
    public void SetUp ()
    {
      _rewriter = new ConstraintsRewriter();
    }

    [Test]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Equal (2)));",
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (_ => object.Equals (_, 2)));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Equal (2) & Rhino.Mocks.Constraints.Is.Equal (1)));",
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (_ => object.Equals (_, 2) && object.Equals (_, 1)));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Equal (2) | Rhino.Mocks.Constraints.Is.Equal (1)));",
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (_ => object.Equals (_, 2) || object.Equals (_, 1)));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Equal (2) & Rhino.Mocks.Constraints.Is.Equal (1) | Rhino.Mocks.Constraints.Is.Equal (3)));",
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (_ => object.Equals (_, 2) && object.Equals (_, 1) || object.Equals (_, 3)));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.NotEqual (2)));",
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (_ => !object.Equals (_, 2)));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Same (2)));",
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (_ => object.ReferenceEquals (_, 2)));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.NotSame (2)));",
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (_ => !object.ReferenceEquals (_, 2)));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.GreaterThan (2)));",
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (_ => _ > 2));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.GreaterThanOrEqual (2)));",
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (_ => _ >= 2));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.LessThan (2)));",
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (_ => _ < 2));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.LessThanOrEqual (2)));",
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (_ => _ <= 2));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Null()));",
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (_ => _ == null));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.NotNull()));",
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (_ => _ != null));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<ITestInterface>.Matches (Rhino.Mocks.Constraints.Property.Value (""A"", 42)));",
        //language=C#
        @"_mock.DoSomething (Arg<ITestInterface>.Matches (_ => _ != null && object.Equals (_.A, 42)));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int[]>.Matches (Rhino.Mocks.Constraints.List.IsIn (2)));",
        //language=C#
        @"_mock.DoSomething (Arg<int[]>.Matches (_ => _.Contains (2)));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int[]>.Matches (Rhino.Mocks.Constraints.List.ContainsAll (new[] {1, 2})));",
        //language=C#
        @"_mock.DoSomething (Arg<int[]>.Matches (_ => new[] {1, 2}.All (_.Contains)));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<ITestInterface>.Matches (Rhino.Mocks.Constraints.Is.NotNull() & Rhino.Mocks.Constraints.Property.Value (""B"", 1337)));",
        //language=C#
        @"_mock.DoSomething (Arg<ITestInterface>.Matches (_ => Rhino.Mocks.Constraints.Is.NotNull() && Rhino.Mocks.Constraints.Property.Value (""B"", 1337)));")]
    [TestCase (
        //language=C#
        @"
_mock.DoSomething (
    Arg<ITestInterface>.Matches (Rhino.Mocks.Constraints.Property.Value (""A"", 42)), 
    Arg<ITestInterface>.Matches (Rhino.Mocks.Constraints.Property.Value (""B"", 1337)));",
        //language=C#
        @"
_mock.DoSomething (
    Arg<ITestInterface>.Matches (_ => _ != null && object.Equals (_.A, 42)), 
    Arg<ITestInterface>.Matches (_ => _ != null && object.Equals (_.B, 1337)));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<List<int>>.Matches (Rhino.Mocks.Constraints.Property.Value (""Count"", 3) & Rhino.Mocks.Constraints.List.IsIn (2)));",
        //language=C#
        @"_mock.DoSomething (Arg<List<int>>.Matches (_ => _ != null && object.Equals (_.Count, 3) && _.Contains (2)));")]
    [TestCase (
        //language=C#
        @"
_mock.DoSomething (
    Arg<List<int>>.Matches (Rhino.Mocks.Constraints.Property.Value (""Count"", 7) & Rhino.Mocks.Constraints.List.ContainsAll (new[] {1, 2})));",
        //language=C#
        @"_mock.DoSomething (Arg<List<int>>.Matches (_ => _ != null && object.Equals (_.Count, 7) && new[] {1, 2}.All (_.Contains)));")]
    [TestCase (
        //language=C#
        @"
_mock.DoSomething (Arg<ITestInterface>.Matches (Rhino.Mocks.Constraints.Property.Value (""A"", 42) & Rhino.Mocks.Constraints.Property.Value (""B"", 1337)));",
        //language=C#
        @"
_mock.DoSomething (Arg<ITestInterface>.Matches (_ => Rhino.Mocks.Constraints.Property.Value (""A"", 42) && Rhino.Mocks.Constraints.Property.Value (""B"", 1337)));")]
    [TestCase (
        //language=C#
        @"
_mock.DoSomething (
    Arg<List<int>>.Matches (Rhino.Mocks.Constraints.Property.Value (""Count"", 7) | Rhino.Mocks.Constraints.List.ContainsAll (new[] {1, 2})));",
        //language=C#
        @"_mock.DoSomething (Arg<List<int>>.Matches (_ => _ != null && object.Equals (_.Count, 7) || new[] {1, 2}.All (_.Contains)));")]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<ITestInterface>.Matches (Rhino.Mocks.Constraints.Is.NotNull() | Rhino.Mocks.Constraints.Property.Value (""B"", 1337)));",
        //language=C#
        @"_mock.DoSomething (Arg<ITestInterface>.Matches (_ => Rhino.Mocks.Constraints.Is.NotNull() || Rhino.Mocks.Constraints.Property.Value (""B"", 1337)));")]
    public void Rewrite_ConstraintsExpression (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context, true);
      _rewriter.Model = model;
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }
  }
}