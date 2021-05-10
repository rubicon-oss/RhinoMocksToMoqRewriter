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
  public class OrderedMockRewriter : RewriterBase
  {
    public override SyntaxNode? VisitMethodDeclaration (MethodDeclarationSyntax node)
    {
      var rhinoMocksMockRepositoryCompilationSymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.MockRepository");
      if (rhinoMocksMockRepositoryCompilationSymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var moqSetupSymbol = Model.Compilation.GetTypeByMetadataName ("Moq.Mock`1");
      var moqCallbackSymbol = Model.Compilation.GetTypeByMetadataName ("Moq.Language.ICallback");
      var moqReturnsSymbol = Model.Compilation.GetTypeByMetadataName ("Moq.Language.IReturns`2");
      var moqVerifiableSymbol = Model.Compilation.GetTypeByMetadataName ("Moq.Language.IVerifies");
      if (moqSetupSymbol == null || moqCallbackSymbol == null || moqReturnsSymbol == null || moqVerifiableSymbol == null)
      {
        throw new InvalidOperationException ("Moq cannot be found.");
      }

      var moqSymbols = GetAllMoqSymbols (moqSetupSymbol, moqCallbackSymbol, moqReturnsSymbol, moqVerifiableSymbol).ToList();

      var usingStatements = GetRhinoMocksOrderedUsingStatements (node, rhinoMocksMockRepositoryCompilationSymbol).ToList();

      var treeWithTrackedNodes = node.TrackNodes (usingStatements, CompilationId);
      for (var i = 0; i < usingStatements.Count; i++)
      {
        var usingStatement = usingStatements[i];
        var trackedUsingStatement = treeWithTrackedNodes.GetCurrentNode (usingStatement, CompilationId);
        var statements = ReplaceExpressionStatements (((BlockSyntax) usingStatement.Statement).Statements, moqSymbols, i + 1);

        treeWithTrackedNodes = treeWithTrackedNodes.ReplaceNode (trackedUsingStatement!, statements);
      }

      return treeWithTrackedNodes;
    }

    private static IEnumerable<ISymbol> GetAllMoqSymbols (
        INamedTypeSymbol moqSetupSymbol,
        INamedTypeSymbol moqCallbackSymbol,
        INamedTypeSymbol moqReturnsSymbol,
        INamedTypeSymbol moqVerifiableSymbol)
    {
      return moqSetupSymbol.GetMembers ("Setup")
          .Concat (moqCallbackSymbol.GetMembers ("Callback"))
          .Concat (moqReturnsSymbol.GetMembers ("Returns"))
          .Concat (moqVerifiableSymbol.GetMembers ("Verifiable"));
    }

    private IEnumerable<SyntaxNode> ReplaceExpressionStatements (
        SyntaxList<StatementSyntax> statements,
        IReadOnlyCollection<ISymbol> moqSymbols,
        int current)
    {
      var nodesToBeReplaced = GetAllMoqExpressionStatements (statements, moqSymbols).ToList();
      var parentTrivia = statements.First().Parent?.GetLeadingTrivia();
      for (var i = 0; i < statements.Count; i++)
      {
        var statement = statements[i];
        if (!nodesToBeReplaced.Any (s => s.IsEquivalentTo (statement, false)))
        {
          statements = statements.Replace (statement, statement.WithLeadingTrivia (parentTrivia));
          continue;
        }

        var firstIdentifierName = statement.GetFirstIdentifierName();
        var newExpressionStatement = statement.ReplaceNode (
            firstIdentifierName,
            MoqSyntaxFactory.InSequenceExpression (firstIdentifierName, current.ToString()));

        statements = statements.Replace (statement, newExpressionStatement.WithLeadingTrivia (parentTrivia));
      }

      statements = statements.Insert (
          0,
          MoqSyntaxFactory.MockSequenceLocalDeclarationStatement (current.ToString())
              .WithLeadingTrivia (parentTrivia)
              .WithTrailingTrivia (SyntaxFactory.Whitespace (Environment.NewLine)));

      return statements;
    }

    private IEnumerable<ExpressionStatementSyntax> GetAllMoqExpressionStatements (
        SyntaxList<StatementSyntax> statements,
        IReadOnlyCollection<ISymbol> moqSymbols)
    {
      return statements
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Where (
              s => Model.GetSymbolInfo (s.Expression).Symbol?.OriginalDefinition is IMethodSymbol symbol
                   && moqSymbols.Contains (symbol, SymbolEqualityComparer.Default));
    }

    private IEnumerable<UsingStatementSyntax> GetRhinoMocksOrderedUsingStatements (MethodDeclarationSyntax node, INamedTypeSymbol mockRepositoryCompilationSymbol)
    {
      var rhinoMocksOrderedSymbol = mockRepositoryCompilationSymbol.GetMembers ("Ordered").Single();

      return node.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.UsingStatement))
          .Select (s => (UsingStatementSyntax) s)
          .Where (
              s => s.Expression is InvocationExpressionSyntax invocationExpression
                   && rhinoMocksOrderedSymbol.Equals (Model.GetSymbolInfo (invocationExpression).Symbol, SymbolEqualityComparer.Default));
    }
  }
}