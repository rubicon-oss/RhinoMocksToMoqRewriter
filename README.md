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
| private T object                                           | private Mock\<T> object^1                                                     |
| private T object                                           | private Mock\<T> object^1                                                      |
| .VerifyAllExpectations()                                   | .Verify()                                                                     |
| .VerifyAll()                                               | .Verify()^2                                                                   |
| .AssertWasCalled (expression)                              | .Verify (expression, Times.AtLeastOnce())^3                                     |
| .AssertWasNotCalled (expression)                           | .Verify (expression, Times.Never())^3                                           |
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
| Rhino.Mocks.Constraints.Property.Value ("Property", Value) | _ != null && object.Equals (_.Property, Value)^4                            |
| .Constraints (constraints)                                 | Arg\<T>.Matches (constraints)                                                 |
| .IgnoreArguments()                                         | mock.Method (It.IsAny\<T>())                                                  |
| .LastCall()                                                | Expect.Call (() => lastCalledMockMethod)^5                                  |
| .Repeat.Any()                                              | -                                                                             |
| .Repeat.Once()                                             | Times.Once()^6                                                             |
| .Repeat.Twice()                                            | Times.Exactly (2)^6                                                        |
| .Repeat.AtLeastOnce()                                      | Times.AtLeastOnce()^6                                                      |
| .Repeat.Times (min, max)                                   | Times.Between (min, max)^6                                                 |
| Expect.Call (expression)                                   | .Expect (expression)                                                          |
| using (mockRepository.Ordered())<br>{<br>mock.Expect()<br>}| var sequence1 = new MockSequence();<br>mock.InSequence (sequence1).Expect()); |

## Footnotes

^1 The `type` of the `field`s won't be automatically converted if theres at least one `field` inside the declaration, which is no mock.

^2 Insert `.Verify()` for every mock that has been created with the calling `MockRepository`.

^3 `AssertWasCalled` and `AssertWasNotCalled` can also be called with an additional `options` parameter. This parameter will not be converted.

^4 `Rhino.Mocks` automatically returns `false` if the the `property` or the `object` itself is `null`, if theres an additional `null` check e.g. `Rhino.Mocks.Constraints.Is.NotNull() & Rhino.Mocks.Constraints.Property.Value ("A", 42)` the rewrite fails, and both constraints will be skipped.

^5 The rewrite fails if the last called mock method is not inside the same method body as the `LastCall` methodcall.

^6 The `Times` expression will be inserted into the `.Verify` e.g. `mock.Verify (mock.Method(), Times.Once())`. If `Repeat.Times` contains a more complex expression e.g. (`x-3*1`) it will be replaced with `"Unable to convert times expression"` which a results in a compile time error.

## Tips for manually fixing the code after the transformation

* `.Record()` and `.Playback()` are currently not supported.
* When using the `Repeat` Keyword, the expectation will be set up `n` times. After the `n`th call the default value will be returned.
* In `Rhino.Mocks` the first expectation is not overwritten, unless it is a `StrictMock`, in this case the first setup is used for the first call and the second setup is used for the second call and so on.
```csharp
mock.Expect (_ => _.A).Return (1);
mock.Expect (_ => _.A).Return (2);

mock.A; // returns 1
mock.A; // still returns 1

strictMock.Expect (_ => _.A).Return (1);
strictMock.Expect (_ => _.A).Return (2);

strictMock.A; // returns 1
strictMock.A; // returns 2
```
* A previously set Expectation can also be reset using the `BackToRecordAll` method.
* In Rhino.Mocks `CallOriginalMethod` calls the original method. For this call the mock behaves like a `PartialMock`. To achieve this behaviour in `Moq` one can insert `Callbase = true;` bevore and `Callbase = false;` after the method invocation.
```csharp
rhinoMocksMock.Expect (_ => _.DoSomething()).CallOriginalMethod();
    
Callbase = true;
moqMock.Setup (_ => _.DoSomething());
Callbase = false;
```
* In Moq a previous `Setup` can be replaced by a new one at any time.
```csharp
moqMock.Setup (_ => _.A).Return (1);
moqMock.Setup (_ => _.A).Return (2);

moqMock.A; // returns 2
```

## Known Issues
### Transformation
* `LastCall` calls the last called mock, since the mock does not have to be in the current method (or class), it is not always possible to automatically convert the statement.
* Helperfunctions make it significanlty harder for the tool to automatically rewrite mocks from `Rhino.Mocks` to `Moq`.

### Formatting
* Sometimes it happens, that instead of one whitespace two whitespaces get inserted, this can be easily fixed by searching this `.  \(` pattern and replacing it with this `. \(`
```csharp
mock.DoSomething  ("abc"); // wrong
mock.DoSomething ("abc");  // correct
```
* It can happen that the `NewLine` character in mulitline statements can be lost during transformation.
* The `[Test]` Attribute can also lose its indentation. A wrongly formatted `Attribute` could look like this:
```csharp

[Test]
    public vod Test ()
```
An easy solution would be to seach for a `NewLine` character followed by an `[Test]` attribute e.g.
```csharp

[Test]
```

## Installation
Move in the same directory as the `.nupkg` file and run the following command:
```
dotnet tool install --global --add-source . RhinoMocksToMoqRewriter
```
## Usage
RhinoMocksToMoqRewriter is a simple command line application and supports the following arguments:
```
-S, --Solution  Path to a solution file (.sln)

-P, --Project   Path to a project file (.csproj)
```

## Recommended Migration Process
1. Prepare the code before starting the migration process (optional)
    * Inline Helperfunctions
    * Change the `Type` of mocked properties to `Mock<type>`
2. Install `Moq`
3. Run the tool
4. Fix formatting errors
5. Fix compiler errors
6. Fix failing unit tests
