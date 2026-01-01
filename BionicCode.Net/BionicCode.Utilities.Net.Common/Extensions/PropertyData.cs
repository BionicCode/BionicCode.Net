[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BionicCode.Utilities.Net.Profiling")]
namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Microsoft.CodeAnalysis;

    internal sealed class PropertyData : MemberInfoData, IPropertyDataInvoker
    {
        private string? displayName;
        private string? shortDisplayName;
        private string? fullyQualifiedDisplayName;
        private string? signature;
        private string? shortSignature;
        private string? shortCompactSignature;
        private string? fullyQualifiedSignature;
        private string? fullyQualifiedRuntimeSignature;
        private string? runtimeSignature;
        private string? runtimeShortSignature;
        private string? runtimeShortCompactSignature;
        private SymbolAttributes symbolAttributes;
        private AccessModifier propertyAccessModifier;
        private AccessModifier setAccessorAccessModifier;
        private AccessModifier getAccessorAccessModifier;
        private ParameterList? indexerParameters;
        private TypeData? propertyTypeData;
        private MethodData? getMethodData;
        private MethodData? setMethodData;
        private bool? isStatic;
        private bool? isOverride;
        private bool? isSealed;
        private bool? canWrite;
        private bool? canRead;
        private bool? isInit;
        private bool? _isPublic;
        private bool? _isPrivate;
        private bool? _isAssembly;
        private bool? _isFamily;
        private bool? _isFamilyOrAssembly;
        private bool? _isFamilyAndAssembly;
        private Func<object?, object[], object?>? _indexerPropertyGetInvoker;
        private Action<object?, object[], object?>? _indexerPropertySetInvoker;
        private Func<object?, object?>? _propertyGetInvoker;
        private Action<object?, object?>? _propertySetInvoker;
        private readonly ConcurrentDictionary<RuntimeTypeHandle, Delegate> _valueTypeSetValueInvokerTable;
        private string? assemblyName;
        private SymbolComponentInfo? symbolComponentInfo;
        private bool? isSetMethodReadOnly;

        public PropertyData(PropertyInfo propertyInfo, SymbolInfoDataCacheKey symbolInfoDataCacheKey) : base(propertyInfo, symbolInfoDataCacheKey)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            this._valueTypeSetValueInvokerTable = new ConcurrentDictionary<RuntimeTypeHandle, Delegate>();
            this.PropertyInfo = propertyInfo;
        }

        public PropertyInfo GetPropertyInfo()
          => this.PropertyInfo;

        protected override MemberInfo GetMemberInfo()
          => GetPropertyInfo();

        public object? GetValue(object? target, object[]? indexerPropertyIndex = null)
        {
            if (!this.CanRead)
            {
                throw new InvalidOperationException($"The property '{this.FullyQualifiedSignature}' does not have a getter.");
            }

            if (this.DeclaringTypeData.IsStruct)
            {
                throw new InvalidOperationException($"Use the '{nameof(SetStructValue)}' method to set property values on struct types.");
            }

            if (this.IsIndexer)
            {
                ArgumentNullExceptionAdvanced.ThrowIfNull(indexerPropertyIndex, nameof(indexerPropertyIndex), "Indexer property index cannot be null for indexer properties.");
                ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(
                    indexerPropertyIndex!.Length,
                    this.IndexerParameters.Count,
                    nameof(indexerPropertyIndex),
                    $"Indexer property index count does not match the indexer parameter count of property '{this.FullyQualifiedSignature}'.");
            }

            if (!this.IsStatic)
            {
                if (target is null)
                {
                    throw new ArgumentNullException(nameof(target), "Target object cannot be null for instance properties.");
                }

                Type targetType = target.GetType();
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                    targetType,
                    this.DeclaringTypeData.UnwrapType(),
                    nameof(target),
                    $"Type mismatch. Reason: The instance type {targetType.ToFullyQualifiedSignatureName()} is not assignable to {this.DeclaringTypeData.FullyQualifiedSignature}");
            }

            object? invocationTarget = this.IsStatic
                ? null
                : target;
            if (this.IsIndexer)
            {
                Func<object?, object[], object?> propertyGetInvoker = GetIndexerGetInvokerInternal();
                return propertyGetInvoker.Invoke(invocationTarget, indexerPropertyIndex!);
            }
            else
            {
                Func<object?, object?> propertySetInvoker = GetGetInvokerInternal();
                return propertySetInvoker.Invoke(target);
            }
        }

        public void SetValue(object? target, object? value, object[]? indexerPropertyIndex = null)
        {
            if (this.IsReadOnly)
            {
                throw new InvalidOperationException($"The property '{this.FullyQualifiedSignature}' does not have a setter.");
            }

            if (this.DeclaringTypeData.IsStruct)
            {
                throw new InvalidOperationException($"Use the '{nameof(SetStructValue)}' method to set property values on struct types.");
            }

            if (this.IsIndexer)
            {
                ArgumentNullExceptionAdvanced.ThrowIfNull(indexerPropertyIndex, nameof(indexerPropertyIndex), "Indexer property index cannot be null for indexer properties.");
                ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(
                    indexerPropertyIndex!.Length,
                    this.IndexerParameters.Count,
                    nameof(indexerPropertyIndex),
                    $"Indexer property index count does not match the indexer parameter count of property '{this.FullyQualifiedSignature}'.");
            }

            if (!this.IsStatic)
            {
                if (target is null)
                {
                    throw new ArgumentNullException(nameof(target), "Target object cannot be null for instance properties.");
                }

                Type targetType = target.GetType();
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                    targetType,
                    this.DeclaringTypeData.UnwrapType(),
                    nameof(target),
                    $"Type mismatch. Reason: The instance type {targetType.ToFullyQualifiedSignatureName()} is not assignable to {this.DeclaringTypeData.FullyQualifiedSignature}");
            }

            object? invocationTarget = this.IsStatic
                ? null
                : target;
            if (this.IsIndexer)
            {
                Action<object?, object[], object?> propertySetInvoker = GetIndexerSetInvokerInternal();
                propertySetInvoker(invocationTarget, indexerPropertyIndex!, value);
            }
            else
            {
                Action<object?, object?> propertySetInvoker = GetSetInvokerInternal();
                propertySetInvoker(invocationTarget, value);
            }
        }

        public void SetStructValue<TTarget, TValue>(ref TTarget target, TValue value, object[]? indexerPropertyIndex = null) where TTarget : struct
        {
            if (this.IsReadOnly)
            {
                throw new InvalidOperationException($"The property '{this.FullyQualifiedSignature}' does not have a setter.");
            }

            if (!this.DeclaringTypeData.IsStruct)
            {
                throw new InvalidOperationException($"Use the '{nameof(SetValue)}' method to set property values on reference types.");
            }

            if (this.IsStatic)
            {
                throw new InvalidOperationException($"Setting static property values on struct types is not supported by this method. Call '{nameof(SetValue)}' instead.");
            }

            if (this.IsIndexer)
            {
                ArgumentNullExceptionAdvanced.ThrowIfNull(indexerPropertyIndex, nameof(indexerPropertyIndex), "Indexer property index cannot be null for indexer properties.");
                ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(
                    indexerPropertyIndex!.Length,
                    this.IndexerParameters.Count,
                    nameof(indexerPropertyIndex),
                    $"Indexer property index count does not match the indexer parameter count of property '{this.FullyQualifiedSignature}'.");
            }

            Type targetType = typeof(TTarget);
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                this.DeclaringTypeData.UnwrapType(),
                nameof(target),
                $"Type mismatch. Reason: The instance type {targetType.ToFullyQualifiedSignatureName()} is not assignable to {this.DeclaringTypeData.FullyQualifiedSignature}");

            if (this.IsIndexer)
            {
                ValueTypeIndexerPropertySetter<TTarget, TValue> propertySetInvoker = GetStructIndexerSetInvokerInternal<TTarget, TValue>();
                propertySetInvoker.Invoke(ref target, indexerPropertyIndex!, value);
            }
            else
            {
                ValueTypeMemberSetter<TTarget, TValue> propertySetInvoker = GetStructSetInvokerInternal<TTarget, TValue>();
                propertySetInvoker.Invoke(ref target, value);
            }
        }

        /// <summary>
        /// Gets a delegate that sets the value of the indexer property represented by this instance.
        /// </summary>
        /// <remarks>Use this method only for properties that are indexers. For non-indexer properties,
        /// use <see cref="GetSetInvoker"/> instead.</remarks>
        /// <returns>An <code>Action&lt;object?, object?[], object?&gt;</code> delegate that sets the value of the indexer property. The
        /// first parameter is the target object, the second parameter is an array of index values, and the third
        /// parameter is the value to set.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the property represented by this instance is not an indexer property.</exception>
        public Action<object?, object[], object?> GetIndexerSetInvoker()
        {
            if (!this.IsIndexer)
            {
                throw new InvalidOperationException($"The property '{this.Signature}' is not an indexer property. Use {nameof(GetGetInvoker)} instead.");
            }

            if (this.IsReadOnly)
            {
                throw new InvalidOperationException($"The property '{this.Signature}' does not have a setter.");
            }

            return GetIndexerSetInvokerInternal();
        }

        private Action<object?, object[], object?> GetIndexerSetInvokerInternal()
            => this._indexerPropertySetInvoker ??= DelegateProvider.CreateIndexerSetter(this);

        /// <summary>
        /// Gets a delegate that sets the value of an indexer property on a struct type for the specified target and
        /// value types.
        /// </summary>
        /// <remarks>Use this method when you need to set the value of an indexer property on a value type
        /// (struct) using a strongly typed delegate. For non-indexer properties or properties declared on reference
        /// types, use the appropriate alternative methods.</remarks>
        /// <typeparam name="TTarget">The struct type that declares the indexer property.</typeparam>
        /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
        /// <returns>A delegate that sets the value of the indexer property on the specified struct type.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the property is not an indexer property, or if the property is not declared on a struct type.</exception>
        public ValueTypeIndexerPropertySetter<TTarget, TValue> GetStructIndexerSetInvoker<TTarget, TValue>() where TTarget : struct
        {
            if (!this.IsIndexer)
            {
                throw new InvalidOperationException($"The property '{this.Signature}' is not an indexer property. Use {nameof(GetStructSetInvoker)} instead.");
            }

            if (this.IsReadOnly)
            {
                throw new InvalidOperationException($"The property '{this.Signature}' does not have a setter.");
            }

            if (!this.DeclaringTypeData.IsStruct)
            {
                throw new InvalidOperationException($"The property '{this.Signature}' is not declared on a struct type. Use {nameof(GetSetInvoker)} instead.");
            }

            return GetStructIndexerSetInvokerInternal<TTarget, TValue>();
        }

        private ValueTypeIndexerPropertySetter<TTarget, TValue> GetStructIndexerSetInvokerInternal<TTarget, TValue>() where TTarget : struct
        {
            RuntimeTypeHandle targetTypeHandle = typeof(TTarget).TypeHandle;
            Delegate? cachedInvoker = this._valueTypeSetValueInvokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.CreateStructIndexerSetter<TTarget, TValue>(this));
            ValueTypeIndexerPropertySetter<TTarget, TValue> invoker = (ValueTypeIndexerPropertySetter<TTarget, TValue>)cachedInvoker;

            return invoker;
        }

        /// <summary>
        /// Gets a delegate that sets the value of this property on a struct of the specified type.
        /// </summary>
        /// <typeparam name="TTarget">The struct type that declares the property.</typeparam>
        /// <typeparam name="TValue">The type of the value to set on the property.</typeparam>
        /// <returns>A delegate that sets the value of the property on a struct of type TTarget.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the property is an indexer or is not declared on a struct type.</exception>
        public ValueTypeMemberSetter<TTarget, TValue> GetStructSetInvoker<TTarget, TValue>() where TTarget : struct
        {
            if (this.IsIndexer)
            {
                throw new InvalidOperationException($"The property '{this.FullyQualifiedSignature}' is an indexer property. Use {nameof(GetIndexerSetInvoker)} instead.");
            }

            if (this.IsReadOnly)
            {
                throw new InvalidOperationException($"The property '{this.FullyQualifiedSignature}' does not have a setter.");
            }

            if (!this.DeclaringTypeData.IsStruct)
            {
                throw new InvalidOperationException($"The property '{this.FullyQualifiedSignature}' is not declared on a struct type. Use {nameof(GetIndexerSetInvoker)} instead.");
            }

            return GetStructSetInvokerInternal<TTarget, TValue>();
        }

        private ValueTypeMemberSetter<TTarget, TValue> GetStructSetInvokerInternal<TTarget, TValue>() where TTarget : struct
        {
            RuntimeTypeHandle targetTypeHandle = typeof(TTarget).TypeHandle;
            Delegate? cachedInvoker = this._valueTypeSetValueInvokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.CreateStructSetter<TTarget, TValue>(this));
            ValueTypeMemberSetter<TTarget, TValue> invoker = (ValueTypeMemberSetter<TTarget, TValue>)cachedInvoker;

            return invoker;
        }

        /// <summary>
        /// Creates a delegate that sets the value of the property represented by this instance.
        /// </summary>
        /// <remarks>The returned delegate can be used to set the value of the property on a specified
        /// object instance. This method does not support indexer properties.</remarks>
        /// <returns>An <see cref="Action{Object, Object}"/> delegate that sets the property value. The first parameter is the
        /// target object; the second parameter is the value to assign to the property.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the property is an indexer. Use <see cref="GetIndexerSetInvoker"/> to obtain a setter for indexer
        /// properties.</exception>
        public Action<object?, object?> GetSetInvoker()
        {
            if (this.IsIndexer)
            {
                throw new InvalidOperationException($"The property '{this.FullyQualifiedSignature}' is an indexer property. Use '{nameof(GetIndexerSetInvoker)}' instead.");
            }

            if (this.IsReadOnly)
            {
                throw new InvalidOperationException($"The property '{this.FullyQualifiedSignature}' does not have a setter.");
            }

            return GetSetInvokerInternal();
        }

        private Action<object?, object?> GetSetInvokerInternal()
            => this._propertySetInvoker ??= DelegateProvider.CreateSetter(this);

        /// <summary>
        /// Creates a delegate that retrieves the value of the property represented by this instance.
        /// </summary>
        /// <remarks>The returned delegate can be used to efficiently access the property value without
        /// using reflection for each invocation. The instance parameter must be of the declaring type or a compatible
        /// type; otherwise, an exception may be thrown at invocation time.</remarks>
        /// <returns>A delegate that takes an object instance and returns the value of the property. The delegate returns null if
        /// the property value is null.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the property is an indexer. Use GetIndexerGetInvoker instead for indexer properties.</exception>
        public Func<object?, object?> GetGetInvoker()
        {
            if (this.IsIndexer)
            {
                throw new InvalidOperationException($"The property '{this.FullyQualifiedSignature}' is an indexer property. Use '{nameof(GetIndexerGetInvoker)}' instead.");
            }

            if (!this.CanRead)
            {
                throw new InvalidOperationException($"The property '{this.FullyQualifiedSignature}' does not have a getter.");
            }

            return GetGetInvokerInternal();
        }

        private Func<object?, object?> GetGetInvokerInternal()
            => this._propertyGetInvoker ??= DelegateProvider.CreateGetter(this);

        /// <summary>
        /// Retrieves a delegate that gets the value of the indexer property represented by this instance.
        /// </summary>
        /// <remarks>Use this method to obtain a strongly-typed accessor for indexer properties. For
        /// non-indexer properties, use GetGetInvoker instead.</remarks>
        /// <returns>A delegate that takes a target object and an array of index values, and returns the value of the indexer
        /// property.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the property represented by this instance is not an indexer property.</exception>
        public Func<object?, object[], object?> GetIndexerGetInvoker()
        {
            if (!this.IsIndexer)
            {
                throw new InvalidOperationException($"The property '{this.FullyQualifiedSignature}' is not an indexer property. Use {nameof(GetGetInvoker)} instead.");
            }

            if (!this.CanRead)
            {
                throw new InvalidOperationException($"The property '{this.FullyQualifiedSignature}' does not have a getter.");
            }

            return GetIndexerGetInvokerInternal();
        }

        private Func<object?, object[], object?> GetIndexerGetInvokerInternal()
            => this._indexerPropertyGetInvoker ??= DelegateProvider.CreateIndexerGetter(this);

        private void GetAccessors()
        {
            (AccessModifier propertyModifier, AccessModifier getMethodModifier, AccessModifier setMethodModifier) = PropertyData.GetPropertyAccessModifier(this.GetMethodData, this.SetMethodData);
            this.propertyAccessModifier = propertyModifier;
            this.setAccessorAccessModifier = setMethodModifier;
            this.getAccessorAccessModifier = getMethodModifier;
        }

        public bool IsIndexer
          => this.IndexerParameters.HasItems;

        public ParameterList IndexerParameters
          => this.indexerParameters ??= ParameterListBuilder.Create(this.PropertyInfo.GetIndexParameters());

        public override AccessModifier AccessModifier
        {
            get
            {
                if (this.propertyAccessModifier is AccessModifier.Undefined)
                {
                    GetAccessors();
                }

                return this.propertyAccessModifier;
            }
        }

        public AccessModifier SetAccessorAccessModifier
        {
            get
            {
                if (this.setAccessorAccessModifier is AccessModifier.Undefined)
                {
                    GetAccessors();
                }

                return this.setAccessorAccessModifier;
            }
        }

        public AccessModifier GetAccessorAccessModifier
        {
            get
            {
                if (this.getAccessorAccessModifier is AccessModifier.Undefined)
                {
                    GetAccessors();
                }

                return this.getAccessorAccessModifier;
            }
        }

        public TypeData PropertyTypeData
          => this.propertyTypeData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetPropertyInfo().PropertyType);

        public PropertyInfo PropertyInfo { get; }

        //public static PropertyData TaskResultPropertyData
        //  => PropertyData._TaskResultPropertyData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeof(Task<>).GetProperty(nameof(Task<object>.Result)));

        //public static PropertyData ValueTaskResultPropertyData
        //  => PropertyData._ValueTaskResultPropertyData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeof(ValueTask<>).GetProperty(nameof(ValueTask<object>.Result)));

        public bool IsSealed
          => this.isSealed ??= (this.CanRead && this.GetMethodData!.IsSealed)
            || (this.CanWrite && this.SetMethodData!.IsSealed);

        public bool CanWrite
          => this.canWrite ??= GetPropertyInfo().CanWrite;

        public bool IsReadOnly
          => !this.CanWrite;

        public bool CanRead
          => this.canRead ??= GetPropertyInfo().CanRead;

        public bool IsInit
          => this.isInit ??= this.CanWrite && PropertyData.IsPropertyInit(this);

        public MethodData? GetMethodData
          => this.getMethodData ??= GetPropertyInfo() is PropertyInfo propertyInfo && propertyInfo.CanRead
                ? SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo.GetGetMethod(true)!)
                : null;

        public MethodData? SetMethodData
          => this.setMethodData ??= GetPropertyInfo() is PropertyInfo propertyInfo && propertyInfo.CanWrite
            ? SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo.GetSetMethod(true)!)
            : null;

        public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
          ? (this.symbolAttributes = PropertyData.GetAttributesInternal(this))
          : this.symbolAttributes;

        public override SymbolComponentInfo SymbolComponentInfo
          => this.symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

        public override string Signature
          => this.signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string ShortSignature
          => this.shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

        public override string ShortCompactSignature
          => this.shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

        public override string FullyQualifiedSignature
          => this.fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string FullyQualifiedRuntimeSignature
          => this.fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeSignature
          => this.runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortSignature
          => this.runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortCompactSignature
          => this.runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

        public override string DisplayName
          => this.displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string ShortDisplayName
          => this.shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

        public override string FullyQualifiedDisplayName
          => this.fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string AssemblyName
          => this.assemblyName ??= this.DeclaringTypeData.AssemblyName;

        public override bool IsStatic
          => this.isStatic ??= (this.CanRead && this.GetMethodData!.IsStatic)
            || (this.CanWrite && this.SetMethodData!.IsStatic);

        public bool IsSetMethodReadOnly
          => this.isSetMethodReadOnly ??= this.CanWrite && this.SetMethodData!.AttributeData.Any(data => data.AttributeType == typeof(IsReadOnlyAttribute));

        public bool IsOverride
          => this.isOverride ??= (this.CanRead && this.GetMethodData!.IsOverride)
            || (this.CanWrite && this.SetMethodData!.IsOverride);

        public override bool IsPublic
            => this._isPublic ??= this.AccessModifier == AccessModifier.Public;

        public override bool IsPrivate
            => this._isPrivate ??= this.AccessModifier == AccessModifier.Private;

        /// <summary>
        /// Gets a value indicating whether the member has internal accessibility within its assembly.
        /// </summary>
        public override bool IsAssembly
            => this._isAssembly ??= this.AccessModifier == AccessModifier.Internal;

        /// <summary>
        /// Gets a value indicating whether the member is protected and thus accessible only within its own class or by
        /// derived class instances.
        /// </summary>
        public override bool IsFamily
            => this._isFamily ??= this.AccessModifier == AccessModifier.Protected;

        public override bool IsFamilyOrAssembly
            => this._isFamilyOrAssembly ??= this.AccessModifier == AccessModifier.ProtectedInternal;

        public override bool IsFamilyAndAssembly
            => this._isFamilyAndAssembly ??= this.AccessModifier == AccessModifier.PrivateProtected;

        #region IPropertyDataInvoker

        bool IPropertyDataInvoker.IsInvocable
            => (this.CanRead && this._propertyGetInvoker is not null)
                || (this.CanWrite && this._propertySetInvoker is not null)
                || (this.IsIndexer && ((this.CanRead && this._indexerPropertyGetInvoker is not null)
                || (this.CanWrite && this._indexerPropertySetInvoker is not null)));

        bool IPropertyDataInvoker.HasGetter
            => this._indexerPropertyGetInvoker is not null || this._propertyGetInvoker is not null;

        bool IPropertyDataInvoker.HasSetter
            => this._indexerPropertySetInvoker is not null || this._propertySetInvoker is not null;

        bool IPropertyDataInvoker.IsIndexer
            => this.IsIndexer;

        #endregion IPropertyDataInvoker

        /// <summary>
        /// Determines the set of symbol attributes for the specified property based on its metadata and accessor
        /// methods.
        /// </summary>
        /// <remarks>For performance reasons avoid querying the attributes and prefer reading the particular property or properties.</remarks>
        /// <param name="propertyData">The metadata describing the property for which to retrieve symbol attributes. Cannot be null.</param>
        /// <returns>A bitwise combination of SymbolAttributes values that represent the characteristics of the property, such as
        /// whether it is static, abstract, virtual, an indexer, or has other modifiers.</returns>
        private static SymbolAttributes GetAttributesInternal(PropertyData propertyData)
        {
            SymbolAttributes propertyAttributes = propertyData.IsIndexer
              ? SymbolAttributes.IndexerProperty
              : SymbolAttributes.Property;

            MethodData? accessorData = propertyData.GetMethodData ?? propertyData.SetMethodData;
            if (accessorData is null)
            {
                return SymbolAttributes.Undefined;
            }

            if (!propertyData.CanWrite)
            {
                propertyAttributes |= SymbolAttributes.Final;
            }

            if (propertyData.IsInit)
            {
                propertyAttributes |= SymbolAttributes.Init;
            }

            if (accessorData.IsAbstract)
            {
                propertyAttributes |= SymbolAttributes.Abstract;
            }

            if (propertyData.IsStatic)
            {
                propertyAttributes |= SymbolAttributes.Static;
            }

            if (accessorData.IsVirtual)
            {
                propertyAttributes |= SymbolAttributes.Virtual;
            }

            if (propertyData.IsOverride)
            {
                propertyAttributes |= SymbolAttributes.Override;
            }

            return propertyAttributes;
        }

        private static bool IsPropertyInit(PropertyData propertyData)
        {
            if (propertyData.CanWrite)
            {
                Type[] requiredModifiers = propertyData.SetMethodData!.GetMethodInfo().ReturnParameter.GetRequiredCustomModifiers();
                if (requiredModifiers.Length > 0)
                {
                    return requiredModifiers.FirstOrDefault(type => type == typeof(IsExternalInit)) != default;
                }
            }

            return false;
        }

        private static (AccessModifier PropertyModifier, AccessModifier GetMethodModifier, AccessModifier SetMethodModifier) GetPropertyAccessModifier(MethodData? getMethodData, MethodData? setMethodData)
        {
            AccessModifier getMethodModifier = getMethodData?.AccessModifier ?? AccessModifier.Undefined;
            AccessModifier setMethodModifier = setMethodData?.AccessModifier ?? AccessModifier.Undefined;

            // Property accessors with the least restriction provides the access modifier for the property.
            AccessModifier propertyAccessModifier = (getMethodModifier, setMethodModifier) switch
            {
                (AccessModifier.Undefined, AccessModifier.Undefined) => AccessModifier.Undefined,
                (AccessModifier.Undefined, _) => setMethodModifier,
                (_, AccessModifier.Undefined) => getMethodModifier,
                _ => (AccessModifier)System.Math.Min((int)getMethodModifier, (int)setMethodModifier)
            };

            return (propertyAccessModifier, getMethodModifier, setMethodModifier);
        }
    }
}
