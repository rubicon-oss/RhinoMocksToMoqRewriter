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
using RhinoMocksToMoqRewriter.Core.Extensions;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class SetupResultForRewriter : RewriterBase
  {
    public override SyntaxNode? VisitExpressionStatement (ExpressionStatementSyntax node)
    {
      var trackedNodes = node.TrackNodes (node.DescendantNodesAndSelf().Where (s => s.IsKind (SyntaxKind.SimpleMemberAccessExpression)), CompilationId);
      var baseCallNode = (ExpressionStatementSyntax) base.VisitExpressionStatement (trackedNodes)!;

      var setupResultForMemberAccessExpression = GetSetupResultForExpressionOrDefault (baseCallNode);

      if (setupResultForMemberAccessExpression == null)
      {
        return baseCallNode;
      }

      var expectCallExpression = baseCallNode.ReplaceNode (
          baseCallNode.GetCurrentNode (setupResultForMemberAccessExpression, CompilationId)!,
          MoqSyntaxFactory.ExpectCallMemberAccessExpression());

      return baseCallNode.WithExpression (
              Formatter.MarkWithFormatAnnotation (MoqSyntaxFactory.RepeatAnyExpressionStatement (expectCallExpression.Expression)))
          .WithLeadingAndTrailingTriviaOfNode (node);
    }

    private SyntaxNode? GetSetupResultForExpressionOrDefault (ExpressionStatementSyntax baseCallNode)
    {
      return baseCallNode.DescendantNodes()
          .Where (s => s.IsKind (SyntaxKind.SimpleMemberAccessExpression))
          .SingleOrDefault (
              s => baseCallNode.GetOriginalNode (s, CompilationId) is { } originalNode
                   && Model.GetSymbolInfo (originalNode).Symbol?.OriginalDefinition is { } symbol
                   && RhinoMocksSymbols.SetupResultForSymbols.Contains (symbol, SymbolEqualityComparer.Default));
    }
  }
}