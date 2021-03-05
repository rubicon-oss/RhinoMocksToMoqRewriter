# RhinoMocksToMoqRewriter

## Features
RhinoMocksToMoqRewriter supports the following conversions:

| Rhino.Mocks                                                | Moq                                                                           |
|------------------------------------------------------------|-------------------------------------------------------------------------------|
| MockRepository.GenerateMock\<T>()                          | new Mock\<T>()                                                                |
| MockRepository.GeneratePartialMock\<T> (args)              | new Mock\<T> (args) { CallBase = true }                                       |
| MockRepository.GenerateStrictMock\<T>()                    | new Mock\<T> (MockBehavior.Strict)                                            |
| MockRepository.GenerateStub\<T>()                          | new Mock\<T>()                                                                |
| Arg<span>.</span>Is (object)                               | object                                                                        |
| Arg\<T>.Is.Equal (object)                                  | object                                                                        |
| Arg\<T>.Is.Anything                                        | It.IsAny\<T>()                                                                |
| Arg\<T>.Is.NotNull                                         | It.IsNotNull\<T>()                                                            |
| Arg\<T>.Is.Null                                            | null                                                                          |
| Arg\<T>.Is.Same (object)                                   | object                                                                        |
| Arg\<T>.Is.NotSame (object)                                | It.Is\<T> (_ => _ != object)                                                  |
| Arg\<T>.Is.GreaterThan(objectToCompare)                    | It.Is\<T> (_ => _ > objectToCompare)                                          |
| Arg\<T>.Is.GreaterThanOrEqual (objectToCompare)            | It.Is\<T> (_ => _ >= objectToCompare)                                         |
| Arg\<T>.Is.LessThan (objectToCompare)                      | It.Is\<T> (_ => _ < objectToCompare)                                          |
| Arg\<T>.Is.LessThanOrEqual (objectToCompare)               | It.Is\<T> (_ => _ <= objectToCompare)                                         |
| Arg\<T>.Matches (expression)                               | It.Is\<T> (expression)                                                        |
| Arg\<T[]>.List.Equal (object)                               | object                                                                       |
| Arg\<T[]>.List.IsIn (item)                                  | It.Is\<T> (_ => _.Contains(item)))                                           |
| Arg\<T[]>.List.ContainsAll (object)                         | It.Is\<T> (_ => object.All(_.Contains))                                      |
| Arg.Text.Like (string)                                     | string                                                                        |
| private T object                                           | private Mock\<T> object*                                                      |
| private T object                                           | private Mock\<T> object*                                                      |
| .VerifyAllExpectations()                                   | .Verify()                                                                     |
| .VerifyAll()                                               | .Verify()**                                                                   |
| .AssertWasCalled (expression)                              | .Verify (expression, Times.AtLeastOnce())                                     |
| .AssertWasNotCalled (expression)                           | .Verify (expression, Times.Never())                                           |
| .Expect (expression)                                       | .Setup (expression)<br>[...]<br>.Verifiable()                                 |
| .Stub (expression)                                         | .Setup (expression)                                                           |
| .Return (object)                                           | .Returns (object)                                                             |
| .Callback (callback)                                       | .Callback (callback)                                                          |
| .WhenCalled (callback)                                     | .Callback (callback)                                                          |
| .Throw (exception)                                         | .Throws (exception)                                                           |
| .Do (callback)                                             | .Callback (callback)                                                          |
| .SetupResult.For (expression)                              | Expect.Call (expression).Repeat.Any()                                         |
| Rhino.Mocks.Constraints.Is.Equal (object)                  | object.Equals (_, object)                                                     |
| Rhino.Mocks.Constraints.Is.NotEqual (object)               | !object.Equals (_, object)                                                    |
| Rhino.Mocks.Constraints.Is.Same (object)                   | object.ReferenceEquals (_, object)                                            |
| Rhino.Mocks.Constraints.Is.NotSame (object)                | !object.ReferenceEquals (_, object)                                           |
| Rhino.Mocks.Constraints.Is.GreaterThan (object)            | _ > object                                                                    |
| Rhino.Mocks.Constraints.Is.GreaterThanOrEqual (object)     | _ >= object                                                                   |
| Rhino.Mocks.Constraints.Is.LessThan(object)                | _ < object                                                                    |
| Rhino.Mocks.Constraints.Is.LessThanOrEqual (object)        | _ <= object                                                                   |
| Rhino.Mocks.Constraints.Is.Null()                          | _ == null                                                                     |
| Rhino.Mocks.Constraints.Is.NotNull()                       | _ != null                                                                     |
| Rhino.Mocks.Constraints.List.IsIn (object)                 | _.Contains (obejct)                                                           |
| Rhino.Mocks.Constraints.List.ContainsAll (collection)      | collection.All (_.Contains)                                                   |
| Rhino.Mocks.Constraints.Property.Value ("Property", Value) | _ != null && object.Equals (_.Property, Value)***                             |
| .Constraints (constraints)                                 | Arg\<T>.Matches (constraints)                                                 |
| .IgnoreArguments()                                         | mock.Method (It.IsAny\<T>())                                                  |
| .LastCall()                                                | Expect.Call (() => lastCalledMockMethod)****                                  |
| .Repeat.Any()                                              | -                                                                             |
| .Repeat.Once()                                             | Times.Once()*****                                                             |
| .Repeat.Twice()                                            | Times.Exactly (2)*****                                                        |
| .Repeat.AtLeastOnce()                                      | Times.AtLeastOnce()*****                                                      |
| .Repeat.Times (min, max)                                   | Times.Between (min, max)*****                                                 |
| Expect.Call (expression)                                   | .Expect (expression)                                                          |
| using (mockRepository.Ordered())<br>{<br>mock.Expect()<br>}| var sequence1 = new MockSequence();<br>mock.InSequence (sequence1).Expect()); |

\* The `type` of the `field`s won't be automatically converted if theres at least one `field` inside the declaration, which is no mock.

\** Insert `.Verify()` for every mock that has been created with the calling `MockRepository`.

\*** `Rhino.Mocks` automatically returns `false` if the the `property` or the `object` itself is `null`, if theres an additional `null` check e.g. `Rhino.Mocks.Constraints.Is.NotNull() & Rhino.Mocks.Constraints.Property.Value ("A", 42)` the rewrite fails, and both constraints will be skipped.

\**** The rewrite fails if the last called mock method is not inside the same method body as the `LastCall`. methodcall.

\***** The `Times` expression will be inserted into the `.Verify` e.g. ``mock.Verify (mock.Method(), Times.Once())`.

## Usage
RhinoMocksToMoqRewriter is a simple command line application and supports the following arguments:
```
-S, --Solution  Path to a solution file (.sln)

-P, --Project   Path to a project file (.csproj)
```