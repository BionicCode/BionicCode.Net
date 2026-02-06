namespace BionicCode.Utilities.Net.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal interface IFieldListBuilder
    {
        IFieldListBuilder Add(FieldData fieldData);
        FieldList Build();
    }

    internal class FieldListBuilder : SymbolDataListBuilder<FieldData>, IFieldListBuilder
    {
        private FieldList? _builderResult;

        private FieldListBuilder(RuntimeTypeHandle declaringTypeHandle) : base(declaringTypeHandle)
        {
        }

        public static IFieldListBuilder New(RuntimeTypeHandle declaringTypeHandle)
        {
            var builder = new FieldListBuilder(declaringTypeHandle);
            return builder;
        }

        internal static FieldList Create(IEnumerable<FieldInfo>? items)
        {
            List<FieldInfo>? fieldInfoList = items?.ToList();
            if (fieldInfoList is null || fieldInfoList.IsEmpty())
            {
                return FieldList.Empty;
            }

            List<FieldData> fields = new List<FieldData>(fieldInfoList.Count);
            RuntimeTypeHandle declaringTypeHandle = default;
            foreach (FieldInfo fieldInfo in fieldInfoList)
            {
                FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(fieldInfo);

                if (declaringTypeHandle.Equals(default))
                {
                    declaringTypeHandle = fieldData.DeclaringTypeHandle;
                }

                if (!fieldData.DeclaringTypeHandle.Equals(declaringTypeHandle))
                {
                    throw new ArgumentException($"The argument '{nameof(items)}' contains invalid items. Reason: All '{nameof(FieldInfo)}' items must belong to the same declaring type.");
                }

                fields.Add(fieldData);
            }

            return fields.ToFieldList();
        }

        internal static FieldList Create(TypeData declaringTypeData)
        {
            ArgumentNullException.ThrowIfNull(declaringTypeData);
            return CreateInternal(declaringTypeData.UnwrapType());
        }

        internal static FieldList Create(Type declaringType)
        {
            ArgumentNullException.ThrowIfNull(declaringType);
            return CreateInternal(declaringType);
        }

        private static FieldList CreateInternal(Type declaringType)
        {
            FieldInfo[] fieldInfoList = declaringType.GetFields(ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
            if (fieldInfoList.IsEmpty())
            {
                return FieldList.Empty;
            }

            IEnumerable<FieldData> fields = fieldInfoList.Select(SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry);

            return fields.ToFieldList();
        }

        IFieldListBuilder IFieldListBuilder.Add(FieldData fieldData)
        {
            Add(fieldData);
            return this;
        }

        FieldList IFieldListBuilder.Build()
            => _builderResult ??= new FieldList(Build(), isIntegrityValidationEnabled: false);
    }

    internal static class FieldListBuilderExtensions
    {
        internal static FieldList ToFieldList(this IEnumerable<FieldData> items)
            => items is null || items.IsEmpty() ? FieldList.Empty : new FieldList(items);

        /// <summary>
        /// Returns an empty <see cref="FieldList"/> if the provided instance is <see langword="null"/>.
        /// </summary>
        /// <param name="items"></param>
        /// <returns>A <see cref="FieldList"/> that is empty if the provided instance is <see langword="null"/>. Otherwise, returns the original instance.</returns>
        public static FieldList OrEmpty(this FieldList items)
            => items ?? FieldList.Empty;
    }
}
