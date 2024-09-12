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
  using System.Diagnostics;

  public class ReflectionExtensionMethodsTestToUnqualifiedSignatureName
  {
    #region Delegate

    private static readonly string TestDelegateWithoutReturnValueSignatureName = $"public delegate void {nameof(TestDelegateWithoutReturnValue)}(int a, {nameof(TestClass)} b, string text);";
    private static readonly string TestDelegateWithoutReturnValueQualifiedSignatureName = $"public delegate void {nameof(TestDelegateWithoutReturnValue)}(int a, {typeof(TestClass).FullName} b, string text);";
    private static readonly string TestDelegateWithReturnValueSignatureName = $"public delegate int {nameof(TestDelegateWithReturnValue)}(int a, {nameof(TestClass)} b, string text);";
    private static readonly string TestDelegateWithReturnValueQualifiedSignatureName = $"public delegate int {nameof(TestDelegateWithReturnValue)}(int a, {typeof(TestClass).FullName} b, string text);";

    [Fact]
    public void ToSignatureName_DelegateWithoutReturnValue_MustReturnDelegateSignature()
    {
      _ = typeof(TestDelegateWithoutReturnValue).ToSignatureName().Should().Be(TestDelegateWithoutReturnValueSignatureName);
    }

    [Fact]
    public void ToSignatureName_DelegateWithReturnValue_MustReturnDelegateSignature()
    {
      _ = typeof(TestDelegateWithReturnValue).ToSignatureName().Should().Be(TestDelegateWithReturnValueSignatureName);
    }

    #endregion Delegate

    #region Class

    private static readonly string TestClassSignatureName = $"public class {nameof(TestClass)}";
    private static readonly string TestClassWithBaseClassSignatureName = $"public class {nameof(TestClassWithBaseClass)} : {nameof(TestClassBase)}";
    
    [Fact]
    public void ToSignatureName_SimpleClass_MustReturnClassSignature()
    {
      _ = typeof(TestClass).ToSignatureName().Should().Be(TestClassSignatureName);
    }

    [Fact]
    public void ToSignatureName_ClassWithBaseClass_MustReturnClassSignature()
    {
      _ = typeof(TestClassWithBaseClass).ToSignatureName().Should().Be(TestClassWithBaseClassSignatureName);
    }

    #endregion Class

    #region Struct

    private static readonly string TestStructSignatureName = $"public struct {nameof(TestStruct)}";
    private static readonly string TestReadOnlyStructSignatureName = $"public readonly struct {nameof(TestReadOnlyStruct)}";

    [Fact]
    public void ToSignatureName_SimpleStruct_MustReturnStructSignature()
    {
      _ = typeof(TestStruct).ToSignatureName().Should().Be(TestStructSignatureName);
    }

    [Fact]
    public void ToSignatureName_SimpleReadOnlyStruct_MustReturnStructSignature()
    {
      _ = typeof(TestReadOnlyStruct).ToSignatureName().Should().Be(TestReadOnlyStructSignatureName);
    }

    #endregion Struct

    #region Field

    private static readonly string TestReadOnlyFieldSignatureName = $"private readonly string readOnlyField;";
    private static readonly string TestFieldSignatureName = $"private string field;";

    [Fact]
    public void ToSignatureName_ReadOnlyField_MustReturnFieldSignature()
    {
      _ = typeof(TestClass).GetField("readOnlyField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).ToSignatureName().Should().Be(TestReadOnlyFieldSignatureName);
    }

    [Fact]
    public void ToSignatureName_Field_MustReturnFieldSignature()
    {
      _ = typeof(TestClass).GetField("field", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).ToSignatureName().Should().Be(TestFieldSignatureName);
    }

    #endregion Field

    #region Enum

    private static readonly string TestEnumSignatureName = $"public enum {nameof(TestEnum)}";

    [Fact]
    public void ToSignatureName_TestEnum_MustReturnFullEnumSignature()
    {
      _ = typeof(TestEnum).ToSignatureName().Should().Be(TestEnumSignatureName);
    }

    #endregion Enum

    #region Method

    private static readonly string TestMethodGenericShortSignatureName = $"public TValue {nameof(TestClass.PublicGenericMethodWithReturnValue)}<TValue>(TValue parameter);";
    private static readonly string TestMethodGenericSignatureName = $"public TValue {nameof(TestClass)}.{nameof(TestClass.PublicGenericMethodWithReturnValue)}<TValue>(TValue parameter);";

    [Fact]
    public void ToSignatureName_GenericMethod_MustReturnMethodSignature()
    {
      _ = typeof(TestClass).GetMethod(nameof(TestClass.PublicGenericMethodWithReturnValue)).ToSignatureName().Should().Be(TestMethodGenericSignatureName);
    }

    [Fact]
    public void ToSignatureName_GenericMethod_MustReturnShortMethodSignature()
    {
      _ = typeof(TestClass).GetMethod(nameof(TestClass.PublicGenericMethodWithReturnValue)).ToSignatureShortName().Should().Be(TestMethodGenericShortSignatureName);
    }

    [Fact]
    public void ToSignatureName_GenericMethodNew_MustBeFasterThanOld()
    {
      Type type = typeof(Generic.TestClassWithBaseClass<,>);
      MethodInfo methodInfo = type.GetMethod("PublicGenericMethodWithReturnValue");
      ParameterInfo[] paramas = methodInfo.GetParameters();
      string s1 = methodInfo.ToSignatureShortName();
      var stringBuilder = new StringBuilder();
      string s2 = stringBuilder.AppendSignatureName(methodInfo, false, false).ToString();
      _ = s1.Should().BeEquivalentTo(s2);
      _ = stringBuilder.Clear();
      var stopWatch = new Stopwatch();
      stopWatch.Start();
      _ = methodInfo.ToSignatureName();
      stopWatch.Stop();
      _ = methodInfo.ExecutionTimeOf(methodInfo => stringBuilder.AppendSignatureName(methodInfo, false, false)).Should().BeLessThan(stopWatch.Elapsed);
      //var s = typeof(Generic.TestClassWithBaseClass<,>).ToSignatureNameNew(false);

      //var stringBuilder = new StringBuilder().AppendDisplayName(typeof(Generic.TestClassWithBaseClass<List<Queue<Task<string>>>, int>), true);
      //var s = stringBuilder.ToString();
      //var stringBuilder2 = new StringBuilder().AppendShortDisplayName(typeof(Generic.TestClassWithBaseClass<List<Queue<Task<string>>>, int>), true);
      //var s2 = stringBuilder2.ToString();
      //Type type = typeof(Generic.TestClassWithBaseClass<,>);
      //var stringBuilder3 = new StringBuilder().AppendSignatureName(methodInfo, false, false);
      //var s3 = stringBuilder3.ToString();
      //ConstructorInfo constructorInfo = typeof(Task<>).MakeGenericType(typeof(Func<,,>)).GetConstructor(new[] { typeof(Func<>).MakeGenericType(typeof(Func<,,>)), typeof(CancellationToken) });
      //string signatureName = constructorInfo.ToSignatureName();
    }

    #endregion Method

    #region Constructor

    private static readonly string TestConstructorOfGenericClassSignatureName = "public {0}.{1}({2} {3});";

    [Fact]
    public void ToSignatureName_ConstructorOfGenericClass_MustReturnConstructorSignature()
    {
      Type type = typeof(Generic.TestClass<,,,>);
      ConstructorInfo constructorInfo = type.GetConstructors()[0];
      ParameterInfo constructorParameter = constructorInfo.GetParameters()[0];
      _ = constructorInfo.ToSignatureName().Should().Be(string.Format(TestConstructorOfGenericClassSignatureName, type.ToDisplayName(), type.ToDisplayName(isShortName: true ), constructorParameter.ParameterType.ToDisplayName(), constructorParameter.Name));
    }

    [Fact]
    public void ToSignatureName_ConstructorOfGenericClassWithTypeArguments_MustReturnConstructorSignature()
    {
      Type type = typeof(Generic.TestClass<string, int, TestClassWithInterfaces, TestClassWithBaseClass>);
      ConstructorInfo constructorInfo = type.GetConstructors()[0];
      ParameterInfo constructorParameter = constructorInfo.GetParameters()[0];
      _ = constructorInfo.ToSignatureName().Should()
        .Be(string.Format(TestConstructorOfGenericClassSignatureName, type.ToDisplayName(), type.ToDisplayName(isShortName: true), constructorParameter.ParameterType.ToDisplayName(), constructorParameter.Name));
    }

    #endregion Constructor
  }

  public class ReflectionExtensionMethodsTestToQualifiedSignatureName
  {
    private static readonly string TestDelegateWithoutReturnValueSignatureName = $"public delegate void {typeof(TestDelegateWithoutReturnValue).FullName}(int a, {typeof(TestClass).FullName} b, string text);";
    private static readonly string TestDelegateWithReturnValueSignatureName = $"public delegate int {typeof(TestDelegateWithReturnValue).FullName}(int a, {typeof(TestClass).FullName} b, string text);";
    private static readonly string TestClassSignatureName = $"public class {typeof(TestClass).FullName}";
    private static readonly string TestClassWithBaseClassSignatureName = $"public class {typeof(TestClassWithBaseClass).FullName} : {typeof(TestClassBase).FullName}";

    [Fact]
    public void ToSignatureName_DelegateWithoutReturnValue_MustReturnFullDelegateSignature()
    {
      _ = typeof(TestDelegateWithoutReturnValue).ToSignatureName(isFullyQualifiedName: true).Should().Be(TestDelegateWithoutReturnValueSignatureName);
    }

    [Fact]
    public void ToSignatureName_DelegateWithReturnValue_MustReturnFullDelegateSignature()
    {
      _ = typeof(TestDelegateWithReturnValue).ToSignatureName(isFullyQualifiedName: true).Should().Be(TestDelegateWithReturnValueSignatureName);
    }

    [Fact]
    public void ToSignatureName_SimpleClass_MustReturnFullClassSignature()
    {
      _ = typeof(TestClass).ToSignatureName(isFullyQualifiedName: true).Should().Be(TestClassSignatureName);
    }

    [Fact]
    public void ToSignatureName_ClassWithBaseClass_MustReturnFullClassSignature()
    {
      _ = typeof(TestClassWithBaseClass).ToSignatureName(isFullyQualifiedName: true).Should().Be(TestClassWithBaseClassSignatureName);
    }
  }
}
