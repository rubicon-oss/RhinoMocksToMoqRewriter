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
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ConstraintStrategies;
namespace RhinoMocksToMoqRewriter.Tests.Rewriters.Strategies.ConstraintStrategies
{
  [TestFixture]
  public class ConstraintRewriteStrategyFactoryTests
  {
    private readonly Context _context =
        new Context()
        {
            //language=C#
            UsingContext =
                @"
using Rhino.Mocks.Constraints;",
            //language=C#
            InterfaceContext =
                @"
public int A { get; set; }
void DoSomething (int a);
void DoSomething (int[] b);
void DoSomething (ITestInterface c);",
            //language=C#
            ClassContext =
                @"
private ITestInterface _mock = MockRepository.GenerateMock<ITestInterface>();"
        };

    [Test]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg.Is (4));",
        typeof (DefaultConstraintRewriteStrategy))]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Equal (2)));",
        typeof (IsEqualConstraintRewriteStrategy))]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.NotEqual (2)));",
        typeof (IsNotEqualConstraintRewriteStrategy))]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Same (2)));",
        typeof (IsSameConstraintRewriteStrategy))]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.NotSame (2)));",
        typeof (IsNotSameConstraintRewriteStrategy))]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.GreaterThan (2)));",
        typeof (IsGreaterThanConstraintRewriteStrategy))]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.GreaterThanOrEqual (2)));",
        typeof (IsGreaterThanOrEqualConstraintRewriteStrategy))]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.LessThan (2)));",
        typeof (IsLessThanConstraintRewriteStrategy))]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.LessThanOrEqual (2)));",
        typeof (IsLessThanOrEqualConstraintRewriteStrategy))]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.Null()));",
        typeof (IsNullConstraintRewriteStrategy))]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int>.Matches (Rhino.Mocks.Constraints.Is.NotNull()));",
        typeof (IsNotNullConstraintRewriteStrategy))]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<ITestInterface>.Matches (Rhino.Mocks.Constraints.Property.Value(""A"", 42)));",
        typeof (PropertyValueConstraintRewriteStrategy))]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int[]>.Matches (Rhino.Mocks.Constraints.List.IsIn (2)));",
        typeof (ListIsInConstraintRewriteStrategy))]
    [TestCase (
        //language=C#
        @"_mock.DoSomething (Arg<int[]>.Matches (Rhino.Mocks.Constraints.List.ContainsAll (new[] {1, 2})));",
        typeof (ListContainsAllConstraintRewriteStrategy))]
    public void GetRewriteStrategy (string source, Type expectedType)
    {
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var expression = node.GetFirstArgument().Expression.GetFirstArgument().Expression;
      var rewriteStrategy = ConstraintRewriteStrategyFactory.GetRewriteStrategy (expression!, model);
      var actualType = rewriteStrategy.GetType();

      Assert.AreEqual (expectedType, actualType);
    }
  }
}