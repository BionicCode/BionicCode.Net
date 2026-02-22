namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

internal abstract class SymbolDataListBuilder<TSymbolInfoData> : SymbolReflectionInfoCache.SymbolInfoDataCacheProvider
    where TSymbolInfoData : SymbolInfoData
{
    private readonly List<TSymbolInfoData> _symbols;
    private readonly RuntimeTypeHandle _declaringTypeHandle;
    private readonly RuntimeMethodHandle _declaringParameterizedMemberHandle;
    private ImmutableList<TSymbolInfoData>? _builderResult;
    private readonly bool _isIntegrityValidationEnabled;

    protected SymbolDataListBuilder(RuntimeTypeHandle declaringTypeHandle)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);

        _symbols = [];
        _declaringTypeHandle = declaringTypeHandle;
        _declaringParameterizedMemberHandle = default;
        _isIntegrityValidationEnabled = true;
    }

    protected SymbolDataListBuilder(RuntimeMethodHandle declaringParameterizedMemberHandle)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringParameterizedMemberHandle);

        _symbols = [];
        _declaringTypeHandle = default;
        _declaringParameterizedMemberHandle = declaringParameterizedMemberHandle;
        _isIntegrityValidationEnabled = true;
    }

    protected void Add(TSymbolInfoData symbolInfoData)
    {
        if (_isIntegrityValidationEnabled)
        {
            if (symbolInfoData is MemberData memberData
                && !memberData.DeclaringTypeHandle.Equals(_declaringTypeHandle))
            {
                throw new ArgumentException(
                    $"The argument {nameof(symbolInfoData)} does not belong to the same declaring type that was specified during builder creation. All added members must belong to the same declaring type.",
                    nameof(symbolInfoData));
            }
            else if (symbolInfoData is ParameterData parameterData
                && !parameterData.MemberData.Handle.Equals(_declaringParameterizedMemberHandle))
            {
                throw new ArgumentException(
                    $"The argument {nameof(symbolInfoData)} does not belong to the same declaring parameterized member that was specified during builder creation. All added parameters must belong to the same declaring parameterized member.",
                    nameof(symbolInfoData));
            }
        }

        _symbols.Add(symbolInfoData);
    }

    protected ImmutableList<TSymbolInfoData> Build()
        => _builderResult ??= _symbols.ToImmutableList();
}
