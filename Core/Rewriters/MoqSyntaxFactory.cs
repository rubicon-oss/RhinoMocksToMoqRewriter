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
          MoqSyntaxFactory.GenericName (MoqSyntaxFactory.MockIdentifier, typeArgumentList).WithLeadingTrivia (SyntaxFactory.Space),
          argumentList);
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
                      SyntaxFactory.GenericName (
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
          : MoqSyntaxFactory.ArgumentList (new[] { Argument (expression), Argument (TimesExpression ((int) times)) });

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
                  Argument (expression),
                  Argument (TimesExpression (-2, times.Min, times!.Max))
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
                  SyntaxFactory.GenericName (SyntaxFactory.Identifier ("Mock"))
                      .WithTypeArgumentList (
                          SyntaxFactory.TypeArgumentList (
                              SyntaxFactory.SingletonSeparatedList (
                                  declarationType))))
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
          SyntaxFactory.IdentifierName ("Moq")
              .WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static UsingDirectiveSyntax RhinoMocksRepositoryAlias ()
    {
      return SyntaxFactory.UsingDirective (
              SyntaxFactory.QualifiedName (
                      SyntaxFactory.QualifiedName (
                          SyntaxFactory.IdentifierName ("Rhino"),
                          SyntaxFactory.IdentifierName ("Mocks")),
                      SyntaxFactory.IdentifierName ("MockRepository"))
                  .WithLeadingTrivia (SyntaxFactory.Space))
          .WithAlias (
              SyntaxFactory.NameEquals (
                  SyntaxFactory.IdentifierName ("MockRepository")
                      .WithLeadingTrivia (SyntaxFactory.Space)
                      .WithTrailingTrivia (SyntaxFactory.Space)));
    }

    public static LocalDeclarationStatementSyntax MockSequenceLocalDeclarationStatement (string number = "")
    {
      return SyntaxFactory.LocalDeclarationStatement (
          SyntaxFactory.VariableDeclaration (
              SyntaxFactory.IdentifierName ("var"),
              SyntaxFactory.SingletonSeparatedList (
                  SyntaxFactory.VariableDeclarator (
                          SyntaxFactory.Identifier ($"sequence{number}")
                              .WithLeadingTrivia (SyntaxFactory.Space)
                              .WithTrailingTrivia (SyntaxFactory.Space))
                      .WithInitializer (
                          SyntaxFactory.EqualsValueClause (
                              SyntaxFactory.ObjectCreationExpression (
                                      SyntaxFactory.IdentifierName ("MockSequence")
                                          .WithLeadingTrivia (SyntaxFactory.Space))
                                  .WithLeadingTrivia (SyntaxFactory.Space)
                                  .WithArgumentList (
                                      SyntaxFactory.ArgumentList()))))));
    }

    public static InvocationExpressionSyntax InSequenceExpression (IdentifierNameSyntax identifierName, string number = "")
    {
      return SyntaxFactory.InvocationExpression (
          SyntaxFactory.MemberAccessExpression (
              SyntaxKind.SimpleMemberAccessExpression,
              identifierName,
              SyntaxFactory.IdentifierName ("InSequence")),
          SyntaxFactory.ArgumentList (
                  SyntaxFactory.SingletonSeparatedList (
                      SyntaxFactory.Argument (
                          SyntaxFactory.IdentifierName ($"sequence{number}"))))
              .WithLeadingTrivia (SyntaxFactory.Space));
    }

    public static BinaryExpressionSyntax EqualOrSameBinaryExpression (ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.BinaryExpression (
          SyntaxKind.EqualsExpression,
          MoqSyntaxFactory.LambdaParameterIdentifierName.WithTrailingTrivia (SyntaxFactory.Space),
          expression.WithLeadingTrivia (SyntaxFactory.Space));
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
      return LambdaExpression (MoqSyntaxFactory.LambdaParameterIdentifier, expressionBody.WithLeadingTrivia (SyntaxFactory.Space));
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

    public static MemberAccessExpressionSyntax SimpleMemberAccessExpression (ExpressionSyntax expression, SimpleNameSyntax name)
    {
      return MoqSyntaxFactory.MemberAccessExpression (expression, name);
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
                  SyntaxFactory.IdentifierName ("Repeat")),
              SyntaxFactory.IdentifierName ("Any")));
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

    #region Private MoqSyntaxFactory

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

    private static ExpressionStatementSyntax ExpressionStatement (ExpressionSyntax expression)
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

    public static IdentifierNameSyntax ExpectIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.ExpectIdentifier);

    private static LiteralExpressionSyntax NullLiteralExpression => SyntaxFactory.LiteralExpression (SyntaxKind.NullLiteralExpression);

    private static LiteralExpressionSyntax TrueLiteralExpression => SyntaxFactory.LiteralExpression (SyntaxKind.TrueLiteralExpression);

    private static PredefinedTypeSyntax ObjectKeyword => SyntaxFactory.PredefinedType (SyntaxFactory.Token (SyntaxKind.ObjectKeyword));

    private static LiteralExpressionSyntax NumericLiteralExpression (int times)
    {
      return SyntaxFactory.LiteralExpression (
          SyntaxKind.NumericLiteralExpression,
          SyntaxFactory.Literal (times));
    }

    public static IdentifierNameSyntax LambdaParameterIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.LambdaParameterIdentifier);

    public static IdentifierNameSyntax SetupIdentifierName => SyntaxFactory.IdentifierName ("Setup");

    public static IdentifierNameSyntax ReturnsIdentifierName => SyntaxFactory.IdentifierName ("Returns");

    public static IdentifierNameSyntax CallbackIdentifierName => SyntaxFactory.IdentifierName ("Callback");

    public static IdentifierNameSyntax ThrowsIdentifierName => SyntaxFactory.IdentifierName ("Throws");

    private static IdentifierNameSyntax ContainsIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.ContainsIdentifier);

    private static IdentifierNameSyntax VerifiableIdentifierName => SyntaxFactory.IdentifierName ("Verifiable");

    private static IdentifierNameSyntax AllIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.AllIdentifier);

    private static IdentifierNameSyntax MockBehaviorIdentifierName => SyntaxFactory.IdentifierName ("MockBehavior");

    private static IdentifierNameSyntax StrictIdentifierName => SyntaxFactory.IdentifierName ("Strict");

    private static IdentifierNameSyntax CallBaseIdentifierName => SyntaxFactory.IdentifierName ("CallBase");

    private static IdentifierNameSyntax RhinoIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.RhinoIdentifier);

    private static IdentifierNameSyntax MocksIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.MocksIdentifier);

    private static IdentifierNameSyntax MatchesIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.MatchesIdentifier);

    private static IdentifierNameSyntax CallIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.CallIdentifier);

    private static IdentifierNameSyntax TimesIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.TimesIdentifier);

    private static IdentifierNameSyntax NeverIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.NeverIdentifier);

    private static IdentifierNameSyntax OnceIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.OnceIdentifier);

    private static IdentifierNameSyntax ExactlyIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.ExactlyIdentifier);

    private static IdentifierNameSyntax AtLeastOnceIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.AtLeastOnceIdentifier);

    private static SimpleNameSyntax BetweenIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.BetweenIdentifier);

    private static IdentifierNameSyntax VerifyIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.VerifyIdentifier);

    private static IdentifierNameSyntax ObjectIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.ObjectIdentifier);

    private static IdentifierNameSyntax ItIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.ItIdentifier);

    private static IdentifierNameSyntax IsIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.IsIdentifier);

    private static SimpleNameSyntax EqualsIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.EqualsIdentifier);

    private static SimpleNameSyntax ReferenceEqualsIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.ReferenceEqualsIdentifier);

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

    #endregion
  }
}