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
      _ = typeof(TestDelegateWithoutReturnValue).ToDisplaySignatureName().Should().Be(TestDelegateWithoutReturnValueDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_DelegateWithReturnValue_MustReturnDelegateSignature()
    {
      _ = typeof(TestDelegateWithReturnValue).ToDisplaySignatureName().Should().Be(TestDelegateWithReturnValueDisplaySignatureName);
    }

    #endregion Delegate

    #region Class

    [Fact]
    public void ToDisplayName_SimpleClass_MustReturnClassSignature()
    {
      _ = typeof(TestClass).ToDisplaySignatureName().Should().Be(TestClassDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_ClassWithBaseClass_MustReturnClassSignature()
    {
      _ = typeof(TestClassWithBaseClass).ToDisplaySignatureName().Should().Be(TestClassWithBaseClassDisplaySignatureName);
    }

    #endregion Class

    #region Struct

    [Fact]
    public void ToDisplayName_SimpleStruct_MustReturnStructSignature()
    {
      _ = typeof(TestStruct).ToDisplaySignatureName().Should().Be(TestStructDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_SimpleReadOnlyStruct_MustReturnStructSignature()
    {
      _ = typeof(TestReadOnlyStruct).ToDisplaySignatureName().Should().Be(TestReadOnlyStructDisplaySignatureName);
    }

    #endregion Struct

    #region Field

    [Fact]
    public void ToDisplayName_ReadOnlyField_MustReturnFieldSignature()
    {
      _ = typeof(TestClass).GetField("readOnlyField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).ToDisplaySignatureName().Should().Be(TestReadOnlyFieldDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_Field_MustReturnFieldSignature()
    {
      _ = typeof(TestClass).GetField("field", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).ToDisplaySignatureName().Should().Be(TestFieldDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_GenericMethod_MustReturnMethodSignature()
    {
      _ = typeof(TestClass).GetMethod(nameof(TestClass.PublicGenericMethodWithReturnValue)).ToDisplaySignatureName().Should().Be(TestMethodGenericDisplaySignatureName);
    }

    #endregion Field

    #region Enum

    [Fact]
    public void ToDisplayName_TestEnum_MustReturnFullEnumSignature()
    {
      _ = typeof(TestEnum).ToDisplaySignatureName().Should().Be(TestEnumDisplaySignatureName);
    }

    #endregion Enum

    #region Constructor

    [Fact]
    public void ToDisplayName_ConstructorWithParameter_MustReturnConstructorSignature()
    {
      _ = typeof(Generic.TestClass<string, int, TestClassWithInterfaces, TestClassWithBaseClass>).GetConstructor(new[] { typeof(int) }).ToDisplaySignatureName().Should().Be(TestMethodGenericDisplaySignatureName);
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
      _ = typeof(TestDelegateWithoutReturnValue).ToDisplaySignatureName(isFullyQualifySymbolEnabled: true).Should().Be(TestDelegateWithoutReturnValueDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_DelegateWithReturnValue_MustReturnFullDelegateSignature()
    {
      _ = typeof(TestDelegateWithReturnValue).ToDisplaySignatureName(isFullyQualifySymbolEnabled: true).Should().Be(TestDelegateWithReturnValueDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_SimpleClass_MustReturnFullClassSignature()
    {
      _ = typeof(TestClass).ToDisplaySignatureName(isFullyQualifySymbolEnabled: true).Should().Be(TestClassDisplaySignatureName);
    }

    [Fact]
    public void ToDisplayName_ClassWithBaseClass_MustReturnFullClassSignature()
    {
      _ = typeof(TestClassWithBaseClass).ToDisplaySignatureName(isFullyQualifySymbolEnabled: true).Should().Be(TestClassWithBaseClassDisplaySignatureName);
    }
  }
}
