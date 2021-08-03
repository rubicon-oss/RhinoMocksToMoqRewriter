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

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class ObsoleteMethodRewriter : RewriterBase
  {
    private readonly IFormatter _formatter;

    public ObsoleteMethodRewriter (IFormatter formatter)
    {
      _formatter = formatter;
    }

    public override SyntaxNode? VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      MethodDeclarationSyntax trackedNodes = null!;
      try
      {
        trackedNodes = node.TrackNodes (node.DescendantNodesAndSelf(), CompilationId);
      }
      catch (Exception)
      {
        Console.Error.WriteLine (
            $"  WARNING: Unable to convert node\r\n"
            + $"  {node.SyntaxTree.FilePath} at line {node.GetLocation().GetMappedLineSpan().StartLinePosition.Line}");
        return node;
      }

      List<SyntaxNode> nodesToBeReplaced = GetNodesToBeReplaced (trackedNodes).ToList();

      foreach (var nodeToBeReplaced in nodesToBeReplaced)
      {
        var currentNode = trackedNodes!.GetCurrentNode (nodeToBeReplaced!, CompilationId)!;
        trackedNodes = trackedNodes!.RemoveNode (
            currentNode,
            nodeToBeReplaced.GetLeadingTrivia().ToFullString().Contains (Environment.NewLine)
                ? SyntaxRemoveOptions.KeepEndOfLine
                : SyntaxRemoveOptions.KeepNoTrivia)!;
      }

      return _formatter.Format (trackedNodes!);
    }

    public override SyntaxNode? VisitFieldDeclaration (FieldDeclarationSyntax node)
    {
      var mockRepositoryTypeSymbol = ModelExtensions.GetSymbolInfo (Model, node.Declaration.Type).Symbol;
      return RhinoMocksSymbols.RhinoMocksMockRepositorySymbol.Equals (mockRepositoryTypeSymbol, SymbolEqualityComparer.Default)
          ? null
          : node;
    }

    private IEnumerable<SyntaxNode> GetNodesToBeReplaced (MethodDeclarationSyntax node)
    {
      return GetObsoleteExpressionStatements (node)
          .Concat (GetObsoleteLocalDeclarationStatements (node))
          .Concat (GetObsoleteAssignmentExpressions (node));
    }

    private IEnumerable<ExpressionStatementSyntax> GetObsoleteAssignmentExpressions (MethodDeclarationSyntax node)
    {
      return node.DescendantNodes()
          .Select (s => node.GetOriginalNode (s, CompilationId)!)
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Where (s => s.Expression.IsKind (SyntaxKind.SimpleAssignmentExpression))
          .Where (
              s => s.Expression is AssignmentExpressionSyntax assignmentExpression
                   && RhinoMocksSymbols.RhinoMocksMockRepositorySymbol.Equals (Model.GetTypeInfo (assignmentExpression).Type, SymbolEqualityComparer.Default));
    }

    private IEnumerable<SyntaxNode> GetObsoleteLocalDeclarationStatements (MethodDeclarationSyntax node)
    {
      return node.DescendantNodes()
          .Select (s => node.GetOriginalNode (s, CompilationId)!)
          .Where (s => s.IsKind (SyntaxKind.LocalDeclarationStatement))
          .Select (s => (LocalDeclarationStatementSyntax) s)
          .Where (s => RhinoMocksSymbols.RhinoMocksMockRepositorySymbol.Equals (Model.GetSymbolInfo (s.Declaration.Type).Symbol, SymbolEqualityComparer.Default));
    }

    private IEnumerable<SyntaxNode> GetObsoleteExpressionStatements (MethodDeclarationSyntax node)
    {
      return node.DescendantNodes()
          .Select (s => node.GetOriginalNode (s, CompilationId)!)
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Where (
              s => s.Expression is InvocationExpressionSyntax invocationExpression
                   && Model.GetSymbolInfo (invocationExpression).Symbol is IMethodSymbol methodSymbol
                   && RhinoMocksSymbols.ObsoleteRhinoMocksSymbols.Contains (methodSymbol.ReducedFrom ?? methodSymbol.OriginalDefinition, SymbolEqualityComparer.Default));
    }
  }
}