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
void Do (int a);
void Do (int[] b);
void Do (ITestInterface c);",
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
        @"_mock.Do (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Equal (2)));",
        //language=C#
        @"_mock.Do (Arg<int>.Matches (_ => _ == 2));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Equal (2) & Rhino.Mocks.Constraints.Is.Equal (1)));",
        //language=C#
        @"_mock.Do (Arg<int>.Matches (_ => _ == 2 && _ == 1));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Equal (2) | Rhino.Mocks.Constraints.Is.Equal (1)));",
        //language=C#
        @"_mock.Do (Arg<int>.Matches (_ => _ == 2 || _ == 1));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Equal (2) & Rhino.Mocks.Constraints.Is.Equal (1) | Rhino.Mocks.Constraints.Is.Equal (3)));",
        //language=C#
        @"_mock.Do (Arg<int>.Matches (_ => _ == 2 && _ == 1 || _ == 3));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.NotEqual (2)));",
        //language=C#
        @"_mock.Do (Arg<int>.Matches (_ => _ != 2));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Same (2)));",
        //language=C#
        @"_mock.Do (Arg<int>.Matches (_ => _ == 2));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.NotSame (2)));",
        //language=C#
        @"_mock.Do (Arg<int>.Matches (_ => _ != 2));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.GreaterThan (2)));",
        //language=C#
        @"_mock.Do (Arg<int>.Matches (_ => _ > 2));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.GreaterThanOrEqual (2)));",
        //language=C#
        @"_mock.Do (Arg<int>.Matches (_ => _ >= 2));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.LessThan (2)));",
        //language=C#
        @"_mock.Do (Arg<int>.Matches (_ => _ < 2));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.LessThanOrEqual (2)));",
        //language=C#
        @"_mock.Do (Arg<int>.Matches (_ => _ <= 2));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Null()));",
        //language=C#
        @"_mock.Do (Arg<int>.Matches (_ => _ == null));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.NotNull()));",
        //language=C#
        @"_mock.Do (Arg<int>.Matches (_ => _ != null));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<ITestInterface>.Matches (Rhino.Mocks.Constraints.Property.Value (""A"", 42)));",
        //language=C#
        @"_mock.Do (Arg<ITestInterface>.Matches (_ => _.A == 42));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int[]>.Matches (Rhino.Mocks.Constraints.List.IsIn (2)));",
        //language=C#
        @"_mock.Do (Arg<int[]>.Matches (_ => _.Contains (2)));")]
    [TestCase (
        //language=C#
        @"_mock.Do (Arg<int[]>.Matches (Rhino.Mocks.Constraints.List.ContainsAll (new[] {1, 2})));",
        //language=C#
        @"_mock.Do (Arg<int[]>.Matches (_ => new[] {1, 2}.All (_.Contains)));")]
    public void Rewrite_ConstraintsExpression (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = _rewriter.Visit (node);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }
  }
}