namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal abstract class MemberData : SymbolInfoData
    {
        private IList<CustomAttributeData> attributeData;
        private TypeData declaringTypeData;
        private RuntimeTypeHandle? _declaringTypeHandle;
        private string? _namespace;
        private BindingFlags? _bindingFlagsVisibilityMask;

        protected MemberData(MemberInfo memberInfo, SymbolKind symbolKind, SymbolReflectionInfoCacheKey symbolInfoDataCacheKey)
            : base(memberInfo.Name, symbolKind, symbolInfoDataCacheKey)
            => ArgumentNullException.ThrowIfNull(memberInfo, nameof(memberInfo));

        protected abstract MemberInfo GetMemberInfo();

        protected virtual Type GetDeclaringType()
          => GetDeclaringTypeInternal();

        private Type GetDeclaringTypeInternal()
          => GetMemberInfo().DeclaringType ?? throw new NotSupportedException($"The underlying '{nameof(MemberInfo)}' instance for the member '{GetMemberInfo().Name}' is not returning a declaring type.");

        private BindingFlags ComputeVisibilityBindingFlagsMask()
        {
            BindingFlags visibilityMask = BindingFlags.Default;
            if (this.IsPublic)
            {
                visibilityMask = BindingFlags.Public;
            }
            else if (this.IsPrivate)
            {
                visibilityMask = BindingFlags.NonPublic;
            }

            if (this.IsStatic)
            {
                visibilityMask |= BindingFlags.Static;
            }
            else
            {
                visibilityMask |= BindingFlags.Instance;
            }

            return visibilityMask;
        }
        public RuntimeTypeHandle DeclaringTypeHandle
            => this._declaringTypeHandle ??= GetDeclaringType().TypeHandle;

        public virtual TypeData DeclaringTypeData
          => this.declaringTypeData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetDeclaringType());

        public string Namespace
            => this._namespace ??= GetDeclaringType().Namespace ?? string.Empty;

        public abstract bool IsStatic { get; }
        public abstract bool IsPublic { get; }
        public abstract bool IsPrivate { get; }
        public abstract bool IsAssembly { get; }
        public abstract bool IsFamily { get; }
        public abstract bool IsFamilyOrAssembly { get; }
        public abstract bool IsFamilyAndAssembly { get; }
        public abstract AccessModifier AccessModifier { get; }
        public BindingFlags BindingFlagsVisibilityMask => this._bindingFlagsVisibilityMask ??= ComputeVisibilityBindingFlagsMask();

        public override IList<CustomAttributeData> AttributeData
          => this.attributeData ??= new List<CustomAttributeData>(GetMemberInfo().GetCustomAttributesData());
    }
}
