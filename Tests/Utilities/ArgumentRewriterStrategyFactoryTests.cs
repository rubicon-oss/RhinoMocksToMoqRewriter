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
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies;
using RhinoMocksToMoqRewriter.Core.Utilities;

namespace RhinoMocksToMoqRewriter.Tests.Utilities
{
  [TestFixture]
  public class ArgumentRewriterStrategyFactoryTests
  {
    private readonly Context _context =
        new Context
        {
            InterfaceContext =
                //language=C#
                @"
  void DoSomething (int b);
  void DoSomething (IEnumerable<int> b);"
        };

    [Test]
    [TestCase ("mock.DoSomething (1)", typeof (DefaultArgumentRewriteStrategy))]
    [TestCase ("mock.DoSomething (Arg<int>.Is.Anything);", typeof (ArgIsAnythingArgumentArgumentRewriteStrategy))]
    [TestCase ("mock.DoSomething (Arg.Is (1));", typeof (ArgIsArgumentArgumentRewriteStrategy))]
    [TestCase ("mock.DoSomething (Arg<int>.Is.GreaterThan (1));", typeof (ArgIsGreaterThanArgumentArgumentRewriteStrategy))]
    [TestCase ("mock.DoSomething (Arg<int>.Is.GreaterThanOrEqual (1));", typeof (ArgIsGreaterThanOrEqualArgumentArgumentRewriteStrategy))]
    [TestCase ("mock.DoSomething (Arg<int>.Is.LessThan (1));", typeof (ArgIsLessThanArgumentArgumentRewriteStrategy))]
    [TestCase ("mock.DoSomething (Arg<int>.Is.LessThanOrEqual (1))", typeof (ArgIsLessThanOrEqualArgumentArgumentRewriteStrategy))]
    [TestCase ("mock.DoSomething (Arg<int>.Is.NotNull)", typeof (ArgIsNotNullArgumentArgumentRewriteStrategy))]
    [TestCase ("mock.DoSomething (Arg<int>.Is.Null)", typeof (ArgIsNullArgumentArgumentRewriteStrategy))]
    [TestCase ("mock.DoSomething (Arg<int[]>.List.ContainsAll (new[] {1, 2, 3}))", typeof (ArgListContainsAllArgumentArgumentRewriteStrategy))]
    [TestCase ("mock.DoSomething (Arg<int[]>.List.Equal (new[] {1, 2, 3}))", typeof (ArgListEqualArgumentArgumentRewriteStrategy))]
    [TestCase ("mock.DoSomething (Arg<int[]>.List.IsIn (3))", typeof (ArgListIsInArgumentArgumentRewriteStrategy))]
    [TestCase ("mock.DoSomething (Arg<int>.Matches (3))", typeof (ArgMatchesArgumentArgumentRewriteStrategy))]
    public void GetRewriteStrategy (string source, Type expectedType)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var argumentNode = node.GetFirstArgumentOrDefault();
      var rewriteStrategy = ArgumentRewriterStrategyFactory.GetRewriterStrategy (argumentNode!, model);
      var actualType = rewriteStrategy.GetType();

      Assert.AreEqual (expectedType, actualType);
    }
  }
}