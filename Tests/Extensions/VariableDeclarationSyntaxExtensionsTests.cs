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
  public class VariableDeclarationSyntaxExtensionsTests
  {
    [Test]
    [TestCase ("private int a;", "")]
    [TestCase ("private int a, b, c;", "")]
    [TestCase ("private int\r\na,\r\nb;\r\n", "\r\n")]
    [TestCase ("private int\n  a,\r\n  b;", "\r\n")]
    [TestCase ("private int\na,\nb\n;", "\n")]
    public void GetNewLineCharacter (string source, string expected)
    {
      var (_, fieldDeclaration) = CompiledSourceFileProvider.CompileFieldDeclaration (source);
      var newLineCharacter = fieldDeclaration.Declaration.GetNewLineCharacter();
      Assert.AreEqual (expected, newLineCharacter);
    }

    [Test]
    [TestCase ("private int a;", "")]
    [TestCase ("private int a, b, c;", " ")]
    [TestCase ("private int\n  a,\r\n  b;", "  ")]
    public void GetIndentation (string source, string expected)
    {
      var (_, fieldDeclaration) = CompiledSourceFileProvider.CompileFieldDeclaration (source);
      var indentation = fieldDeclaration.Declaration.GetIndentation();
      Assert.AreEqual (expected, indentation);
    }
  }
}