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
using RhinoMocksToMoqRewriter.Core.Rewriters;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters
{
  [TestFixture]
  public class FormatterTests
  {
    private readonly IFormatter _formatter = new Formatter();

    [Test]
    [TestCase (
        //language=C#
        @"new Mock<string>{ Callbase = true }",
        //language=C#
        @"new Mock<string> { Callbase = true }")]
    [TestCase (
        //language=C#
        @"new Mock<string>()",
        //language=C#
        @"new Mock<string>()")]
    [TestCase (
        //language=C#
        @"new Mock<string>(1, 2, 3, 4)",
        //language=C#
        @"new Mock<string> (1, 2, 3, 4)")]
    [TestCase (
        //language=C#
        @"new Mock<string>(1,2,3,4)",
        //language=C#
        @"new Mock<string> (1, 2, 3, 4)")]
    [TestCase (
        //language=C#
        @"      new Mock<string> (
          42,
          32,
          43)",
        //language=C#
        @"new Mock<string> (
          42,
          32,
          43)")]
    [TestCase (
        //language=C#
        @"      new Mock<string> (MockBehavior.Strict,
          42,
          32,
          43)",
        //language=C#
        @"new Mock<string> (
          MockBehavior.Strict,
          42,
          32,
          43)")]
    [TestCase (
        //language=C#
        @"new Mock<string> (
MockBehavior.Strict,    42,
    32,
    43)",
        //language=C#
        @"new Mock<string> (
    MockBehavior.Strict,
    42,
    32,
    43)")]
    [TestCase (
        //language=C#
        @"new Mock<string>(){ CallBase = true }",
        //language=C#
        @"new Mock<string>() { CallBase = true }")]
    [TestCase (
        //language=C#
        @"new Mock<string> (
MockBehavior.Strict,    42,
    32){ CallBase = true }",
        //language=C#
        @"new Mock<string> (
    MockBehavior.Strict,
    42,
    32)
    { CallBase = true }")]
    public void Format_ObjectCreationExpression (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileObjectCreationExpression (source, true);
      var formattedNode = _formatter.Format (node);

      Assert.AreEqual (expected, formattedNode.ToString());
    }

    [Test]
    [TestCase (
        //language=C#
        @"(1,2,3,4)",
        //language=C#
        @"(1, 2, 3, 4)")]
    [TestCase (
        //language=C#
        @"(1,
  2,  3,
  4)",
        //language=C#
        @"(
  1,
  2,
  3,
  4)")]
    public void Format_ArgumentList (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileArgumentList (source, true);
      var formattedNode = _formatter.Format (node);

      Assert.AreEqual (expected, formattedNode.ToString());
    }

    [Test]
    [TestCase (
        //language=C#
        @"private Mock<IA> a,b,c,d;",
        //language=C#
        @"private Mock<IA> a, b, c, d;")]
    [TestCase (
        //language=C#
        @"private Mock<IA> a;",
        //language=C#
        @"private Mock<IA> a;")]
    [TestCase (
        //language=C#
        @"private Mock<IA>a,
    b,
    c;",
        //language=C#
        @"private Mock<IA>
    a,
    b,
    c;")]
    public void Format_FieldDeclaration (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileFieldDeclaration (source, true);
      var formattedNode = _formatter.Format (node);

      Assert.AreEqual (expected, formattedNode.ToString());
    }

    [Test]
    [TestCase (
        //language=C#
        @"mock.Setup (_ => _.DoSomething()).Returns (3).Verifiable();",
        //language=C#
        @"mock.Setup (_ => _.DoSomething()).Returns (3).Verifiable();")]
    [TestCase (
        //language=C#
        @"mock.Setup (_ => _.DoSomething())
    .Returns (3).Verifiable();",
        //language=C#
        @"mock.Setup (_ => _.DoSomething())
    .Returns (3)
    .Verifiable();")]
    [TestCase (
        //language=C#
        @"mock.Setup(_ => _.DoSomething (
      1,
      2)).Returns (3).Verifiable();",
        //language=C#
        @"mock.Setup (_ => _.DoSomething (
      1,
      2)).Returns (3).Verifiable();")]
    [TestCase (
        //language=C#
        @"mock.Setup (_ => _.DoSomething (
      1,
      2)).Returns (3).Verifiable();",
        //language=C#
        @"mock.Setup (_ => _.DoSomething (
      1,
      2)).Returns (3).Verifiable();")]
    [TestCase (
        //language=C#
        @"mock.Setup (_ => _.DoSomething (1, 2))
    .Returns (3).Verifiable();",
        //language=C#
        @"mock.Setup (_ => _.DoSomething (1, 2))
    .Returns (3)
    .Verifiable();")]
    public void Format_ExpressionStatement (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileExpressionStatement (source, true);
      var formattedNode = _formatter.Format (node);

      Assert.AreEqual (expected, formattedNode.ToString());
    }

    [Test]
    [TestCase (
        //language=C#
        @"[Test]
private void Test()
{

  Assert.Pass();

}",
        //language=C#
        @"[Test]
private void Test()
{
  Assert.Pass();
}")]
    [TestCase (
        //language=C#
        @"[Test]
private void Test()
{
}",
        //language=C#
        @"[Test]
private void Test()
{
}")]
    [TestCase (
        //language=C#
        @"[Test]
private void Test()
{
  Console.WriteLine(""abc"");
  Console.WriteLine(""abc"");

  Console.WriteLine(""abc"");
  Console.WriteLine(""abc"");


  Console.WriteLine(""abc"");



  Console.WriteLine(""abc"");
  Assert.Pass();
}",
        //language=C#
        @"[Test]
private void Test()
{
  Console.WriteLine(""abc"");
  Console.WriteLine(""abc"");

  Console.WriteLine(""abc"");
  Console.WriteLine(""abc"");

  Console.WriteLine(""abc"");

  Console.WriteLine(""abc"");
  Assert.Pass();
}")]
    [TestCase (
        //language=C#
        @"[Test]
private void Test()
{


}",
        //language=C#
        @"[Test]
private void Test()
{
}")]
    public void Format_MethodDeclarationStatement (string source, string expected)
    {
      var (_, node) = CompiledSourceFileProvider.CompileMethodDeclaration (source, true);
      var formattedNode = _formatter.Format (node);

      Assert.AreEqual (expected, formattedNode.ToString());
    }
  }
}