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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using NUnit.Framework;
using Rhino.Mocks;
using TestProject.TestClasses;
using TestProject.TestInterfaces;

namespace TestProject
{
  [TestFixture]
  public class FormattingTests
  {
    private IValidationRuleCollectorProvider _involvedTypeProviderStub;
    private IValidationRuleCollectorProvider _validationRuleCollectorProviderMock1;
    private IValidationRuleCollectorProvider _validationRuleCollectorProviderMock2;
    private IValidationRuleCollectorProvider _validationRuleCollectorProviderMock3;
    private IValidationRuleCollectorProvider _validationRuleCollectorProviderMock4;
    private AggregatingValidationRuleCollectorProvider _validationRuleCollectorProvider;
    private MockRepository _mockRepository;
    private ValidationRuleCollectorInfo _validationRuleCollectorInfo1;
    private ValidationRuleCollectorInfo _validationRuleCollectorInfo2;
    private ValidationRuleCollectorInfo _validationRuleCollectorInfo3;
    private ValidationRuleCollectorInfo _validationRuleCollectorInfo4;
    private ValidationRuleCollectorInfo _validationRuleCollectorInfo5;
    private ValidationRuleCollectorInfo _validationRuleCollectorInfo6;
    private ValidationRuleCollectorInfo _validationRuleCollectorInfo7;
    private ValidationRuleCollectorInfo _validationRuleCollectorInfo8;
    private ValidationRuleCollectorInfo _validationRuleCollectorInfo9;
    private ValidationRuleCollectorInfo _validationRuleCollectorInfo10;

    [SetUp]
    public void SetUp ()
    {
      _involvedTypeProviderStub = MockRepository.GenerateStub<IValidationRuleCollectorProvider>();

      _mockRepository = new MockRepository();

      _validationRuleCollectorProviderMock1 = _mockRepository.StrictMock<IValidationRuleCollectorProvider>();
      _validationRuleCollectorProviderMock2 = _mockRepository.StrictMock<IValidationRuleCollectorProvider>();
      _validationRuleCollectorProviderMock3 = _mockRepository.StrictMock<IValidationRuleCollectorProvider>();
      _validationRuleCollectorProviderMock4 = _mockRepository.StrictMock<IValidationRuleCollectorProvider>();

      _validationRuleCollectorProvider = new AggregatingValidationRuleCollectorProvider (
          _involvedTypeProviderStub,
          new[]
          {
              _validationRuleCollectorProviderMock1, _validationRuleCollectorProviderMock2,
              _validationRuleCollectorProviderMock3, _validationRuleCollectorProviderMock4
          });

      var validationRuleCollector = MockRepository.GenerateStub<IValidationRuleCollector>();
      _validationRuleCollectorInfo1 = new ValidationRuleCollectorInfo (validationRuleCollector, typeof (AggregatingValidationRuleCollectorProvider));
      _validationRuleCollectorInfo2 = new ValidationRuleCollectorInfo (validationRuleCollector, typeof (AggregatingValidationRuleCollectorProvider));
      _validationRuleCollectorInfo3 = new ValidationRuleCollectorInfo (validationRuleCollector, typeof (AggregatingValidationRuleCollectorProvider));
      _validationRuleCollectorInfo4 = new ValidationRuleCollectorInfo (validationRuleCollector, typeof (AggregatingValidationRuleCollectorProvider));
      _validationRuleCollectorInfo5 = new ValidationRuleCollectorInfo (validationRuleCollector, typeof (AggregatingValidationRuleCollectorProvider));
      _validationRuleCollectorInfo6 = new ValidationRuleCollectorInfo (validationRuleCollector, typeof (AggregatingValidationRuleCollectorProvider));
      _validationRuleCollectorInfo7 = new ValidationRuleCollectorInfo (validationRuleCollector, typeof (AggregatingValidationRuleCollectorProvider));
      _validationRuleCollectorInfo8 = new ValidationRuleCollectorInfo (validationRuleCollector, typeof (AggregatingValidationRuleCollectorProvider));
      _validationRuleCollectorInfo9 = new ValidationRuleCollectorInfo (validationRuleCollector, typeof (AggregatingValidationRuleCollectorProvider));
      _validationRuleCollectorInfo10 = new ValidationRuleCollectorInfo (validationRuleCollector, typeof (AggregatingValidationRuleCollectorProvider));
    }

    [Test]
    public void AnyTest ()
    {
      var typeGroup1 = new[] { typeof (ITestInterface), typeof (ICollection) };
      var typeGroup2 = new[] { typeof (ITestInterface) };
      var typeGroup3 = new[] { typeof (Customer) };

      using (_mockRepository.Ordered())
      {
        _validationRuleCollectorProviderMock1
            .Expect (mock => mock.GetValidationRuleCollectors (typeGroup1))
            .Return (new[] { new[] { _validationRuleCollectorInfo1 }, new[] { _validationRuleCollectorInfo2 } });
        _validationRuleCollectorProviderMock2
            .Expect (mock => mock.GetValidationRuleCollectors (typeGroup1))
            .Return (new[] { new[] { _validationRuleCollectorInfo3 }, new[] { _validationRuleCollectorInfo4 } });

        _validationRuleCollectorProviderMock3
            .Expect (mock => mock.GetValidationRuleCollectors (typeGroup1))
            .Return (new[] { new[] { _validationRuleCollectorInfo5 } });
        _validationRuleCollectorProviderMock4
            .Expect (mock => mock.GetValidationRuleCollectors (typeGroup1))
            .Return (Enumerable.Empty<IEnumerable<ValidationRuleCollectorInfo>>());

        _validationRuleCollectorProviderMock1
            .Expect (mock => mock.GetValidationRuleCollectors (typeGroup2))
            .Return (Enumerable.Empty<IEnumerable<ValidationRuleCollectorInfo>>());
        _validationRuleCollectorProviderMock2
            .Expect (mock => mock.GetValidationRuleCollectors (typeGroup2))
            .Return (Enumerable.Empty<IEnumerable<ValidationRuleCollectorInfo>>());

        _validationRuleCollectorProviderMock3
            .Expect (mock => mock.GetValidationRuleCollectors (typeGroup2)).Return (new[] { new[] { _validationRuleCollectorInfo6 } });
        _validationRuleCollectorProviderMock4
            .Expect (mock => mock.GetValidationRuleCollectors (typeGroup2)).Return (new[] { new[] { _validationRuleCollectorInfo7 } });

        _validationRuleCollectorProviderMock1
            .Expect (mock => mock.GetValidationRuleCollectors (typeGroup3))
            .Return (new[] { new[] { _validationRuleCollectorInfo8, _validationRuleCollectorInfo9 }, new[] { _validationRuleCollectorInfo10 } });
        _validationRuleCollectorProviderMock2
            .Expect (mock => mock.GetValidationRuleCollectors (typeGroup3))
            .Return (Enumerable.Empty<IEnumerable<ValidationRuleCollectorInfo>>());

        _validationRuleCollectorProviderMock3
            .Expect (mock => mock.GetValidationRuleCollectors (typeGroup3)).Return (Enumerable.Empty<IEnumerable<ValidationRuleCollectorInfo>>());
        _validationRuleCollectorProviderMock4
            .Expect (mock => mock.GetValidationRuleCollectors (typeGroup3)).Return (Enumerable.Empty<IEnumerable<ValidationRuleCollectorInfo>>());
      }

      var a = new[] { _validationRuleCollectorProviderMock1, _validationRuleCollectorProviderMock2, _validationRuleCollectorProviderMock3 };

      Assert.That (42, Is.GreaterThan (TimeSpan.FromMilliseconds (5.0))); // total
      Assert.That (21, Is.GreaterThan (TimeSpan.FromMilliseconds (5.0))); // since last checkpoint
    }

    [Test]
    public void Rewrite_MethodDeletion ()
    {
      _mockRepository.ReplayAll();

      _validationRuleCollectorProviderMock1.Replay();
    }

    [Test]
    public void Rewrite_MethodDeletion_123 ()
    {
      var typeGroup1 = new[] { typeof (ITestInterface), typeof (ICollection) };
      _validationRuleCollectorProviderMock1.Stub (
              mock => mock.GetValidationRuleCollectors (typeGroup1)).Return (Enumerable.Empty<IEnumerable<ValidationRuleCollectorInfo>>());
    }

    [Test]
    public void Rewrite_MethodDeletion_WhenCalled_formatting ()
    {
      var typeGroup1 = new[] { typeof (ITestInterface), typeof (ICollection) };
      _validationRuleCollectorProviderMock1
          .Expect (mock => mock.GetValidationRuleCollectors (typeGroup1))
          .Return (Enumerable.Empty<IEnumerable<ValidationRuleCollectorInfo>>())
          .WhenCalled (
              mi =>
              {
                var a = mi.Arguments[0];
              });
    }

    [Test]
    public void Rewrite_MethodDeletion_abc ()
    {
      var typeGroup1 = new[] { typeof (ITestInterface), typeof (ICollection) };

      _validationRuleCollectorProviderMock1.Replay();
    }

    [Test]
    public void d ()
    {
      _mockRepository = new MockRepository();

      var typeGroup1 = new[] { typeof (ITestInterface), typeof (ICollection) };

      _validationRuleCollectorProviderMock1.Stub (_ => _.PropertyValueChanging (null, null, null, null, null)).IgnoreArguments().WhenCalled (
          mi =>
          {
            var propertyDefinition = ((PropertyDefinition) mi.Arguments[2]);
            var newValue = ((string) mi.Arguments[4]);
          });
    }
  }
}