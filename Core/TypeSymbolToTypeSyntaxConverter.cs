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
using Microsoft.CodeAnalysis.Editing;
using RhinoMocksToMoqRewriter.Core.Rewriters;
namespace RhinoMocksToMoqRewriter.Core
{
  public static class TypeSymbolToTypeSyntaxConverter
  {
    public static TypeSyntax ConvertTypeSyntaxNodes (ITypeSymbol typeSymbol, SyntaxGenerator generator)
    {
      if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
      {
        return typeSymbol switch
        {
            IArrayTypeSymbol arrayTypeSymbol => (TypeSyntax) MoqSyntaxFactory.ArrayType (
                (TypeSyntax) generator.NullableTypeExpression (
                    ConvertTypeSyntaxNodes ((arrayTypeSymbol).ElementType, generator))),
            _ => (TypeSyntax) generator.NullableTypeExpression (ConvertTypeSyntaxNodes (((INamedTypeSymbol) typeSymbol).TypeArguments.FirstOrDefault()
                                                                                        ?? (INamedTypeSymbol) typeSymbol.OriginalDefinition, generator))
        };
      }

      if (typeSymbol.SpecialType != SpecialType.None)
      {
        try
        {
          return (PredefinedTypeSyntax) generator.TypeExpression (typeSymbol.SpecialType);
        }
        catch (Exception)
        {
          return (QualifiedNameSyntax) generator.TypeExpression (typeSymbol);
        }
      }

      return typeSymbol switch
      {
          INamedTypeSymbol { TypeArguments: { IsEmpty: true } } => SyntaxFactory.IdentifierName (typeSymbol.Name),
          IArrayTypeSymbol arrayTypeSymbol => MoqSyntaxFactory.ArrayType (ConvertTypeSyntaxNodes (arrayTypeSymbol.ElementType, generator)),
          ITypeParameterSymbol => SyntaxFactory.IdentifierName (typeSymbol.Name),
          _ => MoqSyntaxFactory.GenericName (
              SyntaxFactory.Identifier (typeSymbol.Name),
              MoqSyntaxFactory.SimpleTypeArgumentList (((INamedTypeSymbol) typeSymbol).TypeArguments.Select (s => ConvertTypeSyntaxNodes (s, generator))))
      };
    }
  }
}