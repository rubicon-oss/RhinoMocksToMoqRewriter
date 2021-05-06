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
    public override SyntaxNode? VisitExpressionStatement (ExpressionStatementSyntax node)
    {
      var rhinoMocksExtensionsCompilationSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.RhinoMocksExtensions");
      var rhinoMocksIRepeatSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Interfaces.IRepeat`1");
      if (rhinoMocksExtensionsCompilationSymbol == null || rhinoMocksIRepeatSymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var expectSymbols = rhinoMocksExtensionsCompilationSymbol.GetMembers ("Expect");

      var trackedNodes = TrackDescendantNodes (node);
      var baseCallNode = (ExpressionStatementSyntax) base.VisitExpressionStatement (trackedNodes)!;

      var originalNode = baseCallNode.GetOriginalNode (baseCallNode, CompilationId)!;
      if (NeedsVerifiableExpression (originalNode, expectSymbols))
      {
        baseCallNode = baseCallNode.WithExpression (MoqSyntaxFactory.VerifiableMock (baseCallNode.Expression));
      }

      if (NeedsAdditionalAnnotations (originalNode, rhinoMocksIRepeatSymbol))
      {
        baseCallNode = baseCallNode.WithAdditionalAnnotations (CreateAnnotation (originalNode, baseCallNode, rhinoMocksIRepeatSymbol));
      }

      return baseCallNode;
    }

    public override SyntaxNode? VisitMemberAccessExpression (MemberAccessExpressionSyntax node)
    {
      var rhinoMocksExtensionsCompilationSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.RhinoMocksExtensions");
      var rhinoMocksIMethodOptionsSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Interfaces.IMethodOptions`1");
      var rhinoMocksIRepeatSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Interfaces.IRepeat`1");
      if (rhinoMocksExtensionsCompilationSymbol == null || rhinoMocksIMethodOptionsSymbol == null || rhinoMocksIRepeatSymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var trackedNodes = node.TrackNodes (
          node.DescendantNodesAndSelf().Where (
              s => s.IsKind (SyntaxKind.InvocationExpression) || s.IsKind (SyntaxKind.ExpressionStatement) || s.IsKind (SyntaxKind.IdentifierName)
                   || s.IsKind (SyntaxKind.SimpleMemberAccessExpression)),
          CompilationId);
      var baseCallNode = (MemberAccessExpressionSyntax) base.VisitMemberAccessExpression (trackedNodes)!;

      return ConvertMemberAccessExpression (baseCallNode, rhinoMocksExtensionsCompilationSymbol, rhinoMocksIMethodOptionsSymbol, rhinoMocksIRepeatSymbol);
    }

    public override SyntaxNode? VisitInvocationExpression (InvocationExpressionSyntax node)
    {
      var rhinoMocksExtensionsCompilationSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.RhinoMocksExtensions");
      var rhinoMocksIRepeatSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.Interfaces.IRepeat`1");
      if (rhinoMocksExtensionsCompilationSymbol == null || rhinoMocksIRepeatSymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var trackedNodes = TrackDescendantNodes (node);
      var baseCallNode = (InvocationExpressionSyntax) base.VisitInvocationExpression (trackedNodes)!;

      return ConvertInvocationExpression (baseCallNode, rhinoMocksIRepeatSymbol, rhinoMocksExtensionsCompilationSymbol);
    }

    private SyntaxNode? ConvertInvocationExpression (
        InvocationExpressionSyntax baseCallNode,
        INamedTypeSymbol rhinoMocksIRepeatSymbol,
        INamedTypeSymbol rhinoMocksExtensionsCompilationSymbol)
    {
      var symbol = Model.GetSymbolInfo (baseCallNode.GetOriginalNode (baseCallNode, CompilationId)!).Symbol as IMethodSymbol;
      return symbol switch
      {
          _ when rhinoMocksIRepeatSymbol.GetMembers().Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => ((MemberAccessExpressionSyntax) baseCallNode.Expression).Expression,
          _ when rhinoMocksExtensionsCompilationSymbol.GetMembers ("Expect").Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => RewriteStaticExpression (baseCallNode).WithLeadingAndTrailingTriviaOfNode (baseCallNode),
          _ when rhinoMocksExtensionsCompilationSymbol.GetMembers ("Stub").Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => RewriteStaticExpression (baseCallNode).WithLeadingAndTrailingTriviaOfNode (baseCallNode),
          _ => baseCallNode
      };
    }

    private SyntaxNode? ConvertMemberAccessExpression (
        MemberAccessExpressionSyntax node,
        INamedTypeSymbol rhinoMocksExtensionsCompilationSymbol,
        INamedTypeSymbol rhinoMocksIMethodOptionsSymbol,
        INamedTypeSymbol rhinoMocksIRepeatSymbol)
    {
      var symbol = Model.GetSymbolInfo (node.GetOriginalNode (node, CompilationId)!.Name).Symbol;
      if (symbol is not IMethodSymbol methodSymbol)
      {
        return rhinoMocksIRepeatSymbol.Equals (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
            ? node.Expression
            : node;
      }

      return symbol switch
      {
          _ when rhinoMocksExtensionsCompilationSymbol.GetMembers ("Expect").Contains (methodSymbol?.ReducedFrom, SymbolEqualityComparer.Default)
              => node.WithName (MoqSyntaxFactory.SetupIdentifierName).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when rhinoMocksExtensionsCompilationSymbol.GetMembers ("Stub").Contains (methodSymbol?.ReducedFrom, SymbolEqualityComparer.Default)
              => node.WithName (MoqSyntaxFactory.SetupIdentifierName).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when rhinoMocksIMethodOptionsSymbol.GetMembers ("WhenCalled").Contains (methodSymbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => node.WithName (MoqSyntaxFactory.CallbackIdentifierName).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when rhinoMocksIMethodOptionsSymbol.GetMembers ("Do").Contains (methodSymbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => node.WithName (MoqSyntaxFactory.CallbackIdentifierName).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when rhinoMocksIMethodOptionsSymbol.GetMembers ("Return").Contains (methodSymbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => node.WithName (MoqSyntaxFactory.ReturnsIdentifierName).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when rhinoMocksIMethodOptionsSymbol.GetMembers ("Throw").Contains (methodSymbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => node.WithName (MoqSyntaxFactory.ThrowsIdentifierName).WithLeadingAndTrailingTriviaOfNode (node.Name),
          _ when rhinoMocksIRepeatSymbol.GetMembers().Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default)
              => node.Expression,
          _ => node
      };
    }

    private SyntaxAnnotation CreateAnnotation (ExpressionStatementSyntax originalNode, ExpressionStatementSyntax currentNode, INamedTypeSymbol rhinoMocksIRepeatSymbol)
    {
      var symbol = Model.GetSymbolInfo (originalNode.Expression).Symbol?.OriginalDefinition;
      return symbol switch
      {
          _ when rhinoMocksIRepeatSymbol.GetMembers ("Never").Single().Equals (symbol, SymbolEqualityComparer.Default)
              => MoqSyntaxFactory.VerifyAnnotation (currentNode, 0),
          _ when rhinoMocksIRepeatSymbol.GetMembers ("Once").Single().Equals (symbol, SymbolEqualityComparer.Default)
              => MoqSyntaxFactory.VerifyAnnotation (currentNode, 1),
          _ when rhinoMocksIRepeatSymbol.GetMembers ("Twice").Single().Equals (symbol, SymbolEqualityComparer.Default)
              => MoqSyntaxFactory.VerifyAnnotation (currentNode, 2),
          _ when rhinoMocksIRepeatSymbol.GetMembers ("AtLeastOnce").Single().Equals (symbol, SymbolEqualityComparer.Default)
              => MoqSyntaxFactory.VerifyAnnotation (currentNode, -1),
          _ when rhinoMocksIRepeatSymbol.GetMembers ("Times").Contains (symbol, SymbolEqualityComparer.Default)
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

    private bool NeedsAdditionalAnnotations (ExpressionStatementSyntax node, INamedTypeSymbol rhinoMocksIRepeatSymbol)
    {
      var symbol = Model.GetSymbolInfo (node.Expression).Symbol;
      return rhinoMocksIRepeatSymbol.GetMembers().Where (s => s.Name != "Any").Contains (symbol?.OriginalDefinition, SymbolEqualityComparer.Default);
    }

    private static InvocationExpressionSyntax RewriteStaticExpression (InvocationExpressionSyntax node)
    {
      var mockIdentifierName = (IdentifierNameSyntax) node.GetFirstArgument().Expression;
      var mockedExpression = (LambdaExpressionSyntax) node.ArgumentList.Arguments.Last().Expression;

      return MoqSyntaxFactory.SetupExpression (mockIdentifierName, mockedExpression);
    }

    private bool NeedsVerifiableExpression (ExpressionStatementSyntax node, IReadOnlyList<ISymbol> expectSymbols)
    {
      if (node.Expression is not InvocationExpressionSyntax invocationExpression)
      {
        return false;
      }

      return invocationExpression.Expression.DescendantNodesAndSelf().Any (
          s => Model.GetSymbolInfo (s).Symbol is IMethodSymbol symbol
               && expectSymbols.Contains (symbol.ReducedFrom ?? symbol.OriginalDefinition, SymbolEqualityComparer.Default));
    }

    private T TrackDescendantNodes<T> (T node)
        where T : SyntaxNode
    {
      return node.TrackNodes (
          node.DescendantNodesAndSelf().Where (
              s => s.IsKind (SyntaxKind.InvocationExpression) || s.IsKind (SyntaxKind.ExpressionStatement) || s.IsKind (SyntaxKind.SimpleMemberAccessExpression)),
          CompilationId);
    }
  }
}