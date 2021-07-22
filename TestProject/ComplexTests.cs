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
  public class ComplexTests
  {
    private ITestInterface _memberCaller;
    private readonly MockRepository _mockRepository = new MockRepository();
    private Order _order1;

    private Customer _customer1;
    private DomainObjectMockEventReceiver _order1MockEventReceiver;
    private DomainObjectMockEventReceiver _customer1MockEventReceiver;
    private ClientTransaction _subTransaction;
    private ITestInterface _serviceStub;
    private ITypeInformation _typeInformationForResourceResolutionStub;
    private ITypeInformation _typeInformationStub;
    private IPropertyInformation _propertyInformationStub;
    private IDataStore<string, Lazy<Page>> _innerDataStoreMock;

    [SetUp]
    public void SetUp ()
    {
      _memberCaller = new TestClass();
      _serviceStub = MockRepository.GenerateMock<ITestInterface>();
      _typeInformationStub = MockRepository.GenerateMock<ITypeInformation>();
      _typeInformationForResourceResolutionStub = MockRepository.GenerateMock<ITypeInformation>();
      _propertyInformationStub = MockRepository.GenerateMock<IPropertyInformation>();

      _serviceStub.Stub (_ => _.GetKey (Arg.Is (1))).Return (null);
    }

    [Test]
    public void InitRecursive ()
    {
      MockRepository mockRepository = new MockRepository();
      Page namingContainer = new Page();
      IControl parentControlMock = mockRepository.PartialMock<IControl>();
      IControl childControlMock = mockRepository.PartialMock<IControl>();
      IControl childControlStub = mockRepository.Stub<IControl>();
      IControl anotherStub = MockRepository.GenerateStub<IControl>();

      using (mockRepository.Ordered())
      {
        childControlMock.Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "OnInit", EventArgs.Empty));
        parentControlMock.Expect (mock => PrivateInvoke.InvokeNonPublicMethod (mock, "OnInit", EventArgs.Empty));
      }

      mockRepository.ReplayAll();

      namingContainer.Controls.Add (parentControlMock);
      parentControlMock.Controls.Add (childControlMock);
      _memberCaller.InitRecursive (parentControlMock, namingContainer);

      mockRepository.VerifyAll();
    }

    [Test]
    public void SubCommit_OfDeletedObject_DoesNotRaiseDeletedEvent ()
    {
      using (_subTransaction.EnterDiscardingScope())
      {
        ITestInterface domainObject = new TestClass();

        MockRepository repository = new MockRepository();

        IClientTransactionExtension extensionMock = repository.StrictMock<IClientTransactionExtension>();
        extensionMock.Stub (stub => stub.Key).Return ("Mock");
        extensionMock.Replay();
        _subTransaction.Extensions.Add (extensionMock.ToString());
        try
        {
          extensionMock.BackToRecord();

          extensionMock.ObjectDeleting (_subTransaction, domainObject);
          extensionMock.ObjectDeleted (_subTransaction, domainObject);

          repository.ReplayAll();
          domainObject.Delete();
          repository.VerifyAll();

          repository.BackToRecordAll();
          extensionMock.Committing (null, null, null);
          LastCall.IgnoreArguments();
          extensionMock.CommitValidate (null, null);
          LastCall.IgnoreArguments();
          extensionMock.Committed (null, null);
          LastCall.IgnoreArguments();
          repository.ReplayAll();

          _subTransaction.Commit();
          repository.VerifyAll();
        }
        finally
        {
          _subTransaction.Extensions.Remove ("Mock");
        }
      }
    }

    [Test]
    public void Test_SetupResultFor ()
    {
      _mockRepository.ReplayAll();

      var mock = MockRepository.GenerateMock<ITestInterface>();
      SetupResult.For (mock.A).Return (42);

      mock.VerifyAllExpectations();
      Assert.Pass();
    }

    private void ChangeCustomerNameCallback (object sender, EventArgs e)
    {
      LastCall.IgnoreArguments();
    }

    [Test]
    public void ContainsPropertyDisplayName_NoResourceFound_ReturnsFalse ()
    {
      _serviceStub
          .Stub (
              _ => _.TryGetPropertyDisplayName (
                  Arg.Is (_propertyInformationStub),
                  Arg.Is (_typeInformationForResourceResolutionStub),
                  out Arg<string>.Out (null).Dummy))
          .Return (false);

      var result = _serviceStub.ContainsPropertyDisplayName (_propertyInformationStub, _typeInformationForResourceResolutionStub);

      Assert.That (result, Is.False);
    }

    [Test]
    public void ContainsPropertyDisplayName_ResourceFound_ReturnsTrue ()
    {
      _serviceStub
          .Stub (
              _ => _.TryGetPropertyDisplayName (
                  Arg.Is (_propertyInformationStub),
                  Arg.Is (_typeInformationForResourceResolutionStub),
                  out Arg<string>.Out ("expected").Dummy))
          .Return (true);

      var result = _serviceStub.ContainsPropertyDisplayName (_propertyInformationStub, _typeInformationForResourceResolutionStub);

      Assert.That (result, Is.True);
    }

    [Test]
    public void GetPropertyDisplayName_NoResourceFound_ReturnsShortPropertyName ()
    {
      _serviceStub
          .Stub (
              _ => _.TryGetPropertyDisplayName (
                  MockRepository.GenerateStub<IPropertyInformation>(),
                  Arg.Is (_typeInformationForResourceResolutionStub),
                  out Arg<string>.Out (null).Dummy))
          .Return (false);

      _propertyInformationStub.Stub (_ => _.DeclaringType).Return (_typeInformationStub);

      var result = _serviceStub.GetPropertyDisplayName (MockRepository.GenerateStub<IPropertyInformation>(), _typeInformationForResourceResolutionStub);
      var a = new[] { MockRepository.GenerateStub<IPropertyInformation>(), _propertyInformationStub, MockRepository.GenerateStub<IPropertyInformation>() };
      Assert.That (result, Is.EqualTo ("PropertyName"));
    }

    [Test]
    public void Add ()
    {
      var value = new object ();
      _innerDataStoreMock
          .Expect (store => store.Add (Arg.Is ("key"), Arg<Lazy<Page>>.Matches (c => c.Value == value)))
          .WhenCalled (mi => CheckInnerDataStoreIsProtected ());

      _innerDataStoreMock.VerifyAllExpectations ();
    }

    [Test]
    public void Remove ()
    {
      _innerDataStoreMock
          .Expect (mock => mock.Remove ("key"))
          .Return (true)
          .WhenCalled (mi => CheckInnerDataStoreIsProtected());

      _innerDataStoreMock.VerifyAllExpectations ();
      Assert.That (true, Is.EqualTo (true));
    }

    [Test]
    public void Clear ()
    {
      _innerDataStoreMock
          .Expect (store => store.Clear())
          .WhenCalled (mi => CheckInnerDataStoreIsProtected ());

      _innerDataStoreMock.VerifyAllExpectations ();
    }

    private void CheckInnerDataStoreIsProtected ()
    {
      IPropertyInformation stub = MockRepository.GenerateStub<IPropertyInformation>();
      IPropertyInformation a = stub;

      var testClass = new TestClass();
      testClass.RegisterSingle (() => stub);
    }

    [Test]
    public void TestAssertWasCalledAndAssertWasNotCalled ()
    {
      var stub = MockRepository.GenerateStub<ITestInterface>();

      stub.AssertWasCalled (_ => _.Delete(), o => o.Repeat.Times (42));
      stub.AssertWasNotCalled (_ => _.Read (null), o => o.IgnoreArguments());
    }

    [Test]
    public void RepeatTimes ()
    {
      var stub = MockRepository.GenerateStub<ITestInterface>();

      var anyValue = 1;
      stub.Expect (_ => _.Delete()).Repeat.Times (anyValue - 1);

      stub.VerifyAllExpectations();
    }
  }
}