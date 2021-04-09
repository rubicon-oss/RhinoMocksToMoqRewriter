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
using NUnit.Framework;
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ArgumentStrategies;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters.Strategies.ArgumentStrategies
{
  [TestFixture]
  public class ArgIsNotEqualOrSameArgumentRewriteStrategyTests
  {
    private readonly IArgumentRewriteStrategy _strategy = new ArgIsNotEqualOrSameArgumentRewriteStrategy();

    private readonly Context _context =
        new Context
        {
            //language=C#
            InterfaceContext = @"void DoSomething (int b);",
            //language=C#
            MethodContext = @"var mock = MockRepository.GenerateMock<ITestInterface>();"
        };

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.NotEqual (2));",
        //language=C#
        @"mock.DoSomething (It.Is<int> (param => param != 2));")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (Arg<int>.Is.NotSame (2));",
        //language=C#
        @"mock.DoSomething (It.Is<int> (param => param != 2));")]
    public void Rewrite_ArgIsNotEqualOrSame (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileArgumentWithContext (source, _context);
      var (_, expectedArgumentNode) = CompiledSourceFileProvider.CompileArgumentWithContext (expected, _context);
      var actualNode = _strategy.Rewrite (node);

      Assert.That (expectedArgumentNode.IsEquivalentTo (actualNode, false));
    }
  }
}