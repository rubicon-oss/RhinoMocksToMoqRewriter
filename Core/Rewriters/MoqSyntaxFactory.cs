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
          MoqSyntaxFactory.GenericName (MoqSyntaxFactory.MockIdentifier, typeArgumentList.WithLeadingTrivia (SyntaxFactory.Space)),
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

    public static ArgumentSyntax ItIsGenericArgument (TypeArgumentListSyntax typeArgumentList, LambdaExpressionSyntax lambdaExpression) =>
        SyntaxFactory.Argument (
            SyntaxFactory.InvocationExpression (
                    SyntaxFactory.MemberAccessExpression (
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName ("It"),
                        SyntaxFactory.GenericName (
                                SyntaxFactory.Identifier ("Is"))
                            .WithTypeArgumentList (typeArgumentList)))
                .WithArgumentList (
                    SyntaxFactory.ArgumentList (
                            SyntaxFactory.SingletonSeparatedList (
                                SyntaxFactory.Argument (lambdaExpression)))
                        .WithLeadingTrivia (SyntaxFactory.Space)));

    public static ArgumentSyntax SimpleArgument (ExpressionSyntax expression) =>
        SyntaxFactory.Argument (expression);

    public static ArgumentSyntax IsAnyArgument (TypeArgumentListSyntax typeArgumentList) =>
        SyntaxFactory.Argument (
            SyntaxFactory.InvocationExpression (
                SyntaxFactory.MemberAccessExpression (
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName ("It"),
                    SyntaxFactory.GenericName (
                            SyntaxFactory.Identifier ("IsAny"))
                        .WithTypeArgumentList (typeArgumentList))));

    public static ArgumentSyntax NullArgument () =>
        SyntaxFactory.Argument (
            SyntaxFactory.LiteralExpression (
                SyntaxKind.NullLiteralExpression));

    public static ArgumentSyntax IsNotNullArgument (TypeArgumentListSyntax typeArgumentList) =>
        SyntaxFactory.Argument (
            SyntaxFactory.InvocationExpression (
                SyntaxFactory.MemberAccessExpression (
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName ("It"),
                    SyntaxFactory.GenericName (
                            SyntaxFactory.Identifier ("IsNotNull"))
                        .WithTypeArgumentList (typeArgumentList))));

    public static ArgumentSyntax IsNotSameArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression) =>
        SyntaxFactory.Argument (
            SyntaxFactory.InvocationExpression (
                    SyntaxFactory.MemberAccessExpression (
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName ("It"),
                        SyntaxFactory.GenericName (
                                SyntaxFactory.Identifier ("Is"))
                            .WithTypeArgumentList (typeArgumentList)))
                .WithArgumentList (
                    SyntaxFactory.ArgumentList (
                            SyntaxFactory.SingletonSeparatedList (
                                SyntaxFactory.Argument (
                                    SyntaxFactory.SimpleLambdaExpression (
                                            SyntaxFactory.Parameter (
                                                SyntaxFactory.Identifier ("param")
                                                    .WithLeadingTrivia (SyntaxFactory.Space)))
                                        .WithExpressionBody (
                                            SyntaxFactory.BinaryExpression (
                                                SyntaxKind.NotEqualsExpression,
                                                SyntaxFactory.IdentifierName ("param")
                                                    .WithLeadingTrivia (SyntaxFactory.Space)
                                                    .WithTrailingTrivia (SyntaxFactory.Space),
                                                expression
                                                    .WithLeadingTrivia (SyntaxFactory.Space))))))
                        .WithLeadingTrivia (SyntaxFactory.Space)));

    public static ArgumentSyntax IsGreaterThanArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression) =>
        SyntaxFactory.Argument (
            SyntaxFactory.InvocationExpression (
                    SyntaxFactory.MemberAccessExpression (
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName ("It"),
                        SyntaxFactory.GenericName (
                                SyntaxFactory.Identifier ("Is"))
                            .WithTypeArgumentList (typeArgumentList)))
                .WithArgumentList (
                    SyntaxFactory.ArgumentList (
                            SyntaxFactory.SingletonSeparatedList (
                                SyntaxFactory.Argument (
                                    SyntaxFactory.SimpleLambdaExpression (
                                            SyntaxFactory.Parameter (
                                                SyntaxFactory.Identifier ("param")
                                                    .WithTrailingTrivia (SyntaxFactory.Space)))
                                        .WithExpressionBody (
                                            SyntaxFactory.BinaryExpression (
                                                SyntaxKind.GreaterThanExpression,
                                                SyntaxFactory.IdentifierName ("param")
                                                    .WithLeadingTrivia (SyntaxFactory.Space)
                                                    .WithTrailingTrivia (SyntaxFactory.Space),
                                                expression
                                                    .WithLeadingTrivia (SyntaxFactory.Space))))))
                        .WithLeadingTrivia (SyntaxFactory.Space)));

    public static ArgumentSyntax IsGreaterThanOrEqualArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression) =>
        SyntaxFactory.Argument (
            SyntaxFactory.InvocationExpression (
                    SyntaxFactory.MemberAccessExpression (
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName ("It"),
                        SyntaxFactory.GenericName (
                                SyntaxFactory.Identifier ("Is"))
                            .WithTypeArgumentList (typeArgumentList)))
                .WithArgumentList (
                    SyntaxFactory.ArgumentList (
                            SyntaxFactory.SingletonSeparatedList (
                                SyntaxFactory.Argument (
                                    SyntaxFactory.SimpleLambdaExpression (
                                            SyntaxFactory.Parameter (
                                                SyntaxFactory.Identifier ("param")
                                                    .WithTrailingTrivia (SyntaxFactory.Space)))
                                        .WithExpressionBody (
                                            SyntaxFactory.BinaryExpression (
                                                SyntaxKind.GreaterThanOrEqualExpression,
                                                SyntaxFactory.IdentifierName ("param")
                                                    .WithLeadingTrivia (SyntaxFactory.Space)
                                                    .WithTrailingTrivia (SyntaxFactory.Space),
                                                expression
                                                    .WithLeadingTrivia (SyntaxFactory.Space))))))
                        .WithLeadingTrivia (SyntaxFactory.Space)));

    public static ArgumentSyntax IsLessThanArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression) =>
        SyntaxFactory.Argument (
            SyntaxFactory.InvocationExpression (
                    SyntaxFactory.MemberAccessExpression (
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName ("It"),
                        SyntaxFactory.GenericName (
                                SyntaxFactory.Identifier ("Is"))
                            .WithTypeArgumentList (typeArgumentList)))
                .WithArgumentList (
                    SyntaxFactory.ArgumentList (
                            SyntaxFactory.SingletonSeparatedList (
                                SyntaxFactory.Argument (
                                    SyntaxFactory.SimpleLambdaExpression (
                                            SyntaxFactory.Parameter (
                                                SyntaxFactory.Identifier ("param")
                                                    .WithTrailingTrivia (SyntaxFactory.Space)))
                                        .WithExpressionBody (
                                            SyntaxFactory.BinaryExpression (
                                                SyntaxKind.LessThanExpression,
                                                SyntaxFactory.IdentifierName ("param")
                                                    .WithLeadingTrivia (SyntaxFactory.Space)
                                                    .WithTrailingTrivia (SyntaxFactory.Space),
                                                expression
                                                    .WithLeadingTrivia (SyntaxFactory.Space))))))
                        .WithLeadingTrivia (SyntaxFactory.Space)));

    public static ArgumentSyntax IsLessThanOrEqualArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression) =>
        SyntaxFactory.Argument (
            SyntaxFactory.InvocationExpression (
                    SyntaxFactory.MemberAccessExpression (
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName ("It"),
                        SyntaxFactory.GenericName (
                                SyntaxFactory.Identifier ("Is"))
                            .WithTypeArgumentList (typeArgumentList)))
                .WithArgumentList (
                    SyntaxFactory.ArgumentList (
                            SyntaxFactory.SingletonSeparatedList (
                                SyntaxFactory.Argument (
                                    SyntaxFactory.SimpleLambdaExpression (
                                            SyntaxFactory.Parameter (
                                                SyntaxFactory.Identifier ("param")
                                                    .WithTrailingTrivia (SyntaxFactory.Space)))
                                        .WithExpressionBody (
                                            SyntaxFactory.BinaryExpression (
                                                SyntaxKind.LessThanOrEqualExpression,
                                                SyntaxFactory.IdentifierName ("param")
                                                    .WithLeadingTrivia (SyntaxFactory.Space)
                                                    .WithTrailingTrivia (SyntaxFactory.Space),
                                                expression
                                                    .WithLeadingTrivia (SyntaxFactory.Space))))))
                        .WithLeadingTrivia (SyntaxFactory.Space)));

    public static ArgumentSyntax ContainsAllArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression) =>
        SyntaxFactory.Argument (
            SyntaxFactory.InvocationExpression (
                    SyntaxFactory.MemberAccessExpression (
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName ("It"),
                        SyntaxFactory.GenericName (
                                SyntaxFactory.Identifier ("Is"))
                            .WithTypeArgumentList (typeArgumentList)))
                .WithArgumentList (
                    SyntaxFactory.ArgumentList (
                        SyntaxFactory.SingletonSeparatedList (
                            SyntaxFactory.Argument (
                                SyntaxFactory.SimpleLambdaExpression (
                                        SyntaxFactory.Parameter (
                                            SyntaxFactory.Identifier ("param")
                                                .WithTrailingTrivia (SyntaxFactory.Space)))
                                    .WithExpressionBody (
                                        SyntaxFactory.InvocationExpression (
                                                SyntaxFactory.MemberAccessExpression (
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    expression,
                                                    SyntaxFactory.IdentifierName ("All")))
                                            .WithArgumentList (
                                                SyntaxFactory.ArgumentList (
                                                        SyntaxFactory.SingletonSeparatedList (
                                                            SyntaxFactory.Argument (
                                                                SyntaxFactory.MemberAccessExpression (
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.IdentifierName ("param"),
                                                                    SyntaxFactory.IdentifierName ("Contains")))))
                                                    .WithLeadingTrivia (SyntaxFactory.Space)))))))
                .WithLeadingTrivia (SyntaxFactory.Space));

    public static ArgumentSyntax IsInArgument (TypeArgumentListSyntax typeArgumentList, ExpressionSyntax expression) =>
        SyntaxFactory.Argument (
            SyntaxFactory.InvocationExpression (
                    SyntaxFactory.MemberAccessExpression (
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName ("It"),
                        SyntaxFactory.GenericName (
                                SyntaxFactory.Identifier ("Is"))
                            .WithTypeArgumentList (typeArgumentList)))
                .WithArgumentList (
                    SyntaxFactory.ArgumentList (
                        SyntaxFactory.SingletonSeparatedList (
                            SyntaxFactory.Argument (
                                SyntaxFactory.SimpleLambdaExpression (
                                        SyntaxFactory.Parameter (
                                            SyntaxFactory.Identifier ("param")
                                                .WithTrailingTrivia (SyntaxFactory.Space)))
                                    .WithExpressionBody (
                                        SyntaxFactory.InvocationExpression (
                                                SyntaxFactory.MemberAccessExpression (
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName ("param"),
                                                    SyntaxFactory.IdentifierName ("Contains")))
                                            .WithArgumentList (
                                                SyntaxFactory.ArgumentList (
                                                    SyntaxFactory.SingletonSeparatedList (
                                                        SyntaxFactory.Argument (expression)))))))))
                .WithLeadingTrivia (SyntaxFactory.Space));

    public static ExpressionStatementSyntax VerifyStatement (SyntaxToken identifierName) =>
        SyntaxFactory.ExpressionStatement (
            SyntaxFactory.InvocationExpression (
                SyntaxFactory.MemberAccessExpression (
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName (identifierName),
                    SyntaxFactory.IdentifierName ("Verify"))));

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

    public static ArgumentSyntax MockObjectArgument (IdentifierNameSyntax identifierName)
    {
      return SyntaxFactory.Argument (
          SyntaxFactory.MemberAccessExpression (
                  SyntaxKind.SimpleMemberAccessExpression,
                  identifierName,
                  SyntaxFactory.IdentifierName ("Object"))
              .WithOperatorToken (
                  SyntaxFactory.Token (SyntaxKind.DotToken)));
    }

    public static MemberAccessExpressionSyntax MockObjectExpression (IdentifierNameSyntax firstIdentifierName)
    {
      return SyntaxFactory.MemberAccessExpression (
          SyntaxKind.SimpleMemberAccessExpression,
          firstIdentifierName,
          SyntaxFactory.IdentifierName ("Object"));
    }

    public static IdentifierNameSyntax SetupIdentifierName ()
    {
      return SyntaxFactory.IdentifierName ("Setup");
    }

    public static IdentifierNameSyntax ReturnsIdentifierName ()
    {
      return SyntaxFactory.IdentifierName ("Returns");
    }

    public static IdentifierNameSyntax CallbackIdentifierName ()
    {
      return SyntaxFactory.IdentifierName ("Callback");
    }

    public static ExpressionStatementSyntax VerifiableMock (ExpressionSyntax expression)
    {
      return SyntaxFactory.ExpressionStatement (
          SyntaxFactory.InvocationExpression (
              SyntaxFactory.MemberAccessExpression (
                  SyntaxKind.SimpleMemberAccessExpression,
                  expression,
                  SyntaxFactory.IdentifierName ("Verifiable"))));
    }

    public static InvocationExpressionSyntax SetupExpression (IdentifierNameSyntax identifierName, LambdaExpressionSyntax lambdaExpression)
    {
      return SyntaxFactory.InvocationExpression (
          SyntaxFactory.MemberAccessExpression (
              SyntaxKind.SimpleMemberAccessExpression,
              identifierName,
              SyntaxFactory.IdentifierName ("Setup")
                  .WithTrailingTrivia (SyntaxFactory.Space)),
          SyntaxFactory.ArgumentList (
              SyntaxFactory.SingletonSeparatedList (
                  SyntaxFactory.Argument (
                      lambdaExpression))));
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

    public static ExpressionSyntax NotEqualOrSameBinaryExpression (ExpressionSyntax expression)
    {
      return MoqSyntaxFactory.BinaryExpression (
          SyntaxKind.NotEqualsExpression,
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
      return MoqSyntaxFactory.BinaryExpression (
          SyntaxKind.EqualsExpression,
          MoqSyntaxFactory.MemberAccessExpression (
              MoqSyntaxFactory.LambdaParameterIdentifierName,
              SyntaxFactory.IdentifierName (propertyName.ToString().Replace ("\"", ""))),
          propertyValue);
    }

    public static LambdaExpressionSyntax SimpleLambdaExpression (ExpressionSyntax expressionBody)
    {
      return LambdaExpression (expressionBody);
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

    public static InvocationExpressionSyntax InvocationExpression (ExpressionSyntax expression, ArgumentListSyntax argumentList)
    {
      return argumentList.IsEmpty()
          ? SyntaxFactory.InvocationExpression (expression, argumentList)
          : SyntaxFactory.InvocationExpression (expression, argumentList).WithLeadingTrivia (SyntaxFactory.Space);
    }

    public static MemberAccessExpressionSyntax MemberAccessExpression (ExpressionSyntax expression, SimpleNameSyntax name)
    {
      return SyntaxFactory.MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, expression, MoqSyntaxFactory.DotToken, name);
    }

    #region Private MoqSyntaxFactory

    private static BinaryExpressionSyntax BinaryExpression (SyntaxKind kind, ExpressionSyntax left, ExpressionSyntax right)
    {
      return SyntaxFactory.BinaryExpression (kind, left, right);
    }

    private static LambdaExpressionSyntax LambdaExpression (ExpressionSyntax expressionBody)
    {
      return SyntaxFactory.SimpleLambdaExpression (
          SyntaxFactory.Parameter (MoqSyntaxFactory.LambdaParameterIdentifier.WithTrailingTrivia (SyntaxFactory.Space)),
          expressionBody);
    }

    private static ArgumentListSyntax ArgumentList (ArgumentSyntax? argument = null)
    {
      return argument == null
          ? SyntaxFactory.ArgumentList()
          : SyntaxFactory.ArgumentList (SyntaxFactory.SingletonSeparatedList<ArgumentSyntax> (argument));
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
      return SyntaxFactory.TypeArgumentList (SyntaxFactory.SeparatedList<TypeSyntax> (typeArguments));
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
      return argumentList == null
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

    public static SimpleNameSyntax ExpectIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.ExpectIdentifier);

    public static IdentifierNameSyntax LambdaParameterIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.LambdaParameterIdentifier);

    private static IdentifierNameSyntax ContainsIdentifierName => SyntaxFactory.IdentifierName ("Contains");

    private static IdentifierNameSyntax AllIdentifierName => SyntaxFactory.IdentifierName ("All");

    private static IdentifierNameSyntax MockBehaviorIdentifierName => SyntaxFactory.IdentifierName ("MockBehavior");

    private static IdentifierNameSyntax StrictIdentifierName => SyntaxFactory.IdentifierName ("Strict");

    private static IdentifierNameSyntax CallBaseIdentifierName => SyntaxFactory.IdentifierName ("CallBase");

    private static IdentifierNameSyntax ArgIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.ArgIdentifier);

    private static LiteralExpressionSyntax NullLiteralExpression => SyntaxFactory.LiteralExpression (SyntaxKind.NullLiteralExpression);

    private static LiteralExpressionSyntax TrueLiteralExpression => SyntaxFactory.LiteralExpression (SyntaxKind.TrueLiteralExpression);

    private static IdentifierNameSyntax MatchesIdentifierName => SyntaxFactory.IdentifierName (MoqSyntaxFactory.MatchesIdentifier);

    private static SyntaxToken DotToken => SyntaxFactory.Token (SyntaxKind.DotToken);

    private static SyntaxToken LambdaParameterIdentifier => SyntaxFactory.Identifier ("_");

    private static SyntaxToken MockIdentifier => SyntaxFactory.Identifier ("Mock");

    private static SyntaxToken ArgIdentifier => SyntaxFactory.Identifier ("Arg");

    private static SyntaxToken MatchesIdentifier => SyntaxFactory.Identifier ("Matches");

    private static SyntaxToken ExpectIdentifier => SyntaxFactory.Identifier ("Expect");

    #endregion
  }
}