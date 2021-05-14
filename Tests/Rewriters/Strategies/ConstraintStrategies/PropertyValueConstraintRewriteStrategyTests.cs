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
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ConstraintStrategies;
namespace RhinoMocksToMoqRewriter.Tests.Rewriters.Strategies.ConstraintStrategies
{
  [TestFixture]
  public class PropertyValueConstraintRewriteStrategyTests
  {
    private readonly IConstraintRewriteStrategy _strategy = new PropertyValueConstraintRewriteStrategy();

    [Test]
    [TestCase (
        //language=C#
        @"Rhino.Mocks.Constraints.Property.Value (""A"", 42)",
        //language=C#
        @"_ != null && object.Equals (_.A, 42))")]
    public void Rewrite_PropertyValue (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileExpressionStatement (source, true);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatement (expected, true);
      var actualNode = _strategy.Rewrite (node.Expression);

      Assert.That (expectedNode.Expression.IsEquivalentTo (actualNode, false));
    }
  }
}