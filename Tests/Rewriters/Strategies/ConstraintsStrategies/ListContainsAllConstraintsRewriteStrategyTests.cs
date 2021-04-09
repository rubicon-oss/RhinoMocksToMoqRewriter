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
using RhinoMocksToMoqRewriter.Core.Extensions;
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ConstraintsStrategies;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters.Strategies.ConstraintsStrategies
{
  [TestFixture]
  public class ListContainsAllConstraintsRewriteStrategyTests
  {
    private readonly IConstraintsRewriteStrategy _strategy = new ListContainsAllConstraintsRewriteStrategy();

    [Test]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.Constraints.List.ContainsAll (new[] {2, 3})",
        //language=C#
        @"new[] {2, 3}.All (_.Contains)")]
    public void Rewrite_ListContainsAll (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileExpressionStatement (source, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatement (expected, true);
      var actualNode = _strategy.Rewrite (node.Expression);

      Assert.That (expectedNode.Expression.IsEquivalentTo (actualNode, false));
    }
  }
}