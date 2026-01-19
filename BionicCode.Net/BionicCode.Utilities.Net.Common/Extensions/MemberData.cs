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

        protected MemberData(MemberInfo memberInfo, SymbolKind symbolKind, SymbolInfoDataCacheKey symbolInfoDataCacheKey) : base(memberInfo.Name, symbolKind, symbolInfoDataCacheKey)
            => ArgumentNullException.ThrowIfNull(memberInfo, nameof(memberInfo));

        private Type GetDeclaringType()
          => GetMemberInfo().DeclaringType ?? throw new NotSupportedException($"The underlying '{nameof(MemberInfo)}' instance for the member '{GetMemberInfo().Name}' is not returning a declaring type.");

        protected abstract MemberInfo GetMemberInfo();
        public RuntimeTypeHandle DeclaringTypeHandle
            => this._declaringTypeHandle ??= GetDeclaringType().TypeHandle;

        public TypeData DeclaringTypeData
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

        public override IList<CustomAttributeData> AttributeData
          => this.attributeData ??= new List<CustomAttributeData>(GetMemberInfo().GetCustomAttributesData());
    }
}
