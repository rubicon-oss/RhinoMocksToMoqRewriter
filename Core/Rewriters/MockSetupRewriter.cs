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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Extensions;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class MockSetupRewriter : RewriterBase
  {
    private readonly IFormatter _formatter;

    public MockSetupRewriter (IFormatter formatter)
    {
      _formatter = formatter;
    }

    public override SyntaxNode? VisitExpressionStatement (ExpressionStatementSyntax node)
    {
      var trackedNodes = TrackDescendantNodes (node);
      var baseCallNode = (ExpressionStatementSyntax) base.VisitExpressionStatement (trackedNodes)!;

      var originalNode = baseCallNode.GetOriginalNode (baseCallNode, CompilationId)!;
      if (NeedsProtectedExpression (originalNode))
      {
        baseCallNode = baseCallNode.WithExpression (TransformSetupExpression ((InvocationExpressionSyntax) baseCallNode.Expression));
      }

      if (NeedsVerifiableExpression (originalNode))
      {
        baseCallNode = _formatter.Format (baseCallNode.WithExpression (MoqSyntaxFactory.VerifiableMock (baseCallNode.Expression)));
      }

      if (NeedsAdditionalAnnotations (originalNode))
      {
        baseCallNode = baseCallNode.WithAdditionalAnnotations (CreateAnnotation (originalNode, baseCallNode));
      }

      return baseCallNode.WithLeadingTrivia (node.GetLeadingTrivia()).WithTrailingTrivia (SyntaxFactory.Whitespace (Environment.NewLine));
    }

    private InvocationExpressionSyntax TransformSetupExpression (InvocationExpressionSyntax invocationExpression)
    {
      var identifierName = invocationExpression.GetFirstIdentifierName();
      invocationExpression = invocationExpression.ReplaceNode (
          identifierName,
          MoqSyntaxFactory.ProtectedMock (identifierName));

      var argumentlist = ((InvocationExpressionSyntax) invocationExpression.GetLambdaExpression().Body).ArgumentList.Arguments.Skip (1).ToList();
      argumentlist.Insert (1, MoqSyntaxFactory.SimpleArgument (MoqSyntaxFactory.TrueLiteralExpression));

      invocationExpression = invocationExpression.GetCurrentNode (invocationExpression, CompilationId)!.ReplaceNode (
          invocationExpression.GetLambdaExpression().Parent?.Parent!,
          _formatter.Format (MoqSyntaxFactory.SimpleArgumentList (argumentlist)));

      return invocationExpression;
    }

    public override SyntaxNode? VisitMemberAccessExpression (MemberAccessExpressionSyntax node)
    {
      var trackedNodes = TrackDescendantNodes (node);
      var baseCallNode = (MemberAccessExpressionSyntax) base.VisitMemberAccessExpression (trackedNodes)!;

      return ConvertMemberAccessExpression (baseCallNode);
    }

    public override SyntaxNode? VisitInvocationExpression (InvocationExpressionSyntax node)
    {
      var trackedNodes = TrackDescendantNodes (node);
      var baseCallNode = (InvocationExpressionSyntax) base.VisitInvocationExpression (trackedNodes)!;

      return ConvertInvocationExpression (baseCallNode);
    }

    private SyntaxNode? ConvertInvocationExpression (InvocationExpressionSyntax baseCallNode)
    {
      var symbol = Model.GetSymbolInfo (baseCallNode.GetOriginalNode (baseCallNode, CompilationId)!).Symbol as IMethodSymbol;
      return symbol switch
      {
          _ when RhinoMocksSymbols.AllIRepeatSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => ((MemberAccessExpressionSyntax) baseCallNode.Expression).Expression,
          _ when RhinoMocksSymbols.ExpectSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => RewriteStaticExpression (baseCallNode).WithLeadingAndTrailingTriviaOfNode (baseCallNode),
          _ when RhinoMocksSymbols.StubSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => RewriteStaticExpression (baseCallNode).WithLeadingAndTrailingTriviaOfNode (baseCallNode),
          _ => baseCallNode
      };
    }

    private SyntaxNode? ConvertMemberAccessExpression (MemberAccessExpressionSyntax node)
    {
      var symbol = Model.GetSymbolInfo (node.GetOriginalNode (node, CompilationId)!.Name).Symbol;
      if (symbol is not IMethodSymbol methodSymbol)
      {
        return RhinoMocksSymbols.RhinoMocksIRepeatSymbol.Equals (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
            ? node.Expression
            : node;
      }

      return symbol switch
      {
          _ when RhinoMocksSymbols.ExpectSymbols.Contains (methodSymbol?.ReducedFrom, SymbolEqualityComparer.Default)
              => node.WithName (MoqSyntaxFactory.SetupIdentifierName).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when RhinoMocksSymbols.StubSymbols.Contains (methodSymbol?.ReducedFrom, SymbolEqualityComparer.Default)
              => node.WithName (MoqSyntaxFactory.SetupIdentifierName).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when RhinoMocksSymbols.WhenCalledSymbols.Contains (methodSymbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => node.WithName (MoqSyntaxFactory.CallbackIdentifierName).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when RhinoMocksSymbols.DoSymbols.Contains (methodSymbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => node.WithName (MoqSyntaxFactory.CallbackIdentifierName).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when RhinoMocksSymbols.ReturnSymbols.Contains (methodSymbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => node.WithName (MoqSyntaxFactory.ReturnsIdentifierName).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when RhinoMocksSymbols.ThrowSymbols.Contains (methodSymbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => node.WithName (MoqSyntaxFactory.ThrowsIdentifierName).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when RhinoMocksSymbols.AllIRepeatSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => node.Expression,
          _ => node
      };
    }

    private SyntaxAnnotation CreateAnnotation (ExpressionStatementSyntax originalNode, ExpressionStatementSyntax currentNode)
    {
      var symbol = Model.GetSymbolInfo (originalNode.Expression).Symbol?.OriginalDefinition;
      return symbol switch
      {
          _ when RhinoMocksSymbols.RepeatNeverSymbols.Contains (symbol, SymbolEqualityComparer.Default)
              => MoqSyntaxFactory.VerifyAnnotation (currentNode, 0),
          _ when RhinoMocksSymbols.RepeatOnceSymbols.Contains (symbol, SymbolEqualityComparer.Default)
              => MoqSyntaxFactory.VerifyAnnotation (currentNode, 1),
          _ when RhinoMocksSymbols.RepeatTwiceSymbols.Contains (symbol, SymbolEqualityComparer.Default)
              => MoqSyntaxFactory.VerifyAnnotation (currentNode, 2),
          _ when RhinoMocksSymbols.RepeatAtLeastOnceSymbols.Contains (symbol, SymbolEqualityComparer.Default)
              => MoqSyntaxFactory.VerifyAnnotation (currentNode, -1),
          _ when RhinoMocksSymbols.RepeatTimesSymbols.Contains (symbol, SymbolEqualityComparer.Default)
              => MoqSyntaxFactory.VerifyAnnotation (currentNode, GetTimesValue (originalNode)),
          _ => throw new InvalidOperationException ("Unable to resolve symbol")
      };
    }

    private static string GetTimesValue (ExpressionStatementSyntax originalNode)
    {
      if (originalNode.Expression is not InvocationExpressionSyntax invocationExpression)
      {
        throw new InvalidOperationException ("Unable to resolve times value.");
      }

      return invocationExpression.ArgumentList.Arguments.Count switch
      {
          1 when invocationExpression.ArgumentList.Arguments.Single().Expression is LiteralExpressionSyntax { Token: { Value: int times } } => times.ToString(),
          2 when invocationExpression.ArgumentList.Arguments.First().Expression is LiteralExpressionSyntax { Token: { Value: int min } }
                 && invocationExpression.ArgumentList.Arguments.Last().Expression is LiteralExpressionSyntax { Token: { Value: int max } } => $"{min}:{max}",
          _ => throw new InvalidOperationException ("Unable to resolve Repeat.Times")
      };
    }

    private bool NeedsAdditionalAnnotations (ExpressionStatementSyntax node)
    {
      var symbol = Model.GetSymbolInfo (node.Expression).Symbol;
      return RhinoMocksSymbols.RhinoMocksIRepeatSymbol.GetMembers().Where (s => s.Name != "Any").Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default);
    }

    private static InvocationExpressionSyntax RewriteStaticExpression (InvocationExpressionSyntax node)
    {
      var mockIdentifierName = (IdentifierNameSyntax) node.GetFirstArgument().Expression;
      var mockedExpression = (LambdaExpressionSyntax) node.ArgumentList.Arguments.Last().Expression;

      return MoqSyntaxFactory.SetupExpression (mockIdentifierName, mockedExpression);
    }

    private bool NeedsVerifiableExpression (ExpressionStatementSyntax node)
    {
      if (node.Expression is not InvocationExpressionSyntax invocationExpression)
      {
        return false;
      }

      return invocationExpression.Expression.DescendantNodesAndSelf().Any (
          s => Model.GetSymbolInfo (s).Symbol is IMethodSymbol symbol
               && RhinoMocksSymbols.ExpectSymbols.Contains (symbol.ReducedFrom ?? symbol.OriginalDefinition, SymbolEqualityComparer.Default));
    }

    private T TrackDescendantNodes<T> (T node)
        where T : SyntaxNode
    {
      return node.TrackNodes (
          node.DescendantNodesAndSelf().Where (
              s => s.IsKind (SyntaxKind.InvocationExpression) || s.IsKind (SyntaxKind.ExpressionStatement) || s.IsKind (SyntaxKind.SimpleMemberAccessExpression)),
          CompilationId);
    }

    private static bool NeedsProtectedExpression (SyntaxNode originalNode)
    {
      var argument = originalNode.GetFirstArgumentOrDefault();
      return argument is not null && argument.ToString().Contains ("PrivateInvoke.InvokeNonPublicMethod");
    }
  }
}