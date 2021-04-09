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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ConstraintsStrategies;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters.Strategies.ConstraintsStrategies
{
  [TestFixture]
  public class DefaultConstraintsRewriteStrategyTests
  {
    private readonly IConstraintsRewriteStrategy _strategy = new DefaultConstraintsRewriteStrategy();

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
        @"mock.DoSomething (1);",
        //language=C#
        @"mock.DoSomething (1);")]
    public void Rewrite_Default (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedArgumentNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      var actualNode = _strategy.Rewrite (node.Expression);

      Assert.That (expectedArgumentNode.Expression.IsEquivalentTo (actualNode, false));
    }
  }
}