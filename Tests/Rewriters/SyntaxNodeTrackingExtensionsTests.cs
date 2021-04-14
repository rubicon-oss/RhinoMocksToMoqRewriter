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
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using RhinoMocksToMoqRewriter.Core.Extensions;
using RhinoMocksToMoqRewriter.Core.Rewriters;
using Model = Microsoft.CodeAnalysis.ModelExtensions;
using SyntaxNode = Microsoft.CodeAnalysis.SyntaxNodeExtensions;

namespace RhinoMocksToMoqRewriter.Tests.Rewriters
{
  [TestFixture]
  public class SyntaxNodeTrackingExtensionsTests
  {
    private const string c_annotationId = "Id";

    private readonly Context _context =
        new Context
        {
            //language=C#
            ClassContext =
                @"private ITestInterface _mock = MockRepository.GenerateMock<ITestInterface>();"
        };

    [Test]
    public void TrackNode_Returns_Annotated_Node ()
    {
      //language=C#
      const string source = @"_mock.VerifyAllExpectations();";
      var (_, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var trackedNode = node.TrackNode (node);

      Assert.IsNotNull (trackedNode.GetAnnotations (c_annotationId).SingleOrDefault());
    }

    [Test]
    public void TrackNodes_Returns_Annotated_Nodes ()
    {
      //language=C#
      const string source = @"_mock.VerifyAllExpectations();";
      var (_, node) = CompiledSourceFileProvider.CompileMethodDeclarationWithContext (source, _context);

      var trackedNodes = node.TrackNodes (node.Body!.Statements!);

      Assert.IsNotEmpty (trackedNodes.GetAnnotatedNodes (c_annotationId));
    }

    [Test]
    public void GetOriginalNode_Returns_Valid_Node ()
    {
      //language=C#
      const string source = @"_mock.VerifyAllExpectations();";
      var (model, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);
      var trackedNode = node.TrackNode (node);

      var originalNode = trackedNode.GetOriginalNode (trackedNode);
      Microsoft.CodeAnalysis.ISymbol symbol = null;

      Assert.IsEmpty (originalNode.GetAnnotations (c_annotationId));
      Assert.DoesNotThrow (() => { symbol = Model.GetSymbolInfo (model, originalNode.Expression).Symbol; });
      Assert.IsNotNull (symbol);
    }

    [Test]
    public void GetOriginalNode_Returns_Null_On_Not_Tracked_Node ()
    {
      // language=C#
      const string source = @"_mock.VerifyAllExpectations();";
      var (_, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);

      Assert.IsNull (node.GetOriginalNode (node));
    }

    [Test]
    public void GetCurrentNode_Returns_Modified_Node ()
    {
      // language=C#
      const string source = @"_mock.VerifyAllExpectations();";
      var (_, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);

      var trackedNode = node.TrackNode (node);
      var expectedNode = SyntaxNode.ReplaceNode (trackedNode, trackedNode.GetFirstIdentifierName(), SyntaxFactory.IdentifierName ("_"));

      var actualNode = expectedNode.GetCurrentNode (trackedNode);

      Assert.That (expectedNode.IsEquivalentTo (actualNode, false));
    }

    [Test]
    public void GetCurrentNode_Returns_Null_On_Not_Tracked_Node ()
    {
      // language=C#
      const string source = @"_mock.VerifyAllExpectations();";
      var (_, node) = CompiledSourceFileProvider.CompileExpressionStatementWithContext (source, _context);

      Assert.IsNull (node.GetCurrentNode (node));
    }
  }
}