namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class ParameterListEqualityComparer : IEqualityComparer<ParameterList>, IEqualityComparer<MethodParameterInfoList>
    {
        public bool Equals(ParameterList? x, ParameterList? y)
            => x?.Equals(y) ?? (y is null);
        public int GetHashCode(ParameterList parameterList)
            => parameterList.GetHashCode();

        public bool Equals(MethodParameterInfoList? x, MethodParameterInfoList? y)
            => x?.Equals(y) ?? (y is null);
        public int GetHashCode(MethodParameterInfoList methodParameterInfoList)
            => methodParameterInfoList.GetHashCode();

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

                if (methodParameterInfo.DeclaringMemberDescriptor.HasMemberHandle
                    && parameterData.MemberData.Handle != methodParameterInfo.DeclaringMemberDescriptor.MemberHandle)
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

                if (!(methodParameterInfo.DeclaringMemberDescriptor.HasDeclaringTypeHandle
                    && methodParameterInfo.DeclaringMemberDescriptor.HasDeclaringMemberName
                    && methodParameterInfo.DeclaringMemberDescriptor.HasMemberParameterCount
                    && methodParameterInfo.DeclaringMemberDescriptor.HasMemberTypeHandle
                    && methodParameterInfo.DeclaringMemberDescriptor.HasParameterizedMemberKind
                    && methodParameterInfo.ParameterDescriptor.HasParameterTypeHandle
                    && methodParameterInfo.ParameterDescriptor.HasParameterKind))
                {
                    throw new AmbiguousMatchException("MethodParameterInfo is marked to expect ambiguity but does not have all required descriptor properties set to resolve it.");
                }

                if (!parameterData.MemberData.DeclaringTypeHandle.Equals(methodParameterInfo.DeclaringMemberDescriptor.DeclaringTypeHandle))
                {
                    return false;
                }

                if (!parameterData.MemberData.Name.Equals(methodParameterInfo.DeclaringMemberDescriptor.DeclaringMemberName, StringComparison.Ordinal))
                {
                    return false;
                }

                if (parameterData.MemberData.Parameters.Count != methodParameterInfo.DeclaringMemberDescriptor.MemberParameterCount)
                {
                    return false;
                }

                if (parameterData.MemberData is MethodData methodData && !methodData.ReturnTypeData.Handle.Equals(methodParameterInfo.DeclaringMemberDescriptor.MemberTypeHandle))
                {
                    return false;
                }

                if (parameterData.MemberData.ParameterizedSymbolKind != methodParameterInfo.DeclaringMemberDescriptor.ParameterizedMemberKind)
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

                if (methodParameterInfo.DeclaringMemberDescriptor.HasMemberHandle
                    && parameterData.MemberData.Handle != methodParameterInfo.DeclaringMemberDescriptor.MemberHandle)
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

                if (!(methodParameterInfo.DeclaringMemberDescriptor.HasDeclaringTypeHandle
                    && methodParameterInfo.DeclaringMemberDescriptor.HasDeclaringMemberName
                    && methodParameterInfo.DeclaringMemberDescriptor.HasMemberParameterCount
                    && methodParameterInfo.DeclaringMemberDescriptor.HasMemberTypeHandle
                    && methodParameterInfo.DeclaringMemberDescriptor.HasParameterizedMemberKind
                    && methodParameterInfo.ParameterDescriptor.HasParameterTypeHandle
                    && methodParameterInfo.ParameterDescriptor.HasParameterKind))
                {
                    throw new AmbiguousMatchException("MethodParameterInfo is marked to expect ambiguity but does not have all required descriptor properties set to resolve it.");
                }

                if (!parameterData.MemberData.DeclaringTypeHandle.Equals(methodParameterInfo.DeclaringMemberDescriptor.DeclaringTypeHandle))
                {
                    return false;
                }

                if (!parameterData.MemberData.Name.Equals(methodParameterInfo.DeclaringMemberDescriptor.DeclaringMemberName, StringComparison.Ordinal))
                {
                    return false;
                }

                if (parameterData.MemberData.Parameters.Count != methodParameterInfo.DeclaringMemberDescriptor.MemberParameterCount)
                {
                    return false;
                }

                if (parameterData.MemberData is MethodData methodData && !methodData.ReturnTypeData.Handle.Equals(methodParameterInfo.DeclaringMemberDescriptor.MemberTypeHandle))
                {
                    return false;
                }

                if (parameterData.MemberData.ParameterizedSymbolKind != methodParameterInfo.DeclaringMemberDescriptor.ParameterizedMemberKind)
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
}
