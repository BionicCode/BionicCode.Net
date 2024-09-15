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
  using BionicCode.Utilities.Net.UnitTest.ReflectionExtensionMethodsTests.Resources.Public.Generic;

  public class ReflectionExtensionMethodsTestToUnqualifiedSignatureName
  {
    #region Delegate

    private static readonly string TestDelegateWithoutReturnValueSignatureName = $"public delegate void {nameof(TestDelegateWithoutReturnValue)}(int a, {nameof(TestClass)} b, string text);";
    private static readonly string TestDelegateWithoutReturnValueQualifiedSignatureName = $"public delegate void {typeof(TestDelegateWithoutReturnValue).FullName}(int a, {typeof(TestClass).FullName} b, string text);";
    private static readonly string TestDelegateWithReturnValueSignatureName = $"public delegate int {nameof(TestDelegateWithReturnValue)}(int a, {nameof(TestClass)} b, string text);";
    private static readonly string TestDelegateWithReturnValueQualifiedSignatureName = $"public delegate int {typeof(TestDelegateWithReturnValue).FullName}(int a, {typeof(TestClass).FullName} b, string text);";

    private static readonly string TestGenericDelegateTypeDefinitionSignatureName = $"public delegate TReturn {typeof(Generic.TestDelegateWithReturnValue<,>).ToDisplayName()}(int a, T b, string text){System.Environment.NewLine}  where T : {nameof(TestClassBase)};";
    private static readonly string TestGenericDelegateTypeDefinitionFullyQualifiedSignatureName = $"public delegate TReturn {typeof(Generic.TestDelegateWithReturnValue<,>).ToFullDisplayName()}(int a, T b, string text){System.Environment.NewLine}  where T : {typeof(TestClassBase).FullName};";

    [Fact]
    public void ToSignatureName_GenericDelegateTypeDefinitionWithConstraints_MustReturnDelegateSignature()
    {
      Type type = typeof(Generic.TestDelegateWithReturnValue<,>);

      string delegateSignature = type.ToSignatureName();

      _ = delegateSignature.Should().Be(TestGenericDelegateTypeDefinitionSignatureName);
    }

    [Fact]
    public void ToSignatureName_GenericDelegateTypeDefinitionWithConstraints_MustReturnDelegateFullyQualifiedSignature()
    {
      Type type = typeof(Generic.TestDelegateWithReturnValue<,>);

      string delegateSignature = type.ToSignatureName(isFullyQualifiedName: true);

      _ = delegateSignature.Should().Be(TestGenericDelegateTypeDefinitionFullyQualifiedSignatureName);
    }

    [Fact]
    public void ToSignatureName_DelegateWithoutReturnValue_MustReturnDelegateSignature()
    {
      Type type = typeof(TestDelegateWithoutReturnValue);

      string delegateSignature = type.ToSignatureName();

      _ = delegateSignature.Should().Be(TestDelegateWithoutReturnValueSignatureName);
    }

    [Fact]
    public void ToSignatureName_DelegateWithReturnValue_MustReturnDelegateSignature()
    {
      Type type = typeof(TestDelegateWithReturnValue);

      string delegateSignature = type.ToSignatureName();
      
      _ = delegateSignature.Should().Be(TestDelegateWithReturnValueSignatureName);
    }

    #endregion Delegate

    #region Class

    private static readonly string TestClassSignatureName = $"[TestAttribute(1024.25, \"class\", NamedInt = 128)]{Environment.NewLine}[TestAttribute(64, \"class\", NamedInt = 256)]{Environment.NewLine}public class {nameof(TestClass)}";
    private static readonly string TestClassWithBaseClassSignatureName = $"[TestAttribute(1024.25, \"class\", NamedInt = 128)]{Environment.NewLine}[TestAttribute(64, \"class\", NamedInt = 256)]{Environment.NewLine}public class {nameof(TestClassWithBaseClass)} : {nameof(TestClassBase)}";
    
    [Fact]
    public void ToSignatureName_SimpleClass_MustReturnClassSignature()
    {
      Type type = typeof(TestClass);

      string classSignature = type.ToSignatureName();

      _ = classSignature.Should().Be(TestClassSignatureName);
    }

    [Fact]
    public void ToSignatureName_ClassWithBaseClass_MustReturnClassSignature()
    {
      Type type = typeof(TestClassWithBaseClass);

      string classSignature = type.ToSignatureName();
      
      _ = classSignature.Should().Be(TestClassWithBaseClassSignatureName);
    }

    [Fact]
    public void ToSignatureName_CallToSignatureShort_MustReturnFullClassSignature()
    {
      Type type = typeof(TestClassWithBaseClass);

      string classSignature = type.ToSignatureShortName();

      _ = classSignature.Should().Be(TestClassWithBaseClassSignatureName);
    }

    #endregion Class

    #region Struct

    private static readonly string TestStructSignatureName = $"public struct {nameof(TestStruct)}";
    private static readonly string TestReadOnlyStructSignatureName = $"public readonly struct {nameof(TestReadOnlyStruct)}";

    [Fact]
    public void ToSignatureName_SimpleStruct_MustReturnStructSignature()
    {
      Type type = typeof(TestStruct);

      string structSignature = type.ToSignatureName();

      _ = structSignature.Should().Be(TestStructSignatureName);
    }

    [Fact]
    public void ToSignatureName_CallToSignatureShort_MustReturnFullStructSignature()
    {
      Type type = typeof(TestStruct);

      string structSignature = type.ToSignatureShortName();

      _ = structSignature.Should().Be(TestStructSignatureName);
    }

    [Fact]
    public void ToSignatureName_SimpleReadOnlyStruct_MustReturnStructSignature()
    {
      Type type = typeof(TestReadOnlyStruct);
      
      string structSignature = type.ToSignatureName();
      
      _ = structSignature.Should().Be(TestReadOnlyStructSignatureName);
    }

    #endregion Struct

    #region Field

    private const string TestReadOnlyFieldSignatureName = "private readonly string {0}.readOnlyField;";
    private const string TestReadOnlyFieldShortSignatureName = "private readonly string readOnlyField;";
    private const string TestFieldSignatureName = "private string {0}.field;";
    private const string TestFieldShortSignatureName = "private string field;";

    [Fact]
    public void ToSignatureName_ReadOnlyField_MustReturnFieldSignature()
    {
      Type type = typeof(TestClass);
      FieldInfo fieldInfo = type.GetFieldInternal("readOnlyField");

      string fieldSignature = fieldInfo.ToSignatureName();

      _ = fieldSignature.Should().Be(string.Format(TestReadOnlyFieldSignatureName, type.Name));
    }

    [Fact]
    public void ToSignatureName_ReadOnlyField_MustReturnShortFieldSignature()
    {
      Type type = typeof(TestClass);
      FieldInfo fieldInfo = type.GetFieldInternal("readOnlyField");

      string fieldSignature = fieldInfo.ToSignatureShortName();

      _ = fieldSignature.Should().Be(TestReadOnlyFieldShortSignatureName);
    }

    [Fact]
    public void ToSignatureName_Field_MustReturnShortFieldSignature()
    {
      Type type = typeof(TestClass);
      FieldInfo fieldInfo = type.GetFieldInternal("field");

      string fieldSignature = fieldInfo.ToSignatureShortName();
      
      _ = fieldSignature.Should().Be(TestFieldShortSignatureName);
    }

    [Fact]
    public void ToSignatureName_Field_MustReturnFullFieldSignature()
    {
      Type type = typeof(TestClass);
      FieldInfo fieldInfo = type.GetFieldInternal("field");

      string fieldSignature = fieldInfo.ToSignatureName();

      _ = fieldSignature.Should().Be(string.Format(TestFieldSignatureName, type.Name));
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
    private static readonly string TestMethodGenericOfGenericClassSignatureName = $@"[TestAttribute(1024.25, ""method"", NamedInt = 128)]{System.Environment.NewLine}[TestAttribute(64, ""method"", NamedInt = 256)]{System.Environment.NewLine}public T PublicGenericMethodWithReturnValue<V, W>([TestAttribute(12, ""parameter"", NamedInt = 24)] ref V parameter, W parameter2, out IEnumerable<int> parameter3, in IDictionary<int, string> parameter4){System.Environment.NewLine}  where V : class, IList, ITestClass1, ITestClass3, ITestClass2<T, U>, new(){System.Environment.NewLine}  where W : struct, IComparable, ITestClass2;";
    private static readonly string TestMethodGenericOfGenericClassCompactSignatureName = $@"[TestAttribute(1024.25, ""method"", NamedInt = 128)]{System.Environment.NewLine}[TestAttribute(64, ""method"", NamedInt = 256)]{System.Environment.NewLine}public T PublicGenericMethodWithReturnValue<V, W>([TestAttribute(12, ""parameter"", NamedInt = 24)] ref V parameter, W parameter2, out IEnumerable<int> parameter3, in IDictionary<int, string> parameter4){System.Environment.NewLine}  where V : class, IList, ITestClass1, ITestClass3, ITestClass2<T, U>, new() where W : struct, IComparable, ITestClass2;";
    private static readonly string TestMethodAsyncGenericOfGenericClassSignatureName = $@"[TestAttribute(1024.25, ""method"", NamedInt = 128)]{System.Environment.NewLine}[TestAttribute(64, ""method"", NamedInt = 256)]{System.Environment.NewLine}public async Task<T> PublicGenericMethodWithReturnValueAsync<V, W>([TestAttribute(12, ""parameter"", NamedInt = 24)] V parameter, W parameter2, IEnumerable<int> parameter3, IDictionary<int, string> parameter4) where V : class, IList, ITestClass1, ITestClass3, ITestClass2<T, U>, new() where W : struct, IComparable, ITestClass2;";
    private static readonly string TestMethodAsyncGenericOfGenericClassCompactSignatureName = $@"[TestAttribute(1024.25, ""method"", NamedInt = 128)]{System.Environment.NewLine}[TestAttribute(64, ""method"", NamedInt = 256)]{System.Environment.NewLine}public async Task<T> PublicGenericMethodWithReturnValueAsync<V, W>([TestAttribute(12, ""parameter"", NamedInt = 24)] V parameter, W parameter2, IEnumerable<int> parameter3, IDictionary<int, string> parameter4) where V : class, IList, ITestClass1, ITestClass3, ITestClass2<T, U>, new() where W : struct, IComparable, ITestClass2;";

    [Fact]
    public void ToSignatureName_GenericMethod_MustReturnMethodSignature()
    {
      MethodInfo methodInfo = typeof(TestClass).GetMethodInternal(nameof(TestClass.PublicGenericMethodWithReturnValue));

      string methodSignature = methodInfo.ToSignatureName();

      _ = methodSignature.Should().Be(TestMethodGenericSignatureName);
    }

    [Fact]
    public void ToSignatureName_GenericMethod_MustReturnShortMethodSignature()
    {
      MethodInfo methodInfo = typeof(TestClass).GetMethodInternal(nameof(TestClass.PublicGenericMethodWithReturnValue));
      
      string methodSignature = methodInfo.ToSignatureShortName();
      
      _ = methodSignature.Should().Be(TestMethodGenericShortSignatureName);
    }

    [Fact]
    public void ToSignatureName_GenericMethodWithAttributesAndRefParameter_MustReturnShortMethodSignature()
    {
      Type type = typeof(Generic.TestClassWithBaseClass<,>);
      MethodInfo methodInfo = type.GetMethod("PublicGenericMethodWithReturnValue");
      string methodSignature = methodInfo.ToSignatureShortName();

      _ = methodSignature.Should().Be(TestMethodGenericOfGenericClassSignatureName);
    }

    [Fact]
    public void ToSignatureName_GenericMethodWithAttributesAndRefParameter_MustReturnCompactMethodSignature()
    {
      Type type = typeof(Generic.TestClassWithBaseClass<,>);
      MethodInfo methodInfo = type.GetMethod("PublicGenericMethodWithReturnValue");
      string methodSignature = methodInfo.ToSignatureShortName(isCompact: true);

      _ = methodSignature.Should().Be(TestMethodGenericOfGenericClassCompactSignatureName);
    }

    [Fact]
    public void ToSignatureName_AsyncGenericMethodWithConstraints_MustReturnMethodSignature()
    {
      Type type = typeof(Generic.TestClassWithBaseClass<,>);
      MethodInfo methodInfo = type.GetMethod("PublicGenericMethodWithReturnValueAsync");
      string methodSignature = methodInfo.ToSignatureShortName();

      _ = methodSignature.Should().Be(TestMethodAsyncGenericOfGenericClassSignatureName);
    }

    [Fact]
    public void ToSignatureName_AsyncGenericMethodWithConstraints_MustReturnCompactMethodSignature()
    {
      Type type = typeof(Generic.TestClassWithBaseClass<,>);
      MethodInfo methodInfo = type.GetMethod("PublicGenericMethodWithReturnValueAsync");
      string methodSignature = methodInfo.ToSignatureShortName(isCompact: true);

      _ = methodSignature.Should().Be(TestMethodAsyncGenericOfGenericClassCompactSignatureName);
    }

    [Fact]
    public void ToSignatureName_GenericMethodNew_MustBeFasterThanOld()
    {
      Type type = typeof(Generic.TestClassWithBaseClass<,>);
      MethodInfo methodInfo = type.GetMethod("PublicGenericMethodWithReturnValue");
      ParameterInfo[] paramas = methodInfo.GetParameters();
      string methodSignature = methodInfo.ToSignatureShortName(isCompact: true);

      _ = methodSignature.Should().Be(TestMethodGenericOfGenericClassSignatureName);

      return;
      string s1 = methodInfo.ToSignatureShortName();
      var stringBuilder = new StringBuilder();
      string s2 = stringBuilder.AppendSignatureName(methodInfo, false, false).ToString();
      //_ = s1.Should().BeEquivalentTo(s2);
      _ = stringBuilder.Clear();
      var stopWatch = new Stopwatch();
      stopWatch.Start();
      _ = methodInfo.ToSignatureName();
      stopWatch.Stop();
      _ = methodInfo.ExecutionTimeOf(method => stringBuilder.AppendSignatureName(method, false, false)).Should().BeLessThan(stopWatch.Elapsed);
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

    private static readonly string TestConstructorOfGenericClassSignatureName = "public {0}.{1}({2});";

    [Fact]
    public void ToSignatureName_ConstructorOfGenericClass_MustReturnConstructorSignature()
    {
      Type type = typeof(Generic.TestClass<,,,>);
      ConstructorInfo constructorInfo = type.GetConstructorInternal(Type.EmptyTypes);
      string constructorParameters = constructorInfo.GetParameters()
        .CreateParameterList();

      string constructorSignature = constructorInfo.ToSignatureName();

      _ = constructorSignature.Should()
        .Be(string.Format(TestConstructorOfGenericClassSignatureName, type.ToDisplayName(), type.ToDisplayName(isShortName: true), constructorParameters));
    }

    [Fact]
    public void ToSignatureName_ConstructorOfGenericClassWithTypeArguments_MustReturnConstructorSignature()
    {
      Type type = typeof(Generic.TestClass<string, int, TestClassWithInterfaces, TestClassWithBaseClass>);
      ConstructorInfo constructorInfo = type.GetConstructorInternal(typeof(int));
      string constructorParameters = constructorInfo.GetParameters()
        .CreateParameterList();

      string constructorSignature = constructorInfo.ToSignatureName();

      _ = constructorSignature.Should()
        .Be(string.Format(TestConstructorOfGenericClassSignatureName, type.ToDisplayName(), type.ToDisplayName(isShortName: true), constructorParameters));
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

  public static class HelperExtensionMethods
  {
    public static FieldInfo GetFieldInternal(this Type declaringType, string memberName)
      => declaringType.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

    public static MethodInfo GetMethodInternal(this Type declaringType, string memberName)
      => declaringType.GetMethod(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

    public static ConstructorInfo GetConstructorInternal(this Type declaringType, params Type[] parameters)
      => declaringType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, parameters, null);

    public static PropertyInfo GetPropertyInternal(this Type declaringType, string memberName)
      => declaringType.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

    public static EventInfo GetEventInternal(this Type declaringType, string memberName)
      => declaringType.GetEvent(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

    public static string CreateParameterList(this ParameterInfo[] parameters)
    {
      return parameters.Aggregate(string.Empty, (result, parameterInfo) => result = $"{parameterInfo.ParameterType.ToDisplayName()} {parameterInfo.Name}, ")
        .TrimEnd()
        .TrimEnd(',');
    }
  }
}
