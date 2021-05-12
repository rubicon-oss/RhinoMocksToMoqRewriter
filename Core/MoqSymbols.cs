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
namespace RhinoMocksToMoqRewriter.Core
{
  public class MoqSymbols
  {
    public MoqSymbols (Compilation compilation)
    {
      GenericMoqSymbol = compilation.GetTypeByMetadataName ("Moq.Mock`1")!;
      MoqSymbol = compilation.GetTypeByMetadataName ("Moq.Mock")!;
      MoqCallbackSymbol = compilation.GetTypeByMetadataName ("Moq.Language.ICallback")!;
      MoqReturnsSymbol = compilation.GetTypeByMetadataName ("Moq.Language.IReturns`2")!;
      MoqVerifiableSymbol = compilation.GetTypeByMetadataName ("Moq.Language.IVerifies")!;
      MoqSequenceHelperSymbol = compilation.GetTypeByMetadataName ("Moq.MockSequenceHelper")!;
    }

    #region TypeSymbols

    public INamedTypeSymbol GenericMoqSymbol { get; }
    public INamedTypeSymbol MoqCallbackSymbol { get; }
    public INamedTypeSymbol MoqSymbol { get; }
    public INamedTypeSymbol MoqReturnsSymbol { get; }
    public INamedTypeSymbol MoqVerifiableSymbol { get; }
    public INamedTypeSymbol MoqSequenceHelperSymbol { get; }

    #endregion

    #region Moq Collections

    private IReadOnlyList<ISymbol>? _allMoqSetupSymbols;

    public IReadOnlyList<ISymbol> AllMoqSetupSymbols
    {
      get
      {
        return _allMoqSetupSymbols ??= GenericMoqSymbol.GetMembers()
            .Concat (MoqCallbackSymbol.GetMembers())
            .Concat (MoqReturnsSymbol.GetMembers())
            .Concat (MoqVerifiableSymbol.GetMembers())
            .ToList()
            .AsReadOnly();
      }
    }

    private IReadOnlyList<ISymbol>? _allMockSequenceSymbols;
    public IReadOnlyList<ISymbol> AllMockSequenceSymbols
    {
      get
      {
        return _allMockSequenceSymbols ??= GenericMoqSymbol.GetMembers()
            .Concat (MoqSymbol.GetMembers())
            .Concat (MoqSequenceHelperSymbol.GetMembers())
            .ToList()
            .AsReadOnly();
      }
    }

    #endregion
  }
}