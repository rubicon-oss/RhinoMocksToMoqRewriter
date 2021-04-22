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
using RhinoMocksToMoqRewriter.Core.Extensions;
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ArgumentStrategies;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters.Strategies.ArgumentStrategies
{
  [TestFixture]
  public class ArgumentRewriterStrategyFactoryTests
  {
    private readonly Context _context =
        new Context
        {
            //language=C#
            InterfaceContext =
                @"
  void DoSomething (int b);
  void DoSomething (IEnumerable<int> b);
  void DoSomething (string s);",
            //language=C#
            MethodContext = @"var mock = MockRepository.GenerateMock<ITestInterface>();"
        };

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (1);",
        typeof (DefaultArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.Anything);",
        typeof (ArgIsAnythingArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg.Is (1));",
        typeof (ArgIsArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.Equal (1));",
        typeof (ArgIsArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.NotEqual (1));",
        typeof (ArgIsNotEqualArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.Same (1));",
        typeof (ArgIsArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.NotSame (1));",
        typeof (ArgIsNotSameArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.GreaterThan (1));",
        typeof (ArgIsGreaterThanArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.GreaterThanOrEqual (1));",
        typeof (ArgIsGreaterThanOrEqualArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.LessThan (1));",
        typeof (ArgIsLessThanArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.LessThanOrEqual (1));",
        typeof (ArgIsLessThanOrEqualArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.NotNull);",
        typeof (ArgIsNotNullArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.Null);",
        typeof (ArgIsNullArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int[]>.List.ContainsAll (new[] {1, 2, 3}));",
        typeof (ArgListContainsAllArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int[]>.List.Equal (new[] {1, 2, 3}));",
        typeof (ArgListEqualArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int[]>.List.IsIn (3));",
        typeof (ArgListIsInArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Matches (_ => _ == 3));",
        typeof (ArgMatchesArgumentRewriteStrategy))]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg.Text.Like (""abc""));",
        typeof (ArgIsArgumentRewriteStrategy))]
    public void GetRewriteStrategy (string source, Type expectedType)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var argumentNode = node.GetFirstArgumentOrDefault();
      var rewriteStrategy = ArgumentRewriteStrategyFactory.GetRewriteStrategy (argumentNode!, model);
      var actualType = rewriteStrategy.GetType();

      Assert.AreEqual (expectedType, actualType);
    }
  }
}