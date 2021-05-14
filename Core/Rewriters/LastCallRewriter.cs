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
  public class LastCallRewriter : RewriterBase
  {
    public override SyntaxNode? VisitBlock (BlockSyntax node)
    {
      BlockSyntax trackedNodes = null!;
      try
      {
        trackedNodes = node.TrackNodes (node.DescendantNodes().OfType<StatementSyntax>(), CompilationId);
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine (
            $"WARNING: Unable to convert LastCall"
            + $"\r\n{node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}"
            + $"\r\n{ex}");

        return node;
      }

      if (trackedNodes is null)
      {
        return node;
      }

      var baseCallNode = (BlockSyntax) base.VisitBlock (trackedNodes)!;

      var lastCallExpressionStatementsInBlock = GetLastCallExpressionStatementsInBlock (baseCallNode).ToList();
      if (lastCallExpressionStatementsInBlock.Count == 0)
      {
        return baseCallNode;
      }

      var nodesToBeRemoved = new List<SyntaxNode>();
      foreach (var lastCallExpressionStatement in lastCallExpressionStatementsInBlock)
      {
        var currentNode = baseCallNode.GetCurrentNode (lastCallExpressionStatement.Node, CompilationId)!;
        var lastCalledMock = GetLastCalledMockExpressionStatement (baseCallNode, lastCallExpressionStatement);
        if (lastCalledMock == null)
        {
          Console.Error.WriteLine (
              $"WARNING: Unable to convert LastCall"
              + $"\r\n{node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}");
          continue;
        }

        nodesToBeRemoved.Add (lastCalledMock);

        var newExpressionStatement = RewriteLastCallExpression (currentNode, lastCalledMock);
        baseCallNode = baseCallNode.ReplaceNode (currentNode, newExpressionStatement.WithLeadingTrivia (currentNode.GetLeadingTrivia()));
      }

      baseCallNode = baseCallNode.RemoveNodes (nodesToBeRemoved.Select (s => baseCallNode.GetCurrentNode (s, CompilationId)!), SyntaxRemoveOptions.KeepEndOfLine);

      return baseCallNode;
    }

    private IEnumerable<(ExpressionStatementSyntax Node, int Index)> GetLastCallExpressionStatementsInBlock (BlockSyntax node)
    {
      return node.Statements
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Select ((syntaxNode, index) => new { node = syntaxNode, index })
          .Where (
              s => Model.GetSymbolInfo (node.GetOriginalNode (s.node, CompilationId)!.Expression).Symbol is { } symbol
                   && RhinoMocksSymbols.RhinoMocksLastCallSymbol.Equals (symbol.OriginalDefinition.ContainingSymbol, SymbolEqualityComparer.Default))
          .Select (s => (s.node, s.index));
    }

    private ExpressionStatementSyntax? GetLastCalledMockExpressionStatement (BlockSyntax node, (ExpressionStatementSyntax Node, int Index) lastCallExpressionStatement)
    {
      var reachableAndContainedMockSymbols = GetReachableAndContainedMockSymbols (node);
      var lastCalledExpressions = node.Statements.Where (s => s is ExpressionStatementSyntax { Expression: InvocationExpressionSyntax }).ToList();

      return lastCalledExpressions
          .Select ((syntaxNode, index) => new { node = syntaxNode, index })
          .LastOrDefault (
              e => Model.GetSymbolInfo (node.GetOriginalNode (e.node, CompilationId)!.GetFirstIdentifierName()).Symbol is { } symbol
                   && reachableAndContainedMockSymbols.Contains (symbol)
                   && e.index < lastCallExpressionStatement.Index)?.node as ExpressionStatementSyntax;
    }

    private IEnumerable<ISymbol> GetReachableAndContainedMockSymbols (BlockSyntax node)
    {
      return GetMockFieldSymbols (node).Concat (GetLocalMockSymbolsInAncestorsAndSelf (node));
    }

    private IEnumerable<ISymbol> GetLocalMockSymbolsInAncestorsAndSelf (BlockSyntax node)
    {
      return node.GetOriginalNode (node.Statements.First(), CompilationId)!.AncestorsAndSelf()
          .Where (s => s.IsKind (SyntaxKind.Block))
          .Select (s => (BlockSyntax) s)
          .SelectMany (GetLocalMockSymbols);
    }

    private IEnumerable<ISymbol> GetLocalMockSymbols (BlockSyntax node)
    {
      return node.Statements
          .Where (s => s.IsKind (SyntaxKind.LocalDeclarationStatement))
          .Select (s => (LocalDeclarationStatementSyntax) s)
          .Select (s => s.Declaration.Variables)
          .Where (s => IsRhinoMocksMock (s))
          .SelectMany (s => s)
          .Select (s => Model.GetDeclaredSymbol (s))
          .Where (s => s is not null)!;
    }

    private IEnumerable<ISymbol> GetMockFieldSymbols (BlockSyntax node)
    {
      return GetAllMockFieldDeclarations (node).Concat (GetAllMockFieldAssignmentExpressions (node));
    }

    private IEnumerable<ISymbol> GetAllMockFieldAssignmentExpressions (BlockSyntax node)
    {
      return node.GetOriginalNode (node.Statements.First(), CompilationId)!.SyntaxTree.GetRoot().DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Where (s => s.Expression.IsKind (SyntaxKind.SimpleAssignmentExpression))
          .Select (s => (AssignmentExpressionSyntax) s.Expression)
          .Where (
              s => Model.GetSymbolInfo (s.Right).Symbol is { } symbol
                   && RhinoMocksSymbols.RhinoMocksMockRepositorySymbol.Equals (symbol.OriginalDefinition.ContainingSymbol, SymbolEqualityComparer.Default))
          .Select (s => Model.GetSymbolInfo (s.Left).Symbol)
          .Where (s => s is not null)!;
    }

    private IEnumerable<ISymbol> GetAllMockFieldDeclarations (BlockSyntax node)
    {
      return node.GetOriginalNode (node.Statements.First(), CompilationId)!.SyntaxTree.GetRoot().DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.FieldDeclaration))
          .Select (s => (FieldDeclarationSyntax) s)
          .Select (s => s.Declaration.Variables)
          .Where (s => IsRhinoMocksMock (s))
          .SelectMany (s => s)
          .Select (s => Model.GetDeclaredSymbol (s))
          .Where (s => s is not null)!;
    }

    private bool IsRhinoMocksMock (IReadOnlyList<VariableDeclaratorSyntax> variables)
    {
      return variables.Any (
          s => s.Initializer is { } initializer
               && Model.GetSymbolInfo (initializer.Value).Symbol is { } symbol
               && RhinoMocksSymbols.RhinoMocksMockRepositorySymbol.Equals (
                   symbol.OriginalDefinition.ContainingSymbol,
                   SymbolEqualityComparer.Default));
    }

    private static ExpressionStatementSyntax RewriteLastCallExpression (ExpressionStatementSyntax lastCallNode, ExpressionStatementSyntax lastCalledExpression)
    {
      var lastCallIdentifierName = lastCallNode.GetFirstIdentifierName();
      return lastCallNode.ReplaceNode (
          lastCallIdentifierName,
          MoqSyntaxFactory.ExpectCallExpression (lastCalledExpression.Expression.WithoutLeadingTrivia()));
    }
  }
}