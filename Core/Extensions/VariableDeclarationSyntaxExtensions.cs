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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RhinoMocksToMoqRewriter.Core.Extensions
{
  public static class VariableDeclarationSyntaxExtensions
  {
    private const string c_carriageReturnLineFeed = "\r\n";
    private const string c_lineFeed = "\n";
    private const string c_whiteSpace = " ";

    public static string GetNewLineCharacter (this VariableDeclarationSyntax variableDeclaration)
    {
      if (variableDeclaration.ToString().Contains (c_carriageReturnLineFeed))
      {
        return c_carriageReturnLineFeed;
      }

      if (variableDeclaration.ToString().Contains (c_lineFeed))
      {
        return c_lineFeed;
      }

      return string.Empty;
    }

    public static string GetIndentation (this VariableDeclarationSyntax variableDeclaration)
    {
      if (variableDeclaration.Variables.Count == 0)
      {
        return string.Empty;
      }

      if (variableDeclaration.Variables.Count == 1)
      {
        return variableDeclaration.Variables.First().GetLeadingTrivia().ToString();
      }

      foreach (var variable in variableDeclaration.Variables)
      {
        if (variable.HasLeadingTrivia && !string.IsNullOrEmpty (variable.GetLeadingWhiteSpaces()))
        {
          return variable.GetLeadingTrivia().ToString();
        }
      }

      return c_whiteSpace;
    }
  }
}