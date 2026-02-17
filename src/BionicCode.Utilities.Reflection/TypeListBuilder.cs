namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal interface ITypeListBuilder
{
    ITypeListBuilder Add(TypeData typeData);
    TypeList Build();
}

internal class TypeListBuilder : SymbolDataListBuilder<TypeData>, ITypeListBuilder
{
    private TypeList? _builderResult;

    public static ITypeListBuilder New()
    {
        var builder = new TypeListBuilder();
        return builder;
    }

    internal static TypeList Create(IEnumerable<Type> items)
    {
        var types = items?.ToList();
        if (types is null || types.IsEmpty())
        {
            return TypeList.Empty;
        }

        var typeDataList = new List<TypeData>(types.Count);
        foreach (Type type in types)
        {
            TypeData typeData = GetOrCreateCacheEntry(type);
            typeDataList.Add(typeData);
        }

        return new TypeList(typeDataList);
    }

    /// <summary>
    /// Gets the list of interfaces implemented by the provided type and returns it as a <see cref="TypeList"/>. The list is empty if the provided type does not implement any interfaces.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> for which to retrieve the implemented interfaces.</param>
    /// <returns>A <see cref="TypeList"/> containing the interfaces implemented by the specified type <paramref name="type"/>.</returns>
    internal static TypeList CreateImplementedInterfacesList(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return type.GetInterfaces()
            .Select(GetOrCreateCacheEntry)
            .ToTypeList();
    }

    /// <summary>
    /// Gets the list of interfaces implemented by the provided type and returns it as a <see cref="TypeList"/>. The list is empty if the provided type does not implement any interfaces.
    /// </summary>
    /// <param name="typeData">The <see cref="TypeData"/> for which to retrieve the implemented interfaces.</param>
    /// <returns>A <see cref="TypeList"/> containing the interfaces implemented by the specified type <paramref name="typeData"/>.</returns>
    internal static TypeList CreateImplementedInterfacesList(TypeData typeData)
    {
        ArgumentNullException.ThrowIfNull(typeData, nameof(typeData));

        Type type = typeData.Type;
        return type.GetInterfaces()
            .Select(GetOrCreateCacheEntry)
            .ToTypeList();
    }

    /// <summary>
    /// Creates a list containing the type arguments for the specified generic type.
    /// </summary>
    /// <param name="genericType">The generic <see cref="Type"/> for which to retrieve the type argument list. This parameter must not be null and cannot be a
    /// generic parameter itself.</param>
    /// <returns>A list of type arguments for the specified generic type <paramref name="genericType"/>. Returns an empty list if the provided type is not a
    /// generic type.</returns>
    internal static TypeList CreateGenericTypeArgumentList(Type genericType)
    {
        ArgumentNullException.ThrowIfNull(genericType, nameof(genericType));
        ArgumentExceptionAdvanced.ThrowIfTrue(genericType.IsGenericParameter, nameof(genericType), $"The argument {nameof(genericType)} itself is a generic parameter.");

        return !genericType.IsGenericType
            ? TypeList.Empty
            : CreateGenericTypeArgumentListInternal(genericType);
    }

    /// <summary>
    /// Creates a list containing the type arguments for the specified generic type.
    /// </summary>
    /// <param name="genericTypeData">The generic <see cref="TypeData"/> for which to retrieve the type argument list. This parameter must not be null and cannot be a
    /// generic parameter itself.</param>
    /// <returns>A list of type arguments for the specified generic type <paramref name="genericTypeData"/>. Returns an empty list if the provided type is not a
    /// generic type.</returns>
    internal static TypeList CreateGenericTypeArgumentList(TypeData genericTypeData)
    {
        ArgumentNullException.ThrowIfNull(genericTypeData, nameof(genericTypeData));
        ArgumentExceptionAdvanced.ThrowIfTrue(genericTypeData.IsGenericParameter, nameof(genericTypeData), $"The argument {nameof(genericTypeData)} itself is a generic parameter.");

        return !genericTypeData.IsGenericType
            ? TypeList.Empty
            : CreateGenericTypeArgumentListInternal(genericTypeData.Type);
    }

    internal static TypeList CreateGenericTypeArgumentList(MethodBase genericMethodInfo)
    {
        ArgumentNullException.ThrowIfNull(genericMethodInfo, nameof(genericMethodInfo));

        return !genericMethodInfo.IsGenericMethod || genericMethodInfo is ConstructorInfo
            ? TypeList.Empty
            : CreateGenericTypeArgumentListInternal(genericMethodInfo);
    }

    internal static TypeList CreateGenericTypeArgumentList(MethodData genericMethodData)
    {
        ArgumentNullException.ThrowIfNull(genericMethodData, nameof(genericMethodData));

        return !genericMethodData.IsGenericMethod
            ? TypeList.Empty
            : CreateGenericTypeArgumentListInternal(genericMethodData.MethodInfo);
    }

    internal static TypeList CreateGenericTypeArgumentConstraintList(Type genericType)
    {
        ArgumentNullException.ThrowIfNull(genericType, nameof(genericType));
        ArgumentExceptionAdvanced.ThrowIfTrue(genericType.IsGenericParameter, nameof(genericType), $"The argument {nameof(genericType)} itself is a generic parameter.");

        return !genericType.IsGenericType
            ? TypeList.Empty
            : CreateGenericTypeArgumentConstraintListInternal(genericType);
    }

    internal static TypeList CreateGenericTypeArgumentConstraintList(TypeData genericTypeData)
    {
        ArgumentNullException.ThrowIfNull(genericTypeData, nameof(genericTypeData));
        ArgumentExceptionAdvanced.ThrowIfTrue(genericTypeData.IsGenericParameter, nameof(genericTypeData), $"The argument {nameof(genericTypeData)} itself is a generic parameter.");

        return !genericTypeData.IsGenericType
            ? TypeList.Empty
            : CreateGenericTypeArgumentConstraintListInternal(genericTypeData.Type);
    }

    private static TypeList CreateGenericTypeArgumentListInternal(Type genericType)
    {
        Type[] types = genericType.GetGenericArguments();
        if (types is null || types.IsEmpty())
        {
            return TypeList.Empty;
        }

        IEnumerable<TypeData> typeDataList = types.Select(GetOrCreateCacheEntry);

        return typeDataList.ToTypeList();
    }

    private static TypeList CreateGenericTypeArgumentListInternal(MethodBase genericMethod)
    {
        Type[] types = genericMethod.GetGenericArguments();
        if (types is null || types.IsEmpty())
        {
            return TypeList.Empty;
        }

        IEnumerable<TypeData> typeDataList = types.Select(GetOrCreateCacheEntry);

        return typeDataList.ToTypeList();
    }

    private static TypeList CreateGenericTypeArgumentConstraintListInternal(Type genericType)
    {
        Type[] types = genericType.GetGenericParameterConstraints()
            .Where(constraint => constraint != typeof(object) && constraint != typeof(ValueType))
            .ToArray();
        if (types is null || types.IsEmpty())
        {
            return TypeList.Empty;
        }

        IEnumerable<TypeData> typeDataList = types.Select(GetOrCreateCacheEntry);

        return typeDataList.ToTypeList();
    }

    ITypeListBuilder ITypeListBuilder.Add(TypeData typeData)
    {
        Add(typeData);
        return this;
    }

    TypeList ITypeListBuilder.Build()
        => _builderResult ??= new TypeList(Build());
}

internal static class TypeListBuilderExtensions
{
    internal static TypeList ToTypeList(this IEnumerable<TypeData> items)
        => items is null || items.IsEmpty() ? TypeList.Empty : new TypeList(items);

    /// <summary>
    /// Returns an empty <see cref="TypeList"/> if the provided instance is <see langword="null"/>.
    /// </summary>
    /// <param name="items"></param>
    /// <returns>A <see cref="TypeList"/> that is empty if the provided instance is <see langword="null"/>. Otherwise, returns the original instance.</returns>
    public static TypeList OrEmpty(this TypeList items)
        => items ?? TypeList.Empty;
}
