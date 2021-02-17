﻿//  Copyright (c) rubicon IT GmbH
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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RhinoMocksToMoqRewriter.Core.Extensions
{
  public static class ArgumentListSyntaxExtensions
  {
    private const string c_carriageReturnLineFeed = "\r\n";
    private const string c_lineFeed = "\n";
    private const string c_whiteSpace = " ";

    public static bool HasMultiLineArguments (this ArgumentListSyntax argumentList)
      => argumentList.ToFullString().Contains (c_carriageReturnLineFeed) || argumentList.ToFullString().Contains (c_lineFeed);

    public static bool IsEmpty (this ArgumentListSyntax argumentList) => argumentList.Arguments.Count == 0;

    public static bool IsSingleArgumentList (this ArgumentListSyntax argumentList) => argumentList.Arguments.Count == 1;

    public static string GetNewLineCharacter (this ArgumentListSyntax argumentList)
    {
      if (argumentList.ToString().Contains (c_carriageReturnLineFeed))
      {
        return c_carriageReturnLineFeed;
      }

      if (argumentList.ToString().Contains (c_lineFeed))
      {
        return c_lineFeed;
      }

      return string.Empty;
    }

    public static string GetIndentation (this ArgumentListSyntax argumentList)
    {
      if (argumentList.IsEmpty())
      {
        return string.Empty;
      }

      if (argumentList.IsSingleArgumentList())
      {
        return argumentList.Arguments.First().GetLeadingTrivia().ToString();
      }

      foreach (var argument in argumentList.Arguments)
      {
        if (argument.HasLeadingTrivia && !string.IsNullOrEmpty (argument.GetLeadingWhiteSpaces()))
        {
          return argument.GetLeadingTrivia().ToString();
        }
      }

      return c_whiteSpace;
    }
  }
}