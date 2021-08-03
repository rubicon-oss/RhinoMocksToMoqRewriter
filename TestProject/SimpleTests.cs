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
using NUnit.Framework;
using Rhino.Mocks;
using TestProject.TestClasses;
using TestProject.TestInterfaces;

namespace TestProject
{
  [TestFixture]
  public class SimpleTest
  {
    private MockRepository _mockRepository;
    private ITestInterface _mock1;
    private ITestInterface _mock2;
    private ITestInterface _sut = new TestClass();

    [SetUp]
    public void Setup ()
    {
      _mockRepository = new MockRepository();
    }

    [Test]
    public void Rewrite_MockInstantiation ()
    {
      var anotherMockRepository = new MockRepository();

      _ = MockRepository.GenerateMock<ITestInterface>();
      _ = MockRepository.GenerateStub<ITestInterface>();
      _ = MockRepository.GenerateStrictMock<ITestInterface>();
      _ = MockRepository.GeneratePartialMock<ITestInterface>();

      _ = anotherMockRepository.DynamicMock<ITestInterface>();
      _ = anotherMockRepository.DynamicMultiMock<ITestInterface>();
      _ = anotherMockRepository.StrictMock<ITestInterface>();
      _ = anotherMockRepository.StrictMultiMock<ITestInterface>();
      _ = anotherMockRepository.Stub<ITestInterface>();
      _ = anotherMockRepository.PartialMock<ITestInterface>();
      _ = anotherMockRepository.PartialMultiMock<ITestInterface>();

      Assert.Pass();
    }

    [Test]
    public void Rewrite_VerifyAll ()
    {
      _mock1 = _mockRepository.StrictMock<ITestInterface>();
      _mock2 = _mockRepository.DynamicMock<ITestInterface>();

      _mockRepository.VerifyAll();

      Assert.Pass();
    }

    [Test]
    public void Rewrite_VerifyAllExpectations ()
    {
      _mock1.VerifyAllExpectations();

      Assert.Pass();
    }

    [Test]
    public void Rewrite_AssertWasCalled ()
    {
      _mock1.AssertWasCalled (_ => _.Read());

      Assert.Pass();
    }

    [Test]
    public void Rewrite_AssertWasNotCalled ()
    {
      _mock1.AssertWasNotCalled (_ => _.Read());

      Assert.Pass();
    }

    [Test]
    public void Rewrite_RepeatAny ()
    {
      _mock1.Expect (m => m.Read (null)).Repeat.Any();

      _mock1.VerifyAllExpectations();

      Assert.Pass();
    }

    public void Rewrite_RepeatOnce ()
    {
      _mock1.Expect (m => m.Read (null)).Repeat.Once();

      _sut.Read();

      _mock1.VerifyAllExpectations();

      Assert.Pass();
    }

    [Test]
    public void Rewrite_RepeatAtLeastOnce ()
    {
      _mock1.Expect (m => m.Read (null)).Repeat.AtLeastOnce();

      _mock1.VerifyAllExpectations();

      Assert.Pass();
    }

    [Test]
    public void Rewrite_RepeatTwice ()
    {
      _mock1.Expect (m => m.Read (null)).Repeat.Twice();

      _mock1.VerifyAllExpectations();

      Assert.Pass();
    }

    [Test]
    public void Rewrite_RepeatNever ()
    {
      _mock1.Expect (m => m.Read (null)).Repeat.Never();

      _mock1.VerifyAllExpectations();

      Assert.Pass();
    }

    [Test]
    public void Rewrite_RepeatTimes ()
    {
      _mock1.Expect (m => m.Read (null)).Repeat.Times (3);

      _mock1.VerifyAllExpectations();

      Assert.Pass();
    }

    [Test]
    public void Rewrite_Ordered ()
    {
      using (_mockRepository.Ordered())
      {
        _mock1.Expect (_ => _.Read()).Return (true);
        _mock2.Expect (_ => _.Read()).Return (false);
      }

      Assert.Pass();
    }

    [Test]
    public void Rewrite_MethodDeletion ()
    {
      _mockRepository.ReplayAll();
      _mockRepository.BackToRecordAll();

      _mock2.Replay();
      _mock2.BackToRecord();

      Assert.Pass();
    }

    [Test]
    public void Rewrite_IgnoreArguments ()
    {
      _mock1.Expect (m => m.Read (null)).IgnoreArguments();

      Assert.Pass();
    }

    [Test]
    public void Rewrite_LastCall ()
    {
      _mock1.Read (null);
      LastCall.Constraints (Rhino.Mocks.Constraints.Is.NotNull());

      Assert.Pass();
    }

    [Test]
    public void Rewrite_SetupResultFor ()
    {
      SetupResult.For (_mock1.A).Return (42);

      Assert.Pass();
    }

    [Test]
    public void Rewrite_MockSetup ()
    {
      IControl control = null;

      _mock1.Expect (_ => _.Read()).Return (true);
      _mock1.Expect (_ => _.InitRecursive (Arg<IControl>.Is.Same (control), Arg<Page>.Is.NotNull));
      _mock1.Expect (_ => _.Read()).Return (true);

      Assert.Pass();
    }

    [Test]
    public void Rewrite_StrictMockWithoutVerify ()
    {
      var strictMock = MockRepository.GenerateStrictMock<ITestInterface>();
      strictMock.Expect (m => m.Read (null)).Return (true).Repeat.Once();

      Assert.Pass();
    }

    [Test]
    public void Rewrite_StubWithRepeat ()
    {
      var stub = MockRepository.GenerateStub<ITestInterface>();
      stub.Stub (m => m.Read (null)).Return (true).Repeat.Twice();

      Assert.Pass();
    }
  }
}