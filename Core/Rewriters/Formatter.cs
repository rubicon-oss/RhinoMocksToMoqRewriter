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
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhinoMocksToMoqRewriter.Core.Extensions;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class Formatter : IFormatter
  {
    [Pure]
    public SyntaxNode Format (SyntaxNode node)
    {
      if (node is ObjectCreationExpressionSyntax objectCreationNode)
      {
        var formattedNode = FormatObjectCreationExpression (objectCreationNode);
        return formattedNode;
      }

      if (node is ArgumentListSyntax argumentListNode)
      {
        var indentation = argumentListNode.GetIndentation();
        var newLineCharacter = argumentListNode.GetNewLineCharacter();
        var formattedNode = FormatArgumentList (argumentListNode, indentation, newLineCharacter);
        return formattedNode;
      }

      if (node is FieldDeclarationSyntax fieldDeclarationNode)
      {
        var formattedVariableDeclaration = FormatVariableDeclaration (fieldDeclarationNode.Declaration);
        return fieldDeclarationNode.WithDeclaration (formattedVariableDeclaration);
      }

      return node;
    }

    [Pure]
    private VariableDeclarationSyntax FormatVariableDeclaration (VariableDeclarationSyntax node)
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

    [Pure]
    private TypeSyntax FormatType (TypeSyntax node, string newLineCharacter)
    {
      return newLineCharacter == string.Empty
          ? node.WithTrailingTrivia (SyntaxFactory.Space)
          : node;
    }

    [Pure]
    private SeparatedSyntaxList<VariableDeclaratorSyntax> FormatVariableDeclarator (
        SeparatedSyntaxList<VariableDeclaratorSyntax> variables,
        string indentation,
        string newLineCharacter)
    {
      return newLineCharacter == string.Empty
          ? FormatSingleLineSyntaxNodeList (variables)
          : FormatMultiLineSyntaxNodeList (variables, indentation, newLineCharacter);
    }

    [Pure]
    private ObjectCreationExpressionSyntax FormatObjectCreationExpression (ObjectCreationExpressionSyntax node)
    {
      var newLineCharacter = node.ArgumentList!.GetNewLineCharacter();
      var indentation = node.ArgumentList!.GetIndentation();

      var formattedArgumentList = FormatArgumentList (node.ArgumentList!, indentation, newLineCharacter);
      var genericNameSyntax = node.DescendantNodes().FirstOrDefault (n => n.IsKind (SyntaxKind.GenericName)) as GenericNameSyntax;
      if (genericNameSyntax == null)
      {
        return node.WithArgumentList (formattedArgumentList);
      }

      var formattedObjectInitializer = FormatObjectInitializer (node.Initializer, indentation, newLineCharacter);

      return SyntaxFactory.ObjectCreationExpression (
              SyntaxFactory.GenericName (genericNameSyntax.Identifier)
                  .WithTypeArgumentList (
                      genericNameSyntax.TypeArgumentList
                          .WithoutTrivia())
                  .WithLeadingTrivia (SyntaxFactory.Space))
          .WithArgumentList (formattedArgumentList)
          .WithInitializer (formattedObjectInitializer);
    }

    [Pure]
    private ArgumentListSyntax FormatArgumentList (ArgumentListSyntax argumentList, string indentation, string newLineCharacter)
    {
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

    [Pure]
    private SeparatedSyntaxList<SyntaxNode> FormatMultiLineSyntaxNodeList (
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

    [Pure]
    private SeparatedSyntaxList<SyntaxNode> FormatSingleLineSyntaxNodeList (SeparatedSyntaxList<SyntaxNode> separatedSyntaxList)
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

    [Pure]
    private InitializerExpressionSyntax? FormatObjectInitializer (InitializerExpressionSyntax? initializer, string indentation, string newLineCharacter)
    {
      if (initializer == null)
      {
        return initializer;
      }

      if (newLineCharacter == string.Empty)
      {
        indentation = SyntaxFactory.Space.ToString();
      }

      return initializer.WithLeadingTrivia (
          SyntaxFactory.Whitespace (newLineCharacter + indentation));
    }
  }
}