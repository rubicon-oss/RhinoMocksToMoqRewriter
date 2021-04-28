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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using NUnit.Framework;
using RhinoMocksToMoqRewriter.Core.Rewriters;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters
{
  [TestFixture]
  public class IgnoreArgumentsRewriterTests
  {
    private IgnoreArgumentsRewriter _rewriter;

    private readonly Context _context =
        new Context
        {
            //language=C#
            UsingContext =
                @"
using Moq;
using MockRepository = Rhino.Mocks.MockRepository;
#nullable enable",
            //language=C#
            NamespaceContext =
                @"
public interface IA
{
  int DoSomething();
}

public class A : IA
{
  public int DoSomething()
  {
    throw new System.NotImplementedException();
  }
}",
            //language=C#
            InterfaceContext = @"
int DoSomething (Func<int> i);
int DoSomething (int i);
int DoSomething (Func<int> i, Func<bool> b);
int DoSomething (Func<int> i, Func<bool> b, string s);
T DoSomething<T> (Func<T> func);
int DoSomething (Func<int, string> func);
int DoSomething (Func<Func<int>, string> func);
int DoSomething();
int DoSomething (int? a);
int DoSomething (int? a, int? b);
int DoSomething (Func<int?, bool?> f);
int DoSomething (List<IA> a);
int DoSomething (string a, object[] args);",
            //language=C#
            ClassContext =
                @"
private void DoSomething (Action a) => throw new NotImplementedException();
private static ITestInterface _mock = MockRepository.GenerateMock<ITestInterface>();",
        };

    [SetUp]
    public void Setup ()
    {
      _rewriter = new IgnoreArgumentsRewriter();
      _rewriter.Generator = SyntaxGenerator.GetGenerator (new AdhocWorkspace(), "C#");
    }

    [Test]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething ((Func<int>)null))
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (It.IsAny<Func<int>>()))
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething ((Func<int>)null, (Func<bool>)null))
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (It.IsAny<Func<int>>(), It.IsAny<Func<bool>>()))
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (null, null, """"))
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (It.IsAny<Func<int>>(), It.IsAny<Func<bool>>(), It.IsAny<string>()))
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
_mock.DoSomething (
  () => 
  {
    _mock
      .Expect (m => m.DoSomething (null, null, null))
      .IgnoreArguments()
      .Return (1);
    return _mock;
   });",
        //language=C#
        @"
_mock.DoSomething (
  () => 
  {
    _mock
      .Expect (m => m.DoSomething (It.IsAny<Func<int>>(), It.IsAny<Func<bool>>(), It.IsAny<string>()))
      .Return (1);
    return _mock;
  });")]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething ((Func<int, string>)null))
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (It.IsAny<Func<int, string>>()))
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (0))
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (It.IsAny<int>()))
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (0))
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (0))
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
Rhino.Mocks.RhinoMocksExtensions.Expect(_mock, m => m.DoSomething (0)).Return (1);",
        //language=C#
        @"
Rhino.Mocks.RhinoMocksExtensions.Expect(_mock, m => m.DoSomething (0)).Return (1);")]
    [TestCase (
        //language=C#
        @"
Rhino.Mocks.RhinoMocksExtensions.Expect(_mock, m => m.DoSomething (0))
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock.Expect (m => m.DoSomething (It.IsAny<int>()))
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething ((Func<Func<int>, string>)null))
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (It.IsAny<Func<Func<int>, string>>()))
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething())
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething())
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething ((int?)null))
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (It.IsAny<int?>()))
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething ((int?)null, (int?)null))
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (It.IsAny<int?>(), It.IsAny<int?>()))
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething ((Func<int?, bool?>)null))
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (It.IsAny<Func<int?, bool?>>()))
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
DoSomething (delegate
  {
    _mock
      .Expect (m => m.DoSomething (0))
      .IgnoreArguments()
      .Return (1);
  });",
        //language=C#
        @"
DoSomething (delegate
  {
    _mock
      .Expect (m => m.DoSomething (It.IsAny<int>()))
      .Return (1);
  });")]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething ((Func<int?, bool?>)null))
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (It.IsAny<Func<int?, bool?>>()))
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething ((List<IA>)null))
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (It.IsAny<List<IA>>()))
  .Return (1);")]
    [TestCase (
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething ((string)null, (object[])null))
  .IgnoreArguments()
  .Return (1);",
        //language=C#
        @"
_mock
  .Expect (m => m.DoSomething (It.IsAny<string>(), It.IsAny<object[]>()))
  .Return (1);")]
    public void Rewrite_IgnoreArguments (string source, string expected)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var (_, expectedNode) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (expected, _context);
      _rewriter.Model = model;
      var actualNode = node.Accept (_rewriter);

      Assert.NotNull (actualNode);
      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }
  }
}