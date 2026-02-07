namespace BionicCode.Utilities.Net.Reflection
{
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
            List<Type>? types = items?.ToList();
            if (types is null || types.IsEmpty())
            {
                return TypeList.Empty;
            }

            List<TypeData> typeDataList = new List<TypeData>(types.Count);
            foreach (Type type in types)
            {
                TypeData tyoeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
                typeDataList.Add(tyoeData);
            }

            return new TypeList(typeDataList);
        }

        internal static TypeList CreateImplementedInterfacesList(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            return type.GetInterfaces()
                .Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry)
                .ToTypeList();
        }

        internal static TypeList CreateImplementedInterfacesList(TypeData typeData)
        {
            ArgumentNullException.ThrowIfNull(typeData, nameof(typeData));

            Type type = typeData.UnwrapType();
            return type.GetInterfaces()
                .Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry)
                .ToTypeList();
        }

        internal static TypeList CreateGenericTypeArgumentList(Type genericType)
        {
            ArgumentNullException.ThrowIfNull(genericType, nameof(genericType));
            ArgumentExceptionAdvanced.ThrowIfTrue(genericType.IsGenericParameter, nameof(genericType), $"The argument {nameof(genericType)} itself is a generic parameter.");

            return !genericType.IsGenericType
                ? TypeList.Empty
                : CreateGenericTypeArgumentListInternal(genericType);
        }

        internal static TypeList CreateGenericTypeArgumentList(TypeData genericTypeData)
        {
            ArgumentNullException.ThrowIfNull(genericTypeData, nameof(genericTypeData));
            ArgumentExceptionAdvanced.ThrowIfTrue(genericTypeData.IsGenericParameter, nameof(genericTypeData), $"The argument {nameof(genericTypeData)} itself is a generic parameter.");

            return !genericTypeData.IsGenericType
                ? TypeList.Empty
                : CreateGenericTypeArgumentListInternal(genericTypeData.UnwrapType());
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
                : CreateGenericTypeArgumentConstraintListInternal(genericTypeData.UnwrapType());
        }

        private static TypeList CreateGenericTypeArgumentListInternal(Type genericType)
        {
            Type[] types = genericType.GetGenericArguments();
            if (types is null || types.IsEmpty())
            {
                return TypeList.Empty;
            }

            IEnumerable<TypeData> typeDataList = types.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry);

            return typeDataList.ToTypeList();
        }

        private static TypeList CreateGenericTypeArgumentListInternal(MethodBase genericMethod)
        {
            Type[] types = genericMethod.GetGenericArguments();
            if (types is null || types.IsEmpty())
            {
                return TypeList.Empty;
            }

            IEnumerable<TypeData> typeDataList = types.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry);

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

            IEnumerable<TypeData> typeDataList = types.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry);

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
}
