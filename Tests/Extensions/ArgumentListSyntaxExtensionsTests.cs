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

namespace RhinoMocksToMoqRewriter.Tests.Extensions
{
  [TestFixture]
  public class ArgumentListSyntaxExtensionsTests
  {
    [Test]
    [TestCase ("()", "")]
    [TestCase ("(\r\n42)", "\r\n")]
    [TestCase ("( \n  42)", "\n")]
    [TestCase ("(\r\n42   \n)", "\r\n")]
    [TestCase ("(\n42 \r\n )", "\r\n")]
    [TestCase ("(42,\n21,42\r\n)", "\r\n")]
    [TestCase ("(\r\n)", "\r\n")]
    public void GetNewLineCharacter (string source, string expected)
    {
      var (_, argumentList) = CompiledSourceFileProvider.CompileArgumentList (source);
      var newLineCharacter = argumentList.GetNewLineCharacter();
      Assert.AreEqual (expected, newLineCharacter);
    }

    [Test]
    [TestCase ("()", "")]
    [TestCase ("(\r\n   42)", "   ")]
    [TestCase ("( \n 42)", " ")]
    [TestCase ("(42)", "")]
    [TestCase ("(\n42 \r\n     )", "")]
    [TestCase ("(\n\r42,    21,\r\n    42)", "    ")]
    [TestCase ("(0, 1,  2,   3)", " ")]
    [TestCase ("(0,1,2,3)", " ")]
    public void GetIndentation (string source, string expected)
    {
      var (_, argumentList) = CompiledSourceFileProvider.CompileArgumentList (source);
      var indentation = argumentList.GetIndentation();
      Assert.AreEqual (expected, indentation);
    }

    [Test]
    [TestCase ("()", true)]
    [TestCase ("(   )", true)]
    [TestCase ("(\n42)", false)]
    [TestCase ("(42)", false)]
    [TestCase ("(42, 24, 52)", false)]
    public void IsEmpty (string source, bool expected)
    {
      var (_, argumentList) = CompiledSourceFileProvider.CompileArgumentList (source);
      var isEmpty = argumentList.IsEmpty();
      Assert.AreEqual (expected, isEmpty);
    }

    [Test]
    [TestCase ("()", false)]
    [TestCase ("(42)", true)]
    [TestCase ("(42, 53)", false)]
    public void IsSingleArgumentList (string source, bool expected)
    {
      var (_, argumentList) = CompiledSourceFileProvider.CompileArgumentList (source);
      var isEmpty = argumentList.IsSingleArgumentList();
      Assert.AreEqual (expected, isEmpty);
    }
  }
}