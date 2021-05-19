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
  public class ObjectRewriter : RewriterBase
  {
    public override SyntaxNode? VisitMemberAccessExpression (MemberAccessExpressionSyntax node)
    {
      var trackedNodes = TrackNodes (node);
      var baseCallNode = (MemberAccessExpressionSyntax) base.VisitMemberAccessExpression (trackedNodes)!;

      var nameSymbol = Model.GetSymbolInfo (baseCallNode.GetOriginalNode (baseCallNode, CompilationId)!.Name).GetFirstOverloadOrDefault();
      var typeSymbol = Model.GetTypeInfo (baseCallNode.GetOriginalNode (baseCallNode, CompilationId)!.Expression).Type?.OriginalDefinition;
      if (!MoqSymbols.GenericMoqSymbol.Equals (typeSymbol, SymbolEqualityComparer.Default))
      {
        return baseCallNode;
      }

      if (MoqSymbols.AllProtectedMockSymbols.Contains ((nameSymbol as IMethodSymbol)?.ReducedFrom ?? nameSymbol?.OriginalDefinition, SymbolEqualityComparer.Default))
      {
        return baseCallNode;
      }

      if (MoqSymbols.AllMockSequenceSymbols.Contains ((nameSymbol as IMethodSymbol)?.ReducedFrom ?? nameSymbol?.OriginalDefinition, SymbolEqualityComparer.Default))
      {
        return baseCallNode;
      }

      var currentNode = baseCallNode.GetCurrentNode (baseCallNode, CompilationId);
      if (currentNode is null)
      {
        Console.Error.WriteLine (
            $"WARNING: Unable to insert .Object"
            + $"\r\n{node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}");

        return baseCallNode;
      }

      var identifierNameToBeReplaced = currentNode!.GetFirstIdentifierName();
      try
      {
        return baseCallNode.ReplaceNode (
            identifierNameToBeReplaced,
            MoqSyntaxFactory.MockObjectExpression (identifierNameToBeReplaced)
                .WithLeadingAndTrailingTriviaOfNode (identifierNameToBeReplaced)!);
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine (
            $"WARNING: Unable to insert .Object"
            + $"\r\n{node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}"
            + $"\r\n{ex}");

        return baseCallNode;
      }
    }

    public override SyntaxNode? VisitArgument (ArgumentSyntax node)
    {
      var trackedNodes = TrackNodes (node);

      var baseCallNode = (ArgumentSyntax) base.VisitArgument (trackedNodes)!;
      if (baseCallNode.Expression is not IdentifierNameSyntax identifierName)
      {
        return baseCallNode;
      }

      var typeSymbol = Model.GetTypeInfo (baseCallNode.GetOriginalNode (identifierName, CompilationId)!).Type?.OriginalDefinition;
      if (!MoqSymbols.GenericMoqSymbol.Equals (typeSymbol, SymbolEqualityComparer.Default))
      {
        return baseCallNode;
      }

      return baseCallNode.WithExpression (MoqSyntaxFactory.MockObjectExpression (identifierName));
    }

    public override SyntaxNode? VisitReturnStatement (ReturnStatementSyntax node)
    {
      var trackedNodes = TrackNodes (node);
      var baseCallNode = (ReturnStatementSyntax) base.VisitReturnStatement (trackedNodes)!;
      if (baseCallNode.Expression is not IdentifierNameSyntax identifierName)
      {
        return baseCallNode;
      }

      var typeSymbol = Model.GetTypeInfo (baseCallNode.GetOriginalNode (identifierName, CompilationId)!).Type?.OriginalDefinition;
      if (!MoqSymbols.GenericMoqSymbol.Equals (typeSymbol, SymbolEqualityComparer.Default))
      {
        return baseCallNode;
      }

      return baseCallNode.WithExpression (MoqSyntaxFactory.MockObjectExpression (identifierName));
    }

    public override SyntaxNode? VisitInitializerExpression (InitializerExpressionSyntax node)
    {
      var trackedNodes = TrackNodes (node);

      var baseCallNode = (InitializerExpressionSyntax) base.VisitInitializerExpression (trackedNodes)!;
      var newExpressions = baseCallNode.Expressions;
      for (var i = 0; i < baseCallNode.Expressions.Count; i++)
      {
        var expression = newExpressions[i];
        if (expression is not IdentifierNameSyntax identifierName)
        {
          continue;
        }

        var typeSymbol = Model.GetTypeInfo (baseCallNode.GetOriginalNode (identifierName, CompilationId)!).Type?.OriginalDefinition;
        if (!MoqSymbols.GenericMoqSymbol.Equals (typeSymbol, SymbolEqualityComparer.Default))
        {
          continue;
        }

        try
        {
          newExpressions = newExpressions.Replace (expression, MoqSyntaxFactory.MockObjectExpression (identifierName));
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine (
              $"WARNING: Unable to insert .Object"
              + $"\r\n{node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}"
              + $"\r\n{ex}");

          return baseCallNode;
        }
      }

      return baseCallNode.WithExpressions (newExpressions);
    }

    private T TrackNodes<T> (T node)
        where T : SyntaxNode
    {
      return node.TrackNodes (
          node.DescendantNodesAndSelf()
              .Where (
                  s => s.IsKind (SyntaxKind.SimpleMemberAccessExpression)
                       || s.IsKind (SyntaxKind.IdentifierName)
                       || s.IsKind (SyntaxKind.CollectionInitializerExpression)
                       || s.IsKind (SyntaxKind.ArrayInitializerExpression)
                       || s.IsKind (SyntaxKind.ObjectInitializerExpression)),
          CompilationId);
    }
  }
}