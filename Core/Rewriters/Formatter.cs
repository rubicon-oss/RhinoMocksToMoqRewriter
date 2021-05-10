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
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Extensions;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class Formatter : CSharpSyntaxRewriter, IFormatter
  {
    private static readonly SyntaxAnnotation s_formatAnnotation = new SyntaxAnnotation ("FormatMePleeeease");

    public T Format<T> (T node) where T : SyntaxNode
    {
      return (T) base.Visit (node);
    }

    public static SyntaxTree FormatAnnotatedNodes (SyntaxTree tree, Workspace workspace)
    {
      var root = tree.GetCompilationUnitRoot();
      var formattedRoot = Microsoft.CodeAnalysis.Formatting.Formatter.Format (root, s_formatAnnotation, workspace, workspace.Options);
      return tree.WithFilePath (tree.FilePath).WithRootAndOptions (formattedRoot, tree.Options);
    }

    public static T MarkWithFormatAnnotation<T> (T node)
        where T : SyntaxNode
    {
      return node.WithAdditionalAnnotations (s_formatAnnotation);
    }

    public override SyntaxNode? VisitObjectCreationExpression (ObjectCreationExpressionSyntax node)
    {
      var baseCallNode = (ObjectCreationExpressionSyntax) base.VisitObjectCreationExpression (node)!;
      return FormatObjectCreationExpression (baseCallNode);
    }

    public override SyntaxNode? VisitArgumentList (ArgumentListSyntax node)
    {
      var baseCallNode = (ArgumentListSyntax) base.VisitArgumentList (node)!;
      return FormatArgumentList (baseCallNode);
    }

    public override SyntaxNode? VisitFieldDeclaration (FieldDeclarationSyntax node)
    {
      var baseCallNode = (FieldDeclarationSyntax) base.VisitFieldDeclaration (node)!;
      return baseCallNode.WithDeclaration (FormatVariableDeclaration (baseCallNode.Declaration));
    }

    public override SyntaxNode? VisitExpressionStatement (ExpressionStatementSyntax node)
    {
      var baseCallNode = (ExpressionStatementSyntax) base.VisitExpressionStatement (node)!;
      baseCallNode = RemoveRedundantWhitespaces (baseCallNode);

      if (baseCallNode.Expression is not InvocationExpressionSyntax || !IsMultiLineStatement (node))
      {
        return baseCallNode;
      }

      return baseCallNode.WithExpression (FormatInvocationExpression ((InvocationExpressionSyntax) node.Expression));
    }

    private static ExpressionStatementSyntax RemoveRedundantWhitespaces (ExpressionStatementSyntax baseCallNode)
    {
      var nodeAsString = baseCallNode.ToFullString();
      const string? redundantSpaceBetweenParenthesesPattern = @"(?<!^).{1} {2,}\(";
      var matches = Regex.Matches (nodeAsString, redundantSpaceBetweenParenthesesPattern).Select (s => s.ToString());
      foreach (var match in matches)
      {
        var parenthesesWithFormattedWhiteSpace = $"{match.First()} {match.Last()}";
        nodeAsString = nodeAsString.Replace (match, parenthesesWithFormattedWhiteSpace);
      }

      return (ExpressionStatementSyntax) SyntaxFactory.ParseStatement (nodeAsString);
    }

    private static InvocationExpressionSyntax FormatInvocationExpression (InvocationExpressionSyntax node)
    {
      var indentation = node.Expression.GetIndentation();
      return node.Expression is not MemberAccessExpressionSyntax memberAccessExpression
          ? node
          : node.WithExpression (FormatMemberAccessExpression (memberAccessExpression, indentation)).WithoutTrailingTrivia();
    }

    private static MemberAccessExpressionSyntax FormatMemberAccessExpression (MemberAccessExpressionSyntax node, string indentation)
    {
      return MoqSyntaxFactory.MemberAccessExpression (
          node.Expression.WithTrailingTrivia (SyntaxFactory.Whitespace (Environment.NewLine + indentation)),
          node.Name);
    }

    private static bool IsMultiLineStatement (SyntaxNode node)
    {
      var nodes = node.DescendantNodes().Where (
          s => s.IsKind (SyntaxKind.SimpleMemberAccessExpression)
               && !s.Ancestors().Any (s => s.IsKind (SyntaxKind.Argument))).Select (s => (MemberAccessExpressionSyntax) s);

      return nodes.Any (
          s => s.Expression.GetTrailingTrivia().ToFullString().Contains (Environment.NewLine)
               || s.Expression.GetLeadingTrivia().ToFullString().Contains (Environment.NewLine)
               || s.Name.GetTrailingTrivia().ToFullString().Contains (Environment.NewLine)
               || s.Name.GetLeadingTrivia().ToFullString().Contains (Environment.NewLine)
               || s.OperatorToken.TrailingTrivia.ToFullString().Contains (Environment.NewLine)
               || s.OperatorToken.LeadingTrivia.ToFullString().Contains (Environment.NewLine));
    }

    private static VariableDeclarationSyntax FormatVariableDeclaration (VariableDeclarationSyntax node)
    {
      var newLineCharacter = node.GetNewLineCharacter();
      var indentation = node.GetIndentation();
      var separatedSyntaxList = new SeparatedSyntaxList<SyntaxNode>().AddRange (node.Variables);

      var formattedType = FormatType (node.Type, newLineCharacter);
      var formattedVariables = FormatVariableDeclarator (separatedSyntaxList, indentation, newLineCharacter);

      return node
          .WithType (formattedType)
          .WithVariables (formattedVariables);
    }

    private static TypeSyntax FormatType (TypeSyntax node, string newLineCharacter)
    {
      return newLineCharacter == string.Empty
          ? node.WithTrailingTrivia (SyntaxFactory.Space)
          : node;
    }

    private static SeparatedSyntaxList<VariableDeclaratorSyntax> FormatVariableDeclarator (
        SeparatedSyntaxList<VariableDeclaratorSyntax> variables,
        string indentation,
        string newLineCharacter)
    {
      return newLineCharacter == string.Empty
          ? FormatSingleLineSyntaxNodeList (variables)
          : FormatMultiLineSyntaxNodeList (variables, indentation, newLineCharacter);
    }

    private static ObjectCreationExpressionSyntax FormatObjectCreationExpression (ObjectCreationExpressionSyntax node)
    {
      var newLineCharacter = node.ArgumentList!.GetNewLineCharacter();
      var indentation = node.ArgumentList!.GetIndentation();

      var genericNameSyntax = node.DescendantNodes().FirstOrDefault (n => n.IsKind (SyntaxKind.GenericName)) as GenericNameSyntax;
      if (genericNameSyntax == null)
      {
        return node;
      }

      var formattedObjectInitializer = FormatObjectInitializer (node.Initializer, indentation, newLineCharacter);

      return SyntaxFactory.ObjectCreationExpression (
              SyntaxFactory.GenericName (genericNameSyntax.Identifier)
                  .WithTypeArgumentList (
                      genericNameSyntax.TypeArgumentList
                          .WithoutTrivia())
                  .WithLeadingTrivia (SyntaxFactory.Space))
          .WithArgumentList (node.ArgumentList)
          .WithInitializer (formattedObjectInitializer);
    }

    private static ArgumentListSyntax FormatArgumentList (ArgumentListSyntax argumentList)
    {
      var indentation = argumentList.GetIndentation();
      var newLineCharacter = argumentList.GetNewLineCharacter();

      if (argumentList.IsEmpty())
      {
        return argumentList;
      }

      var separatedList = new SeparatedSyntaxList<SyntaxNode>().AddRange (argumentList.Arguments);
      ArgumentListSyntax newArgumentList;
      if (argumentList.Arguments.HasMultiLineItems())
      {
        var formattedList = FormatMultiLineSyntaxNodeList (separatedList, indentation, newLineCharacter);
        newArgumentList = SyntaxFactory.ArgumentList (formattedList);
      }
      else
      {
        var formattedList = FormatSingleLineSyntaxNodeList (separatedList);
        newArgumentList = SyntaxFactory.ArgumentList (formattedList);
      }

      var memberAccessExpressionNode = argumentList.GetFirstAncestorMemberAccessExpressionOrDefault();
      if (memberAccessExpressionNode != null && memberAccessExpressionNode.HasTrailingTrivia && !argumentList.HasLeadingTrivia)
      {
        return newArgumentList;
      }

      return newArgumentList.WithLeadingTrivia (SyntaxFactory.Space);
    }

    private static SeparatedSyntaxList<SyntaxNode> FormatMultiLineSyntaxNodeList (
        SeparatedSyntaxList<SyntaxNode> separatedSyntaxList,
        string indentation,
        string newLineCharacter)
    {
      for (var i = 0; i < separatedSyntaxList.Count; i++)
      {
        var argument = separatedSyntaxList[i];
        separatedSyntaxList = separatedSyntaxList
            .Replace (
                argument,
                argument
                    .WithLeadingTrivia (
                        SyntaxFactory.Whitespace (newLineCharacter + indentation)));
      }

      return separatedSyntaxList;
    }

    private static SeparatedSyntaxList<SyntaxNode> FormatSingleLineSyntaxNodeList (SeparatedSyntaxList<SyntaxNode> separatedSyntaxList)
    {
      for (var i = 1; i < separatedSyntaxList.Count; i++)
      {
        var item = separatedSyntaxList[i];
        separatedSyntaxList = separatedSyntaxList
            .Replace (
                item,
                item.WithLeadingTrivia (
                    SyntaxFactory.Space));
      }

      return separatedSyntaxList;
    }

    private static InitializerExpressionSyntax? FormatObjectInitializer (InitializerExpressionSyntax? initializer, string indentation, string newLineCharacter)
    {
      if (initializer == null)
      {
        return initializer;
      }

      if (newLineCharacter == string.Empty)
      {
        indentation = SyntaxFactory.Space.ToString();
      }

      return initializer.WithLeadingTrivia (SyntaxFactory.Whitespace (indentation));
    }
  }
}