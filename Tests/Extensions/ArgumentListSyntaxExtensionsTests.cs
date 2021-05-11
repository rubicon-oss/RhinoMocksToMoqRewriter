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
    private readonly Context _context =
        new Context
        {
            //language=C#
            InterfaceContext =
                @"
void DoSomething();
void DoSomething (int a);
void DoSomething (int a, int b);
void DoSomething (int a, int b, int c);
void DoSomething (int a, int b, int c, int d);",
            //language=C#
            MethodContext =
                @"
var mock = MockRepository.GenerateMock<ITestInterface>();"
        };

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething();",
        //language=C#
        @"")]
    [TestCase (
        //language=C#
        "mock.DoSomething(\r\n42);",
        //language=C#
        "\r\n")]
    [TestCase (
        //language=C#
        "mock.DoSomething( \n  42);",
        //language=C#
        "\n")]
    [TestCase (
        //language=C#
        "mock.DoSomething(\r\n42   \n);",
        //language=C#
        "\r\n")]
    [TestCase (
        //language=C#
        "mock.DoSomething(\n42 \r\n );",
        //language=C#
        "\r\n")]
    [TestCase (
        //language=C#
        "mock.DoSomething(42,\n21,42\r\n);",
        //language=C#
        "\r\n")]
    [TestCase (
        //language=C#
        "mock.DoSomething(\r\n);",
        //language=C#
        "\r\n")]
    public void GetNewLineCharacter (string source, string expected)
    {
      var (_, argumentList) = CompiledSourceFileProvider.CompileArgumentListWithContext (source, _context);
      var newLineCharacter = argumentList.GetNewLineCharacter();
      Assert.AreEqual (expected, newLineCharacter);
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething();",
        //language=C#
        @"")]
    [TestCase (
        //language=C#
        "mock.DoSomething (\r\n   42);",
        //language=C#
        @"   ")]
    [TestCase (
        //language=C#
        "mock.DoSomething ( \n 42);",
        //language=C#
        @" ")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (42);",
        //language=C#
        @"")]
    [TestCase (
        //language=C#
        "mock.DoSomething (\n42 \r\n     );",
        //language=C#
        @"")]
    [TestCase (
        //language=C#
        "mock.DoSomething (\n\r42,    21,\r\n    42);",
        //language=C#
        @"    ")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (0, 1,  2,   3);",
        //language=C#
        @" ")]
    [TestCase (
        //language=C#
        @"mock.DoSomething (0,1,2,3);",
        //language=C#
        @" ")]
    public void GetIndentation (string source, string expected)
    {
      var (_, argumentList) = CompiledSourceFileProvider.CompileArgumentListWithContext (source, _context);
      var indentation = argumentList.GetIndentation();
      Assert.AreEqual (expected, indentation);
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething();",
        true)]
    [TestCase (
        //language=C#
        @"mock.DoSomething(   );",
        true)]
    [TestCase (
        //language=C#
        "mock.DoSomething (\n42);",
        false)]
    [TestCase (
        //language=C#
        @"mock.DoSomething (42);",
        false)]
    [TestCase (
        //language=C#
        @"mock.DoSomething (42, 24, 52);",
        false)]
    public void IsEmpty (string source, bool expected)
    {
      var (_, argumentList) = CompiledSourceFileProvider.CompileArgumentListWithContext (source, _context);
      var isEmpty = argumentList.IsEmpty();
      Assert.AreEqual (expected, isEmpty);
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.DoSomething();",
        false)]
    [TestCase (
        //language=C#
        @"mock.DoSomething (42);",
        true)]
    [TestCase (
        //language=C#
        @"mock.DoSomething (42, 53);",
        false)]
    public void IsSingleArgumentList (string source, bool expected)
    {
      var (_, argumentList) = CompiledSourceFileProvider.CompileArgumentListWithContext (source, _context);
      var isEmpty = argumentList.IsSingleArgumentList();
      Assert.AreEqual (expected, isEmpty);
    }
  }
}