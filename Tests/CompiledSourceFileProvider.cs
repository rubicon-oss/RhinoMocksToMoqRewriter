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
using NUnit.Framework;

namespace RhinoMocksToMoqRewriter.Tests
{
  public static class CompiledSourceFileProvider
  {
    public static (SemanticModel, LocalDeclarationStatementSyntax) CompileLocalDeclarationStatement (string source)
    {
      var (semanticModel, syntaxNode) = CompileInMethod ("Test", source);
      var localDeclaration = syntaxNode.DescendantNodes()
          .OfType<LocalDeclarationStatementSyntax>()
          .SingleOrDefault();

      return (semanticModel, localDeclaration);
    }

    public static (SemanticModel, ExpressionStatementSyntax) CompileExpressionStatement (string source)
    {
      var (semanticModel, syntaxNode) = CompileInMethod ("Test", source);
      var expression = syntaxNode.DescendantNodes()
          .OfType<ExpressionStatementSyntax>()
          .SingleOrDefault();

      return (semanticModel, expression);
    }

    public static (SemanticModel, ObjectCreationExpressionSyntax) CompileObjectCreationExpression (string source)
    {
      var (semanticModel, syntaxNode) = CompileInMethod ("Test", source);
      var expression = syntaxNode.DescendantNodes()
          .OfType<ObjectCreationExpressionSyntax>()
          .SingleOrDefault();

      return (semanticModel, expression);
    }

    public static (SemanticModel, ArgumentListSyntax) CompileArgumentList (string source)
    {
      var (semanticModel, syntaxNode) = CompileInMethod ("Test", $"test{source}");
      var argumentList = syntaxNode.DescendantNodes()
          .OfType<ArgumentListSyntax>()
          .SingleOrDefault();

      return (semanticModel, argumentList);
    }

    public static (SemanticModel, FieldDeclarationSyntax) CompileFieldDeclaration (string statementSource)
    {
      var (semanticModel, syntaxNode) = CompileInClass ("Test", statementSource);
      var fieldDeclaration = syntaxNode.DescendantNodes()
          .OfType<FieldDeclarationSyntax>()
          .SingleOrDefault();

      return (semanticModel, fieldDeclaration);
    }

    public static (SemanticModel, InvocationExpressionSyntax) CompileInvocationExpression (string statementSource)
    {
      var (semanticModel, syntaxNode) = CompileInMethod ("Test", statementSource);
      var invocationExpression = syntaxNode.DescendantNodes()
          .OfType<InvocationExpressionSyntax>()
          .SingleOrDefault();

      return (semanticModel, invocationExpression);
    }

    public static (SemanticModel, MethodDeclarationSyntax) CompileMethod (string methodSource)
    {
      var (semanticModel, syntaxNode) = CompileInClass ("TestClass", methodSource);
      var methodDeclaration = syntaxNode.DescendantNodes()
          .OfType<MethodDeclarationSyntax>()
          .SingleOrDefault();

      return (semanticModel, methodDeclaration);
    }

    public static (SemanticModel, ClassDeclarationSyntax) CompileClass (string classSource)
    {
      var (semanticModel, syntaxNode) = CompileInNameSpace ("TestNameSpace", classSource);
      var classDeclaration = syntaxNode.DescendantNodes()
          .OfType<ClassDeclarationSyntax>()
          .Single();

      return (semanticModel, classDeclaration);
    }

    public static (SemanticModel, MethodDeclarationSyntax) CompileMethodInClass (string classSource, string methodName)
    {
      var (semanticModel, syntaxNode) = CompileInNameSpace ("TestNameSpace", classSource);
      var methodSyntax = syntaxNode.DescendantNodes()
          .OfType<MethodDeclarationSyntax>()
          .Single (method => method.Identifier.ToString() == methodName);
      return (semanticModel, methodSyntax);
    }

    public static (SemanticModel, SyntaxNode) CompileInClass (string className, string classContentSource)
    {
      var classTemplate =
          $"public class {className} {{\r\n" +
          $"{classContentSource}" +
          "}";

      return CompileInNameSpace ("TestNameSpace", classTemplate);
    }

    private static (SemanticModel, SyntaxNode) CompileInMethod (string methodName, string methodContentSource)
    {
      var methodTemplate =
          $"public void {methodName} () {{\r\n" +
          $"{methodContentSource}" +
          "}";

      return CompileInClass ("TestClass", methodTemplate);
    }

    public static (SemanticModel, SyntaxNode) CompileInNameSpace (string nameSpaceName, string nameSpaceContent)
    {
      var nameSpaceTemplate =
          "using System;\r\n" +
          "using System.Collections.Generic;\r\n" +
          "using Rhino.Mocks;\r\n" +
          $"namespace {nameSpaceName} {{\r\n" +
          $"{nameSpaceContent}\r\n" +
          "}";

      return Compile (nameSpaceTemplate);
    }

    private static (SemanticModel, SyntaxNode) Compile (string sourceCode)
    {
      var syntaxTree = CSharpSyntaxTree.ParseText (sourceCode);
      var rootNode = syntaxTree.GetRoot();

      var compilation = CSharpCompilation.Create ("TestCompilation")
          .AddReferences (
              MetadataReference.CreateFromFile (typeof (object).Assembly.Location),
              MetadataReference.CreateFromFile (TestContext.CurrentContext.TestDirectory + @"/../../../Resources/Rhino.Mocks.dll"))
          .AddSyntaxTrees (syntaxTree);

      compilation = compilation.WithOptions (
          compilation.Options.WithNullableContextOptions (NullableContextOptions.Enable));

      var semanticModel = compilation.GetSemanticModel (syntaxTree);

      return (semanticModel, rootNode);
    }

    private static (SemanticModel, SyntaxNode) CompileInMethodWithContext (string methodName, string source, Context context)
    {
      var methodTemplate =
          $"public void {methodName} () {{\r\n" +
          $"{context.MethodContext}\r\n" +
          $"{source}" +
          "}";

      return CompileInClassWithContext ("TestClass", methodTemplate, context);
    }

    private static (SemanticModel, SyntaxNode) CompileInClassWithContext (string className, string source, Context context)
    {
      var classTemplateWithContext =
          $"public class {className} {{\r\n" +
          $"{context.ClassContext}\r\n" +
          $"{source}\r\n" +
          "}";

      return CompileInNameSpaceWithContext ("TestNameSpace", classTemplateWithContext, context);
    }

    public static (SemanticModel, ExpressionStatementSyntax) CompileExpressionStatementWithContext (string source, Context context)
    {
      var (semanticModel, syntaxNode) = CompileInMethodWithContext ("Test", source, context);
      var expression = syntaxNode.DescendantNodes()
          .OfType<ExpressionStatementSyntax>()
          .LastOrDefault();

      return (semanticModel, expression);
    }

    private static (SemanticModel, SyntaxNode) CompileInNameSpaceWithContext (string nameSpaceName, string nameSpaceContent, Context context)
    {
      var nameSpaceTemplate =
          "using System;\r\n" +
          "using System.Collections.Generic;\r\n" +
          "using Rhino.Mocks;\r\n" +
          $"namespace {nameSpaceName} {{\r\n" +
          $"interface ITestInterface {{{context.InterfaceContext}}} \r\n" +
          $"{nameSpaceContent}\r\n" +
          "}";

      return Compile (nameSpaceTemplate);
    }

    public static (SemanticModel, ArgumentSyntax) CompileArgument (string source)
    {
      var (semanticModel, syntaxNode) = CompileInMethod ("Test", source);
      var argument = syntaxNode.DescendantNodes()
          .OfType<ArgumentSyntax>()
          .FirstOrDefault();

      return (semanticModel, argument);
    }

    public static (SemanticModel, InvocationExpressionSyntax) CompileInvocationExpressionWithContext (string source, Context context)
    {
      var (semanticModel, syntaxNode) = CompileInMethodWithContext ("Test", source, context);
      var invocationExpression = syntaxNode.DescendantNodes()
          .OfType<InvocationExpressionSyntax>()
          .LastOrDefault();

      return (semanticModel, invocationExpression);
    }

    public static (SemanticModel, MethodDeclarationSyntax) CompileMethodDeclarationWithContext (string source, Context context)
    {
      var (semanticModel, node) = CompileInMethodWithContext ("Test", source, context);
      var methodDeclaration = node.DescendantNodes()
          .OfType<MethodDeclarationSyntax>()
          .LastOrDefault();

      return (semanticModel, methodDeclaration);
    }

    public static (SemanticModel, IEnumerable<ExpressionStatementSyntax>) CompileExpressionStatementsWithContext (string source, Context context)
    {
      var (semanticModel, syntaxNode) = CompileInMethodWithContext ("Test", source, context);
      var expressionStatements = syntaxNode.DescendantNodes()
          .OfType<ExpressionStatementSyntax>();

      return (semanticModel, expressionStatements);
    }
  }
}