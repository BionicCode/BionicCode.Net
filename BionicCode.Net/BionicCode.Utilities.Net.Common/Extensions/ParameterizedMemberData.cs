namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    internal abstract class ParameterizedMemberData : MemberData
    {
        protected ParameterizedMemberData(MemberInfo memberInfo, SymbolKind symbolKind, SymbolInfoDataCacheKey symbolInfoDataCacheKey)
            : base(memberInfo, symbolKind, symbolInfoDataCacheKey)
        {
        }

        public abstract ParameterList Parameters { get; }
        public abstract bool HasParamsParameter { get; }
        public abstract RuntimeMethodHandle Handle { get; }
    }
}
