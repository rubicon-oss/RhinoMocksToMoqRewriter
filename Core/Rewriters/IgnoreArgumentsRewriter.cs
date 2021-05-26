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
  public class IgnoreArgumentsRewriter : RewriterBase
  {
    public override SyntaxNode? VisitExpressionStatement (ExpressionStatementSyntax node)
    {
      var baseCallNode = (ExpressionStatementSyntax) base.VisitExpressionStatement (node)!;

      if (!ContainsIgnoreArgumentsMethod (node))
      {
        return baseCallNode;
      }

      var rhinoMocksExpressions = GetAllRhinoMocksExpressions (baseCallNode).ToList();
      var mockIdentifierName = rhinoMocksExpressions.Any (s => s.ArgumentList.Arguments.Count > 1)
          ? ExtractIdentifierNameFromArguments (rhinoMocksExpressions)
          : baseCallNode.GetFirstIdentifierName();

      try
      {
        var expressionsWithModifiedArgumentLists = rhinoMocksExpressions.Select (s => (s.Name, ConvertArgumentList (s.ArgumentList))).ToList();
        return MoqSyntaxFactory.ExpressionStatement (MoqSyntaxFactory.NestedMemberAccessExpression (mockIdentifierName, expressionsWithModifiedArgumentLists))
            .WithLeadingAndTrailingTriviaOfNode (node);
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine (
            $"  WARNING: Unable to convert .IgnoreArguments\r\n"
            + $"  {node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}\r\n"
            + ex);

        return baseCallNode;
      }
    }

    private IdentifierNameSyntax ExtractIdentifierNameFromArguments (IEnumerable<(SimpleNameSyntax Name, ArgumentListSyntax ArgumentList)> expressions)
    {
      return expressions
          .Where (
              s => s.ArgumentList.Arguments.Count > 1
                   && Model.GetSymbolInfo (s.Name).Symbol is { } symbol
                   && (RhinoMocksSymbols.ExpectSymbols.Contains (symbol.OriginalDefinition, SymbolEqualityComparer.Default)
                       || RhinoMocksSymbols.StubSymbols.Contains (symbol.OriginalDefinition, SymbolEqualityComparer.Default)))
          .Select (s => (IdentifierNameSyntax) s.ArgumentList.Arguments.First().Expression)
          .First();
    }

    private bool ContainsIgnoreArgumentsMethod (ExpressionStatementSyntax node)
    {
      return node.DescendantNodesAndSelf()
          .Any (
              s => Model.GetSymbolInfo (s).Symbol is IMethodSymbol symbol
                   && RhinoMocksSymbols.IgnoreArgumentsSymbols.Contains (symbol.OriginalDefinition, SymbolEqualityComparer.Default));
    }

    private IEnumerable<(SimpleNameSyntax Name, ArgumentListSyntax ArgumentList)> GetAllRhinoMocksExpressions (ExpressionStatementSyntax node)
    {
      return node.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.SimpleMemberAccessExpression) || s.IsKind (SyntaxKind.InvocationExpression))
          .SelectMany (s => s.DescendantNodesAndSelf())
          .Where (s => s.IsKind (SyntaxKind.InvocationExpression))
          .Where (IsRhinoMocksMethod)
          .Distinct()
          .Select (s => (InvocationExpressionSyntax) s)
          .Select (s => (((MemberAccessExpressionSyntax) s.Expression).Name, s.ArgumentList))
          .Reverse();
    }

    private bool IsRhinoMocksMethod (SyntaxNode node)
    {
      return Model.GetSymbolInfo (node).Symbol is IMethodSymbol symbol
             && (RhinoMocksSymbols.ExpectSymbols.Contains (symbol.ReducedFrom ?? symbol.OriginalDefinition, SymbolEqualityComparer.Default)
                 || RhinoMocksSymbols.StubSymbols.Contains (symbol.ReducedFrom ?? symbol.OriginalDefinition, SymbolEqualityComparer.Default)
                 || RhinoMocksSymbols.AllIMethodOptionsSymbols.Contains (symbol.OriginalDefinition, SymbolEqualityComparer.Default));
    }

    private ArgumentListSyntax ConvertArgumentList (ArgumentListSyntax argumentList)
    {
      var lambdaExpression = argumentList.Arguments
          .Where (s => s.Expression is LambdaExpressionSyntax)
          .Select (s => (LambdaExpressionSyntax) s.Expression)
          .SingleOrDefault();

      if (lambdaExpression == null)
      {
        return argumentList;
      }

      if (lambdaExpression.Body is not InvocationExpressionSyntax invocationExpression)
      {
        return argumentList;
      }

      var methodSymbol = Model.GetSymbolInfo (invocationExpression).Symbol as IMethodSymbol;
      if (methodSymbol == null)
      {
        return argumentList;
      }

      var methodParameterTypeSymbols = methodSymbol.Parameters.Select (p => p.Type).ToArray();
      var isInArguments = methodParameterTypeSymbols.Select (
          typeSymbol => MoqSyntaxFactory.IsAnyArgument (
              MoqSyntaxFactory.SimpleTypeArgumentList (TypeSymbolToTypeSyntaxConverter.ConvertTypeSyntaxNodes (typeSymbol, Generator))));

      return MoqSyntaxFactory.SimpleArgumentList (
          MoqSyntaxFactory.SimpleArgument (
              lambdaExpression.WithBody (invocationExpression.WithArgumentList (MoqSyntaxFactory.SimpleArgumentList (isInArguments)))));
    }
  }
}