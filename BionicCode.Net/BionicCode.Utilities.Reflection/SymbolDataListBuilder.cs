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
            _symbols = new List<TSymbolInfoData>();
            _declaringTypeHandle = default;
            isIntegrityValidationEnabled = false;
        }

        protected SymbolDataListBuilder(RuntimeTypeHandle declaringTypeHandle)
        {
            _symbols = new List<TSymbolInfoData>();
            _declaringTypeHandle = declaringTypeHandle;
            isIntegrityValidationEnabled = true;
        }

        protected void Add(TSymbolInfoData symbolInfoData)
        {
            if (isIntegrityValidationEnabled
                && symbolInfoData is MemberData memberData
                && !memberData.DeclaringTypeHandle.Equals(_declaringTypeHandle))
            {
                throw new ArgumentException(
                    $"The argument {nameof(symbolInfoData)} does not belong to the same declaring type that was specified during builder creation. All added members must belong to the same declaring type.",
                    nameof(symbolInfoData));
            }

            _symbols.Add(symbolInfoData);
        }

        protected ImmutableList<TSymbolInfoData> Build()
            => _builderResult ??= _symbols.ToImmutableList();
    }
}
