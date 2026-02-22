namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Reflection;

internal class ParameterListEqualityComparer : IEqualityComparer<ParameterList>, IEqualityComparer<MethodParameterInfoList>
{
    public bool Equals(ParameterList? x, ParameterList? y) => x?.Equals(y) ?? (y is null);
    public int GetHashCode(ParameterList parameterList)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(parameterList;
        return parameterList.GetHashCode();
    }

    public bool Equals(MethodParameterInfoList? x, MethodParameterInfoList? y) => x?.Equals(y) ?? (y is null);
    public int GetHashCode(MethodParameterInfoList methodParameterInfoList)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodParameterInfoList);
        return methodParameterInfoList.GetHashCode();
    }

    public static bool Equals(ParameterList? parameterList, MethodParameterInfoList? methodParameterInfoList)
    {
        if (parameterList is null ^ methodParameterInfoList is null)
        {
            return false;
        }

        if (parameterList!.Count != methodParameterInfoList!.Count)
        {
            return false;
        }

        for (int index = 0; index < parameterList.Count; index++)
        {
            ParameterData parameterData = parameterList.Parameters[index];
            MethodParameterInfo methodParameterInfo = methodParameterInfoList.Parameters[index];

            if (methodParameterInfo.DeclaringMethodDescriptor.HasMemberHandle
                && parameterData.MemberData.Handle != methodParameterInfo.DeclaringMethodDescriptor.MemberHandle)
            {
                return false;
            }

            if (methodParameterInfo.ParameterDescriptor.HasParameterName
                && !parameterData.Name.Equals(methodParameterInfo.ParameterDescriptor.ParameterName, StringComparison.Ordinal))
            {
                return false;
            }

            if (methodParameterInfo.ParameterDescriptor.HasParameterPosition
                && parameterData.Position != methodParameterInfo.ParameterDescriptor.ParameterPosition)
            {
                return false;
            }

            if (!methodParameterInfo.IsAmbiguityExpected)
            {
                continue;
            }

            if (!(methodParameterInfo.DeclaringMethodDescriptor.HasDeclaringTypeHandle
                && methodParameterInfo.DeclaringMethodDescriptor.HasDeclaringMemberName
                && methodParameterInfo.DeclaringMethodDescriptor.HasMemberParameterCount
                && methodParameterInfo.DeclaringMethodDescriptor.HasMemberTypeHandle
                && methodParameterInfo.DeclaringMethodDescriptor.HasParameterizedMemberKind
                && methodParameterInfo.ParameterDescriptor.HasParameterTypeHandle
                && methodParameterInfo.ParameterDescriptor.HasParameterKind))
            {
                throw new AmbiguousMatchException("MethodParameterInfo is marked to expect ambiguity but does not have all required descriptor properties set to resolve it.");
            }

            if (!parameterData.MemberData.DeclaringTypeHandle.Equals(methodParameterInfo.DeclaringMethodDescriptor.DeclaringTypeHandle))
            {
                return false;
            }

            if (!parameterData.MemberData.Name.Equals(methodParameterInfo.DeclaringMethodDescriptor.DeclaringMemberName, StringComparison.Ordinal))
            {
                return false;
            }

            if (parameterData.MemberData.Parameters.Count != methodParameterInfo.DeclaringMethodDescriptor.MemberParameterCount)
            {
                return false;
            }

            if (parameterData.MemberData is MethodData methodData && !methodData.ReturnTypeData.Handle.Equals(methodParameterInfo.DeclaringMethodDescriptor.MemberTypeHandle))
            {
                return false;
            }

            if (parameterData.MemberData.ParameterizedSymbolKind != methodParameterInfo.DeclaringMethodDescriptor.ParameterizedMemberKind)
            {
                return false;
            }

            if (!parameterData.ParameterTypeHandle.Equals(methodParameterInfo.ParameterDescriptor.ParameterTypeHandle))
            {
                return false;
            }

            if (parameterData.ParameterKind != methodParameterInfo.ParameterDescriptor.ParameterKind)
            {
                return false;
            }
        }

        return true;
    }

    public static bool Equals(MethodParameterInfoList? methodParameterInfoList, ParameterList? parameterList)
    {
        if (methodParameterInfoList is null ^ parameterList is null)
        {
            return false;
        }

        if (parameterList!.Count != methodParameterInfoList!.Count)
        {
            return false;
        }

        for (int index = 0; index < parameterList.Count; index++)
        {
            ParameterData parameterData = parameterList.Parameters[index];
            MethodParameterInfo methodParameterInfo = methodParameterInfoList.Parameters[index];

            if (methodParameterInfo.DeclaringMethodDescriptor.HasMemberHandle
                && parameterData.MemberData.Handle != methodParameterInfo.DeclaringMethodDescriptor.MemberHandle)
            {
                return false;
            }

            if (methodParameterInfo.ParameterDescriptor.HasParameterName
                && !parameterData.Name.Equals(methodParameterInfo.ParameterDescriptor.ParameterName, StringComparison.Ordinal))
            {
                return false;
            }

            if (methodParameterInfo.ParameterDescriptor.HasParameterPosition
                && parameterData.Position != methodParameterInfo.ParameterDescriptor.ParameterPosition)
            {
                return false;
            }

            if (!methodParameterInfo.IsAmbiguityExpected)
            {
                continue;
            }

            if (!(methodParameterInfo.DeclaringMethodDescriptor.HasDeclaringTypeHandle
                && methodParameterInfo.DeclaringMethodDescriptor.HasDeclaringMemberName
                && methodParameterInfo.DeclaringMethodDescriptor.HasMemberParameterCount
                && methodParameterInfo.DeclaringMethodDescriptor.HasMemberTypeHandle
                && methodParameterInfo.DeclaringMethodDescriptor.HasParameterizedMemberKind
                && methodParameterInfo.ParameterDescriptor.HasParameterTypeHandle
                && methodParameterInfo.ParameterDescriptor.HasParameterKind))
            {
                throw new AmbiguousMatchException("MethodParameterInfo is marked to expect ambiguity but does not have all required descriptor properties set to resolve it.");
            }

            if (!parameterData.MemberData.DeclaringTypeHandle.Equals(methodParameterInfo.DeclaringMethodDescriptor.DeclaringTypeHandle))
            {
                return false;
            }

            if (!parameterData.MemberData.Name.Equals(methodParameterInfo.DeclaringMethodDescriptor.DeclaringMemberName, StringComparison.Ordinal))
            {
                return false;
            }

            if (parameterData.MemberData.Parameters.Count != methodParameterInfo.DeclaringMethodDescriptor.MemberParameterCount)
            {
                return false;
            }

            if (parameterData.MemberData is MethodData methodData && !methodData.ReturnTypeData.Handle.Equals(methodParameterInfo.DeclaringMethodDescriptor.MemberTypeHandle))
            {
                return false;
            }

            if (parameterData.MemberData.ParameterizedSymbolKind != methodParameterInfo.DeclaringMethodDescriptor.ParameterizedMemberKind)
            {
                return false;
            }

            if (!parameterData.ParameterTypeHandle.Equals(methodParameterInfo.ParameterDescriptor.ParameterTypeHandle))
            {
                return false;
            }

            if (parameterData.ParameterKind != methodParameterInfo.ParameterDescriptor.ParameterKind)
            {
                return false;
            }
        }

        return true;
    }
}
