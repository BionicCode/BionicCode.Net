namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal abstract class MemberData : SymbolInfoData
    {
        private IList<CustomAttributeData> attributeData;
        private TypeData declaringTypeData;

        protected MemberData(MemberInfo memberInfo, SymbolKind symbolKind, SymbolInfoDataCacheKey symbolInfoDataCacheKey) : base(memberInfo.Name, symbolKind, symbolInfoDataCacheKey)
        {
            ArgumentNullException.ThrowIfNull(memberInfo, nameof(memberInfo));
            if (memberInfo.DeclaringType is null)
            {
                throw new NotSupportedException($"The member '{memberInfo.Name}' has no declaring type.");
            }

            this.DeclaringTypeHandle = memberInfo.DeclaringType.TypeHandle;
            this.Namespace = memberInfo.DeclaringType.Namespace ?? string.Empty;
        }

        private Type GetDeclaringType()
          => Type.GetTypeFromHandle(this.DeclaringTypeHandle)!;

        protected abstract MemberInfo GetMemberInfo();
        public RuntimeTypeHandle DeclaringTypeHandle { get; }
        public abstract bool IsStatic { get; }
        public abstract bool IsPublic { get; }
        public abstract bool IsPrivate { get; }
        public abstract bool IsAssembly { get; }
        public abstract bool IsFamily { get; }
        public abstract bool IsFamilyOrAssembly { get; }
        public abstract bool IsFamilyAndAssembly { get; }
        public abstract AccessModifier AccessModifier { get; }
        public string Namespace { get; }

        public override IList<CustomAttributeData> AttributeData
          => this.attributeData ??= new List<CustomAttributeData>(GetMemberInfo().GetCustomAttributesData());

        public TypeData DeclaringTypeData
          => this.declaringTypeData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetDeclaringType());
    }
}
