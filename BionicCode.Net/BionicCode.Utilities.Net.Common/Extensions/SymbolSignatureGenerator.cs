namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Microsoft.CodeAnalysis;

    internal static class SymbolSignatureGenerator
    {
        private const char ExpressionTerminator = ';';
        private const string ParameterSeparator = ", ";
        private static readonly AccessModifierComparer AccessModifierComparer = new AccessModifierComparer();

        internal static readonly FrozenSet<string> IgnorableParameterAttributes = new HashSet<string>
            {
              nameof(AsyncStateMachineAttribute),
              nameof(InAttribute),
              nameof(OutAttribute),
              nameof(DebuggerStepThroughAttribute),
              nameof(DebuggerBrowsableAttribute),
              nameof(DebuggerDisplayAttribute),
              nameof(DebuggerDisplayAttribute),
              nameof(DebuggerHiddenAttribute),
              nameof(DebuggerNonUserCodeAttribute),
              nameof(DebuggerStepperBoundaryAttribute),
              nameof(DebuggerTypeProxyAttribute),
              nameof(DebuggerVisualizerAttribute),
              //nameof(ProfileAttribute),
              //nameof(ProfilerMethodArgumentAttribute),
              //nameof(ProfilerPropertyArgumentAttribute),
              //nameof(ProfilerFactoryAttribute),
              nameof(IsReadOnlyAttribute),
            }.ToFrozenSet();

        /// <summary>
        /// Extension method to convert generic and non-generic valueType names to a readable display genericTypeParameterIdentifier including the symbolNamespace.
        /// </summary>
        /// <param genericTypeParameterIdentifier="memberInfo">The <see cref="Type"/> to extend.</param>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        internal static string ToDisplayNameInternal(SymbolInfoData symbolInfoData, bool isFullyQualifiedName, bool isGenericTypeParameterIncluded, bool isDeclaringTypeIncluded)
        {
            PooledStringBuilder nameBuilder = StringBuilderFactory.GetOrCreate();

            switch (symbolInfoData)
            {
                case ParameterData parameterData:
                    _ = nameBuilder.AppendDisplayNameInternal(parameterData);
                    break;
                case TypeData typeData:
                    _ = nameBuilder.AppendDisplayNameInternal(typeData, isFullyQualifiedName, isGenericTypeParameterIncluded);
                    break;
                case MemberData memberInfoData:
                    _ = nameBuilder.AppendDisplayNameInternal(memberInfoData, isFullyQualifiedName, isGenericTypeParameterIncluded, isDeclaringTypeIncluded);
                    break;
                default:
                    throw new NotImplementedException();
            }

            string symbolName = nameBuilder.ToString();
            nameBuilder.Recycle();

            return symbolName;
        }

        /// <summary>
        /// Builds a SymbolComponentInfo representation of a method's signature, including modifiers, return type, name,
        /// parameters, and generic type information, based on the specified formatting options.
        /// </summary>
        /// <remarks>If isCompact is set to true, custom attributes and generic type constraints are
        /// omitted from the signature. The method supports both generic and non-generic methods, and can include or
        /// exclude the declaring type and fully qualified names as needed.</remarks>
        /// <param name="methodData">The metadata describing the method to be represented. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to include fully qualified type names in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the method signature; otherwise, false.</param>
        /// <param name="isCompact">true to produce a compact signature without custom attributes and generic type constraints; otherwise,
        /// false.</param>
        /// <returns>A SymbolComponentInfo object containing the components of the method's signature as specified by the input
        /// parameters.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(MethodData methodData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        {
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: false)
            {
                IsSymbol = true,
                HasExpressionTerminator = true,
            };

            if (methodData.IsGenericMethod && !methodData.IsGenericMethodDefinition)
            {
                methodData = methodData.GenericMethodDefinitionData;
            }

            SymbolAttributes symbolAttributes = methodData.SymbolAttributes;
            IEnumerable<CustomAttributeData> customAttributesData = methodData.AttributeData;

            if (symbolAttributes.HasFlag(SymbolAttributes.Final))
            {
                customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType);
            }

            if (!isCompact)
            {
                SymbolSignatureGenerator.AddCustomAttributes(symbolComponents, customAttributesData);
            }

            AccessModifier accessModifier = methodData.AccessModifier;
            symbolComponents.AddModifier(accessModifier.ToDisplayStringValue());

            if (methodData.IsStatic)
            {
                symbolComponents.AddModifier("static");
            }

            if (methodData.IsSealed)
            {
                symbolComponents.AddModifier("sealed");
            }

            bool isAbstract = symbolAttributes.HasFlag(SymbolAttributes.Abstract);
            if (isAbstract)
            {
                symbolComponents.AddModifier("abstract");
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Virtual))
            {
                symbolComponents.AddModifier("virtual");
            }

            if (methodData.IsOverride)
            {
                symbolComponents.AddModifier("override");
            }

            if (methodData.IsAsync)
            {
                symbolComponents.AddModifier("async");
            }

            if (methodData.IsReturnValueByRef)
            {
                symbolComponents.AddModifier("ref");
            }

            if (methodData.IsReturnValueReadOnly)
            {
                symbolComponents.AddModifier("readonly");
            }

            symbolComponents.ReturnType = methodData.ReturnTypeData.CompactSymbolComponentInfo;

            // MemberData name
            _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(methodData, isFullyQualifiedName: false, isGenericTypeParameterIncluded: false, isDeclaringTypeIncluded);

            symbolComponents.IsExtensionMethodParameter = methodData.IsExtensionMethod;

            ParameterData[] parameters = methodData.Parameters;
            if (parameters.Length > 0)
            {
                for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
                {
                    ParameterData parameterData = parameters[parameterIndex];
                    SymbolComponentInfo parameterInfo = parameterData.SymbolComponentInfo;
                    symbolComponents.AddParameter(parameterInfo);
                }
            }

            if (methodData.IsGenericMethod)
            {
                IEnumerable<SymbolComponentInfo> genericTypeParameterComponents = methodData.GenericMethodArguments.Select(typeParameterData =>
                {
                    SymbolComponentInfo info = typeParameterData.SymbolComponentInfo;
                    info.IsParameter = true;
                    return info;
                });
                symbolComponents.AddGenericTypeParameterRange(genericTypeParameterComponents);

                if (!isCompact)
                {
                    SymbolSignatureGenerator.AddGenericTypeConstraints(symbolComponents, methodData.GenericMethodArguments, isFullyQualifiedName);
                }
            }

            return symbolComponents;
        }

        /// <summary>
        /// Creates a SymbolComponentInfo instance representing the signature components of the specified type,
        /// including modifiers, attributes, and generic parameters as appropriate.
        /// </summary>
        /// <remarks>When isCompact is false, the returned signature includes access modifiers, custom
        /// attributes, and inheritance or generic constraints where applicable. For delegate types, the signature
        /// includes parameter and return type information. The method does not validate the input typeData; callers
        /// should ensure it represents a valid type.</remarks>
        /// <param name="typeData">The type metadata to convert into signature components. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to use fully qualified type names in the signature; otherwise, false to use simple names.</param>
        /// <param name="isCompact">true to generate a compact signature with minimal modifiers and attributes; otherwise, false to include full
        /// details.</param>
        /// <returns>A SymbolComponentInfo object containing the signature components for the specified type, including
        /// modifiers, attributes, name, generic parameters, and, for delegates, parameter and return type information.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(TypeData typeData, bool isFullyQualifiedName, bool isCompact)
        {
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: typeData.IsBuiltInType)
            {
                IsSymbol = true,
            };

            if (typeData.IsGenericType && !typeData.IsGenericTypeDefinition)
            {
                typeData = typeData.GenericTypeDefinitionData;
            }

            SymbolAttributes symbolAttributes = typeData.SymbolAttributes;
            IEnumerable<CustomAttributeData> customAttributesData = typeData.AttributeData;

            if (symbolAttributes.HasFlag(SymbolAttributes.Final))
            {
                customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
            }

            if (!isCompact)
            {
                SymbolSignatureGenerator.AddCustomAttributes(symbolComponents, customAttributesData);

                AccessModifier accessModifier = typeData.AccessModifier;
                symbolComponents.AddModifier(accessModifier.ToDisplayStringValue());

                if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
                {
                    symbolComponents.AddModifier("delegate");
                }
                else if (symbolAttributes.HasFlag(SymbolAttributes.Struct))
                {
                    if (symbolAttributes.HasFlag(SymbolAttributes.ReadOnlyStruct))
                    {
                        symbolComponents.AddModifier("readonly");
                    }
                    else if (symbolAttributes.HasFlag(SymbolAttributes.RefStruct))
                    {
                        symbolComponents.AddModifier("ref");
                    }

                    symbolComponents.AddModifier("struct");
                }
                else if (symbolAttributes.HasFlag(SymbolAttributes.Class))
                {
                    if (symbolAttributes.HasFlag(SymbolAttributes.Abstract))
                    {
                        symbolComponents.AddModifier("abstract");
                    }
                    else if (symbolAttributes.HasFlag(SymbolAttributes.Static))
                    {
                        symbolComponents.AddModifier("static");
                    }
                    else if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                    {
                        symbolComponents.AddModifier("sealed");
                    }

                    symbolComponents.AddModifier("class");
                }
                else if (symbolAttributes.HasFlag(SymbolAttributes.Interface))
                {
                    symbolComponents.AddModifier("interface");
                }
                else if (symbolAttributes.HasFlag(SymbolAttributes.Enum))
                {
                    symbolComponents.AddModifier("enum");
                }
            }

            MethodData delegateInvocatorData = null;
            TypeData delegateReturnTypeData = null;

            // SetValue return valueType
            if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
            {
                delegateReturnTypeData = typeData.DelegateInvokeMethodData.ReturnTypeData;
                symbolComponents.ReturnType = delegateReturnTypeData.CompactSymbolComponentInfo;
            }

            // ParameterType name
            _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(typeData, isFullyQualifiedName, isGenericTypeParameterIncluded: false);

            if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
            {
                ParameterData[] parameters = delegateInvocatorData.Parameters;
                if (parameters.Length > 0)
                {
                    foreach (ParameterData parameterData in parameters)
                    {
                        SymbolComponentInfo parameterInfo = parameterData.SymbolComponentInfo;
                        symbolComponents.AddParameter(parameterInfo);
                    }
                }
            }
            else if (!isCompact && symbolAttributes.HasFlag(SymbolAttributes.Class))
            {
                SymbolSignatureGenerator.AddInheritanceSignature(symbolComponents, typeData, isFullyQualifiedName);
            }

            if (typeData.IsGenericType)
            {
                TypeData[] genericTypeArguments = typeData.GenericTypeArguments;
                IEnumerable<SymbolComponentInfo> genericTypeParameterComponents = genericTypeArguments.Select(typeParameterData => typeParameterData.SymbolComponentInfo);
                symbolComponents.AddGenericTypeParameterRange(genericTypeParameterComponents);

                if (!isCompact)
                {
                    SymbolSignatureGenerator.AddGenericTypeConstraints(symbolComponents, genericTypeArguments, isFullyQualifiedName);
                }
            }

            symbolComponents.HasExpressionTerminator = symbolAttributes.HasFlag(SymbolAttributes.Delegate);
            return symbolComponents;
        }

        private static void AddInheritanceSignature(SymbolComponentInfo symbolComponents, TypeData typeData, bool isFullyQualified)
        {
            if (typeData.IsDelegate)
            {
                return;
            }

            bool isSubclass = typeData.IsSubclass;
            TypeData[] interfaces = typeData.InterfacesData;
            if (isSubclass)
            {
                var inheritedTypeComponent = new SymbolComponentInfo(isKeyword: typeData.BaseTypeData.IsBuiltInType);
                _ = inheritedTypeComponent.NameBuilder.Append(isFullyQualified ? typeData.BaseTypeData.FullyQualifiedDisplayName : typeData.BaseTypeData.Name);
                symbolComponents.AddInheritedType(inheritedTypeComponent);
            }

            foreach (TypeData interfaceData in interfaces)
            {
                var inheritedTypeComponent = new SymbolComponentInfo(isKeyword: false);
                _ = inheritedTypeComponent.NameBuilder.Append(isFullyQualified ? interfaceData.FullyQualifiedDisplayName : interfaceData.Name);
                symbolComponents.AddInheritedType(inheritedTypeComponent);
            }
        }

        private static void AddGenericTypeConstraints(SymbolComponentInfo symbolComponents, TypeData[] genericTypeDefinitionsData, bool isFullyQualified)
        {
            for (int genericTypeArgumentIndex = 0; genericTypeArgumentIndex < genericTypeDefinitionsData.Length; genericTypeArgumentIndex++)
            {
                TypeData genericTypeDefinitionData = genericTypeDefinitionsData[genericTypeArgumentIndex];
                var constraintComponents = new SymbolComponentInfo(genericTypeDefinitionData.Name);
                TypeData[] constraints = genericTypeDefinitionData.GenericParameterConstraintsData;
                if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask) == GenericParameterAttributes.None
                  && constraints.Length == 0)
                {
                    continue;
                }

                if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                {
                    var constraint = new SymbolComponentInfo("class", isKeyword: true);
                    constraintComponents.AddGenericTypeConstraint(constraint);
                }

                if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                {
                    var constraint = new SymbolComponentInfo("struct", isKeyword: true);
                    constraintComponents.AddGenericTypeConstraint(constraint);
                }

                foreach (TypeData constraintData in constraints)
                {
                    var constraint = new SymbolComponentInfo(isKeyword: constraintData.IsBuiltInType);
                    _ = constraint.NameBuilder.AppendDisplayNameInternal(constraintData, isFullyQualified, isGenericTypeParameterIncluded: true);
                    constraintComponents.AddGenericTypeConstraint(constraint);
                }

                if (!genericTypeDefinitionData.IsValueType && (genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                {
                    var constraint = new SymbolComponentInfo("new()", isKeyword: true);
                    constraintComponents.AddGenericTypeConstraint(constraint);
                }

                symbolComponents.AddGenericTypeConstraint(constraintComponents);
            }
        }

        /// <summary>
        /// Creates a SymbolComponentInfo instance representing the signature components of a parameter, including its
        /// type, modifiers, and custom attributes, according to the specified formatting options.
        /// </summary>
        /// <remarks>Custom attributes are included in the signature unless isCompact is set to true. The
        /// method applies parameter modifiers such as ref, in, or out as appropriate. If the parameter type is a
        /// constructed generic type, its generic type definition is used for display purposes.</remarks>
        /// <param name="parameterData">The parameter metadata to convert into signature components. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to use fully qualified type names in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the signature; otherwise, false.</param>
        /// <param name="isCompact">true to produce a compact signature representation that omits custom attributes; otherwise, false.</param>
        /// <returns>A SymbolComponentInfo object containing the signature components for the specified parameter, formatted
        /// according to the provided options.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(ParameterData parameterData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        {
            TypeData parameterTypeData = parameterData.ParameterTypeData;
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: parameterTypeData.IsBuiltInType)
            {
                IsSymbol = false,
                IsParameter = true,
                HasExpressionTerminator = false,
                HasInlineAttributes = true,
            };

            if (parameterTypeData.IsGenericType && !parameterTypeData.IsGenericTypeDefinition)
            {
                parameterTypeData = parameterTypeData.GenericTypeDefinitionData;
            }

            SymbolAttributes symbolAttributes = parameterData.SymbolAttributes;
            IEnumerable<CustomAttributeData> customAttributesData = parameterData.AttributeData;

            if (symbolAttributes.HasFlag(SymbolAttributes.Final))
            {
                customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
            }

            if (!isCompact)
            {
                SymbolSignatureGenerator.AddCustomAttributes(symbolComponents, customAttributesData);
            }

            if (parameterData.IsRefReadOnly)
            {
                symbolComponents.AddModifier("ref readonly");
            }
            else if (parameterData.IsRef)
            {
                symbolComponents.AddModifier("ref");
            }
            else if (parameterData.IsIn)
            {
                symbolComponents.AddModifier("in");
            }
            else if (parameterData.IsOut)
            {
                symbolComponents.AddModifier("out");
            }
            else if (parameterData.IsParams)
            {
                symbolComponents.AddModifier("params");
            }

            // ParameterType name
            _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(parameterTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true);
            _ = symbolComponents.ValueNameBuilder.AppendDisplayNameInternal(parameterData);

            return symbolComponents;
        }

        /// <summary>
        /// Creates a SymbolComponentInfo instance representing the signature components of the specified field,
        /// including modifiers, attributes, and type information.
        /// </summary>
        /// <remarks>This method is intended for internal use when constructing field signatures for
        /// display or analysis. The output reflects the specified formatting options and may omit certain attributes or
        /// components based on the provided parameters.</remarks>
        /// <param name="fieldData">The field metadata to extract signature components from. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to include the fully qualified name of the field; otherwise, false to use the simple name.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the field's signature; otherwise, false.</param>
        /// <param name="isCompact">true to produce a compact signature that omits custom attributes; otherwise, false to include all relevant
        /// attributes.</param>
        /// <returns>A SymbolComponentInfo object containing the signature components of the field, such as modifiers, type, and
        /// name.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(FieldData fieldData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        {
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: false)
            {
                IsSymbol = true,
                HasExpressionTerminator = true,
            };

            SymbolAttributes symbolAttributes = fieldData.SymbolAttributes;

            if (!isCompact)
            {
                IEnumerable<CustomAttributeData> customAttributesData = fieldData.AttributeData
                    .Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType)
                    .ToHashSet();

                if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                {
                    customAttributesData = customAttributesData;
                }

                SymbolSignatureGenerator.AddCustomAttributes(symbolComponents, customAttributesData);
            }

            AccessModifier accessModifier = fieldData.AccessModifier;
            symbolComponents.AddModifier(accessModifier.ToDisplayStringValue());

            if (symbolAttributes.HasFlag(SymbolAttributes.ConstantField))
            {
                symbolComponents.AddModifier("const");
            }
            else
            {
                if (symbolAttributes.HasFlag(SymbolAttributes.Static))
                {
                    symbolComponents.AddModifier("static");
                }

                if (symbolAttributes.HasFlag(SymbolAttributes.ReadOnlyField))
                {
                    symbolComponents.AddModifier("readonly");
                }

                if (fieldData.IsRef)
                {
                    symbolComponents.AddModifier("ref");
                }
            }

            symbolComponents.ReturnType = fieldData.FieldTypeData.CompactSymbolComponentInfo;

            _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(fieldData, isFullyQualifiedName, isGenericTypeParameterIncluded: false, isDeclaringTypeIncluded);

            return symbolComponents;
        }

        /// <summary>
        /// Creates a SymbolComponentInfo instance representing the signature components of the specified event,
        /// including modifiers, attributes, and event type information.
        /// </summary>
        /// <remarks>This method is intended for internal use when constructing event signatures for
        /// display or analysis. The output reflects the specified formatting options and may exclude certain attributes
        /// or modifiers based on the provided parameters.</remarks>
        /// <param name="eventData">The event metadata to extract signature components from. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to include the fully qualified name of the event in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the event's signature; otherwise, false.</param>
        /// <param name="isCompact">true to produce a compact signature that omits custom attributes; otherwise, false.</param>
        /// <returns>A SymbolComponentInfo object containing the signature components of the event, including modifiers, event
        /// type, and name.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(EventData eventData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        {
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: false)
            {
                IsSymbol = true,
                HasExpressionTerminator = true,
            };

            SymbolAttributes symbolAttributes = eventData.SymbolAttributes;
            IEnumerable<CustomAttributeData> customAttributesData = eventData.AttributeData;

            if (symbolAttributes.HasFlag(SymbolAttributes.Final))
            {
                customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType);
            }

            if (!isCompact)
            {
                SymbolSignatureGenerator.AddCustomAttributes(symbolComponents, customAttributesData);
            }

            AccessModifier accessModifier = eventData.AccessModifier;
            symbolComponents.AddModifier(accessModifier.ToDisplayStringValue());

            if (symbolAttributes.HasFlag(SymbolAttributes.Static))
            {
                symbolComponents.AddModifier("static");
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Abstract))
            {
                symbolComponents.AddModifier("abstract");
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Virtual))
            {
                symbolComponents.AddModifier("virtual");
            }

            if (symbolAttributes.HasFlag(SymbolAttributes.Override))
            {
                symbolComponents.AddModifier("override");
            }

            symbolComponents.AddModifier("event");

            symbolComponents.ReturnType = eventData.EventHandlerTypeData.CompactSymbolComponentInfo;

            _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(eventData, isFullyQualifiedName, isGenericTypeParameterIncluded: false, isDeclaringTypeIncluded);

            return symbolComponents;
        }

        /// <summary>
        /// Builds a SymbolComponentInfo representation of a property signature based on the specified property metadata
        /// and formatting options.
        /// </summary>
        /// <remarks>This method is intended for internal use when constructing property signatures for
        /// display or analysis purposes. The output reflects the specified formatting options and may differ depending
        /// on the property type (e.g., indexer vs. regular property) and the presence of custom attributes or
        /// modifiers.</remarks>
        /// <param name="propertyData">The metadata describing the property, including its type, access modifiers, attributes, and accessor
        /// information. Cannot be null.</param>
        /// <param name="isFullyQualifiedName">true to include the property's fully qualified name in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the property's signature; otherwise, false.</param>
        /// <param name="isCompact">true to generate a compact signature that omits custom attributes; otherwise, false.</param>
        /// <returns>A SymbolComponentInfo object containing the components of the property's signature, including modifiers,
        /// return type, name, parameters (for indexers), and accessor information.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(PropertyData propertyData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        {
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: false)
            {
                IsSymbol = true,
                HasExpressionTerminator = false,
            };

            SymbolAttributes symbolAttributes = propertyData.SymbolAttributes;
            IEnumerable<CustomAttributeData> customAttributesData = propertyData.AttributeData;

            if (symbolAttributes.HasFlag(SymbolAttributes.Final))
            {
                customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != typeof(IsReadOnlyAttribute));
            }

            if (!isCompact)
            {
                SymbolSignatureGenerator.AddCustomAttributes(symbolComponents, customAttributesData);
            }

            AccessModifier accessModifier = propertyData.AccessModifier;
            symbolComponents.AddModifier(accessModifier.ToDisplayStringValue());

            if (symbolAttributes.HasFlag(SymbolAttributes.Static))
            {
                symbolComponents.AddModifier("static");
            }

            bool isAbstract = symbolAttributes.HasFlag(SymbolAttributes.Abstract);
            if (!symbolAttributes.HasFlag(SymbolAttributes.Delegate) && isAbstract)
            {
                symbolComponents.AddModifier("abstract");
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Virtual))
            {
                symbolComponents.AddModifier("virtual");
            }

            if (symbolAttributes.HasFlag(SymbolAttributes.Override))
            {
                symbolComponents.AddModifier("override");
            }

            // SetValue return valueType
            symbolComponents.ReturnType = propertyData.PropertyTypeData.CompactSymbolComponentInfo;

            if (symbolAttributes.HasFlag(SymbolAttributes.IndexerProperty))
            {
                _ = symbolComponents.IsIndexer = true;

                ParameterData[] parameters = propertyData.IndexerParameters;
                if (parameters.Any())
                {
                    foreach (ParameterData parameter in parameters)
                    {
                        SymbolComponentInfo parameterInfo = parameter.SymbolComponentInfo;
                        symbolComponents.AddParameter(parameterInfo);
                    }
                }
            }
            else
            {
                _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(propertyData, isFullyQualifiedName, isGenericTypeParameterIncluded: false, isDeclaringTypeIncluded);
            }

            if (propertyData.CanRead)
            {
                var propertyGet = new SymbolComponentInfo("get", isKeyword: true);
                if (SymbolSignatureGenerator.AccessModifierComparer.Compare(propertyData.GetAccessorAccessModifier, propertyData.AccessModifier) < 0)
                {
                    propertyGet.AddModifier(propertyData.GetAccessorAccessModifier.ToDisplayStringValue());
                }

                symbolComponents.PropertyGet = propertyGet;
            }

            if (propertyData.CanWrite)
            {
                bool isInitProperty = propertyData.SymbolAttributes.HasFlag(SymbolAttributes.InitProperty);
                string accessor = isInitProperty
                  ? "init"
                  : "set";
                var propertySet = new SymbolComponentInfo(accessor, isKeyword: true);
                if (SymbolSignatureGenerator.AccessModifierComparer.Compare(propertyData.SetAccessorAccessModifier, propertyData.AccessModifier) < 0)
                {
                    propertySet.AddModifier(propertyData.SetAccessorAccessModifier.ToDisplayStringValue());
                }

                if (propertyData.IsSetMethodReadOnly)
                {
                    propertySet.AddModifier("readonly");
                }

                symbolComponents.PropertySet = propertySet;
            }

            return symbolComponents;
        }

        /// <summary>
        /// Creates a SymbolComponentInfo instance representing the signature components of a constructor, based on the
        /// specified formatting and inclusion options.
        /// </summary>
        /// <remarks>When isCompact is set to true, custom attributes are excluded from the signature. The
        /// isFullyQualifiedName and isDeclaringTypeIncluded parameters control the level of detail included in the
        /// constructor's name within the signature.</remarks>
        /// <param name="constructorData">The metadata describing the constructor, including its attributes, access modifier, parameters, and other
        /// relevant information.</param>
        /// <param name="isFullyQualifiedName">true to include the fully qualified name of the constructor in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the constructor's signature; otherwise, false.</param>
        /// <param name="isCompact">true to generate a compact signature that omits custom attributes; otherwise, false to include custom
        /// attributes in the signature.</param>
        /// <returns>A SymbolComponentInfo object containing the formatted signature components of the specified constructor.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(ConstructorData constructorData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        {
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: false)
            {
                IsSymbol = true,
                HasExpressionTerminator = true,
            };

            SymbolAttributes symbolAttributes = constructorData.SymbolAttributes;
            IEnumerable<CustomAttributeData> customAttributesData = constructorData.AttributeData;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

            if (symbolAttributes.HasFlag(SymbolAttributes.Final))
            {
                customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType);
            }

            if (!isCompact)
            {
                SymbolSignatureGenerator.AddCustomAttributes(symbolComponents, customAttributesData);
            }

            AccessModifier accessModifier = constructorData.AccessModifier;
            symbolComponents.AddModifier(accessModifier.ToDisplayStringValue());

            if (constructorData.IsStatic)
            {
                symbolComponents.AddModifier("static");
            }

            // MemberData name
            _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(constructorData, isFullyQualifiedName, isGenericTypeParameterIncluded: false, isDeclaringTypeIncluded);

            ParameterData[] parameters = constructorData.Parameters;
            if (parameters.Length > 0)
            {
                foreach (ParameterData parameterData in parameters)
                {
                    SymbolComponentInfo parameterInfo = parameterData.SymbolComponentInfo;
                    symbolComponents.AddParameter(parameterInfo);
                }
            }

            return symbolComponents;
        }

        private static void AddCustomAttributes(SymbolComponentInfo symbolComponentInfo, IEnumerable<CustomAttributeData> attributes)
        {
            foreach (CustomAttributeData attribute in attributes)
            {
                if (SymbolSignatureGenerator.IgnorableParameterAttributes.Contains(attribute.AttributeType.Name))
                {
                    continue;
                }

                var customAttributeInfo = new SymbolComponentInfo(attribute.AttributeType.Name);
                symbolComponentInfo.AddCustomAttribute(customAttributeInfo);

                foreach (CustomAttributeTypedArgument constructorPositionalArgument in attribute.ConstructorArguments)
                {
                    string customAttributeConstructorArg = constructorPositionalArgument.Value.ToArgumentDisplayValue();
                    customAttributeInfo.AddCustomAttributeConstructorArg(customAttributeConstructorArg);
                }

                foreach (CustomAttributeNamedArgument constructorNamedArgument in attribute.NamedArguments)
                {
                    string propertyName = constructorNamedArgument.MemberName;
                    string propertyValue = constructorNamedArgument.TypedValue.Value.ToArgumentDisplayValue();
                    customAttributeInfo.AddCustomAttributeNamedArg((propertyName, propertyValue));
                }
            }
        }

        //    public static StringBuilder AppendSignatureName(this StringBuilder nameBuilder, MemberInfo memberInfo, bool isFullyQualifiedName, bool isDeclaringTypeIncluded)
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
        //      TypeSyntax returnType = SyntaxFactory.ParseTypeName(IsPropertyInit(methodInfo.ReturnType, isFullyQualifiedName, isDeclaringTypeIncluded: false));
        //      string methodName = IsPropertyInit(methodInfo, isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //      MethodDeclarationSyntax methodGraph = SyntaxFactory.MethodDeclaration(returnType, methodName)
        //        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        //      ParameterInfo[] parameters = methodInfo.GetParameters();
        //      foreach (ParameterInfo parameter in parameters)
        //      {
        //        ParameterSyntax parameterSyntax = SyntaxFactory.MemberParameter(SyntaxFactory.Identifier(parameter.EventName))
        //          .WithType(SyntaxFactory.IdentifierName(IsPropertyInit(parameter.ParameterType, isFullyQualifiedName, isDeclaringTypeIncluded: false)));

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
        //        //  AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.Identifier(IsPropertyInit(attributeSyntax.EventName, isFullyQualifiedName, isDeclaringTypeIncluded: false)));
        //        //  parameterSyntax = parameterSyntax.AddAttributeLists(attributeSyntax);
        //        //}
        //        methodGraph = methodGraph.AddParameterListParameters(parameterSyntax);
        //      }

        //      if (methodInfo.IsGenericMethod)
        //      {
        //        ParameterType[] typeArguments = methodInfo.GetGenericArguments();
        //        for (int typeArgumentIndex = 0; typeArgumentIndex < typeArguments.Length; typeArgumentIndex++)
        //        {
        //          ParameterType typeArgument = typeArguments[typeArgumentIndex];
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

        //            ParameterType[] constraintTypes = typeArgument.GetGenericParameterConstraints();
        //            foreach (ParameterType constraintType in constraintTypes)
        //            {
        //              if (constraintType == typeof(object) || constraintType == typeof(ValueType))
        //              {
        //                continue;
        //              }

        //              string constraintName = IsPropertyInit(constraintType, isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //              TypeConstraintSyntax constraintSyntax = SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(constraintName));
        //              constraints = constraints.Add(constraintSyntax);
        //            }

        //            if (!typeArgument.IsValueType && (typeArgument.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
        //            {
        //              constraints = constraints.Add(SyntaxFactory.ConstructorConstraint());
        //            }

        //            string genericTypeParameterName = IsPropertyInit(typeArgument, isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //            methodGraph = methodGraph.AddConstraintClauses(SyntaxFactory.TypeParameterConstraintClause(SyntaxFactory.IdentifierName(genericTypeParameterName), constraints));
        //          }
        //        }
        //      }

        //      methodGraph = methodGraph.NormalizeWhitespace();
        //      return methodGraph;
        //    }

        //    private static SyntaxNode CreateDelegateGraph(MethodInfo methodInfo, bool isFullyQualifiedName)
        //    {
        //      TypeSyntax returnType = SyntaxFactory.ParseTypeName(IsPropertyInit(methodInfo.ReturnType, isFullyQualifiedName, isDeclaringTypeIncluded: false));
        //      string methodName = IsPropertyInit(methodInfo, isFullyQualifiedName, isDeclaringTypeIncluded: true);
        //      MethodDeclarationSyntax methodGraph = SyntaxFactory.MethodDeclaration(returnType, methodName)
        //        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        //      ParameterInfo[] parameters = methodInfo.GetParameters();
        //      foreach (ParameterInfo parameter in parameters)
        //      {
        //        ParameterSyntax parameterSyntax = SyntaxFactory.MemberParameter(SyntaxFactory.Identifier(parameter.EventName))
        //          .WithType(SyntaxFactory.IdentifierName(IsPropertyInit(parameter.ParameterType, isFullyQualifiedName, isDeclaringTypeIncluded: false)));

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
        //        //  AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.Identifier(IsPropertyInit(attributeSyntax.EventName, isFullyQualifiedName, isDeclaringTypeIncluded: false)));
        //        //  parameterSyntax = parameterSyntax.AddAttributeLists(attributeSyntax);
        //        //}
        //        methodGraph = methodGraph.AddParameterListParameters(parameterSyntax);
        //      }

        //      if (methodInfo.IsGenericMethod)
        //      {
        //        ParameterType[] typeArguments = methodInfo.GetGenericArguments();
        //        for (int typeArgumentIndex = 0; typeArgumentIndex < typeArguments.Length; typeArgumentIndex++)
        //        {
        //          ParameterType typeArgument = typeArguments[typeArgumentIndex];
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

        //            ParameterType[] constraintTypes = typeArgument.GetGenericParameterConstraints();
        //            foreach (ParameterType constraintType in constraintTypes)
        //            {
        //              if (constraintType == typeof(object) || constraintType == typeof(ValueType))
        //              {
        //                continue;
        //              }

        //              string constraintName = IsPropertyInit(constraintType, isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //              TypeConstraintSyntax constraintSyntax = SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(constraintName));
        //              constraints = constraints.Add(constraintSyntax);
        //            }

        //            string genericTypeParameterName = IsPropertyInit(typeArgument, isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //            methodGraph = methodGraph.AddConstraintClauses(SyntaxFactory.TypeParameterConstraintClause(SyntaxFactory.IdentifierName(genericTypeParameterName), constraints));
        //          }
        //        }
        //      }

        //      methodGraph = methodGraph.NormalizeWhitespace(indentation: " ", elasticTrivia: true);
        //      return methodGraph;
        //    }

        //    private static TypeParameterSyntax CreateMethodTypeParameter(ParameterType valueType, bool isFullyQualifiedName)
        //    {
        //      IEnumerable<Attribute> attributes = valueType.GetCustomAttributes();
        //      AttributeListSyntax attributeSyntaxList = SyntaxFactory.AttributeList();
        //      foreach (Attribute attribute in attributes)
        //      {
        //        string attributeName = IsPropertyInit(attribute.GetType(), isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //        AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName));
        //        attributeSyntaxList = attributeSyntaxList.AddAttributes(attributeSyntax);
        //      }

        //      SyntaxKind variance = SyntaxKind.None;
        //      if (valueType.IsGenericParameter)
        //      {
        //        if ((valueType.GenericParameterAttributes & GenericParameterAttributes.Covariant) != 0)
        //        {
        //          variance = SyntaxKind.OutKeyword;
        //        }
        //        else if ((valueType.GenericParameterAttributes & GenericParameterAttributes.Contravariant) != 0)
        //        {
        //          variance = SyntaxKind.InKeyword;
        //        }
        //      }

        //      string typeParameterName = IsPropertyInit(valueType, isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //      return SyntaxFactory.TypeParameter(new SyntaxList<AttributeListSyntax>() { attributeSyntaxList }, SyntaxFactory.Token(variance), SyntaxFactory.Identifier(typeParameterName));
        //    }

        //    private static string IsPropertyInit(MemberInfo memberInfo, bool isFullyQualifiedName, bool isDeclaringTypeIncluded)
        //    {
        //      string symbolName = new StringBuilder()
        //        .AppendDisplayNameInternal(memberInfo, isFullyQualifiedName, isDeclaringTypeIncluded)
        //        .ToString();

        //      return symbolName;
        //    }

        //    private static StringBuilder AppendDisplayNameInternal(this StringBuilder nameBuilder, ParameterType valueType, bool isFullyQualifiedName, bool isDeclaringTypeIncluded)
        //    {
        //      var typeReference = new CodeTypeReference(valueType);
        //      ReadOnlySpan<char> typeName = HelperExtensionsCommon.CodeProvider.GetTypeOutput(typeReference).AsSpan();
        //      if (valueType.IsGenericType)
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

        //      if (isDeclaringTypeIncluded)
        //      {
        //        return nameBuilder;
        //      }

        //      if (valueType.IsGenericType)
        //      {
        //        _ = nameBuilder.Append('<');

        //        ParameterType[] typeArguments = valueType.GetGenericArguments();
        //        foreach (ParameterType typeArgument in typeArguments)
        //        {
        //          _ = nameBuilder.AppendDisplayNameInternal(typeArgument, isFullyQualifiedName, isDeclaringTypeIncluded)
        //            .Append(ParameterSeparator);
        //        }

        //        _ = nameBuilder.Remove(nameBuilder.Length - ParameterSeparator.Length, ParameterSeparator.Length)
        //          .Append('>');
        //      }

        //      return nameBuilder;
        //    }

        //  private static StringBuilder AppendDisplayNameInternal(this StringBuilder nameBuilder, MemberInfo memberInfo, bool isFullyQualifiedName, bool isDeclaringTypeIncluded)
        //    {
        //      if (memberInfo is ParameterType valueType)
        //      {
        //        return nameBuilder.AppendDisplayNameInternal(valueType, isFullyQualifiedName, isDeclaringTypeIncluded);
        //      }

        //      if (isFullyQualifiedName)
        //      {
        //        _ = nameBuilder.AppendDisplayNameInternal(memberInfo.DeclaringType, isFullyQualifiedName, isDeclaringTypeIncluded)
        //          .Append('.');
        //      }

        //      if (memberInfo.MemberType.HasFlag(MemberTypes.MemberConstructor))
        //      {
        //        if (memberInfo.DeclaringType.IsGenericType)
        //        {
        //          int genericTypeArgumentPlaceholderIndex = memberInfo.DeclaringType.EventName.IndexOf('`');
        //          return nameBuilder.Append(memberInfo.DeclaringType.EventName, 0, genericTypeArgumentPlaceholderIndex);
        //        }
        //        else
        //        {
        //          return nameBuilder.Append(memberInfo.DeclaringType.EventName);
        //        }
        //      }
        //      else
        //      {
        //        return nameBuilder.Append(memberInfo.EventName);
        //      }
        //    }
        //    private static SymbolAttributes GetKind(this MemberInfo memberInfo)
        //    {
        //      var valueType = memberInfo as ParameterType;
        //      var propertyInfo = memberInfo as PropertyInfo;
        //      MethodInfo methodInfo = memberInfo as MethodInfo // MemberInfo is method
        //        ?? valueType?.GetMethod(HelperExtensionsCommon.DelegateInvocatorMethodName); // MemberInfo is potentially a delegate
        //      MethodInfo propertyGetMethodInfo = propertyInfo?.GetGetMethod(true);
        //      MethodInfo propertySetMethodInfo = propertyInfo?.GetSetMethod(true);
        //      var constructorInfo = memberInfo as ConstructorInfo;
        //      var fieldInfo = memberInfo as FieldInfo;
        //      var eventInfo = memberInfo as EventInfo;
        //      MethodInfo eventAddMethodInfo = eventInfo?.GetAddMethod(true);
        //      FieldInfo eventDeclaredFieldInfo = eventInfo?.DeclaringType.GetField(eventInfo.EventName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        //      ParameterInfo[] indexerPropertyIndexParameters = propertyInfo?.GetIndexParameters() ?? Array.Empty<ParameterInfo>();

        //      bool isDelegate = valueType?.IsDelegate() ?? false;
        //      if (isDelegate)
        //      {
        //        return SymbolAttributes.Delegate;
        //      }

        //      bool isClass = !isDelegate && (valueType?.IsClass ?? false);
        //      if (isClass)
        //      {
        //        SymbolAttributes classKind = SymbolAttributes.Class;
        //        if (valueType.IsAbstract)
        //        {
        //          classKind |= SymbolAttributes.Abstract;
        //        }

        //        if (valueType.IsSealed)
        //        {
        //          classKind |= SymbolAttributes.Final;
        //        }

        //        if (valueType.IsStatic())
        //        {
        //          classKind |= SymbolAttributes.Static;
        //        }

        //        return classKind;
        //      }

        //      bool isEnum = !isDelegate && (valueType?.IsEnum ?? false);
        //      if (isEnum)
        //      {
        //        return SymbolAttributes.Enum;
        //      }

        //      bool isStruct = !isDelegate && (valueType?.IsValueType ?? false);
        //      if (isStruct)
        //      {
        //        SymbolAttributes structKind = SymbolAttributes.Struct;

        //#if NETSTANDARD2_1_OR_GREATER || NET471_OR_GREATER || NET
        //        bool isReadOnlyStruct = isStruct && valueType.GetCustomAttribute(typeof(IsReadOnlyAttribute)) != null;
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
        //        SymbolAttributes constructorKind = SymbolAttributes.MemberConstructor;

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

        //      bool isInterface = !isDelegate && !isClass && (valueType?.IsInterface ?? false);
        //      if (isInterface)
        //      {
        //        SymbolAttributes interfaceKind = SymbolAttributes.Interface;
        //        return interfaceKind;
        //      }

        //      return SymbolAttributes.Undefined;
        //    }
        //    internal static string ToSignatureNameInternal(this MemberInfo memberInfo, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        //    {
        //      var fieldInfo = memberInfo as FieldInfo;
        //      var eventInfo = memberInfo as EventInfo;
        //      var propertyInfo = memberInfo as PropertyInfo;
        //      AccessModifier GetAccessModifier()
        //      {
        //        switch (memberInfo)
        //        {
        //          case ParameterType type:
        //            return type.IsPublic ? AccessModifier.Public
        //              : type.IsNestedPrivate ? AccessModifier.Private
        //              : type.IsNestedAssembly ? AccessModifier.Internal
        //              : type.IsNestedFamily ? AccessModifier.Protected
        //              : type.IsNestedPublic ? AccessModifier.Public
        //              : type.IsNestedFamORAssem ? AccessModifier.ProtectedInternal
        //              : type.IsNestedFamANDAssem ? AccessModifier.PrivateProtected
        //              : !type.IsVisible ? AccessModifier.Internal
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
        //      // TODO::Create valueType specific overloads to eliminate valueType switching and use cached reflection data

        //      var valueType = memberInfo as ParameterType;
        //      MethodInfo methodInfo = memberInfo as MethodInfo // MemberInfo is method
        //        ?? valueType?.GetMethod(HelperExtensionsCommon.DelegateInvocatorMethodName); // MemberInfo is potentially a delegate
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

        //      // SetValue return valueType
        //      if (memberAttributes.HasFlag(SymbolAttributes.Method)
        //        || memberAttributes.HasFlag(SymbolAttributes.Property)
        //        || memberAttributes.HasFlag(SymbolAttributes.Field)
        //        || memberAttributes.HasFlag(SymbolAttributes.Delegate)
        //        || memberAttributes.HasFlag(SymbolAttributes.Event))
        //      {
        //        ParameterType returnType = fieldInfo?.FieldType
        //          ?? methodInfo?.ReturnType
        //          ?? propertyGetMethodInfo?.ReturnType
        //          ?? eventInfo?.EventHandlerType;

        //        _ = signatureNameBuilder.AppendDisplayNameInternal(returnType, isFullyQualifiedName, isDeclaringTypeIncluded: false)
        //          .Append(' ');
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.MemberData) && (!isDeclaringTypeIncluded || isFullyQualifiedName))
        //      {
        //        _ = signatureNameBuilder.AppendDisplayNameInternal(memberInfo.DeclaringType, isFullyQualifiedName, isDeclaringTypeIncluded: false)
        //          .Append('.');
        //      }

        //      // MemberData or valueType name
        //      if (memberAttributes.HasFlag(SymbolAttributes.IndexerProperty))
        //      {
        //        _ = signatureNameBuilder.Append("this");
        //      }
        //      else
        //      {
        //        _ = signatureNameBuilder.AppendDisplayNameInternal(memberInfo, isFullyQualifiedName: isFullyQualifiedName && memberAttributes.HasFlag(SymbolAttributes.ParameterType), isDeclaringTypeIncluded: false);
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.MemberConstructor)
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
        //            isGenericTypeDefinition = valueType.IsGenericTypeDefinition;
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
        //            .AppendDisplayNameInternal(parameter.ParameterType, isFullyQualifiedName, isDeclaringTypeIncluded: false)
        //            .Append(' ')
        //            .Append(parameter.EventName)
        //            .Append(HelperExtensionsCommon.ParameterSeparator);
        //        }

        //        // Remove trailing comma and whitespace
        //        _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.MemberConstructor)
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
        //        signatureNameBuilder = signatureNameBuilder.AppendInheritanceSignature(valueType, isFullyQualifiedName);
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.Generic))
        //      {
        //        ParameterType[] genericTypeParameterDefinitions = ParameterType.EmptyTypes;
        //        if (memberAttributes.HasFlag(SymbolAttributes.GenericType) && valueType.IsGenericTypeDefinition)
        //        {
        //          genericTypeParameterDefinitions = valueType.GetGenericTypeDefinition().GetGenericArguments();
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
        //    private static StringBuilder AppendInheritanceSignature(this StringBuilder memberNameBuilder, ParameterType typeData, bool isFullyQualified)
        //    {
        //      bool isDelegate = HelperExtensionsCommon.DelegateType.IsAssignableFrom(typeData);
        //      if (isDelegate)
        //      {
        //        return memberNameBuilder;
        //      }

        //      bool isSubclass = typeData.BaseType != typeof(object);
        //      ParameterType[] interfaces = typeData.GetInterfaces();
        //      bool hasInterfaces = interfaces.Length > 0;
        //      if (isSubclass || hasInterfaces)
        //      {
        //        _ = memberNameBuilder.Append(" : ");
        //      }

        //      if (isSubclass)
        //      {
        //        _ = memberNameBuilder.Append(isFullyQualified ? typeData.BaseType.FullName : typeData.BaseType.EventName)
        //          .Append(HelperExtensionsCommon.ParameterSeparator);
        //      }

        //      foreach (ParameterType interfaceData in interfaces)
        //      {
        //        _ = memberNameBuilder.Append(isFullyQualified ? interfaceData.FullName : interfaceData.EventName)
        //          .Append(HelperExtensionsCommon.ParameterSeparator);
        //      }

        //      if (isSubclass || hasInterfaces)
        //      {
        //        _ = memberNameBuilder.Remove(memberNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
        //      }

        //      return memberNameBuilder;
        //    }
        //    private static StringBuilder AppendGenericTypeConstraints(this StringBuilder constraintBuilder, ParameterType[] genericTypeDefinitionsData, bool isFullyQualified, bool isCompact)
        //    {
        //      bool hasSingleNewLine = false;
        //      for (int genericTypeArgumentIndex = 0; genericTypeArgumentIndex < genericTypeDefinitionsData.Length; genericTypeArgumentIndex++)
        //      {
        //        ParameterType genericTypeDefinitionData = genericTypeDefinitionsData[genericTypeArgumentIndex];
        //        ParameterType[] constraints = genericTypeDefinitionData.GetGenericParameterConstraints();
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
        //          .Append(genericTypeDefinitionData.EventName)
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

        //        foreach (ParameterType constraint in constraints)
        //        {
        //          _ = constraintBuilder.AppendDisplayNameInternal(constraint, isFullyQualified, isDeclaringTypeIncluded: false)
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

        /// <summary>
        /// Generates a formatted signature string for the specified property, including modifiers, type, name,
        /// accessors, and optional custom attributes.
        /// </summary>
        /// <remarks>The generated signature reflects the property's access level,
        /// static/abstract/virtual/override modifiers, type (with generic arguments if applicable), indexer parameters
        /// (if any), and accessor visibility. When isCompact is true or isRuntimeSymbol is true, custom attributes are
        /// omitted for brevity or runtime compatibility. Use this method to display or analyze property signatures in
        /// code generation, documentation, or tooling scenarios.</remarks>
        /// <param name="propertyData">The property metadata to generate the signature for. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to use fully qualified type names in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the property name; otherwise, false.</param>
        /// <param name="isCompact">true to produce a compact signature without custom attributes or extra formatting; otherwise, false.</param>
        /// <param name="isRuntimeSymbol">true if the property represents a runtime symbol and should be formatted accordingly; otherwise, false.</param>
        /// <returns>A string containing the complete signature of the property, including modifiers, type, name, accessors, and
        /// any applicable custom attributes.</returns>
        internal static string ToSignatureNameInternal(PropertyData propertyData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact, bool isRuntimeSymbol)
        {
            SymbolAttributes symbolAttributes = propertyData.SymbolAttributes;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

            if (!(isRuntimeSymbol || isCompact))
            {
                IEnumerable<CustomAttributeData> customAttributesData = propertyData.AttributeData;

                if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                {
                    customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != typeof(IsReadOnlyAttribute));
                }

                _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);
            }

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

            TypeData propertyTypeData = propertyData.PropertyTypeData;
            if (!isRuntimeSymbol && propertyTypeData.IsGenericType && !propertyTypeData.IsGenericTypeDefinition)
            {
                propertyTypeData = propertyTypeData.GenericTypeDefinitionData;
            }

            // SetValue return valueType
            _ = signatureNameBuilder.AppendDisplayNameInternal(propertyTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
              .Append(' ');

            // MemberData name
            if (symbolAttributes.HasFlag(SymbolAttributes.IndexerProperty))
            {
                _ = signatureNameBuilder.Append("this")
                  .Append('[');

                ParameterData[] parameters = propertyData.IndexerParameters;
                if (parameters.Any())
                {
                    foreach (ParameterData parameter in parameters)
                    {
                        IList<CustomAttributeData> attributes = parameter.AttributeData;
                        _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false)
                          .AppendDisplayNameInternal(parameter.ParameterTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
                          .Append(' ')
                          .Append(parameter.Name)
                          .Append(SymbolSignatureGenerator.ParameterSeparator);
                    }

                    // Remove trailing comma and whitespace
                    _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - SymbolSignatureGenerator.ParameterSeparator.Length, SymbolSignatureGenerator.ParameterSeparator.Length)
                      .Append(']');
                }
            }
            else
            {
                _ = signatureNameBuilder.AppendDisplayNameInternal(propertyData, isFullyQualifiedName, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded);
            }

            _ = signatureNameBuilder
              .Append(' ')
              .Append('{')
              .Append(' ');

            if (propertyData.CanRead)
            {
                if (SymbolSignatureGenerator.AccessModifierComparer.Compare(propertyData.GetAccessorAccessModifier, propertyData.AccessModifier) < 0)
                {
                    _ = signatureNameBuilder
                      .Append(propertyData.GetAccessorAccessModifier.ToDisplayStringValue())
                      .Append(' ');
                }

                _ = signatureNameBuilder
                  .Append("get")
                  .Append(SymbolSignatureGenerator.ExpressionTerminator)
                  .Append(' ');
            }

            if (propertyData.CanWrite)
            {
                if (SymbolSignatureGenerator.AccessModifierComparer.Compare(propertyData.SetAccessorAccessModifier, propertyData.AccessModifier) < 0)
                {
                    _ = signatureNameBuilder
                      .Append(propertyData.SetAccessorAccessModifier.ToDisplayStringValue())
                      .Append(' ');
                }

                if (propertyData.IsSetMethodReadOnly)
                {
                    _ = signatureNameBuilder
                      .Append("readonly")
                      .Append(' ');
                }

                if (propertyData.SymbolAttributes.HasFlag(SymbolAttributes.InitProperty))
                {
                    _ = signatureNameBuilder
                      .Append("init")
                      .Append(SymbolSignatureGenerator.ExpressionTerminator)
                      .Append(' ');
                }
                else
                {
                    _ = signatureNameBuilder
                    .Append("set")
                    .Append(SymbolSignatureGenerator.ExpressionTerminator)
                    .Append(' ');
                }
            }

            _ = signatureNameBuilder.Append('}');

            string fullMemberName = signatureNameBuilder.ToString();
            StringBuilderFactory.Recycle(signatureNameBuilder);

            return fullMemberName;
        }

        /// <summary>
        /// Generates a formatted method signature string based on the specified method metadata and formatting options.
        /// </summary>
        /// <remarks>The generated signature reflects the specified formatting options and may include
        /// custom attributes, access modifiers, and generic type constraints depending on the provided parameters. This
        /// method does not validate the input metadata; callers should ensure that the provided MethodData is
        /// valid.</remarks>
        /// <param name="methodData">The metadata describing the method for which to generate the signature. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to include fully qualified type names in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the method signature; otherwise, false.</param>
        /// <param name="isCompact">true to generate a compact signature without custom attributes or generic constraints; otherwise, false.</param>
        /// <param name="isRuntimeSymbol">true if the signature should be generated for a runtime symbol; otherwise, false.</param>
        /// <returns>A string representing the formatted method signature, including modifiers, return type, method name,
        /// parameters, and, if applicable, custom attributes and generic constraints.</returns>
        internal static string ToSignatureNameInternal(MethodData methodData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact, bool isRuntimeSymbol)
        {
            Debug.WriteLine($"Generating method signature");

            SymbolComponentInfo symbolComponents = null;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();
            SymbolAttributes symbolAttributes = methodData.SymbolAttributes;

            if (!isRuntimeSymbol)
            {
                if (methodData.IsGenericMethod && !methodData.IsGenericMethodDefinition)
                {
                    methodData = methodData.GenericMethodDefinitionData;
                }

                if (!isCompact)
                {
                    IEnumerable<CustomAttributeData> customAttributesData = methodData.AttributeData;

                    if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                    {
                        customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType);
                    }

                    _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);
                }
            }

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
                symbolComponents.AddModifier("sealed");
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

            if (methodData.IsReturnValueReadOnly)
            {
                _ = signatureNameBuilder
                  .Append("readonly")
                  .Append(' ');
            }

            TypeData returnTypeData = methodData.ReturnTypeData;
            //if (!isRuntimeSymbol && returnTypeData.IsGenericType && !returnTypeData.IsGenericTypeDefinition)
            //{
            //    returnTypeData = returnTypeData.GenericTypeDefinitionData;
            //}

            _ = signatureNameBuilder.AppendDisplayNameInternal(returnTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
              .Append(' ');

            // MemberData name
            _ = signatureNameBuilder.AppendDisplayNameInternal(methodData, isFullyQualifiedName, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded)
              .Append('(');

            if (methodData.IsExtensionMethod)
            {
                _ = signatureNameBuilder
                  .Append("this")
                  .Append(' ');
            }

            ParameterList parameters = methodData.Parameters;
            if (parameters.HasItems)
            {
                for (int parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++)
                {
                    ParameterData parameterData = parameters[parameterIndex];

                    if (!isRuntimeSymbol)
                    {
                        IList<CustomAttributeData> attributes = parameterData.AttributeData;
                        _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false);
                    }

                    if (parameterData.IsRefReadOnly)
                    {
                        _ = signatureNameBuilder.Append("ref readonly ");
                    }
                    else if (parameterData.IsRef)
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
                      .AppendDisplayNameInternal(parameterData.ParameterTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
                      .Append(' ')
                      .Append(parameterData.Name)
                      .Append(SymbolSignatureGenerator.ParameterSeparator);
                }

                // Remove trailing comma and whitespace
                _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - SymbolSignatureGenerator.ParameterSeparator.Length, SymbolSignatureGenerator.ParameterSeparator.Length);
            }

            _ = signatureNameBuilder.Append(')');

            if (!isCompact && !isRuntimeSymbol)
            {
                TypeData[] genericTypeParameterDefinitions = methodData.GenericMethodArguments;
                if (genericTypeParameterDefinitions.Length > 0)
                {
                    _ = signatureNameBuilder
                      .AppendGenericTypeConstraints(genericTypeParameterDefinitions, isFullyQualifiedName, isSingleLine: false, methodData.IndentationString);
                }
            }

            _ = signatureNameBuilder.Append(SymbolSignatureGenerator.ExpressionTerminator);

            string fullMemberName = signatureNameBuilder.ToString();
            StringBuilderFactory.Recycle(signatureNameBuilder);

            return fullMemberName;
        }

        /// <summary>
        /// Generates the signature name string for the specified event, using the provided formatting and inclusion
        /// options.
        /// </summary>
        /// <param name="eventData">The event metadata to use when constructing the signature name. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to include the fully qualified type names in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the event signature; otherwise, false.</param>
        /// <param name="isCompact">true to generate a compact signature without custom attributes; otherwise, false.</param>
        /// <param name="isRuntimeSymbol">true if the signature is being generated for a runtime symbol, which affects formatting and attribute
        /// inclusion; otherwise, false.</param>
        /// <returns>A string representing the formatted signature name of the event, including modifiers, type, and name as
        /// specified by the input parameters.</returns>
        internal static string ToSignatureNameInternal(EventData eventData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact, bool isRuntimeSymbol)
        {
            SymbolAttributes symbolAttributes = eventData.SymbolAttributes;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();
            if (!(isRuntimeSymbol || isCompact))
            {
                IEnumerable<CustomAttributeData> customAttributesData = eventData.AttributeData;

                if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                {
                    customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType);
                }

                _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);
            }

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

            TypeData eventHandlerTypeData = eventData.EventHandlerTypeData;
            if (!isRuntimeSymbol && eventHandlerTypeData.IsGenericType && !eventHandlerTypeData.IsGenericTypeDefinition)
            {
                eventHandlerTypeData = eventHandlerTypeData.GenericTypeDefinitionData;
            }

            _ = signatureNameBuilder
              .Append("event")
              .Append(' ')
              .AppendDisplayNameInternal(eventHandlerTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
              .Append(' ');

            _ = signatureNameBuilder.AppendDisplayNameInternal(eventData, isFullyQualifiedName, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded)
              .Append(SymbolSignatureGenerator.ExpressionTerminator);

            string fullMemberName = signatureNameBuilder.ToString();
            StringBuilderFactory.Recycle(signatureNameBuilder);

            return fullMemberName;
        }

        /// <summary>
        /// Generates the formatted signature name for a field, including modifiers, type, and name, based on the
        /// specified formatting options.
        /// </summary>
        /// <remarks>This method is intended for internal use when constructing field signature
        /// representations for display or analysis. The output format may vary depending on the combination of
        /// formatting flags provided.</remarks>
        /// <param name="fieldData">The metadata describing the field for which to generate the signature name.</param>
        /// <param name="isFullyQualifiedName">true to include the fully qualified type name in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the field's signature; otherwise, false.</param>
        /// <param name="isCompact">true to use a compact format that omits custom attributes and some modifiers; otherwise, false.</param>
        /// <param name="isRuntimeSymbol">true if the signature is being generated for a runtime symbol, which may affect formatting; otherwise,
        /// false.</param>
        /// <returns>A string containing the formatted signature name of the field, including modifiers, type, and name,
        /// according to the specified options.</returns>
        internal static string ToSignatureNameInternal(FieldData fieldData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact, bool isRuntimeSymbol)
        {
            SymbolAttributes symbolAttributes = fieldData.SymbolAttributes;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

            if (!(isRuntimeSymbol || isCompact))
            {
                IEnumerable<CustomAttributeData> customAttributesData = fieldData.AttributeData;

                if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                {
                    customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
                }

                _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);
            }

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

            TypeData fieldTypeData = fieldData.FieldTypeData;
            if (!isRuntimeSymbol && fieldTypeData.IsGenericType && !fieldTypeData.IsGenericTypeDefinition)
            {
                fieldTypeData = fieldTypeData.GenericTypeDefinitionData;
            }

            _ = signatureNameBuilder.AppendDisplayNameInternal(fieldTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
              .Append(' ');

            _ = signatureNameBuilder.AppendDisplayNameInternal(fieldData, isFullyQualifiedName, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded)
              .Append(SymbolSignatureGenerator.ExpressionTerminator);

            string fullMemberName = signatureNameBuilder.ToString();
            StringBuilderFactory.Recycle(signatureNameBuilder);

            return fullMemberName;
        }

        /// <summary>
        /// Generates the signature name for a type, including modifiers, attributes, and type parameters, based on the
        /// specified formatting options.
        /// </summary>
        /// <remarks>When generating signatures for delegates, the return type and parameter list are
        /// included. For generic types, type parameters and constraints are appended unless compact formatting is
        /// requested. Attribute and inheritance information is omitted in compact or runtime symbol mode.</remarks>
        /// <param name="typeData">The type metadata used to construct the signature name.</param>
        /// <param name="isFullyQualifiedName">true to use fully qualified type names in the signature; otherwise, false.</param>
        /// <param name="isCompact">true to produce a compact signature without attributes or inheritance information; otherwise, false.</param>
        /// <param name="isRuntimeSymbol">true if the signature is being generated for a runtime symbol; otherwise, false.</param>
        /// <returns>A string representing the formatted signature name of the specified type, including modifiers, attributes,
        /// and type parameters as determined by the input options.</returns>
        internal static string ToSignatureNameInternal(TypeData typeData, bool isFullyQualifiedName, bool isCompact, bool isRuntimeSymbol)
        {
            SymbolAttributes symbolAttributes = typeData.SymbolAttributes;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();
            if (!isRuntimeSymbol)
            {
                if (typeData.IsGenericType && !typeData.IsGenericTypeDefinition)
                {
                    typeData = typeData.GenericTypeDefinitionData;
                }

                if (!isCompact)
                {
                    IEnumerable<CustomAttributeData> customAttributesData = typeData.AttributeData;

                    if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                    {
                        customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
                    }

                    _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);
                }
            }

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

            // SetValue return valueType
            if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
            {
                delegateInvocatorData = typeData.DelegateInvokeMethodData;
                TypeData delegateReturnTypeData = delegateInvocatorData.ReturnTypeData;
                if (!isRuntimeSymbol && delegateReturnTypeData.IsGenericType && !delegateReturnTypeData.IsGenericTypeDefinition)
                {
                    delegateReturnTypeData = delegateReturnTypeData.GenericTypeDefinitionData;
                }

                _ = signatureNameBuilder.AppendDisplayNameInternal(delegateReturnTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
                  .Append(' ');
            }

            // ParameterType name
            _ = signatureNameBuilder.AppendDisplayNameInternal(typeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true);

            if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
            {
                _ = signatureNameBuilder.Append('(');

                ParameterData[] parameters = delegateInvocatorData.Parameters;
                if (parameters.Length > 0)
                {
                    foreach (ParameterData parameterData in parameters)
                    {
                        if (!isRuntimeSymbol)
                        {
                            IList<CustomAttributeData> attributes = parameterData.AttributeData;
                            _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false);
                        }

                        if (parameterData.IsRefReadOnly)
                        {
                            _ = signatureNameBuilder.Append("ref readonly ");
                        }
                        else if (parameterData.IsRef)
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
                          .AppendDisplayNameInternal(parameterData.ParameterTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
                          .Append(' ')
                          .Append(parameterData.Name)
                          .Append(SymbolSignatureGenerator.ParameterSeparator);
                    }

                    // Remove trailing comma and whitespace
                    _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - SymbolSignatureGenerator.ParameterSeparator.Length, SymbolSignatureGenerator.ParameterSeparator.Length)
                      .Append(')');
                }
            }
            else if (!isCompact && symbolAttributes.HasFlag(SymbolAttributes.Class))
            {
                signatureNameBuilder = signatureNameBuilder.AppendInheritanceSignature(typeData, isFullyQualifiedName);
            }

            if (!isCompact && !isRuntimeSymbol)
            {
                TypeData[] genericTypeParameterDefinitions = typeData.GenericTypeArguments;
                if (genericTypeParameterDefinitions.Length > 0)
                {
                    _ = signatureNameBuilder
                      .Append(' ')
                      .AppendGenericTypeConstraints(genericTypeParameterDefinitions, isFullyQualifiedName, isSingleLine: false, typeData.IndentationString);
                }
            }

            if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
            {
                _ = signatureNameBuilder.Append(SymbolSignatureGenerator.ExpressionTerminator);
            }

            string fullMemberName = signatureNameBuilder.ToString();
            StringBuilderFactory.Recycle(signatureNameBuilder);

            return fullMemberName;
        }

        /// <summary>
        /// Generates the formatted signature name for a constructor based on the specified formatting options.
        /// </summary>
        /// <remarks>Custom attributes and certain modifiers are included or omitted in the signature
        /// based on the values of isCompact and isRuntimeSymbol. This method is intended for internal use when
        /// generating display names for constructors in various contexts.</remarks>
        /// <param name="constructorData">The metadata describing the constructor, including its parameters, attributes, and access modifiers.</param>
        /// <param name="isFullyQualifiedName">true to include the fully qualified type name in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the signature; otherwise, false.</param>
        /// <param name="isCompact">true to generate a compact signature without custom attributes; otherwise, false.</param>
        /// <param name="isRuntimeSymbol">true to format the signature for runtime symbol representation, omitting custom attributes and certain
        /// modifiers; otherwise, false.</param>
        /// <returns>A string containing the formatted constructor signature according to the specified options.</returns>
        internal static string ToSignatureNameInternal(ConstructorData constructorData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact, bool isRuntimeSymbol)
        {
            SymbolAttributes symbolAttributes = constructorData.SymbolAttributes;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

            if (!(isRuntimeSymbol || isCompact))
            {
                IEnumerable<CustomAttributeData> customAttributesData = constructorData.AttributeData;

                if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                {
                    customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType);
                }

                _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);
            }

            AccessModifier accessModifier = constructorData.AccessModifier;
            if (accessModifier is not AccessModifier.Undefined)
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

            // MemberData name
            _ = signatureNameBuilder.AppendDisplayNameInternal(constructorData, isFullyQualifiedName, isGenericTypeParameterIncluded: false, isDeclaringTypeIncluded)
              .Append('(');

            ParameterData[] parameters = constructorData.Parameters;
            if (parameters.Length > 0)
            {
                foreach (ParameterData parameterData in parameters)
                {
                    if (!isRuntimeSymbol)
                    {
                        IList<CustomAttributeData> attributes = parameterData.AttributeData;
                        _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false);
                    }

                    if (parameterData.IsRefReadOnly)
                    {
                        _ = signatureNameBuilder.Append("ref readonly ");
                    }
                    else if (parameterData.IsRef)
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
                      .AppendDisplayNameInternal(parameterData.ParameterTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
                      .Append(' ')
                      .Append(parameterData.Name)
                      .Append(SymbolSignatureGenerator.ParameterSeparator);
                }

                // Remove trailing comma and whitespace
                _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - SymbolSignatureGenerator.ParameterSeparator.Length, SymbolSignatureGenerator.ParameterSeparator.Length);
            }

            _ = signatureNameBuilder
                  .Append(')')
                  .Append(SymbolSignatureGenerator.ExpressionTerminator);

            string fullMemberName = signatureNameBuilder.ToString();
            StringBuilderFactory.Recycle(signatureNameBuilder);

            return fullMemberName;
        }
    }
}
