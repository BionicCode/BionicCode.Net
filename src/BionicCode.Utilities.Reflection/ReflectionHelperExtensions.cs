[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BionicCode.Utilities.Net.Reflection")]
namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp;

/// <summary>
/// A collection of extension methods for various default constraintTypes
/// </summary>
public static partial class ReflectionHelperExtensions
{
    private const string ParameterSeparator = ", ";

    /// <summary>
    /// Specifies binding flags that include all instance and static members, regardless of visibility, declared
    /// only on the current targetType.
    /// </summary>
    /// <remarks>This combination of flags is typically used when reflecting over a targetType to retrieve
    /// all of its members, including public, non-public, static, and instance members, but excluding inherited
    /// members from base types.</remarks>
    internal const BindingFlags AllDeclaredMembersFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
    /// <summary>
    /// Specifies binding flags that include all instance and static members, both public and non-public, across the
    /// entire inheritance hierarchy except for members inherited from System.Object.
    /// </summary>
    /// <remarks>This constant is intended for use with reflection methods that require a
    /// comprehensive set of binding flags to access all members of a targetType, including those declared in base
    /// classes. It does not include the DeclaredOnly flag, so inherited members are included. Members inherited
    /// from System.Object may still be excluded depending on the reflection API used.</remarks>
    internal const BindingFlags AllMembersFullHierarchyFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

    internal static CSharpCodeProvider CodeProvider { get; } = new CSharpCodeProvider();
    internal static Type ExtensionAttributeType { get; } = typeof(ExtensionAttribute);
    internal static Type IsReadOnlyAttributeType { get; } = typeof(IsReadOnlyAttribute);

    internal static PooledStringBuilder AppendCustomAttributes(this PooledStringBuilder nameBuilder, IEnumerable<CustomAttributeData> attributes, bool isAppendNewLineEnabled)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(nameBuilder);
        ArgumentNullExceptionAdvanced.ThrowIfNull(attributes);

        foreach (CustomAttributeData attribute in attributes)
        {
            bool hasAttributeArguments = false;

            if (SymbolSignatureGenerator.IgnorableParameterAttributes.Contains(attribute.AttributeType.Name))
            {
                continue;
            }

            _ = nameBuilder.Append('[')
              .Append(attribute.AttributeType.Name)
              .Append('(');

            foreach (CustomAttributeTypedArgument constructorPositionalArgument in attribute.ConstructorArguments)
            {
                hasAttributeArguments = true;

                _ = nameBuilder.Append(constructorPositionalArgument.Value.ToArgumentDisplayValue())
                  .Append(ReflectionHelperExtensions.ParameterSeparator);
            }

            foreach (CustomAttributeNamedArgument constructorNamedArgument in attribute.NamedArguments)
            {
                hasAttributeArguments = true;

                _ = nameBuilder.Append(constructorNamedArgument.MemberName)
                  .Append(" = ")
                  .Append(constructorNamedArgument.TypedValue.Value.ToArgumentDisplayValue())
                  .Append(ReflectionHelperExtensions.ParameterSeparator);
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
                _ = nameBuilder.Remove(nameBuilder.Length - ReflectionHelperExtensions.ParameterSeparator.Length, ReflectionHelperExtensions.ParameterSeparator.Length)
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

    internal static string ToArgumentDisplayValue(this object value)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(value);

        switch (value)
        {
            case string stringValue:
                return $"\"{stringValue}\"";
            case char charValue:
                return $"'{charValue}'";
            case double doubleValue:
                return string.Format(CultureInfo.InvariantCulture, "{0}", doubleValue);
            case Enum enumValue:
                return $"{value.GetType().ToDisplayName()}.{enumValue.ToString()}";
            case Type type:
                return $"typeof({type.ToDisplayName()})";
            case IEnumerable enumerableValue:
                return $"new[] {{ {string.Join(", ", enumerableValue.OfType<object>().Select(val => val.ToArgumentDisplayValue()))} }}";
            default:
                return value.ToString();
        }
    }

    internal static PooledStringBuilder AppendDisplayNameInternal(this PooledStringBuilder nameBuilder, TypeData typeData, bool isFullyQualifiedName, bool isGenericTypeParameterIncluded)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(nameBuilder);
        ArgumentNullExceptionAdvanced.ThrowIfNull(typeData);

        Type type = typeData.Type;
        if (typeData.IsByRef)
        {
            typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntryInternal(type.GetElementType());
            type = typeData.Type;
        }

        var typeReference = new CodeTypeReference(type);
        ReadOnlySpan<char> typeName = ReflectionHelperExtensions.CodeProvider.GetTypeOutput(typeReference).AsSpan();

        if (typeData.IsGenericType)
        {
            int startIndexOfGenericTypeParameters = typeName.IndexOf('<');
            typeName = typeName[..startIndexOfGenericTypeParameters];
        }

        if (!isFullyQualifiedName)
        {
            int startIndexOfUnqualifiedTypeName = typeName.LastIndexOf('.') + 1;
            if (startIndexOfUnqualifiedTypeName > 0)
            {
                typeName = typeName[startIndexOfUnqualifiedTypeName..];
            }
        }

        _ = nameBuilder.Append(typeName.ToArray());

        if (isGenericTypeParameterIncluded && typeData.IsGenericType)
        {
            _ = nameBuilder.AppendGenericTypeArguments(typeData, isFullyQualifiedName);
        }

        return nameBuilder;
    }

    internal static PooledStringBuilder AppendDisplayNameInternal(this PooledStringBuilder nameBuilder, ParameterData parameterData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(nameBuilder);
        ArgumentNullExceptionAdvanced.ThrowIfNull(parameterData);

        _ = nameBuilder.Append(parameterData.Name);
        if (parameterData.IsOptional)
        {
            _ = nameBuilder.Append(" = ");

            object defaultValue = parameterData.DefaultValue;
            _ = defaultValue switch
            {
                string stringValue => nameBuilder.Append(CultureInfo.InvariantCulture, $"""{stringValue}"""),
                char charValue => nameBuilder.Append(CultureInfo.InvariantCulture, $"'{charValue}'"),
                null => nameBuilder.Append("null"),
                bool boolValue => nameBuilder.Append(boolValue ? "true" : "false"),
                _ => nameBuilder.Append(defaultValue.ToString()),
            };
        }

        return nameBuilder;
    }

    internal static PooledStringBuilder AppendDisplayNameInternal(this PooledStringBuilder nameBuilder, MemberData memberInfoData, bool isFullyQualifiedName, bool isGenericTypeParameterIncluded, bool isDeclaringTypeIncluded)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(nameBuilder);
        ArgumentNullExceptionAdvanced.ThrowIfNull(memberInfoData);

        if (isFullyQualifiedName || isDeclaringTypeIncluded)
        {
            _ = nameBuilder.AppendDisplayNameInternal(memberInfoData.DeclaringTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
              .Append('.');
        }

        if (memberInfoData.SymbolAttributes.HasFlag(SymbolAttributes.Constructor))
        {
            _ = nameBuilder.AppendDisplayNameInternal(memberInfoData.DeclaringTypeData, isFullyQualifiedName: false, isGenericTypeParameterIncluded: false);
        }
        else if (memberInfoData is PropertyData propertyData)
        {
            _ = nameBuilder.Append(propertyData.Name);

            if (propertyData.IsIndexer)
            {
                _ = nameBuilder.Append('[');

                foreach (ParameterData indexerParameter in propertyData.PropertyGetMethodParameters)
                {
                    _ = nameBuilder.Append(indexerParameter.ParameterTypeData.ShortDisplayName);
                }

                _ = nameBuilder.Append(']');
            }
        }
        else
        {
            _ = nameBuilder.Append(memberInfoData.Name);

            if (isGenericTypeParameterIncluded
              && memberInfoData.SymbolAttributes.HasFlag(SymbolAttributes.GenericMethod)
              && memberInfoData is MethodData methodData)
            {
                _ = nameBuilder.AppendGenericTypeArguments(methodData, isFullyQualifiedName);
            }
        }

        return nameBuilder;
    }

    internal static PooledStringBuilder AppendGenericTypeArguments(this PooledStringBuilder nameBuilder, MethodData methodData, bool isFullyQualified)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(nameBuilder);
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodData);

        if (!methodData.IsGenericMethod)
        {
            return nameBuilder;
        }

        // Could be an open generic valueType. Therefore we need to obtain all definitions.
        TypeList genericTypeArguments = methodData.GenericMethodParameters;
        TypeList genericTypeParameterDefinitions = methodData.IsGenericMethodDefinition
          ? methodData.GenericMethodParameters
          : TypeList.Empty;

        AppendGenericParameters(nameBuilder, isFullyQualified, genericTypeParameterDefinitions, genericTypeArguments);
        return nameBuilder;
    }

    internal static PooledStringBuilder AppendGenericTypeArguments(this PooledStringBuilder nameBuilder, TypeData typeData, bool isFullyQualified)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(nameBuilder);
        ArgumentNullExceptionAdvanced.ThrowIfNull(typeData);

        if (!typeData.IsGenericType)
        {
            return nameBuilder;
        }

        // Could be an open generic valueType. Therefore we need to obtain all definitions.
        TypeList genericTypeArguments = typeData.GenericTypeArguments;
        TypeList genericTypeParameterDefinitions = typeData.IsGenericTypeDefinition
          ? typeData.GenericTypeArguments
          : TypeList.Empty;

        AppendGenericParameters(nameBuilder, isFullyQualified, genericTypeParameterDefinitions, genericTypeArguments);
        return nameBuilder;
    }

    private static void AppendGenericParameters(PooledStringBuilder nameBuilder, bool isFullyQualified, TypeList genericTypeParameterDefinitions, TypeList genericTypeArguments)
    {
        _ = nameBuilder.Append('<');
        for (int typeArgumentIndex = 0; typeArgumentIndex < genericTypeArguments.Count; typeArgumentIndex++)
        {
            TypeData genericParameterTypeData = genericTypeArguments[typeArgumentIndex];
            if (genericTypeParameterDefinitions.Count > 0)
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

            _ = nameBuilder.AppendDisplayNameInternal(genericParameterTypeData, isFullyQualified, isGenericTypeParameterIncluded: true)
              .Append(ReflectionHelperExtensions.ParameterSeparator);
        }

        // Remove trailing comma and whitespace
        _ = nameBuilder.Remove(nameBuilder.Length - ReflectionHelperExtensions.ParameterSeparator.Length, ReflectionHelperExtensions.ParameterSeparator.Length)
          .Append('>');
    }

    internal static PooledStringBuilder AppendGenericTypeConstraints(this PooledStringBuilder constraintBuilder, TypeList genericTypeDefinitionsData, bool isFullyQualified, bool isSingleLine, int lineIndentation)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(constraintBuilder);
        ArgumentNullExceptionAdvanced.ThrowIfNull(genericTypeDefinitionsData);
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(lineIndentation);

        bool hasSingleNewLine = false;
        for (int genericTypeArgumentIndex = 0; genericTypeArgumentIndex < genericTypeDefinitionsData.Count; genericTypeArgumentIndex++)
        {
            TypeData genericTypeDefinitionData = genericTypeDefinitionsData[genericTypeArgumentIndex];
            TypeList constraints = genericTypeDefinitionData.GenericParameterConstraintsData;
            if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask) == GenericParameterAttributes.None
              && constraints.Count == 0)
            {
                continue;
            }

            if (isSingleLine)
            {
                if (!hasSingleNewLine)
                {
                    _ = constraintBuilder.AppendLine()
                    .Append(' ', lineIndentation);
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
                    .Append(' ', lineIndentation);
            }

            _ = constraintBuilder.Append("where")
              .Append(' ')
              .Append(genericTypeDefinitionData.Name)
              .Append(" : ");

            if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
            {
                _ = constraintBuilder.Append("class")
                  .Append(ReflectionHelperExtensions.ParameterSeparator);
            }

            if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
            {
                _ = constraintBuilder.Append("struct")
                  .Append(ReflectionHelperExtensions.ParameterSeparator);
            }

            foreach (TypeData constraintData in constraints)
            {
                _ = constraintBuilder.AppendDisplayNameInternal(constraintData, isFullyQualified, isGenericTypeParameterIncluded: true)
                  .Append(ReflectionHelperExtensions.ParameterSeparator);
            }

            if (!genericTypeDefinitionData.IsValueType && (genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
            {
                _ = constraintBuilder.Append("new()")
                  .Append(ReflectionHelperExtensions.ParameterSeparator);
            }

            _ = constraintBuilder.Remove(constraintBuilder.Length - ReflectionHelperExtensions.ParameterSeparator.Length, ReflectionHelperExtensions.ParameterSeparator.Length);
        }

        return constraintBuilder;
    }

    internal static PooledStringBuilder AppendInheritanceSignature(this PooledStringBuilder memberNameBuilder, TypeData typeData, bool isFullyQualified)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(memberNameBuilder);
        ArgumentNullExceptionAdvanced.ThrowIfNull(typeData);

        if (typeData.IsDelegate)
        {
            return memberNameBuilder;
        }

        bool isSubclass = typeData.IsSubclass;
        TypeList interfaces = typeData.InterfacesData;
        bool hasInterfaces = interfaces.Count > 0;
        if (isSubclass || hasInterfaces)
        {
            _ = memberNameBuilder.Append(" : ");
        }

        if (isSubclass)
        {
            _ = memberNameBuilder.Append(isFullyQualified ? typeData.BaseTypeData.Type.FullName : typeData.BaseTypeData.Name)
              .Append(ReflectionHelperExtensions.ParameterSeparator);
        }

        foreach (TypeData interfaceData in interfaces)
        {
            _ = memberNameBuilder.Append(isFullyQualified ? interfaceData.Type.FullName : interfaceData.Name)
              .Append(ReflectionHelperExtensions.ParameterSeparator);
        }

        if (isSubclass || hasInterfaces)
        {
            _ = memberNameBuilder.Remove(memberNameBuilder.Length - ReflectionHelperExtensions.ParameterSeparator.Length, ReflectionHelperExtensions.ParameterSeparator.Length);
        }

        return memberNameBuilder;
    }

    /// <summary>
    /// Appends an HTML-formatted representation of the specified symbol component, including attributes, modifiers,
    /// targetType, name, parameters, and constraints, to the provided string builder.
    /// </summary>
    /// <remarks>The generated HTML includes semantic CSS classes for syntax highlighting and is
    /// intended for use in documentation or code display scenarios. The method does not encode user-provided
    /// values; callers should ensure that all symbol component data is safe for HTML output.</remarks>
    /// <param name="signatureBuilder">The string builder to which the HTML-formatted symbol signature will be appended.</param>
    /// <param name="symbolComponentInfo">The symbol component information describing the structure and metadata to render as inline HTML.</param>
    /// <returns>The same <see cref="PooledStringBuilder"/> instance with the appended HTML-formatted symbol signature.</returns>
    internal static PooledStringBuilder AppendInlineHtml(this PooledStringBuilder signatureBuilder, SymbolComponentInfo symbolComponentInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(signatureBuilder);
        ArgumentNullExceptionAdvanced.ThrowIfNull(symbolComponentInfo);

        if (symbolComponentInfo.CustomAttributes.Count != 0)
        {
            foreach (SymbolComponentInfo attribute in symbolComponentInfo.CustomAttributes)
            {
                _ = signatureBuilder.Append($"<span class=\"syntax-delimiter\">[</span>")
                  .Append($"<span class=\"syntax-targetType\">")
                  .Append(attribute.Name);

                bool hasConstructorArgs = symbolComponentInfo.CustomAttributeConstructorArgs.Count != 0;
                bool hasNamedArgs = symbolComponentInfo.CustomAttributeNamedArgs.Count != 0;
                bool hasArguments = hasConstructorArgs || hasNamedArgs;
                if (hasArguments)
                {
                    _ = signatureBuilder.Append('(');
                }

                if (hasConstructorArgs)
                {
                    _ = signatureBuilder.AppendJoin(", ", symbolComponentInfo.CustomAttributeConstructorArgs);
                }

                if (hasNamedArgs)
                {
                    _ = signatureBuilder.Append(' ');
                    foreach ((string PropertyName, string PropertyValue) in symbolComponentInfo.CustomAttributeNamedArgs)
                    {
                        _ = signatureBuilder.Append(PropertyName)
                          .Append(" = ")
                          .Append(PropertyValue)
                          .Append(", ");
                    }

                    _ = signatureBuilder.Remove(signatureBuilder.Length - 2, 2);
                }

                if (hasArguments)
                {
                    _ = signatureBuilder.Append($"<span class=\"syntax-targetType\">")
                      .Append(')');
                }

                _ = signatureBuilder.Append("</span>")
                  .Append($"<span class=\"syntax-delimiter\">]</span>");

                if (symbolComponentInfo.HasInlineAttributes)
                {
                    _ = signatureBuilder.Append(' ');
                }
                else
                {
                    _ = signatureBuilder.Append("<br>");
                }
            }
        }

        if (!symbolComponentInfo.IsParameter && symbolComponentInfo.Modifiers.Count != 0)
        {
            _ = signatureBuilder.Append($"<span class=\"syntax-keyword\">");
            foreach (string modifier in symbolComponentInfo.Modifiers)
            {
                _ = signatureBuilder.Append(modifier)
                  .Append(' ');
            }

            _ = signatureBuilder.Append($"</span>");
        }

        if (symbolComponentInfo.ReturnType != null)
        {
            _ = signatureBuilder.AppendInlineHtml(symbolComponentInfo.ReturnType)
                .Append(' ');
        }

        if (symbolComponentInfo.Name.Length > 0)
        {
            if (symbolComponentInfo.IsKeyword)
            {
                _ = signatureBuilder.Append($"<span class=\"syntax-keyword\">");
            }
            else if (symbolComponentInfo.IsSymbol && !symbolComponentInfo.IsParameter)
            {
                _ = signatureBuilder.Append($"<span class=\"syntax-symbol\">");
            }
            else
            {
                _ = signatureBuilder.Append($"<span class=\"syntax-targetType\">");
            }

            _ = signatureBuilder.Append(symbolComponentInfo.Name)
              .Append("</span>");
        }

        if (symbolComponentInfo.ValueName.Length > 0)
        {
            _ = signatureBuilder.Append($"<span class=\"syntax-value\">")
              .Append(' ')
              .Append(symbolComponentInfo.ValueName)
              .Append("</span>");
        }

        if (symbolComponentInfo.IsIndexer)
        {
            _ = signatureBuilder.Append($"<span class=\"syntax-keyword\">")
              .Append("this").
              Append("</span>");
        }

        if (symbolComponentInfo.GenericTypeParameters.Count != 0)
        {
            _ = signatureBuilder.Append($"<span class=\"syntax-delimiter\">")
              .Append('<'.ToHtmlEncodedReadOnlySpan())
              .Append("</span>")
              .Append($"<span class=\"syntax-targetType\">");

            foreach (SymbolComponentInfo typeParameter in symbolComponentInfo.GenericTypeParameters)
            {
                _ = signatureBuilder.AppendInlineHtml(typeParameter)
                  .Append(',')
                  .Append(' ');
            }

            _ = signatureBuilder.Remove(signatureBuilder.Length - 2, 2)
              .Append("</span>")
              .Append($"<span class=\"syntax-delimiter\">")
              .Append('>'.ToHtmlEncodedReadOnlySpan())
              .Append("</span>");
        }

        if (symbolComponentInfo.Parameters.Count != 0)
        {
            _ = signatureBuilder.Append($"<span class=\"syntax-delimiter\">");
            if (symbolComponentInfo.IsIndexer)
            {
                _ = signatureBuilder.Append('['.ToHtmlEncodedReadOnlySpan());
            }
            else
            {
                _ = signatureBuilder.Append('('.ToHtmlEncodedReadOnlySpan());
            }

            _ = signatureBuilder.Append("</span>")
              .Append($"<span class=\"syntax-targetType\">");

            foreach (SymbolComponentInfo parameter in symbolComponentInfo.Parameters)
            {
                if (parameter.IsExtensionMethodParameter)
                {
                    _ = signatureBuilder.Append($"<span class=\"syntax-keyword\">")
                      .Append("this")
                      .Append("</span>")
                      .Append(' ');
                }

                _ = signatureBuilder.AppendInlineHtml(parameter)
                  .Append(',')
                  .Append(' ');
            }

            _ = signatureBuilder.Remove(signatureBuilder.Length - 2, 2)
              .Append($"<span class=\"syntax-delimiter\">");

            if (symbolComponentInfo.IsIndexer)
            {
                _ = signatureBuilder.Append(']'.ToHtmlEncodedReadOnlySpan())
                .Append("</span>");
            }
            else
            {
                _ = signatureBuilder.Append(')'.ToHtmlEncodedReadOnlySpan())
                .Append("</span>");
            }
        }

        if (symbolComponentInfo.PropertyGet != null)
        {
            _ = signatureBuilder.Append(' ')
              .Append('{')
              .Append(' ')
              .AppendInlineHtml(symbolComponentInfo.PropertyGet)
              .Append(';')
              .Append(' ')
              .Append('}');
        }

        if (symbolComponentInfo.PropertySet != null)
        {
            _ = signatureBuilder.Append(' ')
              .Append('{')
              .Append(' ')
              .AppendInlineHtml(symbolComponentInfo.PropertySet)
              .Append(';')
              .Append(' ')
              .Append('}');
        }

        if (symbolComponentInfo.GenericTypeConstraints.Count != 0)
        {
            foreach (SymbolComponentInfo constraintInfo in symbolComponentInfo.GenericTypeConstraints)
            {
                _ = signatureBuilder.Append(System.Environment.NewLine.ToHtmlEncodedReadOnlySpan())
                  .Append(symbolComponentInfo.IndentationString.ToHtmlEncodedReadOnlySpan())
                  .Append($"<span class=\"syntax-keyword\">")
                  .Append("where")
                  .Append(' ')
                  .Append("</span>")
                  .Append($"<span class=\"syntax-targetType\">")
                  .Append(constraintInfo.Name)
                  .Append(' ')
                  .Append("</span>")
                  .Append($"<span class=\"syntax-delimiter\">")
                  .Append(':')
                  .Append(' ')
                  .Append("</span>");

                foreach (SymbolComponentInfo constraint in constraintInfo.GenericTypeConstraints)
                {
                    if (constraint.IsKeyword)
                    {
                        _ = signatureBuilder.Append($"<span class=\"syntax-keyword\">")
                          .Append(constraint.Name)
                          .Append(',')
                          .Append(' ')
                          .Append("</span>");
                    }
                    else
                    {
                        _ = signatureBuilder.Append($"<span class=\"syntax-targetType\">")
                          .Append(constraint.Name)
                          .Append(',')
                          .Append(' ')
                          .Append("</span>");
                    }

                    _ = signatureBuilder.Remove(signatureBuilder.Length - ", </span>".Length, ", </span>".Length);
                }
            }
        }

        if (symbolComponentInfo.HasExpressionTerminator)
        {
            _ = signatureBuilder
                .Append($"<span class=\"syntax-delimiter\">")
                .Append(';')
                .Append("</span>");
        }

        return signatureBuilder;
    }

    /// <summary>
    /// Determines whether the specified delegate is compatible with the signature of the given event.
    /// </summary>
    /// <remarks>This method checks whether the delegate can be used as an event handler for the
    /// specified event by comparing the parameter types of the delegate's method and the event's handler targetType.
    /// MemberParameter types must match in number and be assignable according to .NET targetType compatibility rules.</remarks>
    /// <param name="clientHandler">The delegate to test for compatibility with the event's handler signature.</param>
    /// <param name="eventDataView">The event whose handler signature is used for compatibility comparison. Cannot be null.</param>
    /// <returns>true if the delegate's method parameters are assignable to the event handler's parameters; otherwise, false.</returns>
    public static bool IsAssignable(this Delegate clientHandler, IEventDataView eventDataView)
    {
        ArgumentNullException.ThrowIfNull(clientHandler);
        ArgumentNullException.ThrowIfNull(eventDataView);

        MethodData eventDelegateInvokeMethod = eventDataView.EventHandlerTypeData.DelegateInvokeMethodData;
        ParameterList eventDelegateParameters = eventDelegateInvokeMethod.Parameters;
        MethodInfo eventHandlerMethod = clientHandler.Method;
        ParameterInfo[] clientHandlerParameters = eventHandlerMethod.GetParameters();

        /* Validate the event EventHandler */

        if (eventDelegateParameters.Count != clientHandlerParameters.Length)
        {
            return false;
        }

        for (int parameterIndex = 0; parameterIndex < eventDelegateParameters.Count; parameterIndex++)
        {
            Type eventDelegateParameterType = eventDelegateParameters[parameterIndex].ParameterTypeData.Type;
            Type eventHandlerParameterType = clientHandlerParameters[parameterIndex].ParameterType;
            if (!eventHandlerParameterType.IsAssignableFrom(eventDelegateParameterType))
            {
                return false;
            }
        }

        return true;
    }
}
