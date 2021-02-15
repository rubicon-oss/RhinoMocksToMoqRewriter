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

namespace RhinoMocksToMoqRewriter.Core
{
  public class MockInstantiationRewriter : CSharpSyntaxRewriter
  {
    private readonly SemanticModel _model;
    private const string c_generateStrictMock = "GenerateStrictMock";
    private const string c_indent = "    ";

    public MockInstantiationRewriter (SemanticModel model)
    {
      _model = model;
    }

    public override SyntaxNode? VisitInvocationExpression (InvocationExpressionSyntax node)
    {
      var compilationSymbol = _model.Compilation.GetTypeByMetadataName ("Rhino.Mocks.MockRepository");
      if (compilationSymbol == null)
      {
        throw new ArgumentException ("Rhino.Mocks cannot be found.");
      }

      var methodSymbol = _model.GetSymbolInfo (node).Symbol as IMethodSymbol;
      if (methodSymbol == null)
      {
        return node;
      }

      var isRhinoMocksMethod = SymbolEqualityComparer.Default.Equals (methodSymbol.ContainingType, compilationSymbol);
      if (!isRhinoMocksMethod)
      {
        return node;
      }

      var rhinoMethodGenericName = node.DescendantNodes().FirstOrDefault (n => n.IsKind (SyntaxKind.GenericName)) as GenericNameSyntax;
      if (rhinoMethodGenericName == null || rhinoMethodGenericName.Identifier.Text != methodSymbol.Name)
      {
        return node;
      }

      var nodeWithRewrittenDescendants = (InvocationExpressionSyntax) base.VisitInvocationExpression (node)!;
      var moqMockTypeArgumentList = SyntaxFactory.TypeArgumentList().AddArguments (rhinoMethodGenericName.TypeArgumentList.Arguments.First());
      var moqMockArgumentSyntaxList = SyntaxFactory.SeparatedList<ArgumentSyntax>().AddRange (nodeWithRewrittenDescendants.ArgumentList.Arguments);

      if (methodSymbol.Name == c_generateStrictMock)
      {
        var strictArgument = CreateMockBehaviourStrictArgument();
        moqMockArgumentSyntaxList = moqMockArgumentSyntaxList.Insert (0, strictArgument);
      }

      moqMockArgumentSyntaxList = MoqMockArgumentSyntaxList (moqMockArgumentSyntaxList, node);

      var moqMockArgumentList = SyntaxFactory.ArgumentList().WithArguments (moqMockArgumentSyntaxList);

      return SyntaxFactory.ObjectCreationExpression (
              SyntaxFactory.GenericName ("Mock")
                  .WithTypeArgumentList (moqMockTypeArgumentList)
                  .WithLeadingTrivia (SyntaxFactory.Space))
          .WithArgumentList (
              moqMockArgumentList
                  .WithLeadingTrivia (
                      moqMockArgumentList.Arguments.Count == 0
                          ? SyntaxFactory.Whitespace (string.Empty)
                          : SyntaxFactory.Space));
    }

    private static SeparatedSyntaxList<ArgumentSyntax> MoqMockArgumentSyntaxList (SeparatedSyntaxList<ArgumentSyntax> argumentSyntaxList, InvocationExpressionSyntax node)
    {
      var (firstLineIndent, indent) = GetIndentation (node);
      for (var i = 0; i < argumentSyntaxList.Count; i++)
      {
        var argument = argumentSyntaxList[i];
        argumentSyntaxList = argumentSyntaxList
            .Replace (
                argument,
                argument.WithLeadingTrivia (
                    SyntaxFactory.Whitespace (
                        i == 0
                            ? firstLineIndent
                            : indent)));
      }

      return argumentSyntaxList;
    }

    private static ArgumentSyntax CreateMockBehaviourStrictArgument ()
    {
      return SyntaxFactory.Argument (
          SyntaxFactory.MemberAccessExpression (
                  SyntaxKind.SimpleMemberAccessExpression,
                  SyntaxFactory.IdentifierName ("MockBehaviour"),
                  SyntaxFactory.IdentifierName ("Strict"))
              .WithOperatorToken (
                  SyntaxFactory.Token (SyntaxKind.DotToken)));
    }

    private static string GetNewLineCharacter (ArgumentListSyntax argumentList)
    {
      if (argumentList.ToString().Contains (SyntaxFactory.CarriageReturnLineFeed.ToString()))
      {
        return SyntaxFactory.CarriageReturnLineFeed.ToString();
      }

      if (argumentList.ToString().Contains (SyntaxFactory.LineFeed.ToString()))
      {
        return SyntaxFactory.LineFeed.ToString();
      }

      return string.Empty;
    }

    private static (string firstLineIndent, string indent) GetIndentation (InvocationExpressionSyntax node)
    {
      var newLineCharacter = GetNewLineCharacter (node.ArgumentList);
      var parentIndent = GetParentIndentation (node);

      var indent = string.IsNullOrEmpty (newLineCharacter)
          ? SyntaxFactory.Space.ToString()
          : newLineCharacter + parentIndent + c_indent;

      var firstLineIndent = string.IsNullOrEmpty (newLineCharacter)
          ? string.Empty
          : newLineCharacter + parentIndent + c_indent;

      return (firstLineIndent, indent);
    }

    private static string GetParentIndentation (SyntaxNode node)
    {
      CSharpSyntaxNode? containingStatement = node.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
      if (containingStatement == null)
      {
        containingStatement = node.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault();
      }

      if (containingStatement == null)
      {
        return string.Empty;
      }

      if (!containingStatement.HasLeadingTrivia)
      {
        return string.Empty;
      }

      return containingStatement.GetLeadingTrivia().ToString();
    }
  }
}