namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;

internal class ParameterListEqualityComparer : IEqualityComparer<ParameterList>, IEqualityComparer<ParameterDescriptorList>, IEqualityComparer<IParameterListView>
{
    public bool Equals(ParameterList? x, ParameterList? y) => x?.Equals(y) ?? (y is null);
    public int GetHashCode(ParameterList parameterList)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(parameterList);
        return parameterList.GetHashCode();
    }

    public bool Equals(ParameterDescriptorList? x, ParameterDescriptorList? y) => x?.Equals(y) ?? (y is null);
    public int GetHashCode(ParameterDescriptorList methodParameterInfoList)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodParameterInfoList);
        return methodParameterInfoList.GetHashCode();
    }

    public bool Equals(IParameterListView? x, IParameterListView? y) => x?.Equals(y) ?? (y is null);
    public int GetHashCode(IParameterListView parameterListView)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(parameterListView);
        return parameterListView.GetHashCode();
    }

    /// <summary>
    /// Equality comparison of a <see cref="ParameterDescriptorList"/> and a <see cref="ParameterList"/> based on the information that <see cref="ParameterDescriptor"/> items provide.
    /// </summary>
    /// <remarks>This is a weak equality comparison of a <see cref="ParameterDescriptorList"/> and a <see cref="ParameterList"/> based on the information that <see cref="ParameterDescriptor"/> items provide.
    /// <br/>This means, if a <see cref="ParameterDescriptor"/> item does not provide the optional parameter information, then equality comparison will not test these missing information, which can lead to false matches.
    /// For example, if the <see cref="ParameterDescriptor"/> does not provide the parameter name, the comparison will not consider the parameter name, potentially resulting in a match with a parameter that has a different name.</remarks>
    /// <param name="parameterDescriptorList">The parameter descriptor list to compare holding potentially weak information if some optional information is missing.</param>
    /// <param name="parameterList">The strict parameter list to compare.</param>
    /// <returns>An <see cref="EqualityComparisonResult"/> indicating the result of the comparison.</returns>
    public static EqualityComparisonResult Equals(ParameterList? parameterList, ParameterDescriptorList? parameterDescriptorList)
    {
        // Return FALSE if exactly one of the parameter lists is NULL, otherwise compare the parameter lists for equality.
        if (parameterList is null ^ parameterDescriptorList is null)
        {
            return EqualityComparisonResult.False;
        }

        // If the parameter list is NULL, the parameter descriptor list must also be NULL at this point, so return TRUE.
        if (parameterList is null)
        {
            return EqualityComparisonResult.True;
        }

        if (parameterList.Count != parameterDescriptorList!.Count)
        {
            return EqualityComparisonResult.False;
        }

        bool isAmbiguityExpected = false;
        for (int index = 0; index < parameterList.Count; index++)
        {
            ParameterData parameterData = parameterList.Parameters[index];
            ParameterDescriptor methodParameterDescriptor = parameterDescriptorList.Parameters[index];
            isAmbiguityExpected |= methodParameterDescriptor.IsAmbiguityExpected;

            if (parameterData.Position != methodParameterDescriptor.ParameterPosition)
            {
                return EqualityComparisonResult.False;
            }

            if (!parameterData.ParameterTypeHandle.Equals(methodParameterDescriptor.ParameterTypeHandle))
            {
                return EqualityComparisonResult.False;
            }

            if (methodParameterDescriptor.HasParameterName
                && !parameterData.Name.Equals(methodParameterDescriptor.ParameterName, StringComparison.Ordinal))
            {
                return EqualityComparisonResult.False;
            }

            if (methodParameterDescriptor.HasParameterizedSymbolKind
                && parameterData.MemberData.ParameterizedSymbolKind != methodParameterDescriptor.ParameterizedSymbolKind)
            {
                return EqualityComparisonResult.False;
            }

            if (methodParameterDescriptor.HasParameterModifier
                && parameterData.ParameterModifier != methodParameterDescriptor.ParameterModifier)
            {
                return EqualityComparisonResult.False;
            }
        }

        return isAmbiguityExpected
            ? EqualityComparisonResult.TrueButAmbiguous
            : EqualityComparisonResult.True;
    }

    /// <summary>
    /// Equality comparison of a <see cref="ParameterDescriptorList"/> and a <see cref="ParameterList"/> based on the information that <see cref="ParameterDescriptor"/> items provide.
    /// </summary>
    /// <remarks>This is a weak equality comparison of a <see cref="ParameterDescriptorList"/> and a <see cref="ParameterList"/> based on the information that <see cref="ParameterDescriptor"/> items provide.
    /// <br/>This means, if a <see cref="ParameterDescriptor"/> item does not provide the optional parameter information, then equality comparison will not test these missing information, which can lead to false matches.
    /// For example, if the <see cref="ParameterDescriptor"/> does not provide the parameter name, the comparison will not consider the parameter name, potentially resulting in a match with a parameter that has a different name.</remarks>
    /// <param name="parameterDescriptorList">The parameter descriptor list to compare holding potentially weak information if some optional information is missing.</param>
    /// <param name="parameterList">The strict parameter list to compare.</param>
    /// <returns>An <see cref="EqualityComparisonResult"/> indicating the result of the comparison.</returns>
    public static EqualityComparisonResult Equals(ParameterDescriptorList? parameterDescriptorList, ParameterList? parameterList)
    {
        // Return FALSE if exactly one of the parameter lists is NULL, otherwise compare the parameter lists for equality.
        if (parameterList is null ^ parameterDescriptorList is null)
        {
            return EqualityComparisonResult.False;
        }

        // If the parameter descriptor list is NULL, the parameter list must also be NULL at this point, so return TRUE.
        if (parameterDescriptorList is null)
        {
            return EqualityComparisonResult.True;
        }

        if (parameterList!.Count != parameterDescriptorList!.Count)
        {
            return EqualityComparisonResult.False;
        }

        bool isAmbiguityExpected = false;
        for (int index = 0; index < parameterList.Count; index++)
        {
            ParameterData parameterData = parameterList.Parameters[index];
            ParameterDescriptor methodParameterDescriptor = parameterDescriptorList.Parameters[index];
            isAmbiguityExpected |= methodParameterDescriptor.IsAmbiguityExpected;

            if (parameterData.Position != methodParameterDescriptor.ParameterPosition)
            {
                return EqualityComparisonResult.False;
            }

            if (!parameterData.ParameterTypeHandle.Equals(methodParameterDescriptor.ParameterTypeHandle))
            {
                return EqualityComparisonResult.False;
            }

            if (methodParameterDescriptor.HasParameterName
                && !parameterData.Name.Equals(methodParameterDescriptor.ParameterName, StringComparison.Ordinal))
            {
                return EqualityComparisonResult.False;
            }

            if (methodParameterDescriptor.HasParameterizedSymbolKind
                && parameterData.MemberData.ParameterizedSymbolKind != methodParameterDescriptor.ParameterizedSymbolKind)
            {
                return EqualityComparisonResult.False;
            }

            if (methodParameterDescriptor.HasParameterModifier
                && parameterData.ParameterModifier != methodParameterDescriptor.ParameterModifier)
            {
                return EqualityComparisonResult.False;
            }
        }

        return isAmbiguityExpected
            ? EqualityComparisonResult.TrueButAmbiguous
            : EqualityComparisonResult.True;
    }

    public static bool Equals(ParameterList? parameterList, IParameterListView? parameterListView)
    {
        // Return FALSE if exactly one of the parameter lists is NULL, otherwise compare the parameter lists for equality.
        if (parameterList is null ^ parameterListView is null)
        {
            return false;
        }

        // If the parameter list is NULL, the parameter list view must also be NULL at this point, so return TRUE.
        // Otherwise, compare the parameter list with the parameter list view for equality.
        return parameterList?.Equals(parameterListView) ?? true;
    }

    public static bool Equals(IParameterListView? parameterListView, ParameterList? parameterList)
    {
        // Return FALSE if exactly one of the parameter lists is NULL, otherwise compare the parameter lists for equality.
        if (parameterList is null ^ parameterListView is null)
        {
            return false;
        }

        return IParameterListView.Equals(parameterListView, parameterList);
    }

    /// <summary>
    /// Equality comparison of a <see cref="ParameterDescriptorList"/> and a <see cref="IParameterListView"/> based on the information that <see cref="ParameterDescriptor"/> items provide.
    /// </summary>
    /// <remarks>This is a weak equality comparison of a <see cref="ParameterDescriptorList"/> and a <see cref="IParameterListView"/> based on the information that <see cref="ParameterDescriptor"/> items provide.
    /// <br/>This means, if a <see cref="ParameterDescriptor"/> item does not provide the optional parameter information, then equality comparison will not test these missing information, which can lead to false matches.
    /// For example, if the <see cref="ParameterDescriptor"/> does not provide the parameter name, the comparison will not consider the parameter name, potentially resulting in a match with a parameter that has a different name.</remarks>
    /// <param name="parameterDescriptorList">The parameter descriptor list to compare holding potentially weak information if some optional information is missing.</param>
    /// <param name="parameterListView">The strict parameter list to compare.</param>
    /// <returns>An <see cref="EqualityComparisonResult"/> indicating the result of the comparison.</returns>
    public static EqualityComparisonResult Equals(ParameterDescriptorList? parameterDescriptorList, IParameterListView? parameterListView)
    {
        // Return FALSE if exactly one of the parameter lists is NULL, otherwise compare the parameter lists for equality.
        if (parameterListView is null ^ parameterDescriptorList is null)
        {
            return EqualityComparisonResult.False;
        }

        // If the parameter descriptor list is NULL, the parameter list view must also be NULL at this point, so return TRUE.
        if (parameterDescriptorList is null)
        {
            return EqualityComparisonResult.True;
        }

        if (parameterListView!.Count != parameterDescriptorList.Count)
        {
            return EqualityComparisonResult.False;
        }

        bool isAmbiguityExpected = false;
        for (int index = 0; index < parameterListView.Count; index++)
        {
            IParameterDataView parameterDataView = parameterListView.Parameters[index];
            ParameterDescriptor methodParameterDescriptor = parameterDescriptorList.Parameters[index];
            isAmbiguityExpected |= methodParameterDescriptor.IsAmbiguityExpected;

            if (parameterDataView.Position != methodParameterDescriptor.ParameterPosition)
            {
                return EqualityComparisonResult.False;
            }

            if (!parameterDataView.ParameterTypeHandle.Equals(methodParameterDescriptor.ParameterTypeHandle))
            {
                return EqualityComparisonResult.False;
            }

            if (methodParameterDescriptor.HasParameterName
                && !parameterDataView.Name.Equals(methodParameterDescriptor.ParameterName, StringComparison.Ordinal))
            {
                return EqualityComparisonResult.False;
            }

            if (methodParameterDescriptor.HasParameterizedSymbolKind
                && parameterDataView.MemberData.ParameterizedSymbolKind != methodParameterDescriptor.ParameterizedSymbolKind)
            {
                return EqualityComparisonResult.False;
            }

            if (methodParameterDescriptor.HasParameterModifier
                && parameterDataView.ParameterModifier != methodParameterDescriptor.ParameterModifier)
            {
                return EqualityComparisonResult.False;
            }
        }

        return isAmbiguityExpected
            ? EqualityComparisonResult.TrueButAmbiguous
            : EqualityComparisonResult.True;
    }

    /// <summary>
    /// Equality comparison of a <see cref="ParameterDescriptorList"/> and a <see cref="IParameterListView"/> based on the information that <see cref="ParameterDescriptor"/> items provide.
    /// </summary>
    /// <remarks>This is a weak equality comparison of a <see cref="ParameterDescriptorList"/> and a <see cref="IParameterListView"/> based on the information that <see cref="ParameterDescriptor"/> items provide.
    /// <br/>This means, if a <see cref="ParameterDescriptor"/> item does not provide the optional parameter information, then equality comparison will not test these missing information, which can lead to false matches.
    /// For example, if the <see cref="ParameterDescriptor"/> does not provide the parameter name, the comparison will not consider the parameter name, potentially resulting in a match with a parameter that has a different name.</remarks>
    /// <param name="parameterDescriptorList">The parameter descriptor list to compare holding potentially weak information if some optional information is missing.</param>
    /// <param name="parameterListView">The strict parameter list to compare.</param>
    /// <returns>An <see cref="EqualityComparisonResult"/> indicating the result of the comparison.</returns>
    public static EqualityComparisonResult Equals(IParameterListView? parameterListView, ParameterDescriptorList? parameterDescriptorList)
    {
        // Return FALSE if exactly one of the parameter lists is NULL, otherwise compare the parameter lists for equality.
        if (parameterListView is null ^ parameterDescriptorList is null)
        {
            return EqualityComparisonResult.False;
        }

        if (parameterDescriptorList is null)
        {
            // If the parameter descriptor list is NULL, the parameter list view must also be NULL at this point, so return TRUE.
            return EqualityComparisonResult.True;
        }

        if (parameterListView!.Count != parameterDescriptorList.Count)
        {
            return EqualityComparisonResult.False;
        }

        bool isAmbiguityExpected = false;
        for (int index = 0; index < parameterListView.Count; index++)
        {
            IParameterDataView parameterDataView = parameterListView.Parameters[index];
            ParameterDescriptor methodParameterDescriptor = parameterDescriptorList.Parameters[index];
            isAmbiguityExpected |= methodParameterDescriptor.IsAmbiguityExpected;

            if (parameterDataView.Position != methodParameterDescriptor.ParameterPosition)
            {
                return EqualityComparisonResult.False;
            }

            if (!parameterDataView.ParameterTypeHandle.Equals(methodParameterDescriptor.ParameterTypeHandle))
            {
                return EqualityComparisonResult.False;
            }

            if (methodParameterDescriptor.HasParameterName
                && !parameterDataView.Name.Equals(methodParameterDescriptor.ParameterName, StringComparison.Ordinal))
            {
                return EqualityComparisonResult.False;
            }

            if (methodParameterDescriptor.HasParameterizedSymbolKind
                && parameterDataView.MemberData.ParameterizedSymbolKind != methodParameterDescriptor.ParameterizedSymbolKind)
            {
                return EqualityComparisonResult.False;
            }

            if (methodParameterDescriptor.HasParameterModifier
                && parameterDataView.ParameterModifier != methodParameterDescriptor.ParameterModifier)
            {
                return EqualityComparisonResult.False;
            }
        }

        return isAmbiguityExpected
            ? EqualityComparisonResult.TrueButAmbiguous
            : EqualityComparisonResult.True;
    }
}
