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

    public override SyntaxNode? VisitArgument (ArgumentSyntax node)
    {
      var argumentIdentifierName = node.Expression as IdentifierNameSyntax;
      if (argumentIdentifierName == null)
      {
        return node;
      }

      if (Model == null)
      {
        throw new InvalidOperationException ("SemanticModel must not be null!");
      }

      var fieldSymbol = Model.GetSymbolInfo (node.Expression).Symbol;
      if (fieldSymbol == null)
      {
        throw new InvalidOperationException ("Unable to get FieldSymbol from FieldDeclaration");
      }

      if (_generateMockFieldSymbols == null)
      {
        throw new InvalidOperationException ("FieldSymbolList must not be null!");
      }

      if (!_generateMockFieldSymbols.Any (s => SymbolEqualityComparer.Default.Equals (s, fieldSymbol)))
      {
        return node;
      }

      node = (ArgumentSyntax) base.VisitArgument (node)!;

      return MoqSyntaxFactory.MockObjectArgument (argumentIdentifierName)
          .WithLeadingTrivia (node.GetLeadingTrivia());
    }

    public override SyntaxNode? VisitInvocationExpression (InvocationExpressionSyntax node)
    {
      if (Model == null)
      {
        throw new InvalidOperationException ("SemanticModel must not be null!");
      }

      var mockRepositorySymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.MockRepository");
      if (mockRepositorySymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var methodSymbol = Model.GetSymbolInfo (node).Symbol as IMethodSymbol;
      if (methodSymbol == null)
      {
        return (InvocationExpressionSyntax) base.VisitInvocationExpression (node)!;
      }

      var isRhinoMocksMethod = SymbolEqualityComparer.Default.Equals (methodSymbol.ContainingType, mockRepositorySymbol);
      if (isRhinoMocksMethod)
      {
        return node;
      }

      var identifierName = node.GetFirstIdentifierName();
      var fieldSymbol = Model.GetSymbolInfo (identifierName).Symbol;
      if (fieldSymbol == null)
      {
        throw new InvalidOperationException ("Unable to get FieldSymbol from FieldDeclaration");
      }

      if (_generateMockFieldSymbols == null)
      {
        throw new InvalidOperationException ("FieldSymbolList must not be null!");
      }

      if (!_generateMockFieldSymbols.Any (s => SymbolEqualityComparer.Default.Equals (s, fieldSymbol)))
      {
        return node;
      }

      var nodeWithObjectExpression = MoqSyntaxFactory.MockObjectExpression (identifierName);

      return node.ReplaceNode (identifierName, nodeWithObjectExpression);
    }

    private IEnumerable<IFieldSymbol> GetFieldSymbols (SyntaxNode node)
    {
      if (Model == null)
      {
        throw new InvalidOperationException ("SemanticModel must not be null!");
      }

      var mockRepositorySymbol = Model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.MockRepository");
      if (mockRepositorySymbol == null)
      {
        throw new InvalidOperationException ("Rhino.Mocks cannot be found.");
      }

      var generateMockMethodSymbols =
          new List<IMethodSymbol>
          {
              (IMethodSymbol) mockRepositorySymbol.GetMembers ("GenerateMock").First(),
              (IMethodSymbol) mockRepositorySymbol.GetMembers ("GenerateStrictMock").First(),
              (IMethodSymbol) mockRepositorySymbol.GetMembers ("GeneratePartialMock").First(),
              (IMethodSymbol) mockRepositorySymbol.GetMembers ("GenerateStub").First()
          };

      var fieldSymbols = GetFieldSymbolsFromMockAssignmentExpressions (node, generateMockMethodSymbols).ToList();
      fieldSymbols.AddRange (GetFieldSymbolsFromMockFieldDeclarations (node, generateMockMethodSymbols));

      return fieldSymbols;
    }

    private IEnumerable<IFieldSymbol> GetFieldSymbolsFromMockFieldDeclarations (SyntaxNode node, List<IMethodSymbol> generateMockMethodSymbols)
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

    private IEnumerable<IFieldSymbol> GetFieldSymbolsFromMockAssignmentExpressions (SyntaxNode node, List<IMethodSymbol> generateMockMethodSymbols)
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
          .Select (s => (IFieldSymbol) Model.GetSymbolInfo (s.Left).Symbol!);
    }
  }
}