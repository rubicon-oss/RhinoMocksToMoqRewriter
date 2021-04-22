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
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ArgumentStrategies;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters.Strategies.ArgumentStrategies
{
  public class ArgIsArgumentRewriteStrategyTests
  {
    private readonly IArgumentRewriteStrategy _strategy = new ArgIsArgumentRewriteStrategy();

    private readonly Context _context =
        new Context
        {
            //language=C#
            InterfaceContext = @"
void DoSomething (int b);
void DoSomething (string s);",
            //language=C#
            MethodContext = @"var mock = MockRepository.GenerateMock<ITestInterface>();"
        };

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg.Is (1));",
        //language=C#
        @"mock.DoSomething (1);")]
    public void Rewrite_ArgIs (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileArgumentWithContext (source, _context);
      var (_, expectedArgumentNode) = CompiledSourceFileProvider.CompileArgumentWithContext (expected, _context);
      var actualNode = _strategy.Rewrite (node);

      Assert.That (expectedArgumentNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.Equal (1));",
        //language=C#
        @"mock.DoSomething (1);")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.Same (1));",
        //language=C#
        @"mock.DoSomething (1);")]
    public void Rewrite_ArgIsEqualOrSame (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileArgumentWithContext (source, _context);
      var (_, expectedArgumentNode) = CompiledSourceFileProvider.CompileArgumentWithContext (expected, _context);
      var actualNode = _strategy.Rewrite (node);

      Assert.That (expectedArgumentNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg.Text.Like (""abc""));",
        //language=C#
        @"mock.DoSomething (""abc"");")]
    public void Rewrite_ArgText (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileArgumentWithContext (source, _context);
      var (_, expectedArgumentNode) = CompiledSourceFileProvider.CompileArgumentWithContext (expected, _context);
      var actualNode = _strategy.Rewrite (node);

      Assert.That (expectedArgumentNode.IsEquivalentTo (actualNode, false));
    }
  }
}