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
  public static class MoqSyntaxFactory
  {
    public static ObjectCreationExpressionSyntax MockCreationExpression (TypeArgumentListSyntax typeArgumentList, ArgumentListSyntax? argumentList = null)
    {
      return MoqSyntaxFactory.ObjectCreationExpression (
              MoqSyntaxFactory.GenericName (MoqSyntaxFactory.MockIdentifier, typeArgumentList),
              argumentList)
          .WithLeadingTrivia (SyntaxFactory.Space);
    }

    public static ObjectCreationExpressionSyntax PartialMockCreationExpression (TypeArgumentListSyntax typeArgumentList, ArgumentListSyntax? argumentList = null)
    {
      return MockCreationExpression (typeArgumentList, argumentList).WithInitializer (MoqSyntaxFactory.CallBaseInitializer());
    }

    public static ObjectCreationExpressionSyntax StrictMockCreationExpression (TypeArgumentListSyntax typeArgumentList, ArgumentListSyntax? argumentList = null)
    {
      argumentList ??= MoqSyntaxFactory.ArgumentList();
      argumentList.Arguments.Insert (0, MoqSyntaxFactory.MockBehaviorStrictArgument());
      var argumentListWithStrictMockArgument = MoqSyntaxFactory.ArgumentList (argumentList.Arguments.Insert (0, MoqSyntaxFactory.MockBehaviorStrictArgument()));
      return MockCreationExpression (typeArgumentList, argumentListWithStrictMockArgument);
    }

    public static ArgumentSyntax ItIsGenericArgument (TypeArgumentListSyntax typeArgumentList, LambdaExpressionSyntax lambdaExpression)
    {
      return MoqSyntaxFactory.Argument (
          MoqSyntaxFactory.InvocationExpression (
              MoqSyntaxFactory.MemberAccessExpression (
                  MoqSyntaxFactory.ItIdentifierName,
                  MoqSyntaxFactory.GenericName (
                      MoqSyntaxFactory.IsIdentifier,
                      typeArgumentList)),
              MoqSyntaxFactory.ArgumentList (
                      MoqSyntaxFactory.Argument (lambdaExpression))
                  .WithLeadingTrivia (SyntaxFactory.Space)));
    }

    public static ArgumentSyntax IsAnyArgument (TypeArgumentListSyntax typeArgumentList)
    {
      return MoqSyntaxFactory.Argument (
          MoqSyntaxFactory.InvocationExpression (
              MoqSyntaxFactory.MemberAccessExpression (
                  MoqSyntaxFactory.ItIdentifierName,
                  MoqSyntaxFactory.GenericName (
                      MoqSyntaxFactory.IsAnyIdentifier,
                      typeArgumentList))));
    }

    public static ArgumentSyntax NullArgument ()
    {
      return SyntaxFactory.Argument (MoqSyntaxFactory.NullLiteralExpression);
    }

    public static ArgumentSyntax IsNotNullArgument (TypeArgumentListSyntax typeArgumentList)
    {
      return MoqSyntaxFactory.Argument (
          MoqSyntaxFactory.InvocationExpression (
              MoqSyntaxFactory.MemberAccessExpression (
                  MoqSyntaxFactory.ItIdentifierName,
                  MoqSyntaxFactory.GenericName (
                      MoqSyntaxFactory.IsNotNullIdentifier,
                      typeArgumentList))));
    }

    public static ArgumentSyntax IsNotSameArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.Argument (
          MoqSyntaxFactory.InvocationExpression (
              MoqSyntaxFactory.MemberAccessExpression (
                  MoqSyntaxFactory.ItIdentifierName,
                  MoqSyntaxFactory.GenericName (MoqSyntaxFactory.IsIdentifier, typeArgumentList)),
              MoqSyntaxFactory.ArgumentList (
                      MoqSyntaxFactory.Argument (
                          MoqSyntaxFactory.SimpleLambdaExpression (
                              MoqSyntaxFactory.Not (MoqSyntaxFactory.ReferenceEquals (expression)))))
                  .WithLeadingTrivia (SyntaxFactory.Space)));
    }

    public static ArgumentSyntax IsNotEqualArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.Argument (
          MoqSyntaxFactory.InvocationExpression (
              MoqSyntaxFactory.MemberAccessExpression (
                  MoqSyntaxFactory.ItIdentifierName,
                  MoqSyntaxFactory.GenericName (MoqSyntaxFactory.IsIdentifier, typeArgumentList)),
              MoqSyntaxFactory.ArgumentList (
                      MoqSyntaxFactory.Argument (
                          MoqSyntaxFactory.SimpleLambdaExpression (
                              MoqSyntaxFactory.Not (MoqSyntaxFactory.Equals (expression)))))
                  .WithLeadingTrivia (SyntaxFactory.Space)));
    }

    public static ArgumentSyntax IsGreaterThanArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.Argument (
              MoqSyntaxFactory.InvocationExpression (
                  MoqSyntaxFactory.MemberAccessExpression (
                      MoqSyntaxFactory.ItIdentifierName,
                      MoqSyntaxFactory.GenericName (
                          MoqSyntaxFactory.IsIdentifier,
                          typeArgumentList)),
                  MoqSyntaxFactory.ArgumentList (
                      MoqSyntaxFactory.Argument (
                          MoqSyntaxFactory.SimpleLambdaExpression (
                              MoqSyntaxFactory.GreaterThanBinaryExpression (
                                  expression))))))
          .WithLeadingTrivia (SyntaxFactory.Space);
    }

    public static ArgumentSyntax IsGreaterThanOrEqualArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.Argument (
          MoqSyntaxFactory.InvocationExpression (
              MoqSyntaxFactory.MemberAccessExpression (
                  MoqSyntaxFactory.ItIdentifierName,
                  MoqSyntaxFactory.GenericName (
                      MoqSyntaxFactory.IsIdentifier,
                      typeArgumentList)),
              MoqSyntaxFactory.ArgumentList (
                      MoqSyntaxFactory.Argument (
                          MoqSyntaxFactory.SimpleLambdaExpression (
                              MoqSyntaxFactory.GreaterThanOrEqualBinaryExpression (
                                  expression))))
                  .WithLeadingTrivia (SyntaxFactory.Space)));
    }

    public static ArgumentSyntax IsLessThanArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.Argument (
          MoqSyntaxFactory.InvocationExpression (
              MoqSyntaxFactory.MemberAccessExpression (
                  MoqSyntaxFactory.ItIdentifierName,
                  MoqSyntaxFactory.GenericName (
                      MoqSyntaxFactory.IsIdentifier,
                      typeArgumentList)),
              MoqSyntaxFactory.ArgumentList (
                      MoqSyntaxFactory.Argument (
                          MoqSyntaxFactory.SimpleLambdaExpression (
                              MoqSyntaxFactory.LessThanBinaryExpression (
                                  expression))))
                  .WithLeadingTrivia (SyntaxFactory.Space)));
    }

    public static ArgumentSyntax IsLessThanOrEqualArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.Argument (
          MoqSyntaxFactory.InvocationExpression (
              MoqSyntaxFactory.MemberAccessExpression (
                  MoqSyntaxFactory.ItIdentifierName,
                  MoqSyntaxFactory.GenericName (
                      MoqSyntaxFactory.IsIdentifier,
                      typeArgumentList)),
              MoqSyntaxFactory.ArgumentList (
                      MoqSyntaxFactory.Argument (
                          MoqSyntaxFactory.SimpleLambdaExpression (
                              MoqSyntaxFactory.LessThanOrEqualBinaryExpression (
                                  expression))))
                  .WithLeadingTrivia (SyntaxFactory.Space)));
    }

    public static ArgumentSyntax ContainsAllArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.Argument (
          MoqSyntaxFactory.InvocationExpression (
                  MoqSyntaxFactory.MemberAccessExpression (
                      MoqSyntaxFactory.ItIdentifierName,
                      MoqSyntaxFactory.GenericName (
                          MoqSyntaxFactory.IsIdentifier,
                          typeArgumentList)),
                  MoqSyntaxFactory.ArgumentList (
                      MoqSyntaxFactory.Argument (
                          MoqSyntaxFactory.SimpleLambdaExpression (
                              MoqSyntaxFactory.InvocationExpression (
                                  MoqSyntaxFactory.MemberAccessExpression (
                                      expression,
                                      MoqSyntaxFactory.AllIdentifierName),
                                  MoqSyntaxFactory.ArgumentList (
                                          MoqSyntaxFactory.Argument (
                                              MoqSyntaxFactory.MemberAccessExpression (
                                                  MoqSyntaxFactory.LambdaParameterIdentifierName,
                                                  MoqSyntaxFactory.ContainsIdentifierName)))
                                      .WithLeadingTrivia (SyntaxFactory.Space))))))
              .WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static ArgumentSyntax IsInArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.Argument (
          MoqSyntaxFactory.InvocationExpression (
                  MoqSyntaxFactory.MemberAccessExpression (
                      MoqSyntaxFactory.ItIdentifierName,
                      MoqSyntaxFactory.GenericName (
                          MoqSyntaxFactory.IsIdentifier,
                          typeArgumentList)),
                  MoqSyntaxFactory.ArgumentList (
                      MoqSyntaxFactory.Argument (
                          MoqSyntaxFactory.SimpleLambdaExpression (
                              MoqSyntaxFactory.InvocationExpression (
                                  MoqSyntaxFactory.MemberAccessExpression (
                                      MoqSyntaxFactory.LambdaParameterIdentifierName,
                                      MoqSyntaxFactory.ContainsIdentifierName),
                                  MoqSyntaxFactory.ArgumentList (
                                      MoqSyntaxFactory.Argument (expression)))))))
              .WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static ArgumentSyntax SimpleArgument (ExpressionSyntax expression)
    {
      return SyntaxFactory.Argument (expression);
    }

    public static ExpressionStatementSyntax VerifyExpressionStatement (IdentifierNameSyntax identifierName)
    {
      return MoqSyntaxFactory.ExpressionStatement (
          MoqSyntaxFactory.VerifyExpression (identifierName));
    }

    public static ExpressionSyntax VerifyExpression (IdentifierNameSyntax identifierName, ExpressionSyntax? expression = null, int? times = null)
    {
      var argumentList = expression is null || times == null
          ? null
          : MoqSyntaxFactory.ArgumentList (
              new[]
              {
                  MoqSyntaxFactory.Argument (expression),
                  MoqSyntaxFactory.Argument (TimesExpression ((int) times)).WithLeadingTrivia (SyntaxFactory.Space)
              });

      return MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (
              identifierName,
              MoqSyntaxFactory.VerifyIdentifierName),
          argumentList);
    }

    public static ExpressionSyntax VerifyExpression (IdentifierNameSyntax identifierName, ExpressionSyntax expression, (int Min, int Max) times)
    {
      return MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (
              identifierName,
              MoqSyntaxFactory.VerifyIdentifierName),
          MoqSyntaxFactory.ArgumentList (
              new[]
              {
                  MoqSyntaxFactory.Argument (expression),
                  MoqSyntaxFactory.Argument (TimesExpression (-2, times.Min, times!.Max).WithLeadingTrivia (SyntaxFactory.Space))
              }));
    }

    public static FieldDeclarationSyntax MockFieldDeclaration (
        SyntaxList<AttributeListSyntax> attributeList,
        SyntaxTokenList modifiers,
        TypeSyntax declarationType,
        IEnumerable<VariableDeclaratorSyntax> variableDeclarators)
    {
      return SyntaxFactory.FieldDeclaration (
          attributeList,
          modifiers,
          SyntaxFactory.VariableDeclaration (
                  MoqSyntaxFactory.GenericName (
                      MoqSyntaxFactory.MockIdentifier,
                      MoqSyntaxFactory.TypeArgumentList (declarationType)))
              .WithTrailingTrivia (SyntaxFactory.Space)
              .WithVariables (
                  SyntaxFactory.SeparatedList (
                      variableDeclarators)));
    }

    public static MemberAccessExpressionSyntax MockObjectExpression (IdentifierNameSyntax identifierName)
    {
      return MoqSyntaxFactory.MemberAccessExpression (
          identifierName,
          MoqSyntaxFactory.ObjectIdentifierName);
    }

    public static InvocationExpressionSyntax VerifiableMock (ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (
              expression,
              MoqSyntaxFactory.VerifiableIdentifierName));
    }

    public static InvocationExpressionSyntax ProtectedMock (IdentifierNameSyntax identifierName)
    {
      return MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (
              identifierName,
              MoqSyntaxFactory.ProtectedIdentifierName));
    }

    public static InvocationExpressionSyntax SetupExpression (IdentifierNameSyntax mockIdentifierName, LambdaExpressionSyntax lambdaExpression)
    {
      return MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (
              mockIdentifierName,
              MoqSyntaxFactory.SetupIdentifierName
                  .WithTrailingTrivia (SyntaxFactory.Space)),
          MoqSyntaxFactory.SimpleArgumentList (
              lambdaExpression));
    }

    public static UsingDirectiveSyntax MoqUsingDirective ()
    {
      return SyntaxFactory.UsingDirective (
          MoqSyntaxFactory.MoqIdentifierName
              .WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static UsingDirectiveSyntax MoqProtectedUsingDirective ()
    {
      return SyntaxFactory.UsingDirective (
          SyntaxFactory.QualifiedName (
                  MoqSyntaxFactory.MoqIdentifierName,
                  MoqSyntaxFactory.ProtectedIdentifierName)
              .WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static UsingDirectiveSyntax RhinoMocksRepositoryAlias ()
    {
      return SyntaxFactory.UsingDirective (
              SyntaxFactory.QualifiedName (
                      SyntaxFactory.QualifiedName (
                          MoqSyntaxFactory.RhinoIdentifierName,
                          MoqSyntaxFactory.MocksIdentifierName),
                      MoqSyntaxFactory.MockRepositoryIdentifierName)
                  .WithLeadingTrivia (SyntaxFactory.Space))
          .WithAlias (
              SyntaxFactory.NameEquals (
                  MoqSyntaxFactory.MockRepositoryIdentifierName
                      .WithLeadingTrivia (SyntaxFactory.Space)
                      .WithTrailingTrivia (SyntaxFactory.Space)));
    }

    public static LocalDeclarationStatementSyntax MockSequenceLocalDeclarationStatement (int? number)
    {
      return SyntaxFactory.LocalDeclarationStatement (
          SyntaxFactory.VariableDeclaration (
              MoqSyntaxFactory.VarIdentifierName,
              SyntaxFactory.SingletonSeparatedList (
                  SyntaxFactory.VariableDeclarator (
                          SyntaxFactory.Identifier ($"sequence{number}")
                              .WithLeadingTrivia (SyntaxFactory.Space)
                              .WithTrailingTrivia (SyntaxFactory.Space))
                      .WithInitializer (
                          SyntaxFactory.EqualsValueClause (
                              MoqSyntaxFactory.ObjectCreationExpression (
                                      MoqSyntaxFactory.MockSequenceIdentifierName
                                          .WithLeadingTrivia (SyntaxFactory.Space),
                                      MoqSyntaxFactory.ArgumentList())
                                  .WithLeadingTrivia (SyntaxFactory.Space))))));
    }

    public static InvocationExpressionSyntax InSequenceExpression (IdentifierNameSyntax identifierName, int? number)
    {
      return SyntaxFactory.InvocationExpression (
          SyntaxFactory.MemberAccessExpression (
              SyntaxKind.SimpleMemberAccessExpression,
              identifierName,
              MoqSyntaxFactory.InSequenceIdentifierName),
          MoqSyntaxFactory.ArgumentList (
                  MoqSyntaxFactory.Argument (
                      SyntaxFactory.IdentifierName ($"sequence{number}")))
              .WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static ExpressionSyntax GreaterThanBinaryExpression (ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.BinaryExpression (
          SyntaxKind.GreaterThanExpression,
          MoqSyntaxFactory.LambdaParameterIdentifierName.WithTrailingTrivia (SyntaxFactory.Space),
          expression.WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static ExpressionSyntax GreaterThanOrEqualBinaryExpression (ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.BinaryExpression (
          SyntaxKind.GreaterThanOrEqualExpression,
          MoqSyntaxFactory.LambdaParameterIdentifierName.WithTrailingTrivia (SyntaxFactory.Space),
          expression.WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static ExpressionSyntax LessThanBinaryExpression (ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.BinaryExpression (
          SyntaxKind.LessThanExpression,
          MoqSyntaxFactory.LambdaParameterIdentifierName.WithTrailingTrivia (SyntaxFactory.Space),
          expression.WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static ExpressionSyntax LessThanOrEqualBinaryExpression (ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.BinaryExpression (
          SyntaxKind.LessThanOrEqualExpression,
          MoqSyntaxFactory.LambdaParameterIdentifierName.WithTrailingTrivia (SyntaxFactory.Space),
          expression.WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static ExpressionSyntax NullBinaryExpression ()
    {
      return MoqSyntaxFactory.BinaryExpression (
          SyntaxKind.EqualsExpression,
          MoqSyntaxFactory.LambdaParameterIdentifierName.WithTrailingTrivia (SyntaxFactory.Space),
          MoqSyntaxFactory.NullLiteralExpression);
    }

    public static ExpressionSyntax NotNullBinaryExpression ()
    {
      return MoqSyntaxFactory.BinaryExpression (
          SyntaxKind.NotEqualsExpression,
          MoqSyntaxFactory.LambdaParameterIdentifierName.WithTrailingTrivia (SyntaxFactory.Space),
          MoqSyntaxFactory.NullLiteralExpression.WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static ExpressionSyntax IsInInvocationExpression (ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (
              MoqSyntaxFactory.LambdaParameterIdentifierName,
              MoqSyntaxFactory.ContainsIdentifierName),
          MoqSyntaxFactory.ArgumentList (MoqSyntaxFactory.Argument (expression)));
    }

    public static ExpressionSyntax ContainsAllInvocationExpression (ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (expression, MoqSyntaxFactory.AllIdentifierName),
          MoqSyntaxFactory.ArgumentList (
              MoqSyntaxFactory.Argument (
                  MoqSyntaxFactory.MemberAccessExpression (MoqSyntaxFactory.LambdaParameterIdentifierName, MoqSyntaxFactory.ContainsIdentifierName))));
    }

    public static ExpressionSyntax LogicalAndBinaryExpression (ExpressionSyntax left, ExpressionSyntax right)
    {
      return MoqSyntaxFactory.BinaryExpression (SyntaxKind.LogicalAndExpression, left, right);
    }

    public static ExpressionSyntax LogicalOrBinaryExpression (ExpressionSyntax left, ExpressionSyntax right)
    {
      return MoqSyntaxFactory.BinaryExpression (SyntaxKind.LogicalOrExpression, left, right);
    }

    public static ExpressionSyntax PropertyValueBinaryExpression (ExpressionSyntax propertyName, ExpressionSyntax propertyValue)
    {
      return MoqSyntaxFactory.LogicalAndBinaryExpression (
          MoqSyntaxFactory.NotNullBinaryExpression(),
          MoqSyntaxFactory.Equals (
                  propertyValue,
                  MoqSyntaxFactory.PropertyMemberAccessExpression (propertyName))
              .WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static LambdaExpressionSyntax SimpleLambdaExpression (ExpressionSyntax expressionBody)
    {
      return MoqSyntaxFactory.LambdaExpression (MoqSyntaxFactory.LambdaParameterIdentifier, expressionBody.WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static ArgumentListSyntax SimpleArgumentList (ArgumentSyntax argument)
    {
      return SyntaxFactory.ArgumentList (SyntaxFactory.SingletonSeparatedList (argument));
    }

    public static ArgumentListSyntax SimpleArgumentList (ExpressionSyntax expression)
    {
      return SyntaxFactory.ArgumentList (SyntaxFactory.SingletonSeparatedList (MoqSyntaxFactory.Argument (expression)));
    }

    public static ArgumentListSyntax SimpleArgumentList (IEnumerable<ArgumentSyntax> arguments)
    {
      return SyntaxFactory.ArgumentList (SyntaxFactory.SeparatedList (arguments));
    }

    public static ExpressionSyntax NestedMemberAccessExpression (
        ExpressionSyntax expression,
        IReadOnlyList<(SimpleNameSyntax name, ArgumentListSyntax argumentList)> accessors)
    {
      if (accessors.Count < 1)
      {
        throw new ArgumentException ("Accessor list must be at least of length 1.");
      }

      var access = MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (expression, accessors[0].name),
          accessors[0].argumentList);

      return accessors.Count == 1
          ? access
          : NestedMemberAccessExpression (access, accessors.Skip (1).ToList());
    }

    public static TypeArgumentListSyntax SimpleTypeArgumentList (IEnumerable<TypeSyntax> typeArguments)
    {
      return MoqSyntaxFactory.TypeArgumentList (typeArguments);
    }

    public static TypeArgumentListSyntax SimpleTypeArgumentList (TypeSyntax typeArguments)
    {
      return MoqSyntaxFactory.TypeArgumentList (new[] { typeArguments });
    }

    public static GenericNameSyntax GenericName (SyntaxToken identifier, TypeArgumentListSyntax? typeArgumentList = null)
    {
      return typeArgumentList == null
          ? SyntaxFactory.GenericName (identifier)
          : SyntaxFactory.GenericName (identifier, typeArgumentList);
    }

    public static ArgumentSyntax MatchesArgument (TypeSyntax type, ArgumentSyntax argument)
    {
      return MoqSyntaxFactory.Argument (
          MoqSyntaxFactory.InvocationExpression (
              MoqSyntaxFactory.MemberAccessExpression (
                  MoqSyntaxFactory.GenericName (MoqSyntaxFactory.ArgIdentifier, MoqSyntaxFactory.TypeArgumentList (new[] { type })),
                  MoqSyntaxFactory.MatchesIdentifierName),
              MoqSyntaxFactory.ArgumentList (argument)));
    }

    public static InvocationExpressionSyntax InvocationExpression (ExpressionSyntax expression, ArgumentListSyntax? argumentList = null)
    {
      return argumentList == null || argumentList.IsEmpty()
          ? SyntaxFactory.InvocationExpression (expression)
          : SyntaxFactory.InvocationExpression (expression, argumentList.WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static MemberAccessExpressionSyntax MemberAccessExpression (ExpressionSyntax expression, SimpleNameSyntax name)
    {
      return SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, expression, MoqSyntaxFactory.DotToken, name);
    }

    public static UsingDirectiveSyntax RhinoMocksUsing ()
    {
      return SyntaxFactory.UsingDirective (
          SyntaxFactory.QualifiedName (MoqSyntaxFactory.RhinoIdentifierName, MoqSyntaxFactory.MocksIdentifierName)
              .WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static InvocationExpressionSyntax ExpectCallExpression (ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (MoqSyntaxFactory.ExpectIdentifierName, MoqSyntaxFactory.CallIdentifierName),
          MoqSyntaxFactory.ArgumentList (MoqSyntaxFactory.Argument (MoqSyntaxFactory.ParenthesizedLambdaExpression (expression))));
    }

    public static TypeArgumentListSyntax TypeArgumentList (TypeSyntax type)
    {
      return SyntaxFactory.TypeArgumentList (SyntaxFactory.SingletonSeparatedList (type));
    }

    public static MemberAccessExpressionSyntax ExpectCallMemberAccessExpression ()
    {
      return MoqSyntaxFactory.MemberAccessExpression (ExpectIdentifierName, CallIdentifierName)
          .WithTrailingTrivia (SyntaxFactory.Space);
    }

    public static InvocationExpressionSyntax RepeatAnyExpressionStatement (ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (
              MoqSyntaxFactory.MemberAccessExpression (
                  expression,
                  MoqSyntaxFactory.RepeatIdentifierName),
              MoqSyntaxFactory.AnyIdentifierName));
    }

    public static TypeSyntax ArrayType (TypeSyntax type)
    {
      return SyntaxFactory.ArrayType (
          type,
          SyntaxFactory.SingletonList (
              SyntaxFactory.ArrayRankSpecifier (
                  SyntaxFactory.SingletonSeparatedList<ExpressionSyntax> (
                      SyntaxFactory.OmittedArraySizeExpression()))));
    }

    public static PrefixUnaryExpressionSyntax Not (ExpressionSyntax expression)
    {
      return SyntaxFactory.PrefixUnaryExpression (SyntaxKind.LogicalNotExpression, expression);
    }

    public static InvocationExpressionSyntax Equals (ExpressionSyntax right, ExpressionSyntax? left = null)
    {
      left ??= MoqSyntaxFactory.LambdaParameterIdentifierName;
      return MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (
              MoqSyntaxFactory.ObjectKeyword,
              MoqSyntaxFactory.EqualsIdentifierName),
          MoqSyntaxFactory.ArgumentList (
              new[]
              {
                  MoqSyntaxFactory.Argument (left),
                  MoqSyntaxFactory.Argument (right).WithLeadingTrivia (SyntaxFactory.Space)
              }));
    }

    public static InvocationExpressionSyntax ReferenceEquals (ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (
              MoqSyntaxFactory.ObjectKeyword,
              MoqSyntaxFactory.ReferenceEqualsIdentifierName),
          MoqSyntaxFactory.ArgumentList (
              new[]
              {
                  MoqSyntaxFactory.Argument (MoqSyntaxFactory.LambdaParameterIdentifierName),
                  MoqSyntaxFactory.Argument (expression).WithLeadingTrivia (SyntaxFactory.Space)
              }));
    }

    public static MemberAccessExpressionSyntax PropertyMemberAccessExpression (ExpressionSyntax propertyName)
    {
      return MoqSyntaxFactory.MemberAccessExpression (
          MoqSyntaxFactory.LambdaParameterIdentifierName,
          SyntaxFactory.IdentifierName (propertyName.ToString().Replace ("\"", "")));
    }

    #region Annotations

    public static SyntaxAnnotation VerifyAnnotation (SyntaxNode? currentNode = null, object? times = null)
    {
      return new SyntaxAnnotation (MoqSyntaxFactory.VerifyAnnotationKind, $"{times?.ToString()}");
    }

    public static string VerifyAnnotationKind => "Verify";

    #endregion

    private static BinaryExpressionSyntax BinaryExpression (SyntaxKind kind, ExpressionSyntax left, ExpressionSyntax right)
    {
      return SyntaxFactory.BinaryExpression (kind, left.WithLeadingTrivia (SyntaxFactory.Space), right.WithTrailingTrivia (SyntaxFactory.Space));
    }

    private static LambdaExpressionSyntax LambdaExpression (SyntaxToken parameter, ExpressionSyntax expressionBody)
    {
      return SyntaxFactory.SimpleLambdaExpression (
          SyntaxFactory.Parameter (parameter.WithTrailingTrivia (SyntaxFactory.Space)),
          expressionBody);
    }

    private static ArgumentListSyntax ArgumentList (ArgumentSyntax? argument = null)
    {
      return argument == null
          ? SyntaxFactory.ArgumentList()
          : SyntaxFactory.ArgumentList (SyntaxFactory.SingletonSeparatedList (argument));
    }

    private static ArgumentListSyntax ArgumentList (IEnumerable<ArgumentSyntax> arguments)
    {
      return SyntaxFactory.ArgumentList (SyntaxFactory.SeparatedList (arguments));
    }

    private static ArgumentSyntax Argument (ExpressionSyntax expression)
    {
      return SyntaxFactory.Argument (expression);
    }

    private static TypeArgumentListSyntax TypeArgumentList (IEnumerable<TypeSyntax> typeArguments)
    {
      return SyntaxFactory.TypeArgumentList (SyntaxFactory.SeparatedList (typeArguments));
    }

    private static InitializerExpressionSyntax ObjectInitializerExpression (ExpressionSyntax expression)
    {
      return SyntaxFactory.InitializerExpression (SyntaxKind.ObjectInitializerExpression, SyntaxFactory.SingletonSeparatedList (expression));
    }

    private static AssignmentExpressionSyntax AssignmentExpression (ExpressionSyntax left, ExpressionSyntax right)
    {
      return SyntaxFactory.AssignmentExpression (SyntaxKind.SimpleAssignmentExpression, left, right);
    }

    private static ObjectCreationExpressionSyntax ObjectCreationExpression (TypeSyntax type, ArgumentListSyntax? argumentList = null)
    {
      return argumentList == null || argumentList.IsEmpty()
          ? SyntaxFactory.ObjectCreationExpression (type).WithArgumentList (argumentList)
          : SyntaxFactory.ObjectCreationExpression (type).WithArgumentList (argumentList).WithLeadingTrivia (SyntaxFactory.Space);
    }

    private static InitializerExpressionSyntax CallBaseInitializer ()
    {
      return MoqSyntaxFactory.ObjectInitializerExpression (
          MoqSyntaxFactory.AssignmentExpression (
              MoqSyntaxFactory.CallBaseIdentifierName.WithTrailingTrivia (SyntaxFactory.Space).WithLeadingTrivia (SyntaxFactory.Space),
              MoqSyntaxFactory.TrueLiteralExpression.WithTrailingTrivia (SyntaxFactory.Space).WithLeadingTrivia (SyntaxFactory.Space)));
    }

    private static ArgumentSyntax MockBehaviorStrictArgument ()
    {
      return MoqSyntaxFactory.Argument (MoqSyntaxFactory.MemberAccessExpression (MoqSyntaxFactory.MockBehaviorIdentifierName, MoqSyntaxFactory.StrictIdentifierName));
    }

    public static ExpressionStatementSyntax ExpressionStatement (ExpressionSyntax expression)
    {
      return SyntaxFactory.ExpressionStatement (expression);
    }

    private static ExpressionSyntax TimesExpression (int times, int min = 0, int max = 0)
    {
      var timesIdentifierName = times switch
      {
          -2 => BetweenIdentifierName,
          -1 => AtLeastOnceIdentifierName,
          0 => NeverIdentifierName,
          1 => OnceIdentifierName,
          _ => ExactlyIdentifierName
      };

      return MoqSyntaxFactory.InvocationExpression (
          MoqSyntaxFactory.MemberAccessExpression (
              TimesIdentifierName,
              timesIdentifierName),
          times switch
          {
              -1 or 0 or 1 => null,
              -2 => MoqSyntaxFactory.ArgumentList (new[] { Argument (NumericLiteralExpression (min)), Argument (NumericLiteralExpression (max)).WithLeadingTrivia (SyntaxFactory.Space) }),
              _ => MoqSyntaxFactory.ArgumentList (Argument (NumericLiteralExpression (times)))
          });
    }

    private static ParenthesizedLambdaExpressionSyntax ParenthesizedLambdaExpression (ExpressionSyntax expression)
    {
      return SyntaxFactory.ParenthesizedLambdaExpression (expression);
    }

    private static LiteralExpressionSyntax NumericLiteralExpression (int times)
    {
      return SyntaxFactory.LiteralExpression (
          SyntaxKind.NumericLiteralExpression,
          SyntaxFactory.Literal (times));
    }

    #region Properties

    public static TypeSyntax VarType => MoqSyntaxFactory.VarIdentifierName;

    public static IdentifierNameSyntax ExpectIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.ExpectIdentifier);
    public static IdentifierNameSyntax LambdaParameterIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.LambdaParameterIdentifier);
    public static IdentifierNameSyntax SetupIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.SetupIdentifier);
    public static IdentifierNameSyntax ReturnsIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.ReturnsIdentifier);
    public static IdentifierNameSyntax CallbackIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.CallbackIdentifier);
    public static IdentifierNameSyntax ThrowsIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.ThrowsIdentifier);
    public static IdentifierNameSyntax ContainsIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.ContainsIdentifier);
    public static IdentifierNameSyntax VerifiableIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.VerifiableIdentifier);
    public static IdentifierNameSyntax AllIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.AllIdentifier);
    public static IdentifierNameSyntax MockBehaviorIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.MockBehaviorIdentifier);
    public static IdentifierNameSyntax StrictIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.StrictIdentifier);
    public static IdentifierNameSyntax CallBaseIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.CallBaseIdentifier);
    public static IdentifierNameSyntax RhinoIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.RhinoIdentifier);
    public static IdentifierNameSyntax MocksIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.MocksIdentifier);
    public static IdentifierNameSyntax MatchesIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.MatchesIdentifier);
    public static IdentifierNameSyntax CallIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.CallIdentifier);
    public static IdentifierNameSyntax TimesIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.TimesIdentifier);
    public static IdentifierNameSyntax NeverIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.NeverIdentifier);
    public static IdentifierNameSyntax OnceIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.OnceIdentifier);
    public static IdentifierNameSyntax ExactlyIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.ExactlyIdentifier);
    public static IdentifierNameSyntax AtLeastOnceIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.AtLeastOnceIdentifier);
    public static IdentifierNameSyntax BetweenIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.BetweenIdentifier);
    public static IdentifierNameSyntax VerifyIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.VerifyIdentifier);
    public static IdentifierNameSyntax ObjectIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.ObjectIdentifier);
    public static IdentifierNameSyntax ItIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.ItIdentifier);
    public static IdentifierNameSyntax IsIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.IsIdentifier);
    public static IdentifierNameSyntax EqualsIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.EqualsIdentifier);
    public static IdentifierNameSyntax ReferenceEqualsIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.ReferenceEqualsIdentifier);
    public static IdentifierNameSyntax MoqIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.MoqIdentifier);
    public static TypeSyntax VarIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.VarIdentifier);
    public static IdentifierNameSyntax InSequenceIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.InSequenceIdentifier);
    public static IdentifierNameSyntax MockSequenceIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.MockSequenceIdentifier);
    public static IdentifierNameSyntax MockRepositoryIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.MockRepositoryIdentifier);
    public static IdentifierNameSyntax RepeatIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.RepeatIdentifier);
    public static IdentifierNameSyntax AnyIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.AnyIdentifier);
    public static IdentifierNameSyntax ProtectedIdentifierName { get; } = SyntaxFactory.IdentifierName (MoqSyntaxFactory.ProtectedIdentifier);

    private static LiteralExpressionSyntax NullLiteralExpression { get; } = SyntaxFactory.LiteralExpression (SyntaxKind.NullLiteralExpression);
    public static LiteralExpressionSyntax TrueLiteralExpression { get; } = SyntaxFactory.LiteralExpression (SyntaxKind.TrueLiteralExpression);

    private static PredefinedTypeSyntax ObjectKeyword { get; } = SyntaxFactory.PredefinedType (SyntaxFactory.Token (SyntaxKind.ObjectKeyword));

    private static SyntaxToken DotToken => SyntaxFactory.Token (SyntaxKind.DotToken);
    private static SyntaxToken LambdaParameterIdentifier => SyntaxFactory.Identifier ("_");
    private static SyntaxToken MockIdentifier => SyntaxFactory.Identifier ("Mock");
    private static SyntaxToken CallIdentifier => SyntaxFactory.Identifier ("Call");
    private static SyntaxToken ArgIdentifier => SyntaxFactory.Identifier ("Arg");
    private static SyntaxToken MatchesIdentifier => SyntaxFactory.Identifier ("Matches");
    private static SyntaxToken RhinoIdentifier => SyntaxFactory.Identifier ("Rhino");
    private static SyntaxToken MocksIdentifier => SyntaxFactory.Identifier ("Mocks");
    private static SyntaxToken TimesIdentifier => SyntaxFactory.Identifier ("Times");
    private static SyntaxToken NeverIdentifier => SyntaxFactory.Identifier ("Never");
    private static SyntaxToken OnceIdentifier => SyntaxFactory.Identifier ("Once");
    private static SyntaxToken ExactlyIdentifier => SyntaxFactory.Identifier ("Exactly");
    private static SyntaxToken BetweenIdentifier => SyntaxFactory.Identifier ("Between");
    private static SyntaxToken AtLeastOnceIdentifier => SyntaxFactory.Identifier ("AtLeastOnce");
    private static SyntaxToken VerifyIdentifier => SyntaxFactory.Identifier ("Verify");
    private static SyntaxToken ExpectIdentifier => SyntaxFactory.Identifier ("Expect");
    private static SyntaxToken ItIdentifier => SyntaxFactory.Identifier ("It");
    private static SyntaxToken IsIdentifier => SyntaxFactory.Identifier ("Is");
    private static SyntaxToken IsAnyIdentifier => SyntaxFactory.Identifier ("IsAny");
    private static SyntaxToken IsNotNullIdentifier => SyntaxFactory.Identifier ("IsNotNull");
    private static SyntaxToken ContainsIdentifier => SyntaxFactory.Identifier ("Contains");
    private static SyntaxToken AllIdentifier => SyntaxFactory.Identifier ("All");
    private static SyntaxToken EqualsIdentifier => SyntaxFactory.Identifier ("Equals");
    private static SyntaxToken ReferenceEqualsIdentifier => SyntaxFactory.Identifier ("ReferenceEquals");
    private static SyntaxToken ObjectIdentifier => SyntaxFactory.Identifier ("Object");
    private static SyntaxToken VarIdentifier => SyntaxFactory.Identifier (SyntaxFactory.TriviaList(), SyntaxKind.VarKeyword, "var", "var", SyntaxFactory.TriviaList());
    private static SyntaxToken MoqIdentifier => SyntaxFactory.Identifier ("Moq");
    private static SyntaxToken CallBaseIdentifier => SyntaxFactory.Identifier ("CallBase");
    private static SyntaxToken StrictIdentifier => SyntaxFactory.Identifier ("Strict");
    private static SyntaxToken MockBehaviorIdentifier => SyntaxFactory.Identifier ("MockBehavior");
    private static SyntaxToken VerifiableIdentifier => SyntaxFactory.Identifier ("Verifiable");
    private static SyntaxToken ThrowsIdentifier => SyntaxFactory.Identifier ("Throws");
    private static SyntaxToken CallbackIdentifier => SyntaxFactory.Identifier ("Callback");
    private static SyntaxToken ReturnsIdentifier => SyntaxFactory.Identifier ("Returns");
    private static SyntaxToken SetupIdentifier => SyntaxFactory.Identifier ("Setup");
    private static SyntaxToken InSequenceIdentifier => SyntaxFactory.Identifier ("InSequence");
    private static SyntaxToken MockSequenceIdentifier => SyntaxFactory.Identifier ("MockSequence");
    private static SyntaxToken MockRepositoryIdentifier => SyntaxFactory.Identifier ("MockRepository");
    private static SyntaxToken RepeatIdentifier => SyntaxFactory.Identifier ("Repeat");
    private static SyntaxToken AnyIdentifier => SyntaxFactory.Identifier ("Any");
    private static SyntaxToken ProtectedIdentifier => SyntaxFactory.Identifier ("Protected");

    #endregion
  }
}