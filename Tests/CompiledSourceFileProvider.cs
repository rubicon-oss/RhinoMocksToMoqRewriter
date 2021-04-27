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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using NUnit.Framework;

namespace RhinoMocksToMoqRewriter.Tests
{
  public static class CompiledSourceFileProvider
  {
    public static (SemanticModel, ObjectCreationExpressionSyntax) CompileObjectCreationExpression (string source, bool ignoreErrors = false)
    {
      var (semanticModel, syntaxNode) = CompileInMethod ("Test", source, ignoreErrors);
      var expression = syntaxNode.DescendantNodes()
          .OfType<ObjectCreationExpressionSyntax>()
          .SingleOrDefault();

      return (semanticModel, expression);
    }

    public static (SemanticModel, ArgumentSyntax) CompileArgumentWithContext (string source, Context context, bool ignoreErrors = false)
    {
      var (semanticModel, syntaxNode) = CompileInMethodWithContext ("Test", source, context, ignoreErrors);
      var argument = syntaxNode.DescendantNodes()
                         .OfType<ArgumentSyntax>()
                         .LastOrDefault (a => a.ToString().Contains ("Arg") || a.ToString().Contains ("It") || a.ToString().Contains ("Constraints"))
                     ?? syntaxNode.DescendantNodes().OfType<ArgumentSyntax>().Last();

      return (semanticModel, argument);
    }

    public static (SemanticModel, ArgumentListSyntax) CompileArgumentList (string source, bool ignoreErrors = false)
    {
      var (semanticModel, syntaxNode) = CompileInMethod ("Test", $"test{source}", ignoreErrors);
      var argumentList = syntaxNode.DescendantNodes()
          .OfType<ArgumentListSyntax>()
          .SingleOrDefault();

      return (semanticModel, argumentList);
    }

    public static (SemanticModel, ArgumentListSyntax) CompileArgumentListWithContext (string source, Context context, bool ignoreErrors = false)
    {
      var (semanticModel, syntaxNode) = CompileInMethodWithContext ("Test", source, context, ignoreErrors);
      var fieldDeclarationSyntax = syntaxNode.DescendantNodes()
          .OfType<ArgumentListSyntax>()
          .Last();

      return (semanticModel, fieldDeclarationSyntax);
    }

    public static (SemanticModel, FieldDeclarationSyntax) CompileFieldDeclaration (string statementSource, bool ignoreErrors = false)
    {
      var (semanticModel, syntaxNode) = CompileInClass ("Test", statementSource, ignoreErrors);
      var fieldDeclaration = syntaxNode.DescendantNodes()
          .OfType<FieldDeclarationSyntax>()
          .SingleOrDefault();

      return (semanticModel, fieldDeclaration);
    }

    public static (SemanticModel, FieldDeclarationSyntax) CompileFieldDeclarationWithContext (string source, Context context, bool ignoreErrors = false)
    {
      var tempContext =
          new Context
          {
              ClassContext = context.ClassContext + Environment.NewLine + source,
              InterfaceContext = context.InterfaceContext,
              MethodContext = context.MethodContext
          };
      var (semanticModel, syntaxNode) = CompileInMethodWithContext ("Test", string.Empty, tempContext, ignoreErrors);
      var fieldDeclarationSyntax = syntaxNode.DescendantNodes()
          .OfType<FieldDeclarationSyntax>()
          .Last();

      return (semanticModel, fieldDeclarationSyntax);
    }

    public static (SemanticModel, ExpressionStatementSyntax) CompileExpressionStatement (string source, bool ignoreErrors = false)
    {
      var (semanticModel, syntaxNode) = CompileInMethod ("Test", source, ignoreErrors);
      var expression = syntaxNode.DescendantNodes()
          .OfType<ExpressionStatementSyntax>()
          .FirstOrDefault();

      return (semanticModel, expression);
    }

    public static (SemanticModel, ExpressionStatementSyntax) CompileExpressionStatementWithContext (string source, Context context, bool ignoreErrors = false)
    {
      var (semanticModel, syntaxNode) = CompileInMethodWithContext ("Test", source, context, ignoreErrors);
      var expression = syntaxNode.DescendantNodes()
          .OfType<ExpressionStatementSyntax>()
          .LastOrDefault();

      return (semanticModel, expression);
    }

    public static (SemanticModel, MethodDeclarationSyntax) CompileMethodDeclarationWithContext (string source, Context context, bool ignoreErrors = false)
    {
      var (semanticModel, node) = CompileInMethodWithContext ("Test", source, context, ignoreErrors);
      var methodDeclaration = node.DescendantNodes()
          .OfType<MethodDeclarationSyntax>()
          .LastOrDefault();

      return (semanticModel, methodDeclaration);
    }

    public static (SemanticModel, LocalDeclarationStatementSyntax) CompileLocalDeclarationStatementWithContext (string source, Context context, bool ignoreErrors = false)
    {
      var (semanticModel, syntaxNode) = CompileInMethodWithContext ("Test", source, context, ignoreErrors);
      var localDeclaration = syntaxNode.DescendantNodes()
          .OfType<LocalDeclarationStatementSyntax>()
          .LastOrDefault();

      return (semanticModel, localDeclaration);
    }

    private static (SemanticModel, SyntaxNode) CompileInMethod (string methodName, string methodContentSource, bool ignoreErrors = false)
    {
      var methodTemplate =
          $"public void {methodName} () {{\r\n" +
          $"{methodContentSource}" +
          "}";

      return CompileInClass ("TestClass", methodTemplate, ignoreErrors);
    }

    private static (SemanticModel, SyntaxNode) CompileInMethodWithContext (string methodName, string source, Context context, bool ignoreErrors = false)
    {
      var methodTemplate =
          $"public void {methodName} () {{\r\n" +
          $"{context.MethodContext}\r\n" +
          $"{source}" +
          "}";

      return CompileInClassWithContext ("TestClass", methodTemplate, context, ignoreErrors);
    }

    public static (SemanticModel, SyntaxNode) CompileCompilationUnitWithContext (string source, Context context, bool ignoreErrors = false)
    {
      context.UsingContext = source;
      source = string.Empty;

      var (semanticModel, syntaxNode) = CompileInMethodWithContext ("Test", source, context, ignoreErrors);
      return (semanticModel, syntaxNode);
    }

    public static (SemanticModel, SyntaxNode) CompileReturnStatementWithContext (string source, Context context, bool ignoreErrors = false)
    {
      var (semanticModel, node) = CompileInMethodWithContext ("Test", source, context, ignoreErrors);
      var returnStatement = node.DescendantNodes()
          .OfType<ReturnStatementSyntax>()
          .LastOrDefault();

      return (semanticModel, returnStatement);
    }

    public static (SemanticModel, SyntaxNode) CompileCompilationUnitWithNewUsingDirectives (string source, bool ignoreErrors = false)
    {
      var nameSpaceTemplate =
          $"{source} \r\n" +
          "namespace TestNameSpace{}";

      return Compile (nameSpaceTemplate, ignoreErrors);
    }

    private static (SemanticModel, SyntaxNode) CompileInClass (string className, string classContentSource, bool ignoreErrors = false)
    {
      var classTemplate =
          $"public class {className} {{\r\n" +
          $"{classContentSource}" +
          "}";

      return CompileInNameSpace ("TestNameSpace", classTemplate, ignoreErrors);
    }

    private static (SemanticModel, SyntaxNode) CompileInClassWithContext (string className, string source, Context context, bool ignoreErrors = false)
    {
      var classTemplateWithContext =
          $"public class {className} {{\r\n" +
          $"{context.ClassContext}\r\n" +
          $"{source}\r\n" +
          "}";

      return CompileInNameSpaceWithContext ("TestNameSpace", classTemplateWithContext, context, ignoreErrors);
    }

    private static (SemanticModel, SyntaxNode) CompileInNameSpace (string nameSpaceName, string nameSpaceContent, bool ignoreErrors = false)
    {
      var nameSpaceTemplate =
          "using System;\r\n" +
          "using System.Collections.Generic;\r\n" +
          "using Rhino.Mocks;\r\n" +
          "using Moq;\r\n" +
          "using MockRepository = Rhino.Mocks.MockRepository;\r\n" +
          $"namespace {nameSpaceName} {{\r\n" +
          $"{nameSpaceContent}\r\n" +
          "}";

      return Compile (nameSpaceTemplate, ignoreErrors);
    }

    private static (SemanticModel, SyntaxNode) CompileInNameSpaceWithContext (string nameSpaceName, string nameSpaceContent, Context context, bool ignoreErrors = false)
    {
      if (context.UsingContext == string.Empty)
      {
        context.UsingContext =
            "using Moq;\r\n" +
            "using MockRepository = Rhino.Mocks.MockRepository;\r\n";
      }

      var nameSpaceTemplate =
          "using System;\r\n" +
          "using System.Collections.Generic;\r\n" +
          "using System.Linq;\r\n" +
          "using Rhino.Mocks;\r\n" +
          $"{context.UsingContext}\r\n" +
          $"namespace {nameSpaceName} {{\r\n" +
          $"{context.NamespaceContext} \r\n" +
          $"public interface ITestInterface {{{context.InterfaceContext}}} \r\n" +
          $"{nameSpaceContent}\r\n" +
          "}";

      return Compile (nameSpaceTemplate, ignoreErrors);
    }

    private static (SemanticModel, SyntaxNode) Compile (string sourceCode, bool ignoreErrors = false)
    {
      var syntaxTree = CSharpSyntaxTree.ParseText (sourceCode);
      var rootNode = syntaxTree.GetRoot();
      var objectAssemblyPath = Path.GetDirectoryName (typeof (object).Assembly.Location);
      var moqAssemblyPath = Path.GetDirectoryName (typeof (Mock<>).Assembly.Location);
      var linqAssemblyPath = Path.GetDirectoryName (typeof (Expression<>).Assembly.Location);
      var systemConsolePath = Path.GetDirectoryName (typeof (Console).Assembly.Location);
      var netstandardAssemblyPath = Directory.GetParent (typeof (Enumerable).GetTypeInfo().Assembly.Location)?.FullName;
      var compilation = CSharpCompilation.Create ("TestCompilation")
          .AddReferences (
              MetadataReference.CreateFromFile (typeof (object).Assembly.Location),
              MetadataReference.CreateFromFile (TestContext.CurrentContext.TestDirectory + @"/../../../Resources/Rhino.Mocks.dll"),
              MetadataReference.CreateFromFile (Path.Combine (objectAssemblyPath!, "System.Runtime.dll")),
              MetadataReference.CreateFromFile (Path.Combine (moqAssemblyPath!, "Moq.dll")),
              MetadataReference.CreateFromFile (Path.Combine (netstandardAssemblyPath!, "netstandard.dll")),
              MetadataReference.CreateFromFile (Path.Combine (linqAssemblyPath!, "System.Linq.Expressions.dll")),
              MetadataReference.CreateFromFile (Path.Combine (linqAssemblyPath!, "System.Linq.dll")),
              MetadataReference.CreateFromFile (Path.Combine (systemConsolePath!, "System.Console.dll")))
          .AddSyntaxTrees (syntaxTree);

      compilation = compilation.WithOptions (
          compilation.Options.WithNullableContextOptions (NullableContextOptions.Disable)
              .WithOutputKind (OutputKind.DynamicallyLinkedLibrary));
      if (!ignoreErrors)
        Assert.IsEmpty (compilation.GetDiagnostics().Where (d => d.Severity == DiagnosticSeverity.Error));
      var semanticModel = compilation.GetSemanticModel (syntaxTree);

      return (semanticModel, rootNode);
    }
  }
}