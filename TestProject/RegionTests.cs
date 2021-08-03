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
using NUnit.Framework;
using Rhino.Mocks;
using TestProject.TestClasses;
using TestProject.TestInterfaces;

namespace TestProject
{
  [TestFixture]
  public class RegionTests
  {
    private const string c_cssFileContent =
    #region
        @"table
{
  background-color: white;
  border-color: black;
  border-style: solid;
  border-width: 1px;
  table-layout: auto;
  border-collapse: collapse;
  border-spacing: 10px;
  empty-cells: show;
  caption-side: top;
  font-family: Arial, Helvetica, sans-serif;
  vertical-align: text-top;
  text-align: left;
}

th, td
{
  border-style: solid;
  border-color: black;
  border-width: 1px;
  table-layout: auto;
  border-collapse: collapse;
  border-spacing: 1px;
  padding: 5px;
}

th
{
   background-color: #CCCCCC;
}    ";
    #endregion


    [Test]
    public void RunSingleFileOutputDirectoryAndExtensionSettingTest ()
    {
      const string directory = "The Directory";

      var mocks = new MockRepository();
      var textWriterFactoryMock = mocks.DynamicMock<ITextWriterFactory>();

      textWriterFactoryMock.Expect (mock => mock.Directory = directory);
      textWriterFactoryMock.Expect (mock => mock.CreateTextWriter (Arg<String>.Is.Anything)).Return (new StringWriter());
      textWriterFactoryMock.Expect (mock => mock.CreateTextWriter (Arg<String>.Is.Anything, Arg<String>.Is.Anything, Arg<String>.Is.Anything)).Return (new StringWriter());

      textWriterFactoryMock.Replay();

      var settings = new TestClass();
      textWriterFactoryMock.VerifyAllExpectations();
    }

#if !DEBUG
    [Ignore ("Skipped unless DEBUG build")]
#endif
    [Test]
    public void Test_WithSecurityStrategyIsNull ()
    {
      var mocks = new MockRepository();
      var textWriterFactoryMock = mocks.DynamicMock<ITextWriterFactory>();

      mocks.ReplayAll();

      Assert.Pass();
    }

#if true
    [Test]
    public void Test_WithSecurityStrategyIsNull1 ()
    {
      var mocks = new MockRepository();
      var textWriterFactoryMock = mocks.DynamicMock<ITextWriterFactory>();

      mocks.ReplayAll();

      Assert.Pass();
    }
#endif
  }
}