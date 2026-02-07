namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

/// <summary>
/// An equality comparer for <see cref="MethodInfo"/> objects that defines equality based on method signature, including method name, return type, generic method parameters and parameters, while ignoring the declaring type. This allows to consider methods with the same signature as equal even if they are declared on different types, such as in the case of interface implementations where the implementing method on the class and the declared method on the interface share the same signature but have different declaring types. <br/>
/// </summary>
/// <remarks>Note: The declaring type is not considered for equality since methods declared on different types can still share the same signature, 
/// for example in the case of interface implementations where the implementing method on the class and the declared method on the interface 
/// share the same signature but have different declaring types.
/// <para/>Additionally, for generic methods equality check is based on the generic method definition, 
/// meaning that all closed or open generic method variations of the same generic method definition are considered equal 
/// since they share the same generic method definition and thus the same signature.
/// </remarks>
public class MethodSignatureEqualityComparer : IEqualityComparer<MethodInfo>, IEqualityComparer<MethodData>
{
    #region MethodInfo		
    /// <summary>
    /// Equality is defined by signature only, meaning that the method name, return type, generic method parameters and parameters must be the same. 
    /// </summary>
    /// <remarks>Note: The declaring type is not considered for equality since methods declared on different types can still share the same signature, 
    /// for example in the case of interface implementations where the implementing method on the class and the declared method on the interface 
    /// share the same signature but have different declaring types.
    /// <para/>Additionally, for generic methods equality check is based on the generic method definition, 
    /// meaning that all closed or open generic method variations of the same generic method definition are considered equal 
    /// since they share the same generic method definition and thus the same signature.
    /// </remarks>
    /// <param name="x">The first method to compare.</param>
    /// <param name="y">The second method to compare.</param>
    /// <returns><see langword="true"/> if the methods are considered equal based on their signature; otherwise, <see langword="false"/>.</returns>
    public bool Equals(MethodInfo? x, MethodInfo? y)
    {
        MethodData xMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(x);
        MethodData yMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(y);

        return Equals(xMethodData, yMethodData);
    }

    public int GetHashCode(MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo);

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);

        return GetHashCode(methodData);
    }
    #endregion MethodInfo

    #region MethodData		
    /// <summary>
    /// Equality is defined by signature only, meaning that the method name, return type, generic method parameters and parameters must be the same. 
    /// </summary>
    /// <remarks>Note: The declaring type is not considered for equality since methods declared on different types can still share the same signature, 
    /// for example in the case of interface implementations where the implementing method on the class and the declared method on the interface 
    /// share the same signature but have different declaring types.
    /// <para/>Additionally, for generic methods equality check is based on the generic method definition, 
    /// meaning that all closed or open generic method variations of the same generic method definition are considered equal 
    /// since they share the same generic method definition and thus the same signature.
    /// </remarks>
    /// <param name="x">The first method to compare.</param>
    /// <param name="y">The second method to compare.</param>
    /// <returns><see langword="true"/> if the methods are considered equal based on their signature; otherwise, <see langword="false"/>.</returns>
    public bool Equals(MethodData? xMethodData, MethodData? yMethodData)
    {
        if (ReferenceEquals(xMethodData, yMethodData))
        {
            return true;
        }

        if (xMethodData is null || yMethodData is null)
        {
            return false;
        }

        if (xMethodData.Name != yMethodData.Name)
        {
            return false;
        }

        if (xMethodData.ReturnTypeData != yMethodData.ReturnTypeData)
        {
            return false;
        }

        // For generic methods, the generic method definition is used to compare
        // the generic method parameters and the parameters since they share the same signature,
        if (xMethodData.IsGenericMethod || yMethodData.IsGenericMethod)
        {
            // If only one of the methods is a generic method, they cannot be considered equal.
            if (xMethodData.IsGenericMethod ^ yMethodData.IsGenericMethod)
            {
                return false;
            }

            // Both must map to the same generic method definition to be considered equal,
            // meaning that generic method definition and closed generic variations are considered equal
            // IF the generic variants share the same generic method definition.
            // When both are equal based on the generic method definition, we can stop further comparison since they are already considered equal.
            return ReferenceEquals(xMethodData.GenericMethodDefinitionData, yMethodData.GenericMethodDefinitionData);
        }

        ParameterList xParameters = xMethodData.Parameters;
        ParameterList yParameters = yMethodData.Parameters;
        if (xParameters.Count != yParameters.Count)
        {
            return false;
        }

        for (int i = 0; i < xParameters.Count; i++)
        {
            ParameterData xParameterData = xParameters[i];
            ParameterData yParameterData = yParameters[i];
            if (!ReferenceEquals(xParameterData.ParameterTypeData, yParameterData.ParameterTypeData)
                || xParameterData.ParameterKind != yParameterData.ParameterKind
                || !xParameterData.Name.Equals(yParameterData.Name, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(MethodData methodData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodData);

        unchecked
        {
            methodData = methodData.IsGenericMethod
                ? methodData.GenericMethodDefinitionData
                : methodData;
            var hashCode = new HashCode();
            hashCode.Add(methodData.Name);
            hashCode.Add(methodData.ReturnTypeData);
            foreach (ParameterData param in methodData.Parameters)
            {
                hashCode.Add(param.ParameterTypeData);
                hashCode.Add(param.Position);
                hashCode.Add(param.Name);
                hashCode.Add(param.ParameterKind);
            }

            return hashCode.ToHashCode();
        }
    }
    #endregion MethodData
}