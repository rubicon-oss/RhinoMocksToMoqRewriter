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

namespace RhinoMocksToMoqRewriter.Core.Rewriters
{
  public static class SyntaxNodeTrackingExtensions
  {
    private static readonly Dictionary<SyntaxAnnotation, WeakReference<SyntaxNode>> s_syntaxNodeDictionary = new();
    private const string c_annotationId = "Id";

    public static TRoot TrackNode<TRoot> (this TRoot root, SyntaxNode node)
        where TRoot : SyntaxNode
    {
      return root.TrackNodes (new[] { node });
    }

    public static TRoot TrackNodes<TRoot> (this TRoot root, IEnumerable<SyntaxNode> nodes)
        where TRoot : SyntaxNode
    {
      var trackedNodes = Microsoft.CodeAnalysis.SyntaxNodeExtensions.TrackNodes (root, nodes);
      foreach (var node in nodes)
      {
        var currentNode = trackedNodes.GetCurrentNode (node)!;
        var annotations = currentNode.GetAnnotations (c_annotationId).ToList();
        var syntaxNodeWeakReference = annotations.Select (a => s_syntaxNodeDictionary.GetValueOrDefault (a)).FirstOrDefault (r => r is not null);

        foreach (var annotation in annotations.Where (a => !s_syntaxNodeDictionary.ContainsKey (a)))
        {
          s_syntaxNodeDictionary.Add (annotation, syntaxNodeWeakReference ?? new WeakReference<SyntaxNode> (node));
        }
      }

      return trackedNodes;
    }

    public static T? GetOriginalNode<T> (this SyntaxNode root, T trackedNode)
        where T : SyntaxNode
    {
      var annotation = trackedNode.GetAnnotations (c_annotationId).FirstOrDefault();
      if (annotation is not null && s_syntaxNodeDictionary[annotation].TryGetTarget (out var target))
      {
        return (T) target;
      }

      return null;
    }

    public static T? GetCurrentNode<T> (this SyntaxNode root, T trackedNode)
        where T : SyntaxNode
    {
      var currentNode = Microsoft.CodeAnalysis.SyntaxNodeExtensions.GetCurrentNode (root, trackedNode);
      if (currentNode != null)
      {
        return currentNode;
      }

      var originalNode = root.GetOriginalNode (trackedNode);
      if (originalNode == null)
      {
        return null;
      }

      return Microsoft.CodeAnalysis.SyntaxNodeExtensions.GetCurrentNode (root, originalNode);
    }
  }
}