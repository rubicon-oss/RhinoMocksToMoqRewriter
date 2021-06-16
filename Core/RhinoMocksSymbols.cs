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
  public class RhinoMocksSymbols
  {
    public RhinoMocksSymbols (Compilation compilation)
    {
      RhinoMocksMockRepositorySymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.MockRepository")!;
      RhinoMocksArgSymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.Arg")!;
      RhinoMocksConstraintsListArgSymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.ListArg`1")!;
      RhinoMocksGenericArgSymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.Arg`1")!;
      RhinoMocksConstraintsIsArgSymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.IsArg`1")!;
      RhinoMocksArgTextSymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.TextArg")!;
      RhinoMocksConstraintIsSymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.Is")!;
      RhinoMocksConstraintListSymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.List")!;
      RhinoMocksConstraintPropertySymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.Constraints.Property")!;
      RhinoMocksExtensionSymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.RhinoMocksExtensions")!;
      RhinoMocksIRepeatSymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.Interfaces.IRepeat`1")!;
      RhinoMocksIMethodOptionsSymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.Interfaces.IMethodOptions`1")!;
      RhinoMocksExpectSymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.Expect")!;
      RhinoMocksLastCallSymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.LastCall")!;
      RhinoMocksSetupResultSymbol = compilation.GetTypeByMetadataName ("Rhino.Mocks.SetupResult")!;
    }

    #region TypeSymbols

    public INamedTypeSymbol RhinoMocksMockRepositorySymbol { get; }
    public INamedTypeSymbol RhinoMocksArgSymbol { get; }
    public INamedTypeSymbol RhinoMocksConstraintsListArgSymbol { get; }
    public INamedTypeSymbol RhinoMocksGenericArgSymbol { get; }
    public INamedTypeSymbol RhinoMocksConstraintsIsArgSymbol { get; }
    public INamedTypeSymbol RhinoMocksArgTextSymbol { get; }
    public INamedTypeSymbol RhinoMocksConstraintIsSymbol { get; }
    public INamedTypeSymbol RhinoMocksConstraintListSymbol { get; }
    public INamedTypeSymbol RhinoMocksConstraintPropertySymbol { get; }
    public INamedTypeSymbol RhinoMocksExtensionSymbol { get; }
    public INamedTypeSymbol RhinoMocksIRepeatSymbol { get; }
    public INamedTypeSymbol RhinoMocksIMethodOptionsSymbol { get; }
    public INamedTypeSymbol RhinoMocksExpectSymbol { get; }
    public INamedTypeSymbol RhinoMocksLastCallSymbol { get; }
    public INamedTypeSymbol RhinoMocksSetupResultSymbol { get; }

    #endregion

    #region MockRepository

    private IReadOnlyList<ISymbol>? _mockRepositoryGenerateMockSymbols;
    public IReadOnlyList<ISymbol> MockRepositoryGenerateMockSymbols => _mockRepositoryGenerateMockSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("GenerateMock").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _mockRepositoryGenerateStubSymbols;
    public IReadOnlyList<ISymbol> MockRepositoryGenerateStubSymbols => _mockRepositoryGenerateStubSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("GenerateStub").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _mockRepositoryDynamicMockSymbols;
    public IReadOnlyList<ISymbol> MockRepositoryDynamicMockSymbols => _mockRepositoryDynamicMockSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("DynamicMock").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _mockRepositoryDynamicMultiMockSymbols;
    public IReadOnlyList<ISymbol> MockRepositoryDynamicMultiMockSymbols => _mockRepositoryDynamicMultiMockSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("DynamicMultiMock").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _mockRepositoryStubSymbols;
    public IReadOnlyList<ISymbol> MockRepositoryStubSymbols => _mockRepositoryStubSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("Stub").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _mockRepositoryPartialMockSymbols;
    public IReadOnlyList<ISymbol> MockRepositoryPartialMockSymbols => _mockRepositoryPartialMockSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("PartialMock").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _mockRepositoryPartialMultiMockSymbols;
    public IReadOnlyList<ISymbol> MockRepositoryPartialMultiMockSymbols => _mockRepositoryPartialMultiMockSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("PartialMultiMock").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _mockRepositoryGeneratePartialMockSymbols;
    public IReadOnlyList<ISymbol> MockRepositoryGeneratePartialMockSymbols => _mockRepositoryGeneratePartialMockSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("GeneratePartialMock").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _mockRepositoryStrictMockSymbols;
    public IReadOnlyList<ISymbol> mockRepositoryStrictMockSymbols => _mockRepositoryStrictMockSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("StrictMock").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _mockRepositoryStrictMultiMockSymbols;
    public IReadOnlyList<ISymbol> MockRepositoryStrictMultiMockSymbols => _mockRepositoryStrictMultiMockSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("StrictMultiMock").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _mockRepositoryGenerateStrictMockSymbols;
    public IReadOnlyList<ISymbol> MockRepositoryGenerateStrictMockSymbols => _mockRepositoryGenerateStrictMockSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("GenerateStrictMock").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _verifyAllSymbols;
    public IReadOnlyList<ISymbol> VerifyAllSymbols => _verifyAllSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("VerifyAll").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _replayAllSymbols;
    public IReadOnlyList<ISymbol> ReplayAllSymbols => _replayAllSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("ReplayAll").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _backToRecordAllSymbols;
    public IReadOnlyList<ISymbol> BackToRecordAllSymbols => _backToRecordAllSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("BackToRecordAll").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _orderedSymbols;
    public IReadOnlyList<ISymbol> OrderedSymbols => _orderedSymbols ??= RhinoMocksMockRepositorySymbol.GetMembers ("Ordered").ToList().AsReadOnly();

    #endregion

    #region Arguments

    #region Arg

    private IReadOnlyList<ISymbol>? _argIsSymbols;
    public IReadOnlyList<ISymbol> ArgIsSymbols => _argIsSymbols ??= RhinoMocksArgSymbol.GetMembers ("Is").ToList().AsReadOnly();

    #endregion

    #region ArgList

    private IReadOnlyList<ISymbol>? _argListEqualSymbols;
    public IReadOnlyList<ISymbol> ArgListEqualSymbols => _argListEqualSymbols ??= (_argListEqualSymbols = RhinoMocksConstraintsListArgSymbol.GetMembers ("Equal")).ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _argListIsInSymbols;
    public IReadOnlyList<ISymbol> ArgListIsInSymbols => _argListIsInSymbols ??= (_argListIsInSymbols = RhinoMocksConstraintsListArgSymbol.GetMembers ("IsIn")).ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _argListContainsAll;
    public IReadOnlyList<ISymbol> ArgListContainsAll => _argListContainsAll ??= (_argListContainsAll = RhinoMocksConstraintsListArgSymbol.GetMembers ("ContainsAll")).ToList().AsReadOnly();

    #endregion

    #region Arg<T>

    private IReadOnlyList<ISymbol>? _argMatchesSymbols;
    public IReadOnlyList<ISymbol> ArgMatchesSymbols => _argMatchesSymbols ??= (_argMatchesSymbols = RhinoMocksGenericArgSymbol.GetMembers ("Matches")).ToList().AsReadOnly();

    #endregion

    #region ArgIs

    private IReadOnlyList<ISymbol>? _argIsAnythingSymbols;
    public IReadOnlyList<ISymbol> ArgIsAnythingSymbols => _argIsAnythingSymbols ??= RhinoMocksConstraintsIsArgSymbol.GetMembers ("Anything").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _argIsEqualSymbols;
    public IReadOnlyList<ISymbol> ArgIsEqualSymbols => _argIsEqualSymbols ??= RhinoMocksConstraintsIsArgSymbol.GetMembers ("Equal").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _argIsNotEqualSymbols;
    public IReadOnlyList<ISymbol> ArgIsNotEqualSymbols => _argIsNotEqualSymbols ??= RhinoMocksConstraintsIsArgSymbol.GetMembers ("NotEqual").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _argIsSameSymbols;
    public IReadOnlyList<ISymbol> ArgIsSameSymbols => _argIsSameSymbols ??= RhinoMocksConstraintsIsArgSymbol.GetMembers ("Same").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _argIsNotSameSymbols;
    public IReadOnlyList<ISymbol> ArgIsNotSameSymbols => _argIsNotSameSymbols ??= RhinoMocksConstraintsIsArgSymbol.GetMembers ("NotSame").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _argIsNullSymbols;
    public IReadOnlyList<ISymbol> ArgIsNullSymbols => _argIsNullSymbols ??= RhinoMocksConstraintsIsArgSymbol.GetMembers ("Null").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _argIsNotNullSymbols;
    public IReadOnlyList<ISymbol> ArgIsNotNullSymbols => _argIsNotNullSymbols ??= RhinoMocksConstraintsIsArgSymbol.GetMembers ("NotNull").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _argIsGreaterThanSymbols;
    public IReadOnlyList<ISymbol> ArgIsGreaterThanSymbols => _argIsGreaterThanSymbols ??= RhinoMocksConstraintsIsArgSymbol.GetMembers ("GreaterThan").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _argIsGreaterThanOrEqualSymbols;
    public IReadOnlyList<ISymbol> ArgIsGreaterThanOrEqualSymbols => _argIsGreaterThanOrEqualSymbols ??= RhinoMocksConstraintsIsArgSymbol.GetMembers ("GreaterThanOrEqual").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _argLessThanSymbols;
    public IReadOnlyList<ISymbol> ArgIsLessThanSymbols => _argLessThanSymbols ??= RhinoMocksConstraintsIsArgSymbol.GetMembers ("LessThan").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _argLessThanOrEqualSymbols;
    public IReadOnlyList<ISymbol> ArgIsLessThaOrEqualSymbols => _argLessThanOrEqualSymbols ??= RhinoMocksConstraintsIsArgSymbol.GetMembers ("LessThanOrEqual").ToList().AsReadOnly();

    #endregion

    #region ArgText

    private IReadOnlyList<ISymbol>? _argTextLikeSymbols;
    public IReadOnlyList<ISymbol> ArgTextLikeSymbols => _argTextLikeSymbols ??= RhinoMocksArgTextSymbol.GetMembers ("Like").ToList().AsReadOnly();

    #endregion

    #endregion

    #region Constraints

    #region Is

    private IReadOnlyList<ISymbol>? _constraintIsEqualSymbols;
    public IReadOnlyList<ISymbol> ConstraintIsEqualSymbols => _constraintIsEqualSymbols ??= RhinoMocksConstraintIsSymbol.GetMembers ("Equal").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _constraintIsNotEqualSymbols;
    public IReadOnlyList<ISymbol> ConstraintIsNotEqualSymbols => _constraintIsNotEqualSymbols ??= RhinoMocksConstraintIsSymbol.GetMembers ("NotEqual").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _constraintIsSameSymbols;
    public IReadOnlyList<ISymbol> ConstraintIsSameSymbols => _constraintIsSameSymbols ??= RhinoMocksConstraintIsSymbol.GetMembers ("Same").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _constraintIsNotSameSymbols;
    public IReadOnlyList<ISymbol> ConstraintIsNotSameSymbols => _constraintIsNotSameSymbols ??= RhinoMocksConstraintIsSymbol.GetMembers ("NotSame").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _constraintIsNullSymbols;
    public IReadOnlyList<ISymbol> ConstraintIsNullSymbols => _constraintIsNullSymbols ??= RhinoMocksConstraintIsSymbol.GetMembers ("Null").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _constraintIsNotNullSymbols;
    public IReadOnlyList<ISymbol> ConstraintIsNotNullSymbols => _constraintIsNotNullSymbols ??= RhinoMocksConstraintIsSymbol.GetMembers ("NotNull").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _constraintIsGreaterThanSymbols;
    public IReadOnlyList<ISymbol> ConstraintIsGreaterThanSymbols => _constraintIsGreaterThanSymbols ??= RhinoMocksConstraintIsSymbol.GetMembers ("GreaterThan").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _constraintIsGreaterThanOrEqualSymbols;
    public IReadOnlyList<ISymbol> ConstraintIsGreaterThanOrEqualSymbols => _constraintIsGreaterThanOrEqualSymbols ??= RhinoMocksConstraintIsSymbol.GetMembers ("GreaterThanOrEqual").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _constraintIsLessThanSymbols;
    public IReadOnlyList<ISymbol> ConstraintIsLessThanSymbols => _constraintIsLessThanSymbols ??= RhinoMocksConstraintIsSymbol.GetMembers ("LessThan").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _constraintIsLessThanOrEqualSymbols;
    public IReadOnlyList<ISymbol> ConstraintIsLessThanOrEqualSymbols => _constraintIsLessThanOrEqualSymbols ??= RhinoMocksConstraintIsSymbol.GetMembers ("LessThanOrEqual").ToList().AsReadOnly();

    #endregion

    #region List

    private IReadOnlyList<ISymbol>? _constraintListIsInSymbols;
    public IReadOnlyList<ISymbol> ConstraintListIsInSymbols => _constraintListIsInSymbols ??= RhinoMocksConstraintListSymbol.GetMembers ("IsIn").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _constraintListContainsAllSymbols;
    public IReadOnlyList<ISymbol> ConstraintListContainsAllSymbols => _constraintListContainsAllSymbols ??= RhinoMocksConstraintListSymbol.GetMembers ("ContainsAll").ToList().AsReadOnly();

    #endregion

    #region Property

    private IReadOnlyList<ISymbol>? _constraintPropertyValueSymbols;
    public IReadOnlyList<ISymbol> ConstraintPropertyValueSymbols => _constraintPropertyValueSymbols ??= RhinoMocksConstraintPropertySymbol.GetMembers ("Value").ToList().AsReadOnly();

    #endregion

    #endregion

    #region RhinoMocksExtension

    private IReadOnlyList<ISymbol>? _expectSymbols;
    public IReadOnlyList<ISymbol> ExpectSymbols => _expectSymbols ??= RhinoMocksExtensionSymbol.GetMembers ("Expect").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _stubSymbols;
    public IReadOnlyList<ISymbol> StubSymbols => _stubSymbols ??= RhinoMocksExtensionSymbol.GetMembers ("Stub").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _verifyAllExpectationsSymbols;
    public IReadOnlyList<ISymbol> VerifyAllExpectationsSymbols => _verifyAllExpectationsSymbols ??= RhinoMocksExtensionSymbol.GetMembers ("VerifyAllExpectations").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _assertWasNotCalledSymbols;
    public IReadOnlyList<ISymbol> AssertWasNotCalledSymbols => _assertWasNotCalledSymbols ??= RhinoMocksExtensionSymbol.GetMembers ("AssertWasNotCalled").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _assertWasCalledSymbols;
    public IReadOnlyList<ISymbol> AssertWasCalledSymbols => _assertWasCalledSymbols ??= RhinoMocksExtensionSymbol.GetMembers ("AssertWasCalled").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _backToRecordSymbols;
    public IReadOnlyList<ISymbol> BackToRecordSymbols => _backToRecordSymbols ??= RhinoMocksExtensionSymbol.GetMembers ("BackToRecord").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _replaySymbols;
    public IReadOnlyList<ISymbol> ReplaySymbols => _replaySymbols ??= RhinoMocksExtensionSymbol.GetMembers ("Replay").ToList().AsReadOnly();

    #endregion

    #region IMethodOptions

    private IReadOnlyList<ISymbol>? _whenCalledSymbols;
    public IReadOnlyList<ISymbol> WhenCalledSymbols => _whenCalledSymbols ??= RhinoMocksIMethodOptionsSymbol.GetMembers ("WhenCalled").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _doSymbols;
    public IReadOnlyList<ISymbol> DoSymbols => _doSymbols ??= RhinoMocksIMethodOptionsSymbol.GetMembers ("Do").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _returnSymbols;
    public IReadOnlyList<ISymbol> ReturnSymbols => _returnSymbols ??= RhinoMocksIMethodOptionsSymbol.GetMembers ("Return").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _throwSymbols;
    public IReadOnlyList<ISymbol> ThrowSymbols => _throwSymbols ??= RhinoMocksIMethodOptionsSymbol.GetMembers ("Throw").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _callbackSymbols;
    public IReadOnlyList<ISymbol> CallbackSymbols => _callbackSymbols ??= RhinoMocksIMethodOptionsSymbol.GetMembers ("Callback").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _ignoreArgumentsSymbols;
    public IReadOnlyList<ISymbol> IgnoreArgumentsSymbols => _ignoreArgumentsSymbols ??= RhinoMocksIMethodOptionsSymbol.GetMembers ("IgnoreArguments").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _repeatSymbols;
    public IReadOnlyList<ISymbol> RepeatSymbols => _repeatSymbols ??= RhinoMocksIMethodOptionsSymbol.GetMembers ("Repeat").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _callOriginalMethodSymbols;
    public IReadOnlyList<ISymbol> CallOriginalMethodSymbols => _callOriginalMethodSymbols ??= RhinoMocksIMethodOptionsSymbol.GetMembers ("CallOriginalMethod").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _constraintsSymbols;
    public IReadOnlyList<ISymbol> ConstraintsSymbols => _constraintsSymbols ??= RhinoMocksIMethodOptionsSymbol.GetMembers ("Constraints").ToList().AsReadOnly();

    #endregion

    #region IRepeat

    private IReadOnlyList<ISymbol>? _repeatNeverSymbols;
    public IReadOnlyList<ISymbol> RepeatNeverSymbols => _repeatNeverSymbols ??= RhinoMocksIRepeatSymbol.GetMembers ("Never").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _repeatOnceSymbols;
    public IReadOnlyList<ISymbol> RepeatOnceSymbols => _repeatOnceSymbols ??= RhinoMocksIRepeatSymbol.GetMembers ("Once").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _repeatTwiceSymbols;
    public IReadOnlyList<ISymbol> RepeatTwiceSymbols => _repeatTwiceSymbols ??= RhinoMocksIRepeatSymbol.GetMembers ("Twice").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _repeatAtLeastOnceSymbols;
    public IReadOnlyList<ISymbol> RepeatAtLeastOnceSymbols => _repeatAtLeastOnceSymbols ??= RhinoMocksIRepeatSymbol.GetMembers ("AtLeastOnce").ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _repeatTimesSymbols;
    public IReadOnlyList<ISymbol> RepeatTimesSymbols => _repeatTimesSymbols ??= RhinoMocksIRepeatSymbol.GetMembers ("Times").ToList().AsReadOnly();

    #endregion

    #region Expect

    private IReadOnlyList<ISymbol>? _expectCallSymbols;
    public IReadOnlyList<ISymbol> ExpectCallSymbols => _expectCallSymbols ??= RhinoMocksExpectSymbol.GetMembers ("Call").ToList().AsReadOnly();

    #endregion

    #region SetupResult

    private IReadOnlyList<ISymbol>? _setupResultForSymbols;
    public IReadOnlyList<ISymbol> SetupResultForSymbols => _setupResultForSymbols ??= RhinoMocksSetupResultSymbol.GetMembers ("For").ToList().AsReadOnly();

    #endregion

    #region RhinoMocksSymbol Collections

    private IReadOnlyList<ISymbol>? _allGenerateMockAndStubSymbols;
    public IReadOnlyList<ISymbol> AllGenerateMockAndStubSymbols
    {
      get
      {
        return _allGenerateMockAndStubSymbols ??= MockRepositoryGenerateMockSymbols
            .Concat (MockRepositoryGenerateStubSymbols)
            .Concat (MockRepositoryDynamicMockSymbols)
            .Concat (MockRepositoryDynamicMultiMockSymbols)
            .Concat (MockRepositoryStubSymbols)
            .ToList()
            .AsReadOnly();
      }
    }

    private IReadOnlyList<ISymbol>? _allPartialMockSymbols;
    public IReadOnlyList<ISymbol> AllPartialMockSymbols
    {
      get
      {
        return _allPartialMockSymbols ??= MockRepositoryPartialMockSymbols
            .Concat (MockRepositoryPartialMultiMockSymbols)
            .Concat (MockRepositoryGeneratePartialMockSymbols)
            .ToList()
            .AsReadOnly();
      }
    }

    private IReadOnlyList<ISymbol>? _allStrictMockSymbols;
    public IReadOnlyList<ISymbol> AllStrictMockSymbols
    {
      get
      {
        return _allStrictMockSymbols ??= mockRepositoryStrictMockSymbols
            .Concat (MockRepositoryStrictMultiMockSymbols)
            .Concat (MockRepositoryGenerateStrictMockSymbols)
            .ToList()
            .AsReadOnly();
      }
    }

    private IReadOnlyList<ISymbol>? _allMockRepositorySymbols;
    public IReadOnlyList<ISymbol> AllMockRepositorySymbols
    {
      get
      {
        return _allMockRepositorySymbols ??= AllGenerateMockAndStubSymbols
            .Concat (AllStrictMockSymbols)
            .Concat (AllPartialMockSymbols)
            .ToList()
            .AsReadOnly();
      }
    }

    private IReadOnlyList<ISymbol>? _allMockSymbols;
    public IReadOnlyList<ISymbol> AllMockSymbols
    {
      get
      {
        return _allMockSymbols ??= AllStrictMockSymbols
            .Concat (AllPartialMockSymbols)
            .Concat (MockRepositoryDynamicMultiMockSymbols)
            .Concat (MockRepositoryDynamicMockSymbols)
            .ToList()
            .AsReadOnly();
      }
    }

    private IReadOnlyList<ISymbol>? _allIRepeatSymbols;
    public IReadOnlyList<ISymbol> AllIRepeatSymbols => _allIRepeatSymbols ??= RhinoMocksIRepeatSymbol.GetMembers().ToList().AsReadOnly();

    private IReadOnlyList<ISymbol>? _allVerifySymbols;
    public IReadOnlyList<ISymbol> AllVerifySymbols
    {
      get
      {
        return _allVerifySymbols ??= VerifyAllSymbols
            .Concat (VerifyAllExpectationsSymbols)
            .Concat (AssertWasCalledSymbols)
            .Concat (AssertWasNotCalledSymbols)
            .ToList()
            .AsReadOnly();
      }
    }

    private IReadOnlyList<ISymbol>? _obsoleteRhinoMocksSymbols;
    public IReadOnlyList<ISymbol> ObsoleteRhinoMocksSymbols
    {
      get
      {
        return _obsoleteRhinoMocksSymbols ??= ReplaySymbols
            .Concat (ReplayAllSymbols)
            .ToList()
            .AsReadOnly();
      }
    }

    private IReadOnlyList<ISymbol>? _allIMethodOptionsSymbols;
    public IReadOnlyList<ISymbol> AllIMethodOptionsSymbols
    {
      get
      {
        return _allIMethodOptionsSymbols ??= ReturnSymbols
            .Concat (WhenCalledSymbols)
            .Concat (CallbackSymbols)
            .Concat (DoSymbols)
            .Concat (RepeatSymbols)
            .Concat (ThrowSymbols)
            .Concat (CallOriginalMethodSymbols)
            .ToList()
            .AsReadOnly();
      }
    }

    private IReadOnlyList<ISymbol>? _allCallbackSymbols;
    public IReadOnlyList<ISymbol> AllCallbackSymbols
    {
      get
      {
        return _allCallbackSymbols ??= WhenCalledSymbols
            .Concat (WhenCalledSymbols)
            .Concat (CallbackSymbols)
            .Concat (DoSymbols)
            .ToList()
            .AsReadOnly();
      }
    }

    private IReadOnlyList<ISymbol>? _allBackToRecordSymbols;
    public IReadOnlyList<ISymbol> AllBackToRecordSymbols
    {
      get
      {
        return _allBackToRecordSymbols ??= BackToRecordSymbols
            .Concat (BackToRecordAllSymbols)
            .ToList()
            .AsReadOnly();
      }
    }

    #endregion
  }
}