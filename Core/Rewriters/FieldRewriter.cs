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
  public class FieldRewriter : RewriterBase
  {
    private readonly IFormatter _formatter;
    private List<IFieldSymbol>? _generateMockFieldSymbols;

    public FieldRewriter (IFormatter formatter)
    {
      _formatter = formatter;
    }

    public override SyntaxNode? Visit (SyntaxNode? node)
    {
      if (node == null)
      {
        return node;
      }

      try
      {
        _generateMockFieldSymbols = new List<IFieldSymbol>();
        _generateMockFieldSymbols.AddRange (GetFieldSymbols (node));
        return base.Visit (node);
      }
      finally
      {
        _generateMockFieldSymbols = null;
      }
    }

    public override SyntaxNode? VisitFieldDeclaration (FieldDeclarationSyntax node)
    {
      var fieldSymbols = node.Declaration.Variables
          .Select (v => Model.GetDeclaredSymbol (v) as IFieldSymbol)
          .ToList();

      if (fieldSymbols.Any (s => s is null))
      {
        throw new InvalidOperationException ("Unable to get FieldSymbol from FieldDeclaration");
      }

      if (_generateMockFieldSymbols == null)
      {
        throw new InvalidOperationException ("FieldSymbolList must not be null!");
      }

      foreach (var fieldSymbol in fieldSymbols)
      {
        if (!_generateMockFieldSymbols.Any (s => SymbolEqualityComparer.Default.Equals (s, fieldSymbol)))
        {
          return node;
        }
      }

      node = (FieldDeclarationSyntax) base.VisitFieldDeclaration (node)!;
      var newNode = MoqSyntaxFactory.MockFieldDeclaration (
              node.AttributeLists,
              node.Modifiers,
              node.Declaration.Type.WithoutTrivia(),
              node.Declaration.Variables)
          .WithLeadingTrivia (node.GetLeadingTrivia())
          .WithTrailingTrivia (
              SyntaxFactory.Whitespace (Environment.NewLine));

      var formattedNode = _formatter.Format (
          newNode.WithDeclaration (
              newNode.Declaration.WithVariables (
                  node.Declaration.Variables)));

      return formattedNode;
    }

    private IEnumerable<IFieldSymbol> GetFieldSymbols (SyntaxNode node)
    {
      var mockRepositorySymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.MockRepository");
      if (mockRepositorySymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var generateMockMethodSymbols = mockRepositorySymbol.GetMembers ("GenerateMock")
          .Concat (mockRepositorySymbol.GetMembers ("GenerateStrictMock"))
          .Concat (mockRepositorySymbol.GetMembers ("GeneratePartialMock"))
          .Concat (mockRepositorySymbol.GetMembers ("GenerateStub"))
          .Concat (mockRepositorySymbol.GetMembers ("StrictMock"))
          .Concat (mockRepositorySymbol.GetMembers ("DynamicMock"))
          .Concat (mockRepositorySymbol.GetMembers ("PartialMock"))
          .Concat (mockRepositorySymbol.GetMembers ("PartialMultiMock"))
          .ToList();

      return GetFieldSymbolsFromMockAssignmentExpressions (node, generateMockMethodSymbols)
          .Concat (GetFieldSymbolsFromMockFieldDeclarations (node, generateMockMethodSymbols));
    }

    private IEnumerable<IFieldSymbol> GetFieldSymbolsFromMockFieldDeclarations (SyntaxNode node, IReadOnlyCollection<ISymbol> generateMockMethodSymbols)
    {
      return node
          .SyntaxTree.GetRoot()
          .DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.FieldDeclaration))
          .Select (s => (FieldDeclarationSyntax) s)
          .SelectMany (s => s.DescendantNodes())
          .Where (s => s.IsKind (SyntaxKind.VariableDeclarator))
          .Select (s => (VariableDeclaratorSyntax) s)
          .Where (
              s =>
                  s.Initializer != null &&
                  Model.GetSymbolInfo (s.Initializer.Value).Symbol is IMethodSymbol symbol &&
                  generateMockMethodSymbols.Contains (
                      symbol.OriginalDefinition,
                      SymbolEqualityComparer.Default))
          .Select (s => (IFieldSymbol) Model.GetDeclaredSymbol (s)!);
    }

    private IEnumerable<IFieldSymbol> GetFieldSymbolsFromMockAssignmentExpressions (SyntaxNode node, IReadOnlyCollection<ISymbol> generateMockMethodSymbols)
    {
      return node
          .SyntaxTree.GetRoot()
          .DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.ExpressionStatement))
          .Select (s => (ExpressionStatementSyntax) s)
          .Where (s => s.Expression.IsKind (SyntaxKind.SimpleAssignmentExpression))
          .Select (s => (AssignmentExpressionSyntax) s.Expression)
          .Where (
              s =>
                  Model.GetSymbolInfo (s.Right).Symbol is IMethodSymbol symbol &&
                  generateMockMethodSymbols.Contains (
                      symbol.OriginalDefinition,
                      SymbolEqualityComparer.Default))
          .Where (s => Model.GetSymbolInfo (s.Left).Symbol is IFieldSymbol)
          .Select (s => (IFieldSymbol) Model.GetSymbolInfo (s.Left).Symbol!);
    }
  }
}