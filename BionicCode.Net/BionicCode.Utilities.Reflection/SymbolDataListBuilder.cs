namespace BionicCode.Utilities.Net.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    internal abstract class SymbolDataListBuilder<TSymbolInfoData>
    {
        private readonly List<TSymbolInfoData> _symbols;
        private readonly RuntimeTypeHandle _declaringTypeHandle;
        private ImmutableList<TSymbolInfoData>? _builderResult;
        private readonly bool isIntegrityValidationEnabled;

        protected SymbolDataListBuilder()
        {
            this._symbols = new List<TSymbolInfoData>();
            this._declaringTypeHandle = default;
            this.isIntegrityValidationEnabled = false;
        }

        protected SymbolDataListBuilder(RuntimeTypeHandle declaringTypeHandle)
        {
            this._symbols = new List<TSymbolInfoData>();
            this._declaringTypeHandle = declaringTypeHandle;
            this.isIntegrityValidationEnabled = true;
        }

        protected void Add(TSymbolInfoData symbolInfoData)
        {
            if (this.isIntegrityValidationEnabled
                && symbolInfoData is MemberData memberData
                && !memberData.DeclaringTypeHandle.Equals(this._declaringTypeHandle))
            {
                throw new ArgumentException(
                    $"The argument {nameof(symbolInfoData)} does not belong to the same declaring type that was specified during builder creation. All added members must belong to the same declaring type.",
                    nameof(symbolInfoData));
            }

            this._symbols.Add(symbolInfoData);
        }

        protected ImmutableList<TSymbolInfoData> Build()
            => this._builderResult ??= this._symbols.ToImmutableList();
    }
}
