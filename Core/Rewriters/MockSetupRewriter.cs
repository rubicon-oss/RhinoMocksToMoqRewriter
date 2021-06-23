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
        baseCallNode = baseCallNode.WithExpression (MoqSyntaxFactory.VerifiableMock (baseCallNode.Expression));
      }

      if (NeedsAdditionalAnnotations (originalNode))
      {
        baseCallNode = baseCallNode.WithAdditionalAnnotations (CreateAnnotation (originalNode, baseCallNode));
      }

      if (node.IsEquivalentTo (baseCallNode, false))
      {
        return baseCallNode.WithLeadingAndTrailingTriviaOfNode (node);
      }

      return _formatter.Format (
              baseCallNode.WithExpression (
                  baseCallNode
                      .WithLeadingTrivia (node.GetLeadingTrivia())
                      .WithTrailingTrivia (SyntaxFactory.Whitespace (Environment.NewLine))
                      .Expression))
          .WithAdditionalAnnotations (
              baseCallNode.GetAnnotations (new[] { "Id", MoqSyntaxFactory.VerifyAnnotationKind }));
    }

    private InvocationExpressionSyntax TransformSetupExpression (InvocationExpressionSyntax invocationExpression)
    {
      var identifierName = invocationExpression.GetFirstIdentifierName();
      invocationExpression = invocationExpression.ReplaceNode (
          identifierName,
          MoqSyntaxFactory.ProtectedMock (identifierName));

      if (invocationExpression.GetLambdaExpressionOrDefault()?.Body is not InvocationExpressionSyntax innerInvocationExpression)
      {
        return invocationExpression;
      }

      var argumentlist = innerInvocationExpression.ArgumentList.Arguments.Skip (1).ToList();
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
          _ when RhinoMocksSymbols.AllCallbackSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => RewriteCallback (baseCallNode).WithLeadingAndTrailingTriviaOfNode (baseCallNode),
          _ when RhinoMocksSymbols.ReturnSymbols.Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default) && ContainsSingleNullOrDefaultArgument (baseCallNode.ArgumentList)
              => baseCallNode.WithArgumentList (RewriteArgumentList (symbol!, baseCallNode.ArgumentList)).WithLeadingAndTrailingTriviaOfNode (baseCallNode),
          _ => baseCallNode
      };
    }

    private IdentifierNameSyntax RewriteSetup (MemberAccessExpressionSyntax node)
    {
      var originalNode = node.GetOriginalNode (node, CompilationId)!;
      var setupInvocationExpression = (InvocationExpressionSyntax) originalNode.Ancestors().First (s => s.IsKind (SyntaxKind.InvocationExpression));
      return setupInvocationExpression.ArgumentList.GetLambdaExpressionOrDefault()?.Body is AssignmentExpressionSyntax
          ? MoqSyntaxFactory.SetupSetIdentifierName
          : MoqSyntaxFactory.SetupIdentifierName;
    }

    private static bool ContainsSingleNullOrDefaultArgument (ArgumentListSyntax argumentList)
    {
      var argument = argumentList.Arguments.Single();
      return argument.IsEquivalentTo (MoqSyntaxFactory.NullArgument(), false)
             || argument.IsEquivalentTo (MoqSyntaxFactory.DefaultArgument(), false);
    }

    private ArgumentListSyntax RewriteArgumentList (ISymbol methodSymbol, ArgumentListSyntax argumentList)
    {
      var argument = argumentList.Arguments.Single();
      var returnType = methodSymbol.ContainingType.TypeArguments.Single();
      try
      {
        var type = TypeSymbolToTypeSyntaxConverter.ConvertTypeSyntaxNodes (returnType, Generator);
        return MoqSyntaxFactory.SimpleArgumentList (
            argument.WithExpression (MoqSyntaxFactory.CastExpression (type, argument.Expression)).WithLeadingAndTrailingTriviaOfNode (argument));
      }
      catch (Exception)
      {
        return argumentList;
      }
    }

    private InvocationExpressionSyntax RewriteCallback (InvocationExpressionSyntax node)
    {
      var originalNode = node.GetOriginalNode (node, CompilationId)!;
      var argument = GetFirstArgumentFromExpectOrStubMethod (originalNode);
      if (argument is null)
      {
        return node;
      }

      List<(TypeSyntax type, IdentifierNameSyntax identifierName)>? parameterTypesAndNames = null!;
      try
      {
        parameterTypesAndNames =
            argument.Expression is not LambdaExpressionSyntax { Body: InvocationExpressionSyntax mockedExpression }
                ? null
                : GetParameterTypesAndNames (mockedExpression)?.ToList();
      }
      catch (Exception)
      {
        return node;
      }

      var parameterList = MoqSyntaxFactory.ParameterList (parameterTypesAndNames);
      var callbackArgument = originalNode.ArgumentList.GetFirstArgumentOrDefault();
      if (callbackArgument?.Expression is not LambdaExpressionSyntax callbackLambdaExpression)
      {
        return node;
      }

      var parameter = callbackArgument.DescendantNodes().First (s => s.IsKind (SyntaxKind.Parameter));
      if (callbackLambdaExpression.Body is not AssignmentExpressionSyntax assignmentExpression || parameterTypesAndNames is null)
      {
        return node.WithArgumentList (
            MoqSyntaxFactory.SimpleArgumentList (
                    MoqSyntaxFactory.ParenthesizedLambdaExpression (
                            parameterList.WithLeadingTrivia (parameter.GetLeadingTrivia()),
                            callbackLambdaExpression.Body.WithLeadingTrivia (parameter.GetLeadingTrivia()))
                        .WithArrowToken (callbackLambdaExpression.ArrowToken))
                .WithOpenParenToken (node.ArgumentList.OpenParenToken)
                .WithCloseParenToken (node.ArgumentList.CloseParenToken)
                .WithLeadingAndTrailingTriviaOfNode (node.ArgumentList));
      }

      if (assignmentExpression.Right.DescendantNodesAndSelf().SingleOrDefault (s => s.IsKind (SyntaxKind.CastExpression)) is not CastExpressionSyntax castExpression)
      {
        return node;
      }

      var index = (int) ((LiteralExpressionSyntax) castExpression.Expression.GetFirstArgument().Expression).Token.Value!;
      var newAssignmentExpression = assignmentExpression.ReplaceNode (
          assignmentExpression.IsEquivalentTo (castExpression.Parent!, false)
              ? castExpression
              : castExpression.Parent!,
          parameterTypesAndNames.ToList()[index].identifierName);

      return node.WithArgumentList (
          MoqSyntaxFactory.SimpleArgumentList (
                  SyntaxFactory.ParenthesizedLambdaExpression (
                      parameterList.WithLeadingTrivia (parameter.GetLeadingTrivia()),
                      newAssignmentExpression.WithLeadingTrivia (SyntaxFactory.Space))).WithOpenParenToken (node.ArgumentList.OpenParenToken)
              .WithOpenParenToken (node.ArgumentList.OpenParenToken)
              .WithCloseParenToken (node.ArgumentList.CloseParenToken)
              .WithLeadingAndTrailingTriviaOfNode (node.ArgumentList));
    }

    private ArgumentSyntax? GetFirstArgumentFromExpectOrStubMethod (InvocationExpressionSyntax node)
    {
      return node.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.InvocationExpression))
          .Select (s => (InvocationExpressionSyntax) s)
          .FirstOrDefault (
              s => Model.GetSymbolInfo (s).Symbol is IMethodSymbol symbol
                   && (RhinoMocksSymbols.ExpectSymbols.Contains (symbol.ReducedFrom ?? symbol.OriginalDefinition, SymbolEqualityComparer.Default)
                       || RhinoMocksSymbols.StubSymbols.Contains (symbol.ReducedFrom ?? symbol.OriginalDefinition, SymbolEqualityComparer.Default)))?
          .ArgumentList
          .GetFirstArgumentOrDefault();
    }

    private IEnumerable<(TypeSyntax, IdentifierNameSyntax)>? GetParameterTypesAndNames (InvocationExpressionSyntax node)
    {

      return Model.GetSymbolInfo (node).Symbol is not IMethodSymbol symbol
          ? null
          : symbol.Parameters.Select (s => (TypeSymbolToTypeSyntaxConverter.ConvertTypeSyntaxNodes (s.Type, Generator), SyntaxFactory.IdentifierName (s.Name)));

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
              => node.WithName (RewriteSetup (node)).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when RhinoMocksSymbols.StubSymbols.Contains (methodSymbol?.ReducedFrom, SymbolEqualityComparer.Default)
              => node.WithName (RewriteSetup (node)).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when RhinoMocksSymbols.AllCallbackSymbols.Contains (methodSymbol?.OriginalDefinition, SymbolEqualityComparer.Default)
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