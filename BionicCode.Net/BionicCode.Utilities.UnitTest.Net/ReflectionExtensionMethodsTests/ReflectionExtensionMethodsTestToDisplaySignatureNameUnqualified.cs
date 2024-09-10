namespace BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests
{ 
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Text;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public;
  using Generic = BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic;
  using FluentAssertions;
  using Xunit;
  using System.Threading;

  public class ReflectionExtensionMethodsTestToUnqualifiedDisplaySignatureName
  {
    private static readonly string TestDelegateWithoutReturnValueDisplaySignatureName = $"public delegate void {nameof(TestDelegateWithoutReturnValue)}(int a, {nameof(TestClass)} b, string text);";
    private static readonly string TestDelegateWithoutReturnValueQualifiedDisplaySignatureName = $"public delegate void {nameof(TestDelegateWithoutReturnValue)}(int a, {typeof(TestClass).FullName} b, string text);";
    private static readonly string TestDelegateWithReturnValueDisplaySignatureName = $"public delegate int {nameof(TestDelegateWithReturnValue)}(int a, {nameof(TestClass)} b, string text);";
    private static readonly string TestDelegateWithReturnValueQualifiedDisplaySignatureName = $"public delegate int {nameof(TestDelegateWithReturnValue)}(int a, {typeof(TestClass).FullName} b, string text);";

    private static readonly string TestMethodGenericDisplaySignatureName = $"public TValue {nameof(TestClass.PublicGenericMethodWithReturnValue)}<TValue>(TValue parameter);";

    private static readonly string TestClassDisplaySignatureName = $"public class {nameof(TestClass)}";
    private static readonly string TestClassWithBaseClassDisplaySignatureName = $"public class {nameof(TestClassWithBaseClass)} : {nameof(TestClassBase)}";

    private static readonly string TestStructDisplaySignatureName = $"public struct {nameof(TestStruct)}";
    private static readonly string TestReadOnlyStructDisplaySignatureName = $"public readonly struct {nameof(TestReadOnlyStruct)}";

    private static readonly string TestReadOnlyFieldDisplaySignatureName = $"private readonly string readOnlyField;";
    private static readonly string TestFieldDisplaySignatureName = $"private string field;";

    private static readonly string TestEnumDisplaySignatureName = $"public enum {nameof(TestEnum)}";

    #region Delegate

    [Fact]
    public void ToDisplayName_DelegateWithoutReturnValue_MustReturnDelegateSignature()
    {
      _ = typeof(TestDelegateWithoutReturnValue).ToSignatureName().Should().Be(TestDelegateWithoutReturnValueDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_DelegateWithReturnValue_MustReturnDelegateSignature()
    {
      _ = typeof(TestDelegateWithReturnValue).ToSignatureName().Should().Be(TestDelegateWithReturnValueDisplaySignatureName);
    }

    #endregion Delegate

    #region Class

    [Fact]
    public void ToDisplayName_SimpleClass_MustReturnClassSignature()
    {
      _ = typeof(TestClass).ToSignatureName().Should().Be(TestClassDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_ClassWithBaseClass_MustReturnClassSignature()
    {
      _ = typeof(TestClassWithBaseClass).ToSignatureName().Should().Be(TestClassWithBaseClassDisplaySignatureName);
    }

    #endregion Class

    #region Struct

    [Fact]
    public void ToDisplayName_SimpleStruct_MustReturnStructSignature()
    {
      _ = typeof(TestStruct).ToSignatureName().Should().Be(TestStructDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_SimpleReadOnlyStruct_MustReturnStructSignature()
    {
      _ = typeof(TestReadOnlyStruct).ToSignatureName().Should().Be(TestReadOnlyStructDisplaySignatureName);
    }

    #endregion Struct

    #region Field

    [Fact]
    public void ToDisplayName_ReadOnlyField_MustReturnFieldSignature()
    {
      _ = typeof(TestClass).GetField("readOnlyField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).ToSignatureName().Should().Be(TestReadOnlyFieldDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_Field_MustReturnFieldSignature()
    {
      _ = typeof(TestClass).GetField("field", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).ToSignatureName().Should().Be(TestFieldDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_GenericMethod_MustReturnMethodSignature()
    {
      _ = typeof(TestClass).GetMethod(nameof(TestClass.PublicGenericMethodWithReturnValue)).ToSignatureName().Should().Be(TestMethodGenericDisplaySignatureName);
    }

    #endregion Field

    #region Enum

    [Fact]
    public void ToDisplayName_TestEnum_MustReturnFullEnumSignature()
    {
      _ = typeof(TestEnum).ToSignatureName().Should().Be(TestEnumDisplaySignatureName);
    }

    #endregion Enum

    #region Constructor

    [Fact]
    public void ToDisplayName_ConstructorWithParameter_MustReturnConstructorSignature()
    {
      //var s = typeof(Generic.TestClassWithBaseClass<,>).ToSignatureNameNew(false);
      var s2 = typeof(Generic.TestClassWithBaseClass<List<List<string>>, int>).ToSignatureNameNew(false);
      ConstructorInfo constructorInfo = typeof(Task<>).MakeGenericType(typeof(Func<,,>)).GetConstructor(new[] { typeof(Func<>).MakeGenericType(typeof(Func<,,>)), typeof(CancellationToken) });
      string signatureName = constructorInfo.ToSignatureName();
      //Type[] genericDefaultTypeArguments = typeof(Task<>).GetGenericArguments();
      //ConstructorInfo constructorInfo = typeof(Task<>).MakeGenericType(typeof(Func<,,>)).GetConstructor(new[] { typeof(Func<>).MakeGenericType(typeof(Func<,,>)), typeof(CancellationToken) });
      //string signatureName = constructorInfo.ToSignatureName();
      Console.WriteLine(signatureName); // "public Task(Func<TResult> function, CancellationToken cancellationToken);"
      _ = typeof(Generic.TestClass<string, int, TestClassWithInterfaces, TestClassWithBaseClass>).GetConstructor(new[] { typeof(int) }).ToSignatureName().Should().Be(TestMethodGenericDisplaySignatureName);
    }

    #endregion Constructor
  }

  public class ReflectionExtensionMethodsTestToQualifiedDisplaySignatureName
  {
    private static readonly string TestDelegateWithoutReturnValueDisplaySignatureName = $"public delegate void {typeof(TestDelegateWithoutReturnValue).FullName}(int a, {typeof(TestClass).FullName} b, string text);";
    private static readonly string TestDelegateWithReturnValueDisplaySignatureName = $"public delegate int {typeof(TestDelegateWithReturnValue).FullName}(int a, {typeof(TestClass).FullName} b, string text);";
    private static readonly string TestClassDisplaySignatureName = $"public class {typeof(TestClass).FullName}";
    private static readonly string TestClassWithBaseClassDisplaySignatureName = $"public class {typeof(TestClassWithBaseClass).FullName} : {typeof(TestClassBase).FullName}";

    [Fact]
    public void ToDisplayName_DelegateWithoutReturnValue_MustReturnFullDelegateSignature()
    {
      _ = typeof(TestDelegateWithoutReturnValue).ToSignatureName(isFullyQualifyNamesEnabled: true).Should().Be(TestDelegateWithoutReturnValueDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_DelegateWithReturnValue_MustReturnFullDelegateSignature()
    {
      _ = typeof(TestDelegateWithReturnValue).ToSignatureName(isFullyQualifyNamesEnabled: true).Should().Be(TestDelegateWithReturnValueDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_SimpleClass_MustReturnFullClassSignature()
    {
      _ = typeof(TestClass).ToSignatureName(isFullyQualifyNamesEnabled: true).Should().Be(TestClassDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_ClassWithBaseClass_MustReturnFullClassSignature()
    {
      _ = typeof(TestClassWithBaseClass).ToSignatureName(isFullyQualifyNamesEnabled: true).Should().Be(TestClassWithBaseClassDisplaySignatureName);
    }
  }
}
