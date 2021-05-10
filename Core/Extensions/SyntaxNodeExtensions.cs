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
using Microsoft.VisualBasic;

namespace RhinoMocksToMoqRewriter.Core.Extensions
{
  public static class SyntaxNodeExtensions
  {
    private const string c_carriageReturnLineFeed = "\r\n";
    private const string c_lineFeed = "\n";
    private const string c_whiteSpace = " ";

    public static string GetLeadingWhiteSpaces (this SyntaxNode node)
    {
      if (!node.HasLeadingTrivia)
      {
        return string.Empty;
      }

      var numberOfSpaces = node.GetLeadingTrivia().ToString().Count (c => c.ToString() == c_whiteSpace);
      return Strings.Space (numberOfSpaces);
    }

    public static TypeArgumentListSyntax? GetTypeArgumentListOrDefault (this ArgumentSyntax node)
    {
      return node.DescendantNodes().FirstOrDefault (
          s => s.IsKind (SyntaxKind.TypeArgumentList) && s.Parent.IsKind (SyntaxKind.GenericName)) as TypeArgumentListSyntax;
    }

    public static LambdaExpressionSyntax? GetLambdaExpressionOrDefault (this ArgumentSyntax node)
    {
      return node.DescendantNodes().FirstOrDefault (
          s => s.IsKind (SyntaxKind.SimpleLambdaExpression)) as LambdaExpressionSyntax;
    }

    public static ArgumentSyntax GetFirstArgument (this SyntaxNode node)
    {
      return node.GetFirstArgumentOrDefault() ?? throw new InvalidOperationException ("Node must have an Argument");
    }

    public static ArgumentSyntax? GetFirstArgumentOrDefault (this SyntaxNode node)
    {
      return node.DescendantNodes()
          .FirstOrDefault (s => s.IsKind (SyntaxKind.Argument)) as ArgumentSyntax;
    }

    public static MemberAccessExpressionSyntax? GetFirstAncestorMemberAccessExpressionOrDefault (this ArgumentListSyntax node)
    {
      var invocationExpressionNode = node.Ancestors()
          .FirstOrDefault (s => s.IsKind (SyntaxKind.InvocationExpression)) as InvocationExpressionSyntax;

      return invocationExpressionNode?.DescendantNodes()
          .FirstOrDefault (s => s.IsKind (SyntaxKind.SimpleMemberAccessExpression)) as MemberAccessExpressionSyntax;
    }

    public static IdentifierNameSyntax GetFirstIdentifierName (this SyntaxNode node)
    {
      return node.GetFirstIdentifierNameOrDefault() ?? throw new InvalidOperationException ("Node must have an IdentifierName");
    }

    public static IdentifierNameSyntax? GetFirstIdentifierNameOrDefault (this SyntaxNode node)
    {
      var identifierName = node.DescendantNodes()
          .FirstOrDefault (s => s.IsKind (SyntaxKind.IdentifierName)) as IdentifierNameSyntax;

      return identifierName;
    }

    public static GenericNameSyntax? GetFirstGenericNameOrDefault (this SyntaxNode node)
    {
      return node.DescendantNodes().FirstOrDefault (s => s.IsKind (SyntaxKind.GenericName)) as GenericNameSyntax;
    }

    public static T WithLeadingAndTrailingTriviaOfNode<T> (this T node, SyntaxNode triviaNode)
        where T : SyntaxNode
    {
      return node.WithLeadingTrivia (triviaNode.GetLeadingTrivia())
          .WithTrailingTrivia (triviaNode.GetTrailingTrivia());
    }

    public static string GetNewLineCharacter (this SyntaxNode node)
    {
      if (node is null)
      {
        return string.Empty;
      }

      var nodeAsString = node.ToString().Split ("(");
      if (node.ToString().Split ("(").Length < 2)
      {
        return string.Empty;
      }

      if (nodeAsString[1].Contains (c_carriageReturnLineFeed))
      {
        return c_carriageReturnLineFeed;
      }

      if (nodeAsString[1].Contains (c_lineFeed))
      {
        return c_lineFeed;
      }

      return string.Empty;
    }

    public static string GetIndentation (this SyntaxNode node)
    {
      var nodeAsString = node.ToFullString();
      return string.Join (
          "",
          nodeAsString.Select ((c, index) => nodeAsString[index..].TakeWhile (e => e.ToString() == c_whiteSpace))
              .OrderByDescending (e => e.Count())
              .First());
    }
  }
}