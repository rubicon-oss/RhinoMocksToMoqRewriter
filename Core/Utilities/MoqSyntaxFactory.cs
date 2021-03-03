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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RhinoMocksToMoqRewriter.Core.Utilities
{
  public static class MoqSyntaxFactory
  {
    public static ArgumentSyntax MockBehaviorStrictArgument () =>
        SyntaxFactory.Argument (
            SyntaxFactory.MemberAccessExpression (
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName ("MockBehavior"),
                    SyntaxFactory.IdentifierName ("Strict"))
                .WithOperatorToken (
                    SyntaxFactory.Token (SyntaxKind.DotToken)));

    public static ObjectCreationExpressionSyntax MockCreationExpression (
        TypeArgumentListSyntax typeArgumentList,
        ArgumentListSyntax? argumentList = null,
        InitializerExpressionSyntax? initializer = null) =>
        SyntaxFactory.ObjectCreationExpression (
                SyntaxFactory.GenericName ("Mock")
                    .WithTypeArgumentList (typeArgumentList)
                    .WithLeadingTrivia (SyntaxFactory.Space))
            .WithArgumentList (argumentList)
            .WithInitializer (initializer);

    public static InitializerExpressionSyntax CallBaseInitializer () =>
        SyntaxFactory.InitializerExpression (
            SyntaxKind.ObjectInitializerExpression,
            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax> (
                SyntaxFactory.AssignmentExpression (
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName ("CallBase")
                        .WithTrailingTrivia (SyntaxFactory.Space)
                        .WithLeadingTrivia (SyntaxFactory.Space),
                    SyntaxFactory.LiteralExpression (SyntaxKind.TrueLiteralExpression)
                        .WithTrailingTrivia (SyntaxFactory.Space)
                        .WithLeadingTrivia (SyntaxFactory.Space))));

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
  }
}