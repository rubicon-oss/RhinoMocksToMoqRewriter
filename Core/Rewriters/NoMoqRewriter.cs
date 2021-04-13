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
  public class NoMoqRewriter : RewriterBase
  {
    public override SyntaxNode? VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      var rhinoMocksExtensionsCompilationSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.RhinoMocksExtensions");
      var rhinoMocksMockRepositoryCompilationSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.MockRepository");
      if (rhinoMocksExtensionsCompilationSymbol == null || rhinoMocksMockRepositoryCompilationSymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var nodesToBeReplaced = GetAllNodesToBeReplaced (node, rhinoMocksMockRepositoryCompilationSymbol, rhinoMocksExtensionsCompilationSymbol).ToList();
      return node.RemoveNodes (nodesToBeReplaced, SyntaxRemoveOptions.KeepNoTrivia);
    }

    public override SyntaxNode? VisitFieldDeclaration (FieldDeclarationSyntax node)
    {
      var rhinoMocksMockRepositoryCompilationSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.MockRepository");
      if (rhinoMocksMockRepositoryCompilationSymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var mockRepositoryTypeSymbol = ModelExtensions.GetSymbolInfo (Model, node.Declaration.Type).Symbol;
      return rhinoMocksMockRepositoryCompilationSymbol.Equals (mockRepositoryTypeSymbol, SymbolEqualityComparer.Default)
          ? null
          : node;
    }

    private IEnumerable<SyntaxNode> GetAllNodesToBeReplaced (
        MethodDeclarationSyntax node,
        INamedTypeSymbol rhinoMocksMockRepositoryCompilationSymbol,
        INamedTypeSymbol rhinoMocksExtensionsCompilationSymbol)
    {
      return GetReplayAndReplayAllExpressionStatements (node, rhinoMocksExtensionsCompilationSymbol, rhinoMocksMockRepositoryCompilationSymbol)
          .Concat (GetAllRhinoMocksMockRepositoryLocalDeclarationStatements (node, rhinoMocksMockRepositoryCompilationSymbol))
          .Concat (GetAllRhinoMocksAssignmentExpressions (node, rhinoMocksMockRepositoryCompilationSymbol));
    }

    private IEnumerable<ExpressionStatementSyntax> GetAllRhinoMocksAssignmentExpressions (
        MethodDeclarationSyntax node,
        INamedTypeSymbol rhinoMocksMockRepositoryCompilationSymbol)
    {
      return node.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Where (s => s.Expression.IsKind (SyntaxKind.SimpleAssignmentExpression))
          .Where (
              s => s.Expression is AssignmentExpressionSyntax assignmentExpression
                   && rhinoMocksMockRepositoryCompilationSymbol.Equals (Model.GetTypeInfo (assignmentExpression).Type, SymbolEqualityComparer.Default));
    }

    private IEnumerable<SyntaxNode> GetAllRhinoMocksMockRepositoryLocalDeclarationStatements (
        MethodDeclarationSyntax node,
        INamedTypeSymbol rhinoMocksMockRepositoryCompilationSymbol)
    {
      return node.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.LocalDeclarationStatement))
          .Select (s => (LocalDeclarationStatementSyntax) s)
          .Where (s => rhinoMocksMockRepositoryCompilationSymbol.Equals (Model.GetSymbolInfo (s.Declaration.Type).Symbol, SymbolEqualityComparer.Default));
    }

    private IEnumerable<SyntaxNode> GetReplayAndReplayAllExpressionStatements (
        MethodDeclarationSyntax node,
        INamedTypeSymbol rhinoMocksExtensionsCompilationSymbol,
        INamedTypeSymbol rhinoMocksMockRepositoryCompilationSymbol)
    {
      var replayMethodSymbol = rhinoMocksExtensionsCompilationSymbol.GetMembers ("Replay").Single();
      var replayAllMethodSymbol = rhinoMocksMockRepositoryCompilationSymbol.GetMembers ("ReplayAll").Single();

      return node.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Where (
              s => s.Expression is InvocationExpressionSyntax invocationExpression
                   && Model.GetSymbolInfo (invocationExpression).Symbol is IMethodSymbol methodSymbol
                   && (replayMethodSymbol.Equals (methodSymbol.ReducedFrom ?? methodSymbol.OriginalDefinition, SymbolEqualityComparer.Default)
                       || replayAllMethodSymbol.Equals (methodSymbol, SymbolEqualityComparer.Default)));
    }
  }
}