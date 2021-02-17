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
using RhinoMocksToMoqRewriter.Core.Utilities;

namespace RhinoMocksToMoqRewriter.Tests.Utilities
{
  [TestFixture]
  public class FormatterTests
  {
    private readonly IFormatter _formatter = new Formatter();

    [Test]
    [TestCase ("new Mock<string>()", "new Mock<string>()")]
    [TestCase ("new Mock<string>(1, 2, 3, 4)", "new Mock<string> (1, 2, 3, 4)")]
    [TestCase ("new Mock<string>(1,2,3,4)", "new Mock<string> (1, 2, 3, 4)")]
    [TestCase (
        @"      new Mock<string> (
          42,
          32,
          43)",
        @"new Mock<string> (
          42,
          32,
          43)")]
    [TestCase (
        @"      new Mock<string> (MockBehaviour.Strict,
          42,
          32,
          43)",
        @"new Mock<string> (
          MockBehaviour.Strict,
          42,
          32,
          43)")]
    [TestCase (
        @"new Mock<string> (
MockBehaviour.Strict,    42,
    32,
    43)",
        @"new Mock<string> (
    MockBehaviour.Strict,
    42,
    32,
    43)")]
    public void FormatNodes (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileObjectCreationExpression (source);
      var formattedNode = _formatter.Format (node);
      Assert.AreEqual (expected, formattedNode.ToString());
    }
  }
}