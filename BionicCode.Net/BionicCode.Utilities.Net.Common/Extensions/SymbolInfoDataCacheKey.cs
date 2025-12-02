namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal readonly struct SymbolInfoDataCacheKey : IEquatable<SymbolInfoDataCacheKey>
    {
        public readonly string Name { get; }
        public readonly RuntimeTypeHandle DeclaringTypeHandle { get; }
        public readonly RuntimeTypeHandle TypeHandle { get; }
        public readonly RuntimeMethodHandle MethodHandle { get; }
        public readonly RuntimeMethodHandle GetMethodHandle { get; }
        public readonly RuntimeMethodHandle SetMethodHandle { get; }
        public readonly RuntimeMethodHandle AddMethodHandle { get; }
        public readonly RuntimeMethodHandle RemoveMethodHandle { get; }
        public readonly SymbolAttributes SymbolKind { get; }
        public ParameterList ParameterList { get; }

        private SymbolInfoDataCacheKey(string name,
            RuntimeTypeHandle declaringTypeHandle,
            RuntimeTypeHandle typeHandle,
            RuntimeMethodHandle methodHandle,
            RuntimeMethodHandle getMethodHandle,
            RuntimeMethodHandle setMethodHandle,
            RuntimeMethodHandle addMethodHandle,
            RuntimeMethodHandle removeMethodHandle,
            ParameterList parameterList,
            SymbolAttributes symbolKind)
        {
            this.Name = name;
            this.DeclaringTypeHandle = declaringTypeHandle;
            this.TypeHandle = typeHandle;
            this.MethodHandle = methodHandle;
            this.GetMethodHandle = getMethodHandle;
            this.SetMethodHandle = setMethodHandle;
            this.AddMethodHandle = addMethodHandle;
            this.RemoveMethodHandle = removeMethodHandle;
            this.ParameterList = parameterList;
            this.SymbolKind = symbolKind;
        }

        public static SymbolInfoDataCacheKey CreateForEvent(EventInfo eventInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(eventInfo, nameof(eventInfo));
            RuntimeTypeHandle declaringTypeHandle = eventInfo.DeclaringType.TypeHandle;
            string name = eventInfo.Name;
            RuntimeMethodHandle addMethodHandle = eventInfo.AddMethod.MethodHandle;
            RuntimeMethodHandle removeMethodHandle = eventInfo.RemoveMethod.MethodHandle;
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                default,
                default,
                default,
                default,
                addMethodHandle,
                removeMethodHandle,
                ParameterList.Empty,
                SymbolAttributes.Event);
        }

        public static SymbolInfoDataCacheKey CreateForProperty(PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));
            RuntimeTypeHandle declaringTypeHandle = propertyInfo.DeclaringType.TypeHandle;
            string name = propertyInfo.Name;
            RuntimeTypeHandle typeHandle = propertyInfo.PropertyType.TypeHandle;
            RuntimeMethodHandle getMethodHandle = propertyInfo.GetMethod?.MethodHandle ?? default;
            RuntimeMethodHandle setMethodHandle = propertyInfo.SetMethod?.MethodHandle ?? default;
            ParameterList parameterList = MethodParameterInfo.ConvertFrom(propertyInfo.GetIndexParameters());
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                typeHandle,
                default,
                getMethodHandle,
                setMethodHandle,
                default,
                default,
                parameterList,
                SymbolAttributes.Property);
        }

        public static SymbolInfoDataCacheKey CreateForMethod(MethodInfo methodInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));
            RuntimeTypeHandle declaringTypeHandle = methodInfo.DeclaringType.TypeHandle;
            string name = methodInfo.Name;
            RuntimeTypeHandle typeHandle = methodInfo.ReturnType.TypeHandle;
            RuntimeMethodHandle methodHandle = methodInfo.MethodHandle;
            ParameterList parameterList = MethodParameterInfo.ConvertFrom(methodInfo.GetParameters());
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                typeHandle,
                methodHandle,
                default,
                default,
                default,
                default,
                parameterList,
                SymbolAttributes.Method);
        }

        public static SymbolInfoDataCacheKey CreateForAnonymousMethod(RuntimeTypeHandle declaringTypeHandle, RuntimeTypeHandle methodReturnTypeHandle, string methodName, ParameterList methodParameters)
        {
            ArgumentNullExceptionEx.ThrowIfNullOrWhiteSpace(methodName, nameof(methodName));
            ArgumentNullExceptionEx.th(methodName, nameof(methodName));
            RuntimeTypeHandle declaringTypeHandle = methodInfo.DeclaringType.TypeHandle;
            string name = methodInfo.Name;
            RuntimeTypeHandle typeHandle = methodInfo.ReturnType.TypeHandle;
            RuntimeMethodHandle methodHandle = methodInfo.MethodHandle;
            ParameterList parameterList = MethodParameterInfo.ConvertFrom(methodInfo.GetParameters());
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                typeHandle,
                methodHandle,
                default,
                default,
                default,
                default,
                parameterList);
        }

        public static SymbolInfoDataCacheKey CreateForType(Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));
            RuntimeTypeHandle typeHandle = type.TypeHandle;
            RuntimeTypeHandle declaringTypeHandle = default;
            RuntimeTypeHandle methodTypeHandle = default;
            RuntimeMethodHandle methodHandle = default;
            ParameterList parameterList = ParameterList.Empty;
            MethodInfo invokeMethod = default;

            if (type.IsDelegate())
            {
                invokeMethod = type.GetMethod("Invoke");
                methodHandle = invokeMethod.MethodHandle;
                methodTypeHandle = invokeMethod.ReturnType.TypeHandle;
                declaringTypeHandle = typeHandle;
                parameterList = MethodParameterInfo.ConvertFrom(invokeMethod.GetParameters());
            }

            string name = type.FullName ?? type.Name;
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                invokeMethod is not null ? methodTypeHandle : typeHandle,
                methodHandle,
                default,
                default,
                default,
                default,
                parameterList);
        }

        public static SymbolInfoDataCacheKey CreateForField(FieldInfo fieldInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(fieldInfo, nameof(fieldInfo));
            RuntimeTypeHandle declaringTypeHandle = fieldInfo.DeclaringType.TypeHandle;
            string name = fieldInfo.Name;
            RuntimeTypeHandle fieldTypeHandle = fieldInfo.FieldType.TypeHandle;
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                fieldTypeHandle,
                default,
                default,
                default,
                default,
                default,
                ParameterList.Empty);
        }

        public static SymbolInfoDataCacheKey CreateForConstructor(ConstructorInfo constructorInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));
            RuntimeTypeHandle declaringTypeHandle = constructorInfo.DeclaringType.TypeHandle;
            string name = constructorInfo.Name;
            RuntimeMethodHandle methodHandle = constructorInfo.MethodHandle;
            ParameterList parameterList = MethodParameterInfo.ConvertFrom(constructorInfo.GetParameters());
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                default,
                methodHandle,
                default,
                default,
                default,
                default,
                parameterList);
        }

        public override int GetHashCode()
        {
            int hashCode = 1248511333;
            hashCode = (hashCode * -1521134295) + this.Name.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.DeclaringTypeHandle.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.TypeHandle.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.MethodHandle.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.GetMethodHandle.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.SetMethodHandle.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.AddMethodHandle.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.RemoveMethodHandle.GetHashCode();

            foreach (object argument in this.ParameterList)
            {
                hashCode = (hashCode * -1521134295) + argument.GetHashCode();
            }

            return hashCode;
        }

        public override bool Equals(object obj) => obj is SymbolInfoDataCacheKey other && Equals(other);

        public bool Equals(SymbolInfoDataCacheKey other) => this.Name == other.Name
            && this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
            && this.TypeHandle.Equals(other.TypeHandle)
            && this.MethodHandle.Equals(other.MethodHandle)
            && this.GetMethodHandle.Equals(other.GetMethodHandle)
            && this.SetMethodHandle.Equals(other.SetMethodHandle)
            && this.AddMethodHandle.Equals(other.AddMethodHandle)
            && this.RemoveMethodHandle.Equals(other.RemoveMethodHandle)
            && this.ParameterList, other.ParameterList);

        public static bool operator ==(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => left.Equals(right);
        public static bool operator !=(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => !(left == right);
    }
}
