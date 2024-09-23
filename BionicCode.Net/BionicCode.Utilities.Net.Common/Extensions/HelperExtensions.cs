namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Text;
  using System.Threading.Tasks;
  using System.CodeDom;
  using Microsoft.CSharp;
  using System.IO;
  using System.CodeDom.Compiler;
  using Microsoft.CodeAnalysis.CSharp;
  using Microsoft.CodeAnalysis.CSharp.Syntax;
  using Microsoft.CodeAnalysis;
  using System.Runtime.InteropServices;
  using Microsoft.CodeAnalysis.Operations;
  using System.Xml.Linq;
  using System.Reflection.Metadata;
  using System.Globalization;
  using Microsoft.Extensions.Caching.Memory;
  using Microsoft.Extensions.Logging;
  using System.Management;

  /// <summary>
  /// A collection of extension methods for various default constraintTypes
  /// </summary>
  public static partial class HelperExtensionsCommon
  {
    private const string ParameterSeparator = ", ";
    private const char ExpressionTerminator = ';';
    private const string Indentation = "  ";

    private static readonly Type ValueTaskType = typeof(ValueTask);
    private static readonly Type ValueTaskGenericType = typeof(ValueTask<>);
    private static readonly Type TaskType = typeof(Task); 
    private static readonly Type AsyncStateMachineAttributeType = typeof(AsyncStateMachineAttribute);
    private static readonly Type ExtensionAttributeType = typeof(ExtensionAttribute); 
    private static readonly Type DelegateType = typeof(Delegate);
#if !NETSTANDARD2_0
    private static readonly Type IsReadOnlyAttributeType = typeof(IsReadOnlyAttribute);
#endif

    /// <summary>
    /// The property genericTypeParameterIdentifier of an indexer property. This genericTypeParameterIdentifier is compiler generated and equals the typeName of the <see langword="static"/>field <see cref="System.Windows.Data.Binding.IndexerName" />.
    /// </summary>
    /// <typeName>The generated property genericTypeParameterIdentifier of an indexer is <c>Item</c>.</typeName>
    /// <remarks>This field exists to enable writing of cross-platform compatible reflection code without the requirement to import the PresentationFramework.dll.</remarks>
    public static readonly string IndexerName = "Item";

    private static readonly AccessModifierComparer AccessModifierComparer = new AccessModifierComparer();
    private static readonly CSharpCodeProvider CodeProvider = new CSharpCodeProvider();
    private static readonly HashSet<string> IgnorableParameterAttributes = new HashSet<string>
    {
      nameof(InAttribute),
      nameof(OutAttribute),
#if !NETSTANDARD2_0
      nameof(IsReadOnlyAttribute),
#endif
    };

    /// <summary>
    /// Converts a <see cref="Predicate{T}"/> to a <see cref="Func{T, TResult}"/>.
    /// </summary>
    /// <typeparam genericTypeParameterIdentifier="TParam">The parameter type for the predicate.</typeparam>
    /// <param genericTypeParameterIdentifier="predicate">The predicate to convert.</param>
    /// <returns>A <c>Func<typeparamref genericTypeParameterIdentifier="TParam"/>, bool></c> that returns the result of <paramref genericTypeParameterIdentifier="predicate"/>.</returns>
    public static Func<TParam, bool> ToFunc<TParam>(this Predicate<TParam> predicate) => predicate.Invoke;

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureName(this MethodInfo methodInfo)
    {
      MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
      return methodData.FullyQualifiedSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureName(this Type type)
    {
      TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
      return typeData.FullyQualifiedSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureName(this FieldInfo fieldInfo)
    {
      FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
      return fieldData.FullyQualifiedSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureName(this PropertyInfo propertyInfo)
    {
      PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
      return propertyData.FullyQualifiedSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureName(this ConstructorInfo constructorInfo)
    {
      ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
      return constructorData.FullyQualifiedSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureName(this EventInfo eventInfo)
    {
      EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
      return eventData.FullyQualifiedSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureShortName(this MethodInfo methodInfo)
    {
      MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
      return methodData.Signature;
    }
    
    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureShortName(this Type type)
    {
      TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
      return typeData.Signature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureShortName(this FieldInfo fieldInfo)
    {
      FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
      return fieldData.Signature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureShortName(this PropertyInfo propertyInfo)
    {
      PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
      return propertyData.Signature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureShortName(this ConstructorInfo constructorInfo)
    {
      ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
      return constructorData.Signature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureShortName(this EventInfo eventInfo)
    {
      EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
      return eventData.Signature;
    }

    #region REMOVE AFTER BENCHMARK COMPARISON!!!

//    public static StringBuilder AppendSignatureName(this StringBuilder nameBuilder, MemberInfo memberInfo, bool isFullyQualifiedName, bool isShortName)
//    {
//      SyntaxNode syntaxGraph = null;
//      bool isTerminationRequested = false;
//      if (memberInfo is MethodInfo methodInfo)
//      {
//        syntaxGraph = CreateMethodGraph(methodInfo, isFullyQualifiedName);
//        isTerminationRequested = true;
//      }

//      if (syntaxGraph != null)
//      {
//        _ = nameBuilder.Append(syntaxGraph.ToString());
//        if (isTerminationRequested)
//        {
//          _ = nameBuilder.Append(HelperExtensionsCommon.ExpressionTerminator);
//        }
//      }

//      return nameBuilder;
//    }

//    private static SyntaxNode CreateMethodGraph(MethodInfo methodInfo, bool isFullyQualifiedName)
//    {
//      TypeSyntax returnType = SyntaxFactory.ParseTypeName(ToDisplayNameInternal(methodInfo.ReturnType, isFullyQualifiedName, isShortName: false));
//      string methodName = ToDisplayNameInternal(methodInfo, isFullyQualifiedName, isShortName: false);
//      MethodDeclarationSyntax methodGraph = SyntaxFactory.MethodDeclaration(returnType, methodName)
//        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

//      ParameterInfo[] parameters = methodInfo.GetParameters();
//      foreach (ParameterInfo parameter in parameters)
//      {
//        ParameterSyntax parameterSyntax = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.Name))
//          .WithType(SyntaxFactory.IdentifierName(ToDisplayNameInternal(parameter.ParameterType, isFullyQualifiedName, isShortName: false)));

//        if (parameter.IsRef())
//        {
//          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.RefKeyword));
//        }
//        else if (parameter.IsIn)
//        {
//          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.InKeyword));
//        }
//        else if (parameter.IsOut)
//        {
//          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.OutKeyword));
//        }
//        //IList<CustomAttributeData> parameterAttributes = parameter.GetCustomAttributesData();
//        //foreach(CustomAttributeData parameterAttribute in parameterAttributes)
//        //{
//        //  AttributeArgumentListSyntax argumentList = SyntaxFactory.AttributeArgumentList();

//        //  IList<CustomAttributeTypedArgument> arguments = parameterAttribute.ConstructorArguments;
//        //  foreach (CustomAttributeTypedArgument argument in arguments)
//        //  {
//        //    var argumentSyntax = SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression)
//        //  }
//        //  AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.Identifier(ToDisplayNameInternal(attributeSyntax.Name, isFullyQualifiedName, isShortName: false)));
//        //  parameterSyntax = parameterSyntax.AddAttributeLists(attributeSyntax);
//        //}
//        methodGraph = methodGraph.AddParameterListParameters(parameterSyntax);
//      }

//      if (methodInfo.IsGenericMethod)
//      {
//        Type[] typeArguments = methodInfo.GetGenericArguments();
//        for (int typeArgumentIndex = 0; typeArgumentIndex < typeArguments.Length; typeArgumentIndex++)
//        {
//          Type typeArgument = typeArguments[typeArgumentIndex];
//          //TypeParameterSyntax typeParameter = CreateMethodTypeParameter(typeArgument, isFullyQualifiedName);
//          //methodGraph = methodGraph.AddTypeParameterListParameters(typeParameter);

//          if (methodInfo.IsGenericMethodDefinition)
//          {
//            SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints = SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>();
//            if ((typeArgument.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
//            {
//              constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.StructConstraint));
//            }

//            if ((typeArgument.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
//            {
//              constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint));
//            }

//            Type[] constraintTypes = typeArgument.GetGenericParameterConstraints();
//            foreach (Type constraintType in constraintTypes)
//            {
//              if (constraintType == typeof(object) || constraintType == typeof(ValueType))
//              {
//                continue;
//              }

//              string constraintName = ToDisplayNameInternal(constraintType, isFullyQualifiedName, isShortName: false);
//              TypeConstraintSyntax constraintSyntax = SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(constraintName));
//              constraints = constraints.Add(constraintSyntax);
//            }

//            if (!typeArgument.IsValueType && (typeArgument.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
//            {
//              constraints = constraints.Add(SyntaxFactory.ConstructorConstraint());
//            }

//            string genericTypeParameterName = ToDisplayNameInternal(typeArgument, isFullyQualifiedName, isShortName: false);
//            methodGraph = methodGraph.AddConstraintClauses(SyntaxFactory.TypeParameterConstraintClause(SyntaxFactory.IdentifierName(genericTypeParameterName), constraints));
//          }
//        }
//      }

//      methodGraph = methodGraph.NormalizeWhitespace();
//      return methodGraph;
//    }

//    private static SyntaxNode CreateDelegateGraph(MethodInfo methodInfo, bool isFullyQualifiedName)
//    {
//      TypeSyntax returnType = SyntaxFactory.ParseTypeName(ToDisplayNameInternal(methodInfo.ReturnType, isFullyQualifiedName, isShortName: false));
//      string methodName = ToDisplayNameInternal(methodInfo, isFullyQualifiedName, isShortName: true);
//      MethodDeclarationSyntax methodGraph = SyntaxFactory.MethodDeclaration(returnType, methodName)
//        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

//      ParameterInfo[] parameters = methodInfo.GetParameters();
//      foreach (ParameterInfo parameter in parameters)
//      {
//        ParameterSyntax parameterSyntax = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.Name))
//          .WithType(SyntaxFactory.IdentifierName(ToDisplayNameInternal(parameter.ParameterType, isFullyQualifiedName, isShortName: false)));

//        if (parameter.IsRef())
//        {
//          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.RefKeyword));
//        }
//        else if (parameter.IsIn)
//        {
//          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.InKeyword));
//        }
//        else if (parameter.IsOut)
//        {
//          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.OutKeyword));
//        }
//        //IList<CustomAttributeData> parameterAttributes = parameter.GetCustomAttributesData();
//        //foreach(CustomAttributeData parameterAttribute in parameterAttributes)
//        //{
//        //  AttributeArgumentListSyntax argumentList = SyntaxFactory.AttributeArgumentList();

//        //  IList<CustomAttributeTypedArgument> arguments = parameterAttribute.ConstructorArguments;
//        //  foreach (CustomAttributeTypedArgument argument in arguments)
//        //  {
//        //    var argumentSyntax = SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression)
//        //  }
//        //  AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.Identifier(ToDisplayNameInternal(attributeSyntax.Name, isFullyQualifiedName, isShortName: false)));
//        //  parameterSyntax = parameterSyntax.AddAttributeLists(attributeSyntax);
//        //}
//        methodGraph = methodGraph.AddParameterListParameters(parameterSyntax);
//      }

//      if (methodInfo.IsGenericMethod)
//      {
//        Type[] typeArguments = methodInfo.GetGenericArguments();
//        for (int typeArgumentIndex = 0; typeArgumentIndex < typeArguments.Length; typeArgumentIndex++)
//        {
//          Type typeArgument = typeArguments[typeArgumentIndex];
//          TypeParameterSyntax typeParameter = CreateMethodTypeParameter(typeArgument, isFullyQualifiedName);
//          methodGraph = methodGraph.AddTypeParameterListParameters(typeParameter);

//          if (methodInfo.IsGenericMethodDefinition)
//          {
//            SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints = SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>();
//            if ((typeArgument.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
//            {
//              constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.StructConstraint));
//            }

//            if ((typeArgument.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
//            {
//              constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint));
//            }

//            if (!typeArgument.IsValueType && (typeArgument.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
//            {
//              constraints = constraints.Add(SyntaxFactory.ConstructorConstraint());
//            }

//            Type[] constraintTypes = typeArgument.GetGenericParameterConstraints();
//            foreach (Type constraintType in constraintTypes)
//            {
//              if (constraintType == typeof(object) || constraintType == typeof(ValueType))
//              {
//                continue;
//              }

//              string constraintName = ToDisplayNameInternal(constraintType, isFullyQualifiedName, isShortName: false);
//              TypeConstraintSyntax constraintSyntax = SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(constraintName));
//              constraints = constraints.Add(constraintSyntax);
//            }

//            string genericTypeParameterName = ToDisplayNameInternal(typeArgument, isFullyQualifiedName, isShortName: false);
//            methodGraph = methodGraph.AddConstraintClauses(SyntaxFactory.TypeParameterConstraintClause(SyntaxFactory.IdentifierName(genericTypeParameterName), constraints));
//          }
//        }
//      }

//      methodGraph = methodGraph.NormalizeWhitespace(indentation: " ", elasticTrivia: true);
//      return methodGraph;
//    }

//    private static TypeParameterSyntax CreateMethodTypeParameter(Type type, bool isFullyQualifiedName)
//    {
//      IEnumerable<Attribute> attributes = type.GetCustomAttributes();
//      AttributeListSyntax attributeSyntaxList = SyntaxFactory.AttributeList();
//      foreach (Attribute attribute in attributes)
//      {
//        string attributeName = ToDisplayNameInternal(attribute.GetType(), isFullyQualifiedName, isShortName: false);
//        AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName));
//        attributeSyntaxList = attributeSyntaxList.AddAttributes(attributeSyntax);
//      }

//      SyntaxKind variance = SyntaxKind.None;
//      if (type.IsGenericParameter)
//      {
//        if ((type.GenericParameterAttributes & GenericParameterAttributes.Covariant) != 0)
//        {
//          variance = SyntaxKind.OutKeyword;
//        }
//        else if ((type.GenericParameterAttributes & GenericParameterAttributes.Contravariant) != 0)
//        {
//          variance = SyntaxKind.InKeyword;
//        }
//      }

//      string typeParameterName = ToDisplayNameInternal(type, isFullyQualifiedName, isShortName: false);
//      return SyntaxFactory.TypeParameter(new SyntaxList<AttributeListSyntax>() { attributeSyntaxList }, SyntaxFactory.Token(variance), SyntaxFactory.Identifier(typeParameterName));
//    }

//    private static string ToDisplayNameInternal(MemberInfo memberInfo, bool isFullyQualifiedName, bool isShortName)
//    {
//      string symbolName = new StringBuilder()
//        .AppendDisplayNameInternal(memberInfo, isFullyQualifiedName, isShortName)
//        .ToString();

//      return symbolName;
//    }

//    private static StringBuilder AppendDisplayNameInternal(this StringBuilder nameBuilder, Type type, bool isFullyQualifiedName, bool isShortName)
//    {
//      var typeReference = new CodeTypeReference(type);
//      ReadOnlySpan<char> typeName = HelperExtensionsCommon.CodeProvider.GetTypeOutput(typeReference).AsSpan();
//      if (type.IsGenericType)
//      {
//        int startIndexOfGenericTypeParameters = typeName.IndexOf('<');
//        typeName = typeName.Slice(0, startIndexOfGenericTypeParameters);
//      }

//      if (!isFullyQualifiedName)
//      {
//        int startIndexOfUnqualifiedTypeName = typeName.LastIndexOf('.') + 1;
//        if (startIndexOfUnqualifiedTypeName > 0)
//        {
//          typeName = typeName.Slice(startIndexOfUnqualifiedTypeName, typeName.Length - startIndexOfUnqualifiedTypeName);
//        }
//      }

//      _ = nameBuilder.Append(typeName.ToArray());

//      if (isShortName)
//      {
//        return nameBuilder;
//      }

//      if (type.IsGenericType)
//      {
//        _ = nameBuilder.Append('<');

//        Type[] typeArguments = type.GetGenericArguments();
//        foreach (Type typeArgument in typeArguments)
//        {
//          _ = nameBuilder.AppendDisplayNameInternal(typeArgument, isFullyQualifiedName, isShortName)
//            .Append(ParameterSeparator);
//        }

//        _ = nameBuilder.Remove(nameBuilder.Length - ParameterSeparator.Length, ParameterSeparator.Length)
//          .Append('>');
//      }

//      return nameBuilder;
//    }

//  private static StringBuilder AppendDisplayNameInternal(this StringBuilder nameBuilder, MemberInfo memberInfo, bool isFullyQualifiedName, bool isShortName)
//    {
//      if (memberInfo is Type type)
//      {
//        return nameBuilder.AppendDisplayNameInternal(type, isFullyQualifiedName, isShortName);
//      }

//      if (isFullyQualifiedName)
//      {
//        _ = nameBuilder.AppendDisplayNameInternal(memberInfo.DeclaringType, isFullyQualifiedName, isShortName)
//          .Append('.');
//      }

//      if (memberInfo.MemberType.HasFlag(MemberTypes.Constructor))
//      {
//        if (memberInfo.DeclaringType.IsGenericType)
//        {
//          int genericTypeArgumentPlaceholderIndex = memberInfo.DeclaringType.Name.IndexOf('`');
//          return nameBuilder.Append(memberInfo.DeclaringType.Name, 0, genericTypeArgumentPlaceholderIndex);
//        }
//        else
//        {
//          return nameBuilder.Append(memberInfo.DeclaringType.Name);
//        }
//      }
//      else
//      {
//        return nameBuilder.Append(memberInfo.Name);
//      }
//    }
//    private static SymbolAttributes GetKind(this MemberInfo memberInfo)
//    {
//      var type = memberInfo as Type;
//      var propertyInfo = memberInfo as PropertyInfo;
//      MethodInfo methodInfo = memberInfo as MethodInfo // MemberInfo is method
//        ?? type?.GetMethod("Invoke"); // MemberInfo is potentially a delegate
//      MethodInfo propertyGetMethodInfo = propertyInfo?.GetGetMethod(true);
//      MethodInfo propertySetMethodInfo = propertyInfo?.GetSetMethod(true);
//      var constructorInfo = memberInfo as ConstructorInfo;
//      var fieldInfo = memberInfo as FieldInfo;
//      var eventInfo = memberInfo as EventInfo;
//      MethodInfo eventAddMethodInfo = eventInfo?.GetAddMethod(true);
//      FieldInfo eventDeclaredFieldInfo = eventInfo?.DeclaringType.GetField(eventInfo.Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

//      ParameterInfo[] indexerPropertyIndexParameters = propertyInfo?.GetIndexParameters() ?? Array.Empty<ParameterInfo>();

//      bool isDelegate = type?.IsDelegate() ?? false;
//      if (isDelegate)
//      {
//        return SymbolAttributes.Delegate;
//      }

//      bool isClass = !isDelegate && (type?.IsClass ?? false);
//      if (isClass)
//      {
//        SymbolAttributes classKind = SymbolAttributes.Class;
//        if (type.IsAbstract)
//        {
//          classKind |= SymbolAttributes.Abstract;
//        }

//        if (type.IsSealed)
//        {
//          classKind |= SymbolAttributes.Final;
//        }

//        if (type.IsStatic())
//        {
//          classKind |= SymbolAttributes.Static;
//        }

//        return classKind;
//      }

//      bool isEnum = !isDelegate && (type?.IsEnum ?? false);
//      if (isEnum)
//      {
//        return SymbolAttributes.Enum;
//      }

//      bool isStruct = !isDelegate && (type?.IsValueType ?? false);
//      if (isStruct)
//      {
//        SymbolAttributes structKind = SymbolAttributes.Struct;

//#if NETSTANDARD2_1_OR_GREATER || NET471_OR_GREATER || NET
//        bool isReadOnlyStruct = isStruct && type.GetCustomAttribute(typeof(IsReadOnlyAttribute)) != null;
//        if (isReadOnlyStruct)
//        {
//          structKind |= SymbolAttributes.Final;
//        }
//#endif
//        return structKind;
//      }

//      bool isProperty = propertyInfo != null;
//      if (isProperty)
//      {
//        bool isIndexerProperty = indexerPropertyIndexParameters.Length > 0;
//        SymbolAttributes propertyKind = isIndexerProperty
//          ? SymbolAttributes.IndexerProperty
//          : SymbolAttributes.Property;

//        MethodInfo getMethod = propertyInfo.GetGetMethod();
//        if (!propertyInfo.CanWrite)
//        {
//          propertyKind |= SymbolAttributes.Final;
//        }

//        if (getMethod.IsAbstract)
//        {
//          propertyKind |= SymbolAttributes.Abstract;
//        }

//        if (getMethod.IsStatic)
//        {
//          propertyKind |= SymbolAttributes.Static;
//        }

//        if (getMethod.IsVirtual)
//        {
//          propertyKind |= SymbolAttributes.Virtual;
//        }

//        if (getMethod.IsOverride())
//        {
//          propertyKind |= SymbolAttributes.Override;
//        }

//        return propertyKind;
//      }

//      bool isMethod = !isDelegate && !isClass && memberInfo.MemberType.HasFlag(MemberTypes.Method);
//      if (isMethod)
//      {
//        SymbolAttributes methodKind = SymbolAttributes.Method;
//        if (methodInfo.IsFinal)
//        {
//          methodKind |= SymbolAttributes.Final;
//        }

//        if (methodInfo.IsAbstract)
//        {
//          methodKind |= SymbolAttributes.Abstract;
//        }

//        if (methodInfo.IsStatic)
//        {
//          methodKind |= SymbolAttributes.Static;
//        }

//        if (methodInfo.IsVirtual)
//        {
//          methodKind |= SymbolAttributes.Virtual;
//        }

//        if (methodInfo.IsOverride())
//        {
//          methodKind |= SymbolAttributes.Override;
//        }

//        return methodKind;
//      }

//      bool isEvent = eventInfo != null;
//      if (isEvent)
//      {
//        SymbolAttributes eventKind = SymbolAttributes.Event;
//        MethodInfo addHandlerMethod = eventInfo.GetAddMethod(true);
//        if (addHandlerMethod.IsFinal)
//        {
//          eventKind |= SymbolAttributes.Final;
//        }

//        if (addHandlerMethod.IsAbstract)
//        {
//          eventKind |= SymbolAttributes.Abstract;
//        }

//        if (addHandlerMethod.IsStatic)
//        {
//          eventKind |= SymbolAttributes.Static;
//        }

//        if (addHandlerMethod.IsVirtual)
//        {
//          eventKind |= SymbolAttributes.Virtual;
//        }

//        if (addHandlerMethod.IsOverride())
//        {
//          eventKind |= SymbolAttributes.Override;
//        }

//        return eventKind;
//      }

//      bool isConstructor = constructorInfo != null;
//      if (isConstructor)
//      {
//        SymbolAttributes constructorKind = SymbolAttributes.Constructor;

//        if (constructorInfo.IsStatic)
//        {
//          constructorKind |= SymbolAttributes.Static;
//        }

//        return constructorKind;
//      }

//      bool isField = fieldInfo != null;
//      if (isField)
//      {
//        SymbolAttributes fieldKind = SymbolAttributes.Event;
//        if (fieldInfo.IsInitOnly)
//        {
//          fieldKind |= SymbolAttributes.Final;
//        }

//        if (fieldInfo.IsStatic)
//        {
//          fieldKind |= SymbolAttributes.Static;
//        }

//        return fieldKind;
//      }

//      bool isInterface = !isDelegate && !isClass && (type?.IsInterface ?? false);
//      if (isInterface)
//      {
//        SymbolAttributes interfaceKind = SymbolAttributes.Interface;
//        return interfaceKind;
//      }

//      return SymbolAttributes.Undefined;
//    }
//    internal static string ToSignatureNameInternal(this MemberInfo memberInfo, bool isFullyQualifiedName, bool isShortName, bool isCompact)
//    {
//      var fieldInfo = memberInfo as FieldInfo;
//      var eventInfo = memberInfo as EventInfo;
//      var propertyInfo = memberInfo as PropertyInfo;
//      AccessModifier GetAccessModifier()
//      {
//        switch (memberInfo)
//        {
//          case Type typeInfo:
//            return typeInfo.IsPublic ? AccessModifier.Public
//              : typeInfo.IsNestedPrivate ? AccessModifier.Private
//              : typeInfo.IsNestedAssembly ? AccessModifier.Internal
//              : typeInfo.IsNestedFamily ? AccessModifier.Protected
//              : typeInfo.IsNestedPublic ? AccessModifier.Public
//              : typeInfo.IsNestedFamORAssem ? AccessModifier.ProtectedInternal
//              : typeInfo.IsNestedFamANDAssem ? AccessModifier.PrivateProtected
//              : !typeInfo.IsVisible ? AccessModifier.Internal
//              : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
//          case MethodBase methodBaseInfo:
//            return methodBaseInfo.IsPublic ? AccessModifier.Public
//              : methodBaseInfo.IsPrivate ? AccessModifier.Private
//              : methodBaseInfo.IsAssembly ? AccessModifier.Internal
//              : methodBaseInfo.IsFamily ? AccessModifier.Protected
//              : methodBaseInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
//              : methodBaseInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
//              : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
//          case FieldInfo _:
//            return fieldInfo.IsPublic ? AccessModifier.Public
//              : fieldInfo.IsPrivate ? AccessModifier.Private
//              : fieldInfo.IsAssembly ? AccessModifier.Internal
//              : fieldInfo.IsFamily ? AccessModifier.Protected
//              : fieldInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
//              : fieldInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
//              : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
//          case EventInfo _:
//            return eventInfo.GetAddMethod(true).GetAccessModifier();
//          case PropertyInfo _:
//            return propertyInfo.GetAccessors(true)
//        .Select(accessor => accessor.GetAccessModifier())
//        .Min();
//          default:
//            throw new NotSupportedException("The provided MemberInfo is not supported");
//        }
//      }
//      // TODO::Create type specific overloads to eliminate type switching and use cached reflection data

//      var type = memberInfo as Type;
//      MethodInfo methodInfo = memberInfo as MethodInfo // MemberInfo is method
//        ?? type?.GetMethod("Invoke"); // MemberInfo is potentially a delegate
//      MethodInfo propertyGetMethodInfo = propertyInfo?.GetGetMethod(true);
//      MethodInfo propertySetMethodInfo = propertyInfo?.GetSetMethod(true);
//      var constructorInfo = memberInfo as ConstructorInfo;

//      ParameterInfo[] indexerPropertyIndexParameters = propertyInfo?.GetIndexParameters() ?? Array.Empty<ParameterInfo>();

//      SymbolAttributes memberAttributes = memberInfo.GetKind();
//      StringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();
//      IEnumerable<CustomAttributeData> symbolAttributes = memberInfo.GetCustomAttributesData();
//#if !NETSTANDARD2_0
//      if (memberAttributes.HasFlag(SymbolAttributes.Final))
//      {
//        symbolAttributes = symbolAttributes.Where(attributeData => attributeData.AttributeType != typeof(IsReadOnlyAttribute));
//      }
//#endif

//      _ = signatureNameBuilder.AppendCustomAttributes(symbolAttributes, isAppendNewLineEnabled: true);

//      AccessModifier accessModifier = GetAccessModifier();
//      _ = signatureNameBuilder
//        .Append(accessModifier.ToDisplayStringValue())
//        .Append(' ');


//      if (!memberAttributes.HasFlag(SymbolAttributes.Delegate)
//        && !memberAttributes.HasFlag(SymbolAttributes.Struct)
//        && !memberAttributes.HasFlag(SymbolAttributes.Field)
//        && memberAttributes.HasFlag(SymbolAttributes.Final))
//      {
//        _ = signatureNameBuilder
//          .Append("sealed")
//          .Append(' ');
//      }

//      if (!memberAttributes.HasFlag(SymbolAttributes.Delegate)
//        && memberAttributes.HasFlag(SymbolAttributes.Static))
//      {
//        _ = signatureNameBuilder
//          .Append("static")
//          .Append(' ');
//      }

//      bool isAbstract = memberAttributes.HasFlag(SymbolAttributes.Abstract);
//      if (!memberAttributes.HasFlag(SymbolAttributes.Delegate) && isAbstract)
//      {
//        _ = signatureNameBuilder
//          .Append("abstract")
//          .Append(' ');
//      }

//      if (!isAbstract
//        && !memberAttributes.HasFlag(SymbolAttributes.Delegate)
//        && !memberAttributes.HasFlag(SymbolAttributes.Class)
//        && memberAttributes.HasFlag(SymbolAttributes.Virtual))
//      {
//        _ = signatureNameBuilder
//          .Append("virtual")
//          .Append(' ');
//      }

//      if (memberAttributes.HasFlag(SymbolAttributes.ReadOnlyStruct)
//        || memberAttributes.HasFlag(SymbolAttributes.ReadOnlyField))
//      {
//        _ = signatureNameBuilder
//          .Append("readonly")
//          .Append(' ');
//      }

//      if (memberAttributes.HasFlag(SymbolAttributes.Struct))
//      {
//        _ = signatureNameBuilder
//          .Append("struct")
//          .Append(' ');
//      }

//      if (memberAttributes.HasFlag(SymbolAttributes.Class))
//      {
//        _ = signatureNameBuilder
//          .Append("class")
//          .Append(' ');
//      }

//      if (memberAttributes.HasFlag(SymbolAttributes.Interface))
//      {
//        _ = signatureNameBuilder
//          .Append("interface")
//          .Append(' ');
//      }

//      if (memberAttributes.HasFlag(SymbolAttributes.Delegate))
//      {
//        _ = signatureNameBuilder
//          .Append("delegate")
//          .Append(' ');
//      }

//      if (memberAttributes.HasFlag(SymbolAttributes.Event))
//      {
//        _ = signatureNameBuilder
//          .Append("event")
//          .Append(' ');
//      }

//      if (memberAttributes.HasFlag(SymbolAttributes.Enum))
//      {
//        _ = signatureNameBuilder
//          .Append("enum")
//          .Append(' ');
//      }

//      if (!memberAttributes.HasFlag(SymbolAttributes.Delegate)
//        && !memberAttributes.HasFlag(SymbolAttributes.Class)
//        && memberAttributes.HasFlag(SymbolAttributes.Override))
//      {
//        _ = signatureNameBuilder
//          .Append("override")
//          .Append(' ');
//      }

//      // Set return type
//      if (memberAttributes.HasFlag(SymbolAttributes.Method)
//        || memberAttributes.HasFlag(SymbolAttributes.Property)
//        || memberAttributes.HasFlag(SymbolAttributes.Field)
//        || memberAttributes.HasFlag(SymbolAttributes.Delegate)
//        || memberAttributes.HasFlag(SymbolAttributes.Event))
//      {
//        Type returnType = fieldInfo?.FieldType
//          ?? methodInfo?.ReturnType
//          ?? propertyGetMethodInfo?.ReturnType
//          ?? eventInfo?.EventHandlerType;

//        _ = signatureNameBuilder.AppendDisplayNameInternal(returnType, isFullyQualifiedName, isShortName: false)
//          .Append(' ');
//      }

//      if (memberAttributes.HasFlag(SymbolAttributes.Member) && (!isShortName || isFullyQualifiedName))
//      {
//        _ = signatureNameBuilder.AppendDisplayNameInternal(memberInfo.DeclaringType, isFullyQualifiedName, isShortName: false)
//          .Append('.');
//      }

//      // Member or type name
//      if (memberAttributes.HasFlag(SymbolAttributes.IndexerProperty))
//      {
//        _ = signatureNameBuilder.Append("this");
//      }
//      else
//      {
//        _ = signatureNameBuilder.AppendDisplayNameInternal(memberInfo, isFullyQualifiedName: isFullyQualifiedName && memberAttributes.HasFlag(SymbolAttributes.Type), isShortName: false);
//      }

//      if (memberAttributes.HasFlag(SymbolAttributes.Constructor)
//        || memberAttributes.HasFlag(SymbolAttributes.Method)
//        || memberAttributes.HasFlag(SymbolAttributes.Delegate))
//      {
//        _ = signatureNameBuilder.Append('(');

//        if (memberAttributes.HasFlag(SymbolAttributes.Method) && (methodInfo?.IsExtensionMethod() ?? false))
//        {
//          _ = signatureNameBuilder
//            .Append("this")
//            .Append(' ');
//        }
//      }
//      else if (memberAttributes.HasFlag(SymbolAttributes.IndexerProperty))
//      {
//        _ = signatureNameBuilder.Append('[');
//      }

//      IEnumerable<ParameterInfo> parameters = methodInfo?.GetParameters()
//        ?? constructorInfo?.GetParameters()
//        ?? indexerPropertyIndexParameters
//        ?? Enumerable.Empty<ParameterInfo>();

//      if (parameters.Any())
//      {
//        foreach (ParameterInfo parameter in parameters)
//        {
//          bool isGenericTypeDefinition = false;
//          if (memberAttributes.HasFlag(SymbolAttributes.GenericMethod))
//          {
//            isGenericTypeDefinition = methodInfo.IsGenericMethodDefinition;
//          }
//          else if (memberAttributes.HasFlag(SymbolAttributes.GenericType))
//          {
//            isGenericTypeDefinition = type.IsGenericTypeDefinition;
//          }

//          if (isGenericTypeDefinition)
//          {
//            IEnumerable<CustomAttributeData> attributes = parameter.GetCustomAttributesData();
//            _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false);
//          }

//          if (parameter.IsRef())
//          {
//            _ = signatureNameBuilder.Append("ref ");
//          }
//          else if (parameter.IsIn)
//          {
//            _ = signatureNameBuilder.Append("in ");
//          }
//          else if (parameter.IsOut)
//          {
//            _ = signatureNameBuilder.Append("out ");
//          }

//          _ = signatureNameBuilder
//            .AppendDisplayNameInternal(parameter.ParameterType, isFullyQualifiedName, isShortName: false)
//            .Append(' ')
//            .Append(parameter.Name)
//            .Append(HelperExtensionsCommon.ParameterSeparator);
//        }

//        // Remove trailing comma and whitespace
//        _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
//      }

//      if (memberAttributes.HasFlag(SymbolAttributes.Constructor)
//        || memberAttributes.HasFlag(SymbolAttributes.Method)
//        || memberAttributes.HasFlag(SymbolAttributes.Delegate))
//      {
//        _ = signatureNameBuilder.Append(')');
//      }
//      else if (memberAttributes.HasFlag(SymbolAttributes.IndexerProperty))
//      {
//        _ = signatureNameBuilder.Append(']');
//      }

//      if (memberAttributes.HasFlag(SymbolAttributes.Property))
//      {
//        _ = signatureNameBuilder
//          .Append(' ')
//          .Append('{')
//          .Append(' ');

//        if (propertyGetMethodInfo != null)
//        {
//          _ = signatureNameBuilder
//            .Append("get")
//            .Append(HelperExtensionsCommon.ExpressionTerminator)
//            .Append(' ');
//        }

//        if (propertySetMethodInfo != null)
//        {
//          _ = signatureNameBuilder
//            .Append("set")
//            .Append(HelperExtensionsCommon.ExpressionTerminator)
//            .Append(' ');
//        }

//        _ = signatureNameBuilder.Append('}');
//      }

//      if (memberAttributes.HasFlag(SymbolAttributes.Class))
//      {
//        signatureNameBuilder = signatureNameBuilder.AppendInheritanceSignature(type, isFullyQualifiedName);
//      }

//      if (memberAttributes.HasFlag(SymbolAttributes.Generic))
//      {
//        Type[] genericTypeParameterDefinitions = Type.EmptyTypes;
//        if (memberAttributes.HasFlag(SymbolAttributes.GenericType) && type.IsGenericTypeDefinition)
//        {
//          genericTypeParameterDefinitions = type.GetGenericTypeDefinition().GetGenericArguments();
//        }
//        else if (memberAttributes.HasFlag(SymbolAttributes.GenericMethod) && methodInfo.IsGenericMethodDefinition)
//        {
//          genericTypeParameterDefinitions = methodInfo.GetGenericMethodDefinition().GetGenericArguments();
//        }

//        _ = signatureNameBuilder.AppendGenericTypeConstraints(genericTypeParameterDefinitions, isFullyQualifiedName, isCompact);
//      }

//      if (!memberAttributes.HasFlag(SymbolAttributes.Class)
//        && !memberAttributes.HasFlag(SymbolAttributes.Struct)
//        && !memberAttributes.HasFlag(SymbolAttributes.Enum))
//      {
//        _ = signatureNameBuilder.Append(HelperExtensionsCommon.ExpressionTerminator);
//      }

//      string fullMemberName = signatureNameBuilder.ToString();
//      StringBuilderFactory.Recycle(signatureNameBuilder);

//      return fullMemberName;
//    }
//    private static StringBuilder AppendInheritanceSignature(this StringBuilder memberNameBuilder, Type typeData, bool isFullyQualified)
//    {
//      bool isDelegate = HelperExtensionsCommon.DelegateType.IsAssignableFrom(typeData);
//      if (isDelegate)
//      {
//        return memberNameBuilder;
//      }

//      bool isSubclass = typeData.BaseType != typeof(object);
//      Type[] interfaces = typeData.GetInterfaces();
//      bool hasInterfaces = interfaces.Length > 0;
//      if (isSubclass || hasInterfaces)
//      {
//        _ = memberNameBuilder.Append(" : ");
//      }

//      if (isSubclass)
//      {
//        _ = memberNameBuilder.Append(isFullyQualified ? typeData.BaseType.FullName : typeData.BaseType.Name)
//          .Append(HelperExtensionsCommon.ParameterSeparator);
//      }

//      foreach (Type interfaceData in interfaces)
//      {
//        _ = memberNameBuilder.Append(isFullyQualified ? interfaceData.FullName : interfaceData.Name)
//          .Append(HelperExtensionsCommon.ParameterSeparator);
//      }

//      if (isSubclass || hasInterfaces)
//      {
//        _ = memberNameBuilder.Remove(memberNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
//      }

//      return memberNameBuilder;
//    }
//    private static StringBuilder AppendGenericTypeConstraints(this StringBuilder constraintBuilder, Type[] genericTypeDefinitionsData, bool isFullyQualified, bool isCompact)
//    {
//      bool hasSingleNewLine = false;
//      for (int genericTypeArgumentIndex = 0; genericTypeArgumentIndex < genericTypeDefinitionsData.Length; genericTypeArgumentIndex++)
//      {
//        Type genericTypeDefinitionData = genericTypeDefinitionsData[genericTypeArgumentIndex];
//        Type[] constraints = genericTypeDefinitionData.GetGenericParameterConstraints();
//        if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask) == GenericParameterAttributes.None
//          && constraints.Length == 0)
//        {
//          continue;
//        }

//        if (isCompact)
//        {
//          if (!hasSingleNewLine)
//          {
//            _ = constraintBuilder.AppendLine()
//            .Append(HelperExtensionsCommon.Indentation);
//            hasSingleNewLine = true;
//          }
//          else
//          {
//            _ = constraintBuilder.Append(' ');
//          }
//        }
//        else
//        {
//          _ = constraintBuilder.AppendLine()
//            .Append(HelperExtensionsCommon.Indentation);
//        }

//        _ = constraintBuilder.Append("where")
//          .Append(' ')
//          .Append(genericTypeDefinitionData.Name)
//          .Append(" : ");

//        if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
//        {
//          _ = constraintBuilder.Append("class")
//            .Append(HelperExtensionsCommon.ParameterSeparator);
//        }

//        if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
//        {
//          _ = constraintBuilder.Append("struct")
//            .Append(HelperExtensionsCommon.ParameterSeparator);
//        }

//        foreach (Type constraint in constraints)
//        {
//          _ = constraintBuilder.AppendDisplayNameInternal(constraint, isFullyQualified, isShortName: false)
//            .Append(HelperExtensionsCommon.ParameterSeparator);
//        }

//        if (!genericTypeDefinitionData.IsValueType && (genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
//        {
//          _ = constraintBuilder.Append("new()")
//            .Append(HelperExtensionsCommon.ParameterSeparator);
//        }

//        _ = constraintBuilder.Remove(constraintBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
//      }

//      return constraintBuilder;
//    }
    #endregion REMOVE AFTER BENCHMARK COMPARISON!!!

    internal static string ToSignatureNameInternal(PropertyData propertyData, bool isFullyQualifiedName, bool isShortName, bool isCompact)
    {
      SymbolAttributes symbolAttributes = propertyData.SymbolAttributes;
      HashSet<CustomAttributeData> customAttributesData = propertyData.AttributeData;
      StringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

#if !NETSTANDARD2_0
      if (symbolAttributes.HasFlag(SymbolAttributes.Final))
      {
        customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != typeof(IsReadOnlyAttribute)).ToHashSet();
      }
#endif

      _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);

      AccessModifier accessModifier = propertyData.AccessModifier;
      _ = signatureNameBuilder
        .Append(accessModifier.ToDisplayStringValue())
        .Append(' ');

      if (symbolAttributes.HasFlag(SymbolAttributes.Static))
      {
        _ = signatureNameBuilder
          .Append("static")
          .Append(' ');
      }

      bool isAbstract = symbolAttributes.HasFlag(SymbolAttributes.Abstract);
      if (!symbolAttributes.HasFlag(SymbolAttributes.Delegate) && isAbstract)
      {
        _ = signatureNameBuilder
          .Append("abstract")
          .Append(' ');
      }
      else if (symbolAttributes.HasFlag(SymbolAttributes.Virtual))
      {
        _ = signatureNameBuilder
          .Append("virtual")
          .Append(' ');
      }

      if (symbolAttributes.HasFlag(SymbolAttributes.Override))
      {
        _ = signatureNameBuilder
          .Append("override")
          .Append(' ');
      }

      // Set return type
      _ = signatureNameBuilder.AppendDisplayNameInternal(propertyData.PropertyTypeData, isFullyQualifiedName, isShortName: false)
        .Append(' ');

      if (!isShortName)
      {
        _ = signatureNameBuilder.AppendDisplayNameInternal(propertyData.DeclaringTypeData, isFullyQualifiedName, isShortName: false)
          .Append('.');
      }

      // Member name
      if (symbolAttributes.HasFlag(SymbolAttributes.IndexerProperty))
      {
        _ = signatureNameBuilder.Append("this")
          .Append('[');

        ParameterData[] parameters = propertyData.IndexerParameters;
        if (parameters.Any())
        {
          foreach (ParameterData parameter in parameters)
          {
            HashSet<CustomAttributeData> attributes = parameter.AttributeData;
            _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false)
              .AppendDisplayNameInternal(parameter.ParameterTypeData, isFullyQualifiedName, isShortName: false)
              .Append(' ')
              .Append(parameter.Name)
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }

          // Remove trailing comma and whitespace
          _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length)
            .Append(']');
        }
      }
      else
      {
        _ = signatureNameBuilder.AppendDisplayNameInternal(propertyData, isFullyQualifiedName: false, isShortName: false);
      }

      _ = signatureNameBuilder
        .Append(' ')
        .Append('{')
        .Append(' ');

      if (propertyData.CanRead)
      {
        if (HelperExtensionsCommon.AccessModifierComparer.Compare(propertyData.GetAccessorAccessModifier, propertyData.AccessModifier) < 0)
        {
          _ = signatureNameBuilder
            .Append(propertyData.GetAccessorAccessModifier)
            .Append(' ');
        }

        _ = signatureNameBuilder
          .Append("get")
          .Append(HelperExtensionsCommon.ExpressionTerminator)
          .Append(' ');
      }

      if (propertyData.CanWrite)
      {
        if (HelperExtensionsCommon.AccessModifierComparer.Compare(propertyData.SetAccessorAccessModifier, propertyData.AccessModifier) < 0)
        {
          _ = signatureNameBuilder
            .Append(propertyData.SetAccessorAccessModifier)
            .Append(' ');
        }

#if !NETSTANDARD2_0
        if (propertyData.IsSetMethodReadOnly)
        {
          _ = signatureNameBuilder
            .Append("readonly")
            .Append(' ');
        }
#endif
        if (propertyData.SymbolAttributes.HasFlag(SymbolAttributes.InitProperty))
        {
          _ = signatureNameBuilder
            .Append("init")
            .Append(HelperExtensionsCommon.ExpressionTerminator)
            .Append(' ');
        }
        else
        {
          _ = signatureNameBuilder
          .Append("set")
          .Append(HelperExtensionsCommon.ExpressionTerminator)
          .Append(' ');
        }
      }

      _ = signatureNameBuilder.Append('}');

      string fullMemberName = signatureNameBuilder.ToString();
      StringBuilderFactory.Recycle(signatureNameBuilder);

      return fullMemberName;
    }

    internal static string ToSignatureNameInternal(EventData eventData, bool isFullyQualifiedName, bool isShortName, bool isCompact)
    {
      SymbolAttributes symbolAttributes = eventData.SymbolAttributes;
      HashSet<CustomAttributeData> customAttributesData = eventData.AttributeData;

#if !NETSTANDARD2_0
      if (symbolAttributes.HasFlag(SymbolAttributes.Final))
      {
        customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
      }
#endif

      StringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate()
        .AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);

      AccessModifier accessModifier = eventData.AccessModifier;
      _ = signatureNameBuilder
        .Append(accessModifier.ToDisplayStringValue())
        .Append(' ');

      if (symbolAttributes.HasFlag(SymbolAttributes.Static))
      {
        _ = signatureNameBuilder
          .Append("static")
          .Append(' ');
      }
      else if (symbolAttributes.HasFlag(SymbolAttributes.Abstract))
      {
        _ = signatureNameBuilder
          .Append("abstract")
          .Append(' ');
      }
      else if (symbolAttributes.HasFlag(SymbolAttributes.Virtual))
      {
        _ = signatureNameBuilder
          .Append("virtual")
          .Append(' ');
      }

      if (symbolAttributes.HasFlag(SymbolAttributes.Override))
      {
        _ = signatureNameBuilder
          .Append("override")
          .Append(' ');
      }

      _ = signatureNameBuilder
        .Append("event")
        .Append(' ')
        .AppendDisplayNameInternal(eventData.EventHandlerTypeData, isFullyQualifiedName, isShortName: true)
        .Append(' ');

      if (!isShortName)
      {
        _ = signatureNameBuilder.AppendDisplayNameInternal(eventData.DeclaringTypeData, isFullyQualifiedName, isShortName: false)
          .Append('.');
      }

      _ = signatureNameBuilder.AppendDisplayNameInternal(eventData, isFullyQualifiedName: false, isShortName: false)
        .Append(HelperExtensionsCommon.ExpressionTerminator);

      string fullMemberName = signatureNameBuilder.ToString();
      StringBuilderFactory.Recycle(signatureNameBuilder);

      return fullMemberName;
    }

    internal static string ToSignatureNameInternal(FieldData fieldData, bool isFullyQualifiedName, bool isShortName, bool isCompact)
    {
      SymbolAttributes symbolAttributes = fieldData.SymbolAttributes;
      HashSet<CustomAttributeData> customAttributesData = fieldData.AttributeData;

#if !NETSTANDARD2_0
      if (symbolAttributes.HasFlag(SymbolAttributes.Final))
      {
        customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
      }
#endif

      StringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate()
        .AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);

      AccessModifier accessModifier = fieldData.AccessModifier;
      _ = signatureNameBuilder
        .Append(accessModifier.ToDisplayStringValue())
        .Append(' ');

      if (symbolAttributes.HasFlag(SymbolAttributes.ConstantField))
      {
        _ = signatureNameBuilder
          .Append("const")
          .Append(' ');
      }
      else
      {
        if (symbolAttributes.HasFlag(SymbolAttributes.Static))
        {
          _ = signatureNameBuilder
            .Append("static")
            .Append(' ');
        }

        if (symbolAttributes.HasFlag(SymbolAttributes.ReadOnlyField))
        {
          _ = signatureNameBuilder
            .Append("readonly")
            .Append(' ');
        }

        if (fieldData.IsRef)
        {
          _ = signatureNameBuilder
            .Append("ref")
            .Append(' ');
        }
      }

      _ = signatureNameBuilder.AppendDisplayNameInternal(fieldData.FieldTypeData, isFullyQualifiedName, isShortName: true)
        .Append(' ');

      if (!isShortName)
      {
        _ = signatureNameBuilder.AppendDisplayNameInternal(fieldData.DeclaringTypeData, isFullyQualifiedName, isShortName: false)
          .Append('.');
      }

      _ = signatureNameBuilder.AppendDisplayNameInternal(fieldData, isFullyQualifiedName: false, isShortName: false)
        .Append(HelperExtensionsCommon.ExpressionTerminator);

      string fullMemberName = signatureNameBuilder.ToString();
      StringBuilderFactory.Recycle(signatureNameBuilder);

      return fullMemberName;
    }

    internal static string ToSignatureNameInternal(TypeData typeData, bool isFullyQualifiedName, bool isShortName, bool isCompact)
    {
      if (typeData.IsGenericType && !typeData.IsGenericTypeDefinition)
      {
        typeData = typeData.GenericTypeDefinitionData;
      }

      SymbolAttributes symbolAttributes = typeData.SymbolAttributes;
      HashSet<CustomAttributeData> customAttributesData = typeData.AttributeData;
      StringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

#if !NETSTANDARD2_0
      if (symbolAttributes.HasFlag(SymbolAttributes.Final))
      {
        customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
      }
#endif

      _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);

      AccessModifier accessModifier = typeData.AccessModifier;
      _ = signatureNameBuilder
        .Append(accessModifier.ToDisplayStringValue())
        .Append(' ');

      if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
      {
        _ = signatureNameBuilder
          .Append("delegate")
          .Append(' ');
      }
      else if (symbolAttributes.HasFlag(SymbolAttributes.Struct))
      {
        if (symbolAttributes.HasFlag(SymbolAttributes.ReadOnlyStruct))
        {
          _ = signatureNameBuilder
            .Append("readonly")
            .Append(' ');
        }
        else if (symbolAttributes.HasFlag(SymbolAttributes.RefStruct))
        {
          _ = signatureNameBuilder
            .Append("ref")
            .Append(' ');
        }

        _ = signatureNameBuilder
          .Append("struct")
          .Append(' ');
      }
      else if (symbolAttributes.HasFlag(SymbolAttributes.Class))
      {
        if (symbolAttributes.HasFlag(SymbolAttributes.Abstract))
        {
          _ = signatureNameBuilder
            .Append("abstract")
            .Append(' ');
        }
        else if (symbolAttributes.HasFlag(SymbolAttributes.Static))
        {
          _ = signatureNameBuilder
            .Append("static")
            .Append(' ');
        }
        else if (symbolAttributes.HasFlag(SymbolAttributes.Final))
        {
          _ = signatureNameBuilder
            .Append("sealed")
            .Append(' ');
        }

        _ = signatureNameBuilder
          .Append("class")
          .Append(' ');
      }
      else if (symbolAttributes.HasFlag(SymbolAttributes.Interface))
      {
        _ = signatureNameBuilder
          .Append("interface")
          .Append(' ');
      }
      else if (symbolAttributes.HasFlag(SymbolAttributes.Enum))
      {
        _ = signatureNameBuilder
          .Append("enum")
          .Append(' ');
      }      

      MethodData delegateInvocatorData = null;
      TypeData delegateReturnTypeData = null;

      // Set return type
      if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
      {
        delegateInvocatorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeData.GetType().GetMethod("Invoke"));
        delegateReturnTypeData = delegateInvocatorData.ReturnTypeData;
        _ = signatureNameBuilder.AppendDisplayNameInternal(delegateReturnTypeData, isFullyQualifiedName, isShortName: false)
          .Append(' ');
      }

      // Type name
      _ = signatureNameBuilder.AppendDisplayNameInternal(typeData, isFullyQualifiedName, isShortName: false);

      if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
      {
        _ = signatureNameBuilder.Append('(');

        ParameterData[] parameters = delegateInvocatorData.Parameters;
        if (parameters.Length > 0)
        {
          foreach (ParameterData parameterData in parameters)
          {
            HashSet<CustomAttributeData> attributes = parameterData.AttributeData;
            _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false);

            if (parameterData.IsRef)
            {
              _ = signatureNameBuilder.Append("ref ");
            }
            else if (parameterData.IsIn)
            {
              _ = signatureNameBuilder.Append("in ");
            }
            else if (parameterData.IsOut)
            {
              _ = signatureNameBuilder.Append("out ");
            }

            _ = signatureNameBuilder
              .AppendDisplayNameInternal(parameterData.ParameterTypeData, isFullyQualifiedName, isShortName: false)
              .Append(' ')
              .Append(parameterData.Name)
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }

          // Remove trailing comma and whitespace
          _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length)
            .Append(')');
        }
      }
      else if (symbolAttributes.HasFlag(SymbolAttributes.Class))
      {
        signatureNameBuilder = signatureNameBuilder.AppendInheritanceSignature(typeData, isFullyQualifiedName);
      }

      if (typeData.IsGenericType)
      {
        TypeData[] genericTypeParameterDefinitions = typeData.GenericTypeArguments;
        if (genericTypeParameterDefinitions.Length > 0)
        {
          _ = signatureNameBuilder
            .Append(' ')
            .AppendGenericTypeConstraints(genericTypeParameterDefinitions, isFullyQualifiedName, isCompact);
        }
      }

      string fullMemberName = signatureNameBuilder.Append(HelperExtensionsCommon.ExpressionTerminator)
        .ToString();
      StringBuilderFactory.Recycle(signatureNameBuilder);

      return fullMemberName;
    }

    internal static string ToSignatureNameInternal(MethodData methodData, bool isFullyQualifiedName, bool isShortName, bool isCompact)
    {
      Debug.WriteLine($"Generating method signature");
      if (methodData.IsGenericMethod && !methodData.IsGenericMethodDefinition)
      {
        methodData = methodData.GenericMethodDefinitionData;
      }

      SymbolAttributes symbolAttributes = methodData.SymbolAttributes;
      HashSet<CustomAttributeData> symbolCustomAttributesData = methodData.AttributeData;
      StringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

#if !NETSTANDARD2_0
      if (symbolAttributes.HasFlag(SymbolAttributes.Final))
      {
        symbolCustomAttributesData = symbolCustomAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
      }
#endif

      _ = signatureNameBuilder.AppendCustomAttributes(symbolCustomAttributesData, isAppendNewLineEnabled: true);

      AccessModifier accessModifier = methodData.AccessModifier;
      _ = signatureNameBuilder
        .Append(accessModifier.ToDisplayStringValue())
        .Append(' ');

      if (methodData.IsStatic)
      {
        _ = signatureNameBuilder
          .Append("static")
          .Append(' ');
      }

      if (methodData.IsSealed)
      {
        _ = signatureNameBuilder
          .Append("sealed")
          .Append(' ');
      }

      bool isAbstract = symbolAttributes.HasFlag(SymbolAttributes.Abstract);
      if (isAbstract)
      {
        _ = signatureNameBuilder
          .Append("abstract")
          .Append(' ');
      }
      else if (symbolAttributes.HasFlag(SymbolAttributes.Virtual))
      {
        _ = signatureNameBuilder
          .Append("virtual")
          .Append(' ');
      }

      if (methodData.IsOverride)
      {
        _ = signatureNameBuilder
          .Append("override")
          .Append(' ');
      }

      if (methodData.IsAsync)
      {
        _ = signatureNameBuilder
          .Append("async")
          .Append(' ');
      }

      if (methodData.IsReturnValueByRef)
      {
        _ = signatureNameBuilder
          .Append("ref")
          .Append(' ');
      }

#if !NETSTANDARD2_0
      if (methodData.IsReturnValueReadOnly)
      {
        _ = signatureNameBuilder
          .Append("readonly")
          .Append(' ');
      }
#endif

      _ = signatureNameBuilder.AppendDisplayNameInternal(methodData.ReturnTypeData, isFullyQualifiedName, isShortName: true)
        .Append(' ');

      if (!isShortName)
      {
        _ = signatureNameBuilder.AppendDisplayNameInternal(methodData.DeclaringTypeData, isFullyQualifiedName, isShortName: false)
          .Append('.');
      }

      // Member name
      _ = signatureNameBuilder.AppendDisplayNameInternal(methodData, isFullyQualifiedName: false, isShortName: false)
        .Append('(');

      if (methodData.IsExtensionMethod)
      {
        _ = signatureNameBuilder
          .Append("this")
          .Append(' ');
      }

      ParameterData[] parameters = methodData.Parameters;
      if (parameters.Length > 0)
      {
        foreach (ParameterData parameterData in parameters)
        {
          HashSet<CustomAttributeData> attributes = parameterData.AttributeData;
          _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false);

          if (parameterData.IsRef)
          {
            _ = signatureNameBuilder.Append("ref ");
          }
          else if (parameterData.IsIn)
          {
            _ = signatureNameBuilder.Append("in ");
          }
          else if (parameterData.IsOut)
          {
            _ = signatureNameBuilder.Append("out ");
          }

          _ = signatureNameBuilder
            .AppendDisplayNameInternal(parameterData.ParameterTypeData, isFullyQualifiedName, isShortName: false)
            .Append(' ')
            .Append(parameterData.Name)
            .Append(HelperExtensionsCommon.ParameterSeparator);
        }

        // Remove trailing comma and whitespace
        _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length)
          .Append(')');
      }

      if (methodData.IsGenericMethod)
      {
        TypeData[] genericTypeParameterDefinitions = methodData.GenericTypeArguments;
        if (genericTypeParameterDefinitions.Length > 0)
        {
          _ = signatureNameBuilder
            .Append(' ')
            .AppendGenericTypeConstraints(genericTypeParameterDefinitions, isFullyQualifiedName, isCompact);
        }
      }

      _ = signatureNameBuilder.Append(HelperExtensionsCommon.ExpressionTerminator);

      string fullMemberName = signatureNameBuilder.ToString();
      StringBuilderFactory.Recycle(signatureNameBuilder);

      return fullMemberName;
    }

    internal static string ToSignatureNameInternal(ConstructorData constructorData, bool isFullyQualifiedName, bool isShortName, bool isCompact)
    {
      SymbolAttributes symbolAttributes = constructorData.SymbolAttributes;
      HashSet<CustomAttributeData> symbolCustomAttributesData = constructorData.AttributeData;
      StringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

#if !NETSTANDARD2_0
      if (symbolAttributes.HasFlag(SymbolAttributes.Final))
      {
        symbolCustomAttributesData = symbolCustomAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
      }
#endif

      _ = signatureNameBuilder.AppendCustomAttributes(symbolCustomAttributesData, isAppendNewLineEnabled: true);

      AccessModifier accessModifier = constructorData.AccessModifier;
      if (accessModifier is AccessModifier.Undefined)
      {
        _ = signatureNameBuilder
        .Append(accessModifier.ToDisplayStringValue())
        .Append(' ');
      }

      if (constructorData.IsStatic)
      {
        _ = signatureNameBuilder
          .Append("static")
          .Append(' ');
      }

      if (!isShortName)
      {
        _ = signatureNameBuilder.AppendDisplayNameInternal(constructorData.DeclaringTypeData, isFullyQualifiedName, isShortName: false)
          .Append('.');
      }

      // Member name
      _ = signatureNameBuilder.AppendDisplayNameInternal(constructorData, isFullyQualifiedName: false, isShortName: false)
        .Append('(');

      ParameterData[] parameters = constructorData.Parameters;
      if (parameters.Length > 0)
      {
        foreach (ParameterData parameterData in parameters)
        {
          HashSet<CustomAttributeData> attributes = parameterData.AttributeData;
          _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false);

          if (parameterData.IsRef)
          {
            _ = signatureNameBuilder.Append("ref ");
          }
          else if (parameterData.IsIn)
          {
            _ = signatureNameBuilder.Append("in ");
          }
          else if (parameterData.IsOut)
          {
            _ = signatureNameBuilder.Append("out ");
          }

          _ = signatureNameBuilder
            .AppendDisplayNameInternal(parameterData.ParameterTypeData, isFullyQualifiedName, isShortName: false)
            .Append(' ')
            .Append(parameterData.Name)
            .Append(HelperExtensionsCommon.ParameterSeparator);
        }

        // Remove trailing comma and whitespace
        _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length)
          .Append(')');
      }

      _ = signatureNameBuilder.Append(HelperExtensionsCommon.ExpressionTerminator);

      string fullMemberName = signatureNameBuilder.ToString();
      StringBuilderFactory.Recycle(signatureNameBuilder);

      return fullMemberName;
    }

    private static StringBuilder AppendCustomAttributes(this StringBuilder nameBuilder, IEnumerable<CustomAttributeData> attributes, bool isAppendNewLineEnabled)
    {
      foreach (CustomAttributeData attribute in attributes)
      {
        bool hasAttributeArguments = false;

        if (HelperExtensionsCommon.IgnorableParameterAttributes.Contains(attribute.AttributeType.Name))
        {
          continue;
        }

        _ = nameBuilder.Append('[')
          .Append(attribute.AttributeType.Name)
          .Append('(');

        foreach (CustomAttributeTypedArgument constructorPositionalArgument in attribute.ConstructorArguments)
        {
          hasAttributeArguments = true;

          if (constructorPositionalArgument.ArgumentType == typeof(string))
          {
            _ = nameBuilder.Append('"')
              .Append((string)constructorPositionalArgument.Value)
              .Append('"')
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }
          else if (constructorPositionalArgument.ArgumentType == typeof(char))
          {
            _ = nameBuilder.Append('\'')
              .Append((char)constructorPositionalArgument.Value)
              .Append('\'')
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }
          else
          {
            _ = nameBuilder.AppendPrimitiveType(constructorPositionalArgument.Value)
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }
        }

        foreach (CustomAttributeNamedArgument constructorNamedArgument in attribute.NamedArguments)
        {
          hasAttributeArguments = true;

          _ = nameBuilder.Append(constructorNamedArgument.MemberName)
            .Append(" = ");

          if (constructorNamedArgument.TypedValue.ArgumentType == typeof(string))
          {
            _ = nameBuilder.Append('"')
              .Append((string)constructorNamedArgument.TypedValue.Value)
              .Append('"')
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }
          else if (constructorNamedArgument.TypedValue.ArgumentType == typeof(char))
          {
            _ = nameBuilder.Append('\'')
              .Append((char)constructorNamedArgument.TypedValue.Value)
              .Append('\'')
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }
          else
          {
            _ = nameBuilder.AppendPrimitiveType(constructorNamedArgument.TypedValue.Value)
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }
        }

        if (!hasAttributeArguments)
        {
          // Remove trailing opening parenthesis
          _ = nameBuilder.Remove(nameBuilder.Length - 1, 1)
            .Append(']');
        }
        else
        {
          // Remove trailing comma and whitespace
          _ = nameBuilder.Remove(nameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length)
            .Append(')')
            .Append(']');
        }

        if (isAppendNewLineEnabled)
        {
          _ = nameBuilder.AppendLine();
        }
        else
        {
          _ = nameBuilder.Append(' ');
        }
      }

      return nameBuilder;
    }

    private static StringBuilder AppendPrimitiveType(this StringBuilder stringBuilder, object value)
    {
      if (!value.GetType().IsPrimitive)
      {
        throw new ArgumentException("Only primitive typeArguments allowed)");
      }

      switch (value)
      {
        case int intValue:
          return stringBuilder.Append(intValue);
        case double doubleValue:
          return stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", doubleValue);
        default:
          throw new NotImplementedException();
      }
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> symbolAttributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param genericTypeParameterIdentifier="type"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="type"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="type"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref genericTypeParameterIdentifier="type"/> is not supported.</exception>
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
    public static AccessModifier GetAccessModifier(this Type type)
    {
      TypeData entry = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
      return entry.AccessModifier;
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> symbolAttributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param genericTypeParameterIdentifier="type"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="type"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="type"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref genericTypeParameterIdentifier="type"/> is not supported.</exception>
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
    public static AccessModifier GetAccessModifier(this MethodInfo method)
    {
      MethodData entry = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(method);
      return entry.AccessModifier;
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> symbolAttributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param genericTypeParameterIdentifier="type"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="type"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="type"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref genericTypeParameterIdentifier="type"/> is not supported.</exception>
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
    public static AccessModifier GetAccessModifier(this ConstructorInfo constructor)
    {
      ConstructorData entry = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructor);
      return entry.AccessModifier;
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> symbolAttributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param genericTypeParameterIdentifier="type"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="type"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="type"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref genericTypeParameterIdentifier="type"/> is not supported.</exception>
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
    public static AccessModifier GetAccessModifier(this PropertyInfo property)
    {
      PropertyData entry = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(property);
      return entry.AccessModifier;
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> symbolAttributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param genericTypeParameterIdentifier="type"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="type"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="type"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref genericTypeParameterIdentifier="type"/> is not supported.</exception>
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
    public static AccessModifier GetAccessModifier(this EventInfo eventInfo)
    {
      EventData entry = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
      return entry.AccessModifier;
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> symbolAttributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param genericTypeParameterIdentifier="type"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="type"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="type"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref genericTypeParameterIdentifier="type"/> is not supported.</exception>
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
    public static AccessModifier GetAccessModifier(this FieldInfo field)
    {
      FieldData entry = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(field);
      return entry.AccessModifier;
    }

    internal static AccessModifier GetAccessModifierInternal(PropertyData propertyData) 
      => GetPropertyAccessModifier(propertyData.GetMethodData, propertyData.SetMethodData).PropertyModifier;

    internal static AccessModifier GetAccessModifierInternal(EventData eventData) 
      => GetAccessModifierInternal(eventData.AddMethodData);

    internal static AccessModifier GetAccessModifierInternal(FieldData fieldData)
    {
      FieldInfo fieldInfo = fieldData.GetFieldInfo();
      return fieldInfo.IsPublic ? AccessModifier.Public
        : fieldInfo.IsPrivate ? AccessModifier.Private
        : fieldInfo.IsAssembly ? AccessModifier.Internal
        : fieldInfo.IsFamily ? AccessModifier.Protected
        : fieldInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
        : fieldInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
        : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
    }

    internal static AccessModifier GetAccessModifierInternal(MethodData methodData)
    {
      MethodInfo methodInfo = methodData.GetMethodInfo();
      return methodInfo.IsPublic ? AccessModifier.Public
        : methodInfo.IsPrivate ? AccessModifier.Private
        : methodInfo.IsAssembly ? AccessModifier.Internal
        : methodInfo.IsFamily ? AccessModifier.Protected
        : methodInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
        : methodInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
        : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
    }

    internal static AccessModifier GetAccessModifierInternal(ConstructorData constructorData)
    {
      ConstructorInfo constructorInfo = constructorData.GetConstructorInfo();
      return constructorInfo.IsPublic ? AccessModifier.Public
        : constructorInfo.IsPrivate ? AccessModifier.Private
        : constructorInfo.IsAssembly ? AccessModifier.Internal
        : constructorInfo.IsFamily ? AccessModifier.Protected
        : constructorInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
        : constructorInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
        : constructorInfo.IsStatic ? AccessModifier.Undefined
        : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
    }

    internal static AccessModifier GetAccessModifierInternal(TypeData typeData)
    {
      Type typeInfo = typeData.GetType();
      return typeInfo.IsPublic ? AccessModifier.Public
        : typeInfo.IsNestedPrivate ? AccessModifier.Private
        : typeInfo.IsNestedAssembly ? AccessModifier.Internal
        : typeInfo.IsNestedFamily ? AccessModifier.Protected
        : typeInfo.IsNestedPublic ? AccessModifier.Public
        : typeInfo.IsNestedFamORAssem ? AccessModifier.ProtectedInternal
        : typeInfo.IsNestedFamANDAssem ? AccessModifier.PrivateProtected
        : !typeInfo.IsVisible ? AccessModifier.Internal
        : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
    }

    internal static (AccessModifier PropertyModifier, AccessModifier GetMethodModifier, AccessModifier SetMethodModifier) GetPropertyAccessModifier(MethodData getMethodData, MethodData setMethodData)
    {
      AccessModifier getMethodModifier = getMethodData.AccessModifier;
      AccessModifier setMethodModifier = setMethodData.AccessModifier;

      // Property accessors with the least restriction provides the access modifier for the property.
      AccessModifier propertyAccessModifier = (AccessModifier)System.Math.Min((int)getMethodModifier, (int)setMethodModifier);

      return (propertyAccessModifier, getMethodModifier, setMethodModifier);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToDisplayName(this Type type, bool isShortName = false)
    {
      TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
      return ToDisplayNameInternal(typeData, isFullyQualifiedName: false, isShortName);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToDisplayName(this MethodInfo methodInfo, bool isShortName = false)
    {
      MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
      return ToDisplayNameInternal(methodData, isFullyQualifiedName: false, isShortName);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToDisplayName(this ConstructorInfo constructorInfo, bool isShortName = false)
    {
      ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
      return ToDisplayNameInternal(constructorData, isFullyQualifiedName: false, isShortName);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToDisplayName(this PropertyInfo propertyInfo, bool isShortName = false)
    {
      PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
      return ToDisplayNameInternal(propertyData, isFullyQualifiedName: false, isShortName);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToDisplayName(this FieldInfo fieldInfo, bool isShortName = false)
    {
      FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
      return ToDisplayNameInternal(fieldData, isFullyQualifiedName: false, isShortName);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToDisplayName(this ParameterInfo parameterInfo)
    {
      ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);
      return ToDisplayNameInternal(parameterData, isFullyQualifiedName: false, isShortName: true);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToDisplayName(this EventInfo eventInfo, bool isShortName = false)
    {
      EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
      return ToDisplayNameInternal(eventData, isFullyQualifiedName: false, isShortName);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToFullDisplayName(this Type type, bool isShortName = false)
    {
      TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
      return ToDisplayNameInternal(typeData, isFullyQualifiedName: true, isShortName);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToFullDisplayName(this MethodInfo methodInfo, bool isShortName = false)
    {
      MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
      return ToDisplayNameInternal(methodData, isFullyQualifiedName: true, isShortName);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToFullDisplayName(this ConstructorInfo constructorInfo, bool isShortName = false)
    {
      ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
      return ToDisplayNameInternal(constructorData, isFullyQualifiedName: true, isShortName);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToFullDisplayName(this PropertyInfo propertyInfo, bool isShortName = false)
    {
      PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
      return ToDisplayNameInternal(propertyData, isFullyQualifiedName: true, isShortName);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToFullDisplayName(this FieldInfo fieldInfo, bool isShortName = false)
    {
      FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
      return ToDisplayNameInternal(fieldData, isFullyQualifiedName: true, isShortName);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToFullDisplayName(this EventInfo eventInfo, bool isShortName = false)
    {
      EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
      return ToDisplayNameInternal(eventData, isFullyQualifiedName: true, isShortName);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic type names to a readable display genericTypeParameterIdentifier including the symbolNamespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="memberInfo">The <see cref="Type"/> to extend.</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    private static string ToDisplayNameInternal(SymbolInfoData symbolInfoData, bool isFullyQualifiedName, bool isShortName)
    {
      StringBuilder nameBuilder = StringBuilderFactory.GetOrCreate();

      switch (symbolInfoData)
      {
        case ParameterData parameterData:
          _ = nameBuilder.AppendDisplayNameInternal(parameterData);
          break;
        case TypeData typeData:
          _ = nameBuilder.AppendDisplayNameInternal(typeData, isFullyQualifiedName, isShortName);
          break;
        case MemberInfoData memberInfoData:
          _ = nameBuilder.AppendDisplayNameInternal(memberInfoData, isFullyQualifiedName, isShortName);
          break;
        default:
          throw new NotImplementedException();
      }

      string symbolName = nameBuilder.ToString();
      StringBuilderFactory.Recycle(nameBuilder);

      return symbolName;
    }

    public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, Type type, bool isShortName = false)
    {
      TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
      return AppendDisplayNameInternal(nameBuilder, typeData, isFullyQualifiedName: false, isShortName);
    }

    public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, MethodInfo methodInfo, bool isShortName = false)
    {
      MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
      return AppendDisplayNameInternal(nameBuilder, methodData, isFullyQualifiedName: false, isShortName);
    }

    public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, EventInfo eventInfo, bool isShortName = false)
    {
      EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
      return AppendDisplayNameInternal(nameBuilder, eventData, isFullyQualifiedName: false, isShortName);
    }

    public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, ConstructorInfo constructorInfo, bool isShortName = false)
    {
      ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
      return AppendDisplayNameInternal(nameBuilder, constructorData, isFullyQualifiedName: false, isShortName);
    }

    public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, PropertyInfo propertyInfo, bool isShortName = false)
    {
      PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
      return AppendDisplayNameInternal(nameBuilder, propertyData, isFullyQualifiedName: false, isShortName);
    }

    public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, ParameterInfo parameterInfo)
    {
      ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);
      return AppendDisplayNameInternal(nameBuilder, parameterData);
    }

    public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, FieldInfo fieldInfo, bool isShortName = false)
    {
      FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
      return AppendDisplayNameInternal(nameBuilder, fieldData, isFullyQualifiedName: false, isShortName);
    }

    public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, Type type, bool isShortName = false)
    {
      TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
      return AppendDisplayNameInternal(nameBuilder, typeData, isFullyQualifiedName: true, isShortName);
    }

    public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, MethodInfo methodInfo, bool isShortName = false)
    {
      MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
      return AppendDisplayNameInternal(nameBuilder, methodData, isFullyQualifiedName: true, isShortName);
    }

    public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, EventInfo eventInfo, bool isShortName = false)
    {
      EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
      return AppendDisplayNameInternal(nameBuilder, eventData, isFullyQualifiedName: true, isShortName);
    }

    public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, ConstructorInfo constructorInfo, bool isShortName = false)
    {
      ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
      return AppendDisplayNameInternal(nameBuilder, constructorData, isFullyQualifiedName: true, isShortName);
    }

    public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, PropertyInfo propertyInfo, bool isShortName = false)
    {
      PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
      return AppendDisplayNameInternal(nameBuilder, propertyData, isFullyQualifiedName: true, isShortName);
    }

    public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, ParameterInfo parameterInfo)
    {
      ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);
      return AppendDisplayNameInternal(nameBuilder, parameterData);
    }

    public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, FieldInfo fieldInfo, bool isShortName = false)
    {
      FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
      return AppendDisplayNameInternal(nameBuilder, fieldData, isFullyQualifiedName: true, isShortName);
    }

    private static StringBuilder AppendDisplayNameInternal(this StringBuilder nameBuilder, TypeData typeData, bool isFullyQualifiedName, bool isShortName)
    {
      Type type = typeData.GetType();
      if (typeData.IsByRef)
      {
        typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type.GetElementType());
        type = typeData.GetType();
      }

      var typeReference = new CodeTypeReference(type);
      ReadOnlySpan<char> typeName = HelperExtensionsCommon.CodeProvider.GetTypeOutput(typeReference).AsSpan();

      if (typeData.IsGenericType)
      {
        int startIndexOfGenericTypeParameters = typeName.IndexOf('<');
        typeName = typeName.Slice(0, startIndexOfGenericTypeParameters);
      }

      if (!isFullyQualifiedName)
      {
        int startIndexOfUnqualifiedTypeName = typeName.LastIndexOf('.') + 1;
        if (startIndexOfUnqualifiedTypeName > 0)
        {
          typeName = typeName.Slice(startIndexOfUnqualifiedTypeName, typeName.Length - startIndexOfUnqualifiedTypeName);
        }
      }

      _ = nameBuilder.Append(typeName.ToArray());

      if (isShortName)
      {
        return nameBuilder;
      }

      if (typeData.IsGenericType)
      {
        _ = nameBuilder.AppendGenericTypeArguments(typeData, isFullyQualifiedName);
      }

      return nameBuilder;
    }

    private static StringBuilder AppendDisplayNameInternal(this StringBuilder nameBuilder, ParameterData parameterData) 
      => nameBuilder.Append(parameterData.Name);

    private static StringBuilder AppendDisplayNameInternal(this StringBuilder nameBuilder, MemberInfoData memberInfoData, bool isFullyQualifiedName, bool isShortName)
    {
      if (isFullyQualifiedName)
      {
        _ = nameBuilder.AppendDisplayNameInternal(memberInfoData.DeclaringTypeData, isFullyQualifiedName, isShortName)
          .Append('.');
      }

      if (memberInfoData.SymbolAttributes.HasFlag(SymbolAttributes.Constructor))
      {
        _ = nameBuilder.AppendDisplayNameInternal(memberInfoData.DeclaringTypeData, isFullyQualifiedName: false, isShortName: true);
      }
      else
      {
        _ = nameBuilder.Append(memberInfoData.Name);

        if (!isShortName 
          && memberInfoData.SymbolAttributes.HasFlag(SymbolAttributes.GenericMethod) 
          && memberInfoData is MethodData methodData)
        {
          _ = nameBuilder.AppendGenericTypeArguments(methodData, isFullyQualifiedName);
        }
      }

      return nameBuilder;
    }

    private static StringBuilder AppendGenericTypeArguments(this StringBuilder nameBuilder, MethodData methodData, bool isFullyQualified)
    {
      if (!methodData.IsGenericMethod)
      {
        return nameBuilder;
      }

      // Could be an open generic type. Therefore we need to obtain all definitions.
      TypeData[] genericTypeParameterDefinitions;
      TypeData[] genericTypeArguments;
      genericTypeArguments = methodData.GenericTypeArguments;
      genericTypeParameterDefinitions = methodData.IsGenericMethodDefinition
        ? methodData.GenericTypeArguments
        : methodData.GenericMethodDefinitionData.GenericTypeArguments;

      AppendGenericParameters(nameBuilder, isFullyQualified, genericTypeParameterDefinitions, genericTypeArguments);
      return nameBuilder;
    }

    private static StringBuilder AppendGenericTypeArguments(this StringBuilder nameBuilder, TypeData typeData, bool isFullyQualified)
    {
      if (!typeData.IsGenericType)
      {
        return nameBuilder;
      }

      // Could be an open generic type. Therefore we need to obtain all definitions.
      TypeData[] genericTypeParameterDefinitions;
      TypeData[] genericTypeArguments;
      genericTypeArguments = typeData.GenericTypeArguments;
      genericTypeParameterDefinitions = typeData.IsGenericTypeDefinition
        ? typeData.GenericTypeArguments
        : typeData.GenericTypeDefinitionData.GenericTypeArguments;

      AppendGenericParameters(nameBuilder, isFullyQualified, genericTypeParameterDefinitions, genericTypeArguments);
      return nameBuilder;
    }

    private static void AppendGenericParameters(StringBuilder nameBuilder, bool isFullyQualified, TypeData[] genericTypeParameterDefinitions, TypeData[] genericTypeArguments)
    {
      _ = nameBuilder.Append('<');
      for (int typeArgumentIndex = 0; typeArgumentIndex < genericTypeArguments.Length; typeArgumentIndex++)
      {
        TypeData genericParameterTypeData = genericTypeArguments[typeArgumentIndex];
        if (genericTypeParameterDefinitions.Length > 0)
        {
          TypeData genericTypeParameterDefinitionData = genericTypeParameterDefinitions[typeArgumentIndex];
          if ((genericTypeParameterDefinitionData.GenericParameterAttributes & GenericParameterAttributes.Covariant) != 0)
          {
            _ = nameBuilder.Append("out")
              .Append(' ');
          }
          else if ((genericTypeParameterDefinitionData.GenericParameterAttributes & GenericParameterAttributes.Contravariant) != 0)
          {
            _ = nameBuilder.Append("in")
              .Append(' ');
          }
        }

        _ = nameBuilder.AppendDisplayNameInternal(genericParameterTypeData, isFullyQualified, isShortName: false)
          .Append(HelperExtensionsCommon.ParameterSeparator);
      }

      // Remove trailing comma and whitespace
      _ = nameBuilder.Remove(nameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length)
        .Append('>');
    }

    private static StringBuilder AppendGenericTypeConstraints(this StringBuilder constraintBuilder, TypeData[] genericTypeDefinitionsData, bool isFullyQualified, bool isCompact)
    {
      bool hasSingleNewLine = false;
      for (int genericTypeArgumentIndex = 0; genericTypeArgumentIndex < genericTypeDefinitionsData.Length; genericTypeArgumentIndex++)
      {
        TypeData genericTypeDefinitionData = genericTypeDefinitionsData[genericTypeArgumentIndex];
        TypeData[] constraints = genericTypeDefinitionData.GenericParameterConstraintsData;
        if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask) == GenericParameterAttributes.None
          && constraints.Length == 0)
        {
          continue;
        }

        if (isCompact)
        {
          if (!hasSingleNewLine)
          {
            _ = constraintBuilder.AppendLine()
            .Append(HelperExtensionsCommon.Indentation);
            hasSingleNewLine = true;
          }
          else
          {
            _ = constraintBuilder.Append(' ');
          }
        }
        else
        {
          _ = constraintBuilder.AppendLine()
            .Append(HelperExtensionsCommon.Indentation);
        }

        _ = constraintBuilder.Append("where")
          .Append(' ')
          .Append(genericTypeDefinitionData.Name)
          .Append(" : ");

        if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
        {
          _ = constraintBuilder.Append("class")
            .Append(HelperExtensionsCommon.ParameterSeparator);
        }

        if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
        {
          _ = constraintBuilder.Append("struct")
            .Append(HelperExtensionsCommon.ParameterSeparator);
        }

        foreach (TypeData constraintData in constraints)
        {
          Type constraint = constraintData.GetType();
          _ = constraintBuilder.AppendDisplayNameInternal(constraintData, isFullyQualified, isShortName: false)
            .Append(HelperExtensionsCommon.ParameterSeparator);
        }

        if (!genericTypeDefinitionData.IsValueType && (genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
        {
          _ = constraintBuilder.Append("new()")
            .Append(HelperExtensionsCommon.ParameterSeparator);
        }

        _ = constraintBuilder.Remove(constraintBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
      }

      return constraintBuilder;
    }

    private static StringBuilder AppendInheritanceSignature(this StringBuilder memberNameBuilder, TypeData typeData, bool isFullyQualified)
    {
      if (typeData.IsDelegate)
      {
        return memberNameBuilder;
      }

      bool isSubclass = typeData.IsSubclass;
      TypeData[] interfaces = typeData.InterfacesData;
      bool hasInterfaces = interfaces.Length > 0;
      if (isSubclass || hasInterfaces)
      {
        _ = memberNameBuilder.Append(" : ");
      }

      if (isSubclass)
      {
        _ = memberNameBuilder.Append(isFullyQualified ? typeData.BaseTypeData.GetType().FullName : typeData.BaseTypeData.Name)
          .Append(HelperExtensionsCommon.ParameterSeparator);
      }

      foreach (TypeData interfaceData in interfaces)
      {
        _ = memberNameBuilder.Append(isFullyQualified ? interfaceData.GetType().FullName : interfaceData.Name)
          .Append(HelperExtensionsCommon.ParameterSeparator);
      }

      if (isSubclass || hasInterfaces)
      {
        _ = memberNameBuilder.Remove(memberNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
      }

      return memberNameBuilder;
    }

    public static StringBuilder AppendStringBuilder(this StringBuilder stringBuilder, StringBuilder value)
    {
#if NETSTANDARD2_0 || NETFRAMEWORK
      char[] tempArray = new char[value.Length];
      value.CopyTo(0, tempArray, 0, value.Length);
      return stringBuilder.Append(tempArray);
#else
      return stringBuilder.Append(value);
#endif
    }

    public static StringBuilder AppendReadOnlySpan(this StringBuilder stringBuilder, ReadOnlySpan<char> value)
    {
#if NETSTANDARD2_0 || NETFRAMEWORK
      return stringBuilder.Append(value.ToArray());
#else
      return stringBuilder.Append(value);
#endif
    }

    // TODO::Test if checking get() is enough to determine if a property is overridden
    public static bool IsDelegate(this Type typeInfo)
    {
      TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeInfo);
      return typeData.SymbolAttributes.HasFlag(SymbolAttributes.Delegate);
    }

    internal static bool IsDelegateInternal(this Type typeInfo)
      => HelperExtensionsCommon.DelegateType.IsAssignableFrom(typeInfo);

    // TODO::Test if checking get() is enough to determine if a property is overridden
    public static bool IsOverride(this PropertyInfo propertyInfo)
    {
      PropertyData memberInfoData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
      return memberInfoData.IsOverride;
    }

    internal static bool IsOverrideInternal(this PropertyInfo propertyInfo)
      => propertyInfo.CanRead ? propertyInfo.GetGetMethod(true).IsOverride() : propertyInfo.GetSetMethod().IsOverride();

    public static bool IsConst(this FieldInfo fieldInfo)
    {
      FieldData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
      return methodData.SymbolAttributes.HasFlag(SymbolAttributes.Constant);
    }

    internal static bool IsConstInternal(FieldData fieldData)
      => fieldData.GetFieldInfo().IsLiteral;

    public static bool IsOverride(this MethodInfo methodInfo)
    {
      MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
      return methodData.IsOverride;
    }

#if NET
    public static bool IsInitOnly(this PropertyInfo propertyInfo)
    {
      PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
      return propertyData.SymbolAttributes.HasFlag(SymbolAttributes.InitProperty);
    }

    internal static bool IsInitOnlyInternal(PropertyData propertyData)
    {
      if (propertyData.CanWrite)
      {
        Type[] requiredModifiers = propertyData.SetMethodData.GetMethodInfo().ReturnParameter.GetRequiredCustomModifiers();
        if (requiredModifiers.Length > 0)
        {
          return requiredModifiers.FirstOrDefault(type => type == typeof(IsExternalInit)) != default;
        }
      }

      return false;
    }
#endif

    internal static bool IsOverrideInternal(MethodData methodData)
    {
      MethodInfo methodInfo = methodData.GetMethodInfo();
      return !methodInfo.Equals(methodInfo.GetBaseDefinition());
    }

    /// <summary>
    /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
    /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
    /// <remarks>The method first checks if the return type is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned type (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate type (awaiter).
    /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned type that would make the type awaitable. If this fails too, the method is not awaitable.</remarks>
    public static bool IsAwaitable(this MethodInfo methodInfo)
    {
      MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
      return methodData.IsAwaitable;
    }

    /// <summary>
    /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
    /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
    /// <remarks>The method first checks if the return type is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned type (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate type (awaiter).
    /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned type that would make the type awaitable. If this fails too, the method is not awaitable.</remarks>
    internal static bool IsAwaitableInternal(MethodData methodData)
      => IsAwaitableInternal(methodData.ReturnTypeData);

    /// <summary>
    /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
    /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
    /// <remarks>The method first checks if the return type is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned type (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate type (awaiter).
    /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned type that would make the type awaitable. If this fails too, the method is not awaitable.</remarks>
    public static bool IsAwaitable(this Type type)
    {
      TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
      return typeData.IsAwaitable;
    }

    /// <summary>
    /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
    /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
    /// <remarks>The method first checks if the return type is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned type (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate type (awaiter).
    /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned type that would make the type awaitable. If this fails too, the method is not awaitable.</remarks>
    internal static bool IsAwaitableInternal(TypeData typeData)
    {
      Type type = typeData.GetType();
      if (IsAwaitableTask(type) || IsAwaitableValueTask(type))
      {
        return true;
      }

      if (type.GetMethod(nameof(Task.GetAwaiter)) != null)
      {
        return true;
      }

      // The return type of the method is not directly returning an awaitable type.
      // So, search for an extension method named "GetAwaiter" for the return type of the currently validated method that effectively converts the type into an awaitable object.
      // By compiler convention the "GetAwaiter" method must return an awaiter object that implements the INotifyComplete interface
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        foreach (System.Reflection.TypeInfo typeInfo in assembly.GetExportedTypes())
        {
          if (!typeInfo.CanDeclareExtensionMethods())
          {
            continue;
          }

          MethodInfo extensionMethodInfo = typeInfo.GetMethod(nameof(Task.GetAwaiter), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { type }, null);
          if (extensionMethodInfo == null
            || !extensionMethodInfo.IsExtensionMethodOf(type))
          {
            return false;
          }

          if (extensionMethodInfo.ReturnType.GetProperty("IsCompleted") != null
            && extensionMethodInfo.ReturnType.GetInterface(nameof(INotifyCompletion)) != null
            && extensionMethodInfo.ReturnType.GetMethod("GetResult") is MethodInfo getResultMethodInfo
            && getResultMethodInfo.GetParameters().Length == 0)
          {
            return true;
          }
        }
      }

      return false;
    }

    internal static bool IsAwaitableTask(Type type)
      => HelperExtensionsCommon.TaskType.IsAssignableFrom(type)
        || HelperExtensionsCommon.TaskType.IsAssignableFrom(type.BaseType);

    internal static bool IsAwaitableValueTask(Type type)
      => HelperExtensionsCommon.ValueTaskType == type
        || (type.IsGenericType && HelperExtensionsCommon.ValueTaskGenericType == type.GetGenericTypeDefinition());

    public static bool IsMarkedAsync(this MethodInfo methodInfo)
    {
      MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
      return methodData.IsAsync;
    }

    internal static bool IsMarkedAsyncInternal(MethodData methodData)
      => methodData.GetMethodInfo().GetCustomAttribute(HelperExtensionsCommon.AsyncStateMachineAttributeType) != null;

    /// <summary>
    /// Extension method to check if a <see cref="Type"/> is static.
    /// </summary>
    /// <param genericTypeParameterIdentifier="typeInfo">The extended <see cref="Type"/> instance.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="typeInfo"/> is static. Otherwise <see langword="false"/>.</returns>
    public static bool IsStatic(this Type typeInfo)
    {
      TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeInfo);
      return typeData.IsStatic;
    }

    internal static bool IsStaticInternal(TypeData typeData) 
      => typeData.IsAbstract && typeData.IsSealed;

    /// <summary>
    /// Extension method to check if a <see cref="ParameterInfo"/> represents a <see langword="ref"/> parameter.
    /// </summary>
    /// <returns><see langword="true"/> if the <paramref name="parameterInfo"/> represents a <see langword="ref"/> parameter. Otherwise <see langword="false"/>.</returns>
    public static bool IsRef(this ParameterInfo parameterInfo)
    {
      ParameterData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);
      return typeData.IsRef;
    }

    internal static bool IsRefInternal(ParameterData parameterData) 
      => parameterData.IsByRef && !parameterData.IsOut && !parameterData.IsIn;

    /// <summary>
    /// Extension method that checks if the provided <see cref="Type"/> is qualified to define extension methods.
    /// </summary>
    /// <param genericTypeParameterIdentifier="typeInfo">The extended <see cref="Type"/> instance.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="typeInfo"/> is allowed to define extension methods. Otherwise <see langword="false"/>.</returns>
    /// <remarks>To be able to define extension methods a class must be static, non-generic, a top level type. 
    /// <br/>In addition this method checks if the declaring class and the method are both decorated with the <see cref="ExtensionAttribute"/> which is added by the compiler.</remarks>
    public static bool CanDeclareExtensionMethods(this Type typeInfo)
    {
      TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeInfo);
      return typeData.CanDeclareExtensionMethod;
    }

    internal static bool CanDeclareExtensionMethodsInternal(TypeData typeData)
    {
      Type typeInfo = typeData.GetType();
      if (!typeData.IsStatic || typeInfo.IsNested || typeInfo.IsGenericType)
      {
        return false;
      }

      Attribute typeExtensionAttribute = typeInfo.GetCustomAttribute(HelperExtensionsCommon.ExtensionAttributeType, false);
      return typeExtensionAttribute != null;
    }

    internal static bool CanDeclareExtensionMethodsInternalUncached(Type type)
    {
      bool isStatic = type.IsAbstract && type.IsSealed;
      if (!isStatic || type.IsNested || type.IsGenericType)
      {
        return false;
      }

      Attribute typeExtensionAttribute = type.GetCustomAttribute(HelperExtensionsCommon.ExtensionAttributeType, false);
      return typeExtensionAttribute != null;
    }

    /// <summary>
    /// Extension method to check if a <see cref="MethodInfo"/> is the info of an extension method.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The extended <see cref="MethodInfo"/> instance to validate.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="methodInfo"/> is an extension method. Otherwise <see langword="false"/>.</returns>
    public static bool IsExtensionMethod(this MethodInfo methodInfo)
    {
      MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
      return methodData.IsExtensionMethod;
    }

    internal static bool IsExtensionMethodInternal(MethodData methodData)
    {
      // Check if the declaring class satisfies the constraints to declare extension methods
      TypeData declaringTypeData = methodData.DeclaringTypeData;
      if (!declaringTypeData.CanDeclareExtensionMethod)
      {
        return false;
      }

      /* Check if the method satisfies the constraints to act as an extension methods */

      if (!methodData.IsStatic)
      {
        return false;
      }

      MethodInfo methodInfo = methodData.GetMethodInfo(); 
      Attribute methodExtensionAttribute = methodInfo.GetCustomAttribute(HelperExtensionsCommon.ExtensionAttributeType, false);
      if (methodExtensionAttribute == null)
      {
        return false;
      }

      // Must have at least the 'this' parameter
      ParameterData[] parameterInfoData = methodData.Parameters;
      if (parameterInfoData.Length < 1)
      {
        return false;
      }

      return true;
    }

    internal static bool IsExtensionMethodInternalUncached(MethodInfo methodInfo)
    {
      // Check if the declaring class satisfies the constraints to declare extension methods
      Type declaringType = methodInfo.DeclaringType;
      if (!declaringType.CanDeclareExtensionMethods())
      {
        return false;
      }

      /* Check if the method satisfies the constraints to act as an extension methods */

      if (!methodInfo.IsStatic)
      {
        return false;
      }

      Attribute methodExtensionAttribute = methodInfo.GetCustomAttribute(HelperExtensionsCommon.ExtensionAttributeType, false);
      if (methodExtensionAttribute == null)
      {
        return false;
      }

      // Must have at least the 'this' parameter
      ParameterInfo[] parameterInfoData = methodInfo.GetParameters();
      if (parameterInfoData.Length < 1)
      {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Extension method to check if a <see cref="MethodInfo"/> is the info of an extension method for a particular type.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The extended <see cref="MethodInfo"/> instance to validate.</param>
    /// <param genericTypeParameterIdentifier="typeToExtend">The <see cref="Type"/> the <paramref genericTypeParameterIdentifier="methodInfo"/> is expected to extend.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="methodInfo"/> is an extension method for <paramref genericTypeParameterIdentifier="typeToExtend"/>. Otherwise <see langword="false"/>.</returns>
    public static bool IsExtensionMethodOf(this MethodInfo methodInfo, Type typeToExtend)
    {
      // Check if the declaring class satisfies the constraints to declare extension methods
      if (!methodInfo.DeclaringType.CanDeclareExtensionMethods())
      {
        return false;
      }

      /* Check if the method satisfies the constraints to act as an extension methods */

      if (!IsExtensionMethodInternalUncached(methodInfo))
      {
        return false;
      }

      ParameterInfo[] parameterInfos = methodInfo.GetParameters();
      if (parameterInfos.Length > 0)
      {
        ParameterInfo firstParameterInfo = parameterInfos[0];
        if (firstParameterInfo.ParameterType.IsAssignableFrom(typeToExtend))
        {
          return true;
        }
      }

      return false;
    }

#if !NETSTANDARD2_0
    public static bool IsReadOnlyStruct(Type type)
    {
      TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
      return typeData.SymbolAttributes.HasFlag(SymbolAttributes.ReadOnlyStruct);
    }

    internal static bool IsReadOnlyStructInternal(Type type)
      => type.IsValueType && type.GetCustomAttribute(HelperExtensionsCommon.IsReadOnlyAttributeType) != null;
#endif

    //public static object GetAwaiter(this object obj)
    //{
    //  MethodInfo getAwaiterMethodInfo = obj.GetType().GetMethod(nameof(Task.GetAwaiter));
    //  if (getAwaiterMethodInfo != null)
    //  {
    //    return (Task)getAwaiterMethodInfo.Invoke(obj, null);
    //  }

    //  // The return type of the method is not directly returning an awaitable type.
    //  // So search for an extension method named "GetAwaiter" for the return type that effectively converts the type into an awaitable object.
    //  foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
    //  {
    //    foreach (TypeInfo typeInfo in assembly.GetExportedTypes())
    //    {
    //      if (!typeInfo.IsSealed || typeInfo.IsGenericType)
    //      {
    //        continue;
    //      }

    //      getAwaiterMethodInfo = typeInfo.GetMethod(nameof(Task.GetAwaiter), BindingFlags.Static | BindingFlags.Public, null, new[] { obj.GetType() }, null);
    //      if (getAwaiterMethodInfo != null)
    //      {
    //        return (Task)getAwaiterMethodInfo.Invoke(obj, null);
    //      }

    //      //foreach (MethodInfo extensionMethodCandidate in typeInfo.GetMethods(BindingFlags.Static | BindingFlags.Public))
    //      //{
    //      //  if (!extensionMethodCandidate.Name.Equals(nameof(Task.GetAwaiter), StringComparison.Ordinal))
    //      //  {
    //      //    continue;
    //      //  }

    //      //  ParameterInfo[] parameterInfos = extensionMethodCandidate.GetParameters();
    //      //  if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == getAwaiterMethodInfo.ReturnType)
    //      //  {
    //      //    return true;
    //      //  }
    //      //}
    //    }
    //  }

    //  return null;
    //}

    internal static SymbolAttributes GetAttributesInternal(TypeData typeData)
    {
      Type type =  typeData.GetType();
      if (IsDelegateInternal(type))
      {
        SymbolAttributes delegateAttributes = SymbolAttributes.Delegate;
        if (typeData.IsGenericType)
        {
          delegateAttributes |= SymbolAttributes.Generic;
        }

        return delegateAttributes;
      }

      if (type.IsClass)
      {
        SymbolAttributes classAttributes = SymbolAttributes.Class;
        if (type.IsAbstract)
        {
          classAttributes |= SymbolAttributes.Abstract;
        }

        if (typeData.IsSealed)
        {
          classAttributes |= SymbolAttributes.Final;
        }

        if (typeData.IsStatic)
        {
          classAttributes |= SymbolAttributes.Static;
        }

        if (typeData.IsGenericType)
        {
          classAttributes |= SymbolAttributes.Generic;
        }

        return classAttributes;
      }

      if (type.IsInterface)
      {
        SymbolAttributes interfaceAttributes = SymbolAttributes.Interface;
        return interfaceAttributes;
      }

      if (type.IsEnum)
      {
        SymbolAttributes enumAttributes = SymbolAttributes.Enum;
        return enumAttributes;
      }

      if (type.IsValueType)
      {
        SymbolAttributes structAttributes = SymbolAttributes.Struct;

        if (typeData.IsGenericType)
        {
          structAttributes |= SymbolAttributes.Generic;
        }

#if !NETFRAMEWORK && !NETSTANDARD2_0
        if (typeData.IsByRefLike)
        {
          structAttributes |= SymbolAttributes.ByReference;
        }
#endif

#if !NETSTANDARD2_0
        bool isReadOnlyStruct = IsReadOnlyStructInternal(type);
        if (isReadOnlyStruct)
        {
          structAttributes |= SymbolAttributes.Final;
        }
#endif
        return structAttributes;
      }

      return SymbolAttributes.Undefined;
    }

    internal static SymbolAttributes GetAttributesInternal(PropertyData propertyData)
    {
      PropertyInfo propertyInfo = propertyData.PropertyInfo;
      SymbolAttributes propertyAttributes = propertyData.IsIndexer
        ? SymbolAttributes.IndexerProperty
        : SymbolAttributes.Property;

      MethodData accessorData = propertyData.GetMethodData ?? propertyData.SetMethodData;
      MethodInfo accessorMethodInfo = accessorData.GetMethodInfo();
      if (!propertyData.CanWrite)
      {
        propertyAttributes |= SymbolAttributes.Final;
      }

#if NET
      if (IsInitOnlyInternal(propertyData))
      {
        propertyAttributes |= SymbolAttributes.Init;
      }
#endif

      if (accessorMethodInfo.IsAbstract)
      {
        propertyAttributes |= SymbolAttributes.Abstract;
      }

      if (propertyData.IsStatic)
      {
        propertyAttributes |= SymbolAttributes.Static;
      }

      if (accessorMethodInfo.IsVirtual)
      {
        propertyAttributes |= SymbolAttributes.Virtual;
      }

      if (propertyData.IsOverride)
      {
        propertyAttributes |= SymbolAttributes.Override;
      }

      return propertyAttributes;
    }

    internal static SymbolAttributes GetAttributesInternal(FieldData fieldData)
    {
      FieldInfo fieldInfo = fieldData.GetFieldInfo();
      SymbolAttributes fieldAttributes = SymbolAttributes.Field;
      if (fieldInfo.IsInitOnly)
      {
        fieldAttributes |= SymbolAttributes.Final;
      }

      if (fieldData.IsRef)
      {
        fieldAttributes |= SymbolAttributes.ByReference;
      }

      if (fieldData.IsStatic)
      {
        fieldAttributes |= SymbolAttributes.Static;
      }

      if (IsConstInternal(fieldData))
      {
        fieldAttributes |= SymbolAttributes.Constant;
      }

      return fieldAttributes;
    }

    internal static SymbolAttributes GetAttributesInternal(ParameterData parameterData)
    {
      SymbolAttributes parameterAttributes = SymbolAttributes.Parameter;
      if (parameterData.IsIn)
      {
        parameterAttributes |= SymbolAttributes.InParameter;
      }

      if (parameterData.IsRef)
      {
        parameterAttributes |= SymbolAttributes.RefParameter;
      }

      if (parameterData.IsOut)
      {
        parameterAttributes |= SymbolAttributes.OutParameter;
      }

      if (parameterData.IsOptional)
      {
        parameterAttributes |= SymbolAttributes.OptionalParameter;
      }

      return parameterAttributes;
    }

    internal static SymbolAttributes GetAttributesInternal(MethodData methodData)
    {
      MethodInfo methodInfo = methodData.GetMethodInfo();
      SymbolAttributes methodAttributes = SymbolAttributes.Method;
      if (methodInfo.IsFinal)
      {
        methodAttributes |= SymbolAttributes.Final;
      }

      if (methodInfo.IsAbstract)
      {
        methodAttributes |= SymbolAttributes.Abstract;
      }

      if (methodData.IsStatic)
      {
        methodAttributes |= SymbolAttributes.Static;
      }

      if (methodInfo.IsVirtual)
      {
        methodAttributes |= SymbolAttributes.Virtual;
      }

      if (methodData.IsOverride)
      {
        methodAttributes |= SymbolAttributes.Override;
      }

      if (methodData.IsGenericMethod)
      {
        methodAttributes |= SymbolAttributes.Generic;
      }

      return methodAttributes;
    }

    internal static SymbolAttributes GetAttributesInternal(ConstructorData constructorData)
    {
      SymbolAttributes constructorAttributes = SymbolAttributes.Constructor;

      if (constructorData.IsStatic)
      {
        constructorAttributes |= SymbolAttributes.Static;
      }

      return constructorAttributes;
    }

    internal static SymbolAttributes GetAttributesInternal(EventData eventData)
    {
      MethodData eventAddMethodData = eventData.AddMethodData;
      SymbolAttributes eventAttributes = SymbolAttributes.Event;
      MethodInfo addHandlerMethod = eventAddMethodData.GetMethodInfo();
      if (addHandlerMethod.IsFinal)
      {
        eventAttributes |= SymbolAttributes.Final;
      }

      if (addHandlerMethod.IsAbstract)
      {
        eventAttributes |= SymbolAttributes.Abstract;
      }

      if (eventAddMethodData.IsStatic)
      {
        eventAttributes |= SymbolAttributes.Static;
      }

      if (addHandlerMethod.IsVirtual)
      {
        eventAttributes |= SymbolAttributes.Virtual;
      }

      if (eventAddMethodData.IsOverride)
      {
        eventAttributes |= SymbolAttributes.Override;
      }

      return eventAttributes;
    }


/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net48)'
Before:
      SymbolInfoDataCacheKey cacheKey = CreateSymbolInfoDataCacheKey(symbolInfo);
      if (!HelperExtensionsCommon.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = CreateMemberInfoDataCacheEntry(symbolInfo, cacheKey);
      }
After:
      SymbolInfoDataCacheKey cacheKey = Net.SymbolReflectionInfoCache.CreateSymbolInfoDataCacheKey(symbolInfo);
      if (!Net.SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = Net.SymbolReflectionInfoCache.CreateMemberInfoDataCacheEntry(symbolInfo, cacheKey);
      }
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net48)'
Before:
      HelperExtensionsCommon.SymbolInfoDataCache.Add(key, entry);
      return entry;
After:
      Net.SymbolReflectionInfoCache.SymbolInfoDataCache.Add(key, entry);
      return entry;
*/
    public static dynamic Cast(this object obj, Type type)
        => typeof(HelperExtensionsCommon).GetMethod(nameof(HelperExtensionsCommon.Cast), BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(object) }, null).GetGenericMethodDefinition().MakeGenericMethod(type).Invoke(obj, null);

    private static T Cast<T>(this object obj) => (T)obj;

#if !NET7_0_OR_GREATER
    public static double TotalMicroseconds(this TimeSpan duration) => System.Math.Round(duration.Ticks / (double)Stopwatch.Frequency * 1E6, 1);
    public static double TotalNanoseconds(this TimeSpan duration) => System.Math.Round(duration.Ticks / (double)Stopwatch.Frequency * 1E9, 0);
#endif
  }
}