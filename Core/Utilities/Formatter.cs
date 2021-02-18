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

namespace RhinoMocksToMoqRewriter.Core.Utilities
{
  public class Formatter : IFormatter
  {
    [Pure]
    public SyntaxNode Format<T> (T node)
        where T : SyntaxNode
    {
      if (node is ObjectCreationExpressionSyntax objectCreationNode)
      {
        var formattedNode = FormatObjectCreationExpression (objectCreationNode);
        return formattedNode;
      }

      return node;
    }

    [Pure]
    private ObjectCreationExpressionSyntax FormatObjectCreationExpression (ObjectCreationExpressionSyntax node)
    {
      var formattedArgumentList = FormatArgumentList (node.ArgumentList!);

      var genericNameSyntax = node.DescendantNodes().FirstOrDefault (n => n.IsKind (SyntaxKind.GenericName)) as GenericNameSyntax;
      if (genericNameSyntax == null)
      {
        return node.WithArgumentList (formattedArgumentList);
      }

      return SyntaxFactory.ObjectCreationExpression (
              SyntaxFactory.GenericName (genericNameSyntax.Identifier)
                  .WithTypeArgumentList (
                      genericNameSyntax.TypeArgumentList
                          .WithoutTrivia())
                  .WithLeadingTrivia (SyntaxFactory.Space))
          .WithArgumentList (
              formattedArgumentList);
    }

    [Pure]
    private ArgumentListSyntax FormatArgumentList (ArgumentListSyntax argumentList)
    {
      if (argumentList.IsEmpty())
      {
        return argumentList;
      }

      return argumentList.HasMultiLineArguments()
          ? FormatMultiLineArgumentList (argumentList)
          : FormatSingleLineArgumentList (argumentList);
    }

    [Pure]
    private ArgumentListSyntax FormatMultiLineArgumentList (ArgumentListSyntax argumentList)
    {
      var arguments = SyntaxFactory.SeparatedList<ArgumentSyntax>().AddRange (argumentList.Arguments);
      var newLineCharacter = argumentList.GetNewLineCharacter();
      var indentation = argumentList.GetIndentation();

      for (var i = 0; i < arguments.Count; i++)
      {
        var argument = arguments[i];
        arguments = arguments
            .Replace (
                argument,
                argument
                    .WithLeadingTrivia (
                        SyntaxFactory.Whitespace (newLineCharacter + indentation)));
      }

      return SyntaxFactory.ArgumentList (arguments).WithLeadingTrivia (SyntaxFactory.Space);
    }

    [Pure]
    private ArgumentListSyntax FormatSingleLineArgumentList (ArgumentListSyntax argumentList)
    {
      var arguments = SyntaxFactory.SeparatedList<ArgumentSyntax>().AddRange (argumentList.Arguments);
      for (var i = 1; i < arguments.Count; i++)
      {
        var argument = arguments[i];
        arguments = arguments
            .Replace (
                argument,
                argument.WithLeadingTrivia (
                    SyntaxFactory.Space));
      }

      return SyntaxFactory.ArgumentList (arguments).WithLeadingTrivia (SyntaxFactory.Space);
    }
  }
}