namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal abstract class MemberInfoData : SymbolInfoData
    {
        private IList<CustomAttributeData> attributeData;
        private TypeData declaringTypeData;

        protected MemberInfoData(MemberInfo memberInfo, SymbolInfoDataCacheKey symbolInfoDataCacheKey) : base(memberInfo.Name, symbolInfoDataCacheKey)
        {
            ArgumentNullException.ThrowIfNull(memberInfo, nameof(memberInfo));

            this.DeclaringTypeHandle = memberInfo.DeclaringType.TypeHandle;
            this.Namespace = memberInfo.DeclaringType.Namespace;
        }

        private Type GetDeclaringType()
          => Type.GetTypeFromHandle(this.DeclaringTypeHandle);

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
