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
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Extensions;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class ExpectCallRewriter : RewriterBase
  {
    public override SyntaxNode? VisitExpressionStatement (ExpressionStatementSyntax node)
    {
      var trackedNodes = node.TrackNodes (node.DescendantNodesAndSelf(), CompilationId);
      var baseCallNode = (ExpressionStatementSyntax) base.VisitExpressionStatement (trackedNodes)!;

      var rhinoMocksIMethodOptionsSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Interfaces.IMethodOptions`1");
      var rhinoMocksExpectSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Expect");
      var rhinoMocksIRepeatSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Interfaces.IRepeat`1");
      if (rhinoMocksIMethodOptionsSymbol == null || rhinoMocksExpectSymbol == null || rhinoMocksIRepeatSymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var callSymbols = rhinoMocksExpectSymbol.GetMembers ("Call");
      var constraintsSymbols = rhinoMocksIMethodOptionsSymbol.GetMembers ("Constraints");
      var simpleRhinoMocksSymbols = GetAllSimpleRhinoMocksSymbols (rhinoMocksIMethodOptionsSymbol, rhinoMocksIRepeatSymbol).ToList();

      return RewriteExpectCall (baseCallNode, callSymbols, simpleRhinoMocksSymbols, constraintsSymbols).WithLeadingTrivia (node.GetLeadingTrivia());
    }

    private SyntaxNode RewriteExpectCall (
        SyntaxNode node,
        IReadOnlyList<ISymbol> callSymbols,
        IReadOnlyList<ISymbol> simpleRhinoMocksSymbols,
        IReadOnlyList<ISymbol> constraintsSymbols)
    {
      var symbol = Model.GetSymbolInfo (node.GetOriginalNode (node, CompilationId)!).Symbol?.OriginalDefinition;

      if (node is ExpressionStatementSyntax expressionStatement)
      {
        return expressionStatement.WithExpression (
            (ExpressionSyntax) RewriteExpectCall (expressionStatement.Expression, callSymbols, simpleRhinoMocksSymbols, constraintsSymbols));
      }

      if (node is MemberAccessExpressionSyntax rhinoMocksRepeatMemberAccessExpression && simpleRhinoMocksSymbols.Contains (symbol, SymbolEqualityComparer.Default))
      {
        return rhinoMocksRepeatMemberAccessExpression.ReplaceNode (
            node.GetCurrentNode (rhinoMocksRepeatMemberAccessExpression.Expression, CompilationId)!,
            (ExpressionSyntax) RewriteExpectCall (rhinoMocksRepeatMemberAccessExpression.Expression, callSymbols, simpleRhinoMocksSymbols, constraintsSymbols));
      }

      if (node is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax rhinoMocksMemberAccessExpression } rhinoMocksInvocationExpression)
      {
        return node;
      }

      if (callSymbols.Contains (symbol, SymbolEqualityComparer.Default))
      {
        return ConvertExpectExpression (rhinoMocksInvocationExpression);
      }

      if (constraintsSymbols.Contains (symbol, SymbolEqualityComparer.Default))
      {
        var rewrittenContainedExpression = RewriteExpectCall (rhinoMocksMemberAccessExpression.Expression, callSymbols, simpleRhinoMocksSymbols, constraintsSymbols);
        return ConvertMockedExpression (rewrittenContainedExpression, rhinoMocksMemberAccessExpression, rhinoMocksInvocationExpression);
      }

      if (simpleRhinoMocksSymbols.Contains (symbol, SymbolEqualityComparer.Default))
      {
        return rhinoMocksInvocationExpression.ReplaceNode (
            node.GetCurrentNode (rhinoMocksMemberAccessExpression.Expression, CompilationId)!,
            (ExpressionSyntax) RewriteExpectCall (rhinoMocksMemberAccessExpression.Expression, callSymbols, simpleRhinoMocksSymbols, constraintsSymbols));
      }

      return node;
    }

    private static IEnumerable<ISymbol> GetAllSimpleRhinoMocksSymbols (INamedTypeSymbol rhinoMocksIMethodOptionsSymbol, INamedTypeSymbol rhinoMocksIRepeatSymbol)
    {
      return rhinoMocksIMethodOptionsSymbol.GetMembers ("Return")
          .Concat (rhinoMocksIMethodOptionsSymbol.GetMembers ("WhenCalled"))
          .Concat (rhinoMocksIMethodOptionsSymbol.GetMembers ("Callback"))
          .Concat (rhinoMocksIMethodOptionsSymbol.GetMembers ("IgnoreArguments"))
          .Concat (rhinoMocksIMethodOptionsSymbol.GetMembers ("Do"))
          .Concat (rhinoMocksIMethodOptionsSymbol.GetMembers ("Repeat"))
          .Concat (rhinoMocksIMethodOptionsSymbol.GetMembers ("Throw"))
          .Concat (rhinoMocksIRepeatSymbol.GetMembers());
    }

    private SyntaxNode ConvertMockedExpression (
        SyntaxNode rewrittenContainedExpression,
        MemberAccessExpressionSyntax originalRhinoMocksMemberAccessExpression,
        InvocationExpressionSyntax originalRhinoMocksInvocationExpression)
    {
      var rewrittenInvocationExpression =
          (InvocationExpressionSyntax) ((SimpleLambdaExpressionSyntax) rewrittenContainedExpression.GetFirstArgument().Expression).ExpressionBody!;

      var originalContainedInvocationExpression = (InvocationExpressionSyntax) originalRhinoMocksMemberAccessExpression.Expression.GetFirstArgument().Expression;
      var containedInvocationExpressionParameterSymbols = GetMethodParameterTypes (originalContainedInvocationExpression).ToList();
      var rewrittenArgumentList = ConvertConstraints (originalRhinoMocksInvocationExpression.ArgumentList, containedInvocationExpressionParameterSymbols);

      return rewrittenContainedExpression.ReplaceNode (rewrittenInvocationExpression.ArgumentList, rewrittenArgumentList);
    }

    private static InvocationExpressionSyntax ConvertExpectExpression (InvocationExpressionSyntax rhinoMocksInvocationExpression)
    {
      var mockMethodCallExpression = rhinoMocksInvocationExpression.ArgumentList.Arguments.First().Expression;
      var firstIdentifierName = mockMethodCallExpression.GetFirstIdentifierName();

      var newExpression = mockMethodCallExpression.ReplaceNode (firstIdentifierName, MoqSyntaxFactory.LambdaParameterIdentifierName);

      return MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (firstIdentifierName, MoqSyntaxFactory.ExpectIdentifierName),
          MoqSyntaxFactory.SimpleArgumentList (MoqSyntaxFactory.SimpleLambdaExpression (newExpression!)));
    }

    private IEnumerable<ITypeSymbol> GetMethodParameterTypes (InvocationExpressionSyntax invocationExpression)
    {
      return ((IMethodSymbol) Model.GetSymbolInfo (invocationExpression.GetOriginalNode (invocationExpression, CompilationId)!).Symbol!).Parameters.Select (s => s.Type);
    }

    private ArgumentListSyntax ConvertConstraints (ArgumentListSyntax originalArgumentList, IReadOnlyList<ITypeSymbol> parameterTypes)
    {
      var convertedArguments = parameterTypes.Select ((s, i) => MoqSyntaxFactory.MatchesArgument (ConvertTypeSyntaxNodes (s), originalArgumentList.Arguments[i]));
      return MoqSyntaxFactory.SimpleArgumentList (convertedArguments);
    }

    private TypeSyntax ConvertTypeSyntaxNodes (ITypeSymbol typeSymbol)
    {
      if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
      {
        return (TypeSyntax) Generator.NullableTypeExpression (ConvertTypeSyntaxNodes (((INamedTypeSymbol) typeSymbol).TypeArguments.First()));
      }

      if (typeSymbol.SpecialType != SpecialType.None)
      {
        return (PredefinedTypeSyntax) Generator.TypeExpression (typeSymbol.SpecialType);
      }

      if (((INamedTypeSymbol) typeSymbol).TypeArguments.IsEmpty)
      {
        return SyntaxFactory.IdentifierName (typeSymbol.Name);
      }

      return MoqSyntaxFactory.GenericName (
          SyntaxFactory.Identifier (typeSymbol.Name),
          MoqSyntaxFactory.SimpleTypeArgumentList (((INamedTypeSymbol) typeSymbol).TypeArguments.Select (ConvertTypeSyntaxNodes)));
    }
  }
}