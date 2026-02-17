namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using static BionicCode.Utilities.Net.Reflection.SymbolReflectionInfoCache;

internal abstract class SymbolDataListBuilder<TSymbolInfoData> : SymbolInfoDataCacheProvider
{
    private readonly List<TSymbolInfoData> _symbols;
    private readonly RuntimeTypeHandle _declaringTypeHandle;
    private ImmutableList<TSymbolInfoData>? _builderResult;
    private readonly bool _isIntegrityValidationEnabled;

    protected SymbolDataListBuilder()
    {
        _symbols = [];
        _declaringTypeHandle = default;
        _isIntegrityValidationEnabled = false;
    }

    protected SymbolDataListBuilder(RuntimeTypeHandle declaringTypeHandle)
    {
        _symbols = [];
        _declaringTypeHandle = declaringTypeHandle;
        _isIntegrityValidationEnabled = true;
    }

    protected void Add(TSymbolInfoData symbolInfoData)
    {
        if (_isIntegrityValidationEnabled
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
