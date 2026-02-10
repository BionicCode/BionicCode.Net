namespace BionicCode.Utilities.Net.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal interface IMethodListBuilder
    {
        IMethodListBuilder Add(MethodData propertyData);
        MethodList Build();
    }

    internal class MethodListBuilder : SymbolDataListBuilder<MethodData>, IMethodListBuilder
    {
        private MethodList? _builderResult;

        private MethodListBuilder(RuntimeTypeHandle declaringTypeHandle) : base(declaringTypeHandle)
        {
        }

        public static IMethodListBuilder New(RuntimeTypeHandle declaringTypeHandle)
        {
            var builder = new MethodListBuilder(declaringTypeHandle);
            return builder;
        }
        internal static MethodList Create(IEnumerable<MethodInfo>? items)
        {
            List<MethodInfo>? methodInfoList = items?.ToList();
            if (methodInfoList is null || methodInfoList.IsEmpty())
            {
                return MethodList.Empty;
            }

            List<MethodData> methods = new List<MethodData>(methodInfoList.Count);
            RuntimeTypeHandle declaringTypeHandle = default;
            foreach (MethodInfo methodInfo in methodInfoList)
            {
                MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);

                if (declaringTypeHandle.Equals(default))
                {
                    declaringTypeHandle = methodData.DeclaringTypeHandle;
                }

                if (!methodData.DeclaringTypeHandle.Equals(declaringTypeHandle))
                {
                    throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(MethodInfo)}' items must belong to the same declaring type.");
                }

                methods.Add(methodData);
            }

            return methods.ToMethodList();
        }

        internal static MethodList Create(TypeData declaringTypeData)
        {
            ArgumentNullException.ThrowIfNull(declaringTypeData);
            return CreateInternal(declaringTypeData.Type);
        }

        internal static MethodList Create(Type declaringType)
        {
            ArgumentNullException.ThrowIfNull(declaringType);
            return CreateInternal(declaringType);
        }

        private static MethodList CreateInternal(Type declaringType)
        {
            MethodInfo[] methodInfoList = declaringType.GetMethods(ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
            if (methodInfoList.IsEmpty())
            {
                return MethodList.Empty;
            }

            IEnumerable<MethodData> methods = methodInfoList.Select(SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry);

            return methods.ToMethodList();
        }

        IMethodListBuilder IMethodListBuilder.Add(MethodData methodData)
        {
            Add(methodData);
            return this;
        }

        MethodList IMethodListBuilder.Build()
            => _builderResult ??= new MethodList(Build(), isIntegrityValidationEnabled: false);
    }

    internal static class MethodListBuilderExtensions
    {
        public static MethodList ToMethodList(this IEnumerable<MethodData> items)
            => items is null || items.IsEmpty() ? MethodList.Empty : new MethodList(items);

        /// <summary>
        /// Returns an empty <see cref="MethodList"/> if the provided instance is <see langword="null"/>.
        /// </summary>
        /// <param name="items"></param>
        /// <returns>A <see cref="MethodList"/> that is empty if the provided instance is <see langword="null"/>. Otherwise, returns the original instance.</returns>
        public static MethodList OrEmpty(this MethodList items)
            => items ?? MethodList.Empty;
    }
}
