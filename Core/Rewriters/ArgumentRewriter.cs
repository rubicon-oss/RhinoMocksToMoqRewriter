﻿//  Copyright (c) rubicon IT GmbH
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
using RhinoMocksToMoqRewriter.Core.Rewriters.Strategies.ArgumentStrategies;

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public class ArgumentRewriter : RewriterBase
  {
    public override SyntaxNode? VisitArgument (ArgumentSyntax node)
    {
      var trackedNodes = node.TrackNodes (node.DescendantNodesAndSelf().Where (s => s.IsKind (SyntaxKind.Argument)), CompilationId);
      var baseCallNode = (ArgumentSyntax) base.VisitArgument (trackedNodes)!;
      var strategy = ArgumentRewriteStrategyFactory.GetRewriteStrategy (baseCallNode.GetOriginalNode (baseCallNode, CompilationId)!, Model, RhinoMocksSymbols);
      return baseCallNode.WithExpression (strategy.Rewrite (baseCallNode).Expression);
    }
  }
}