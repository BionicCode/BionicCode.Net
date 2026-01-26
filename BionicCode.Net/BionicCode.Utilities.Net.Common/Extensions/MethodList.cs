namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class MethodList : IReadOnlyList<MethodData>, IEquatable<MethodList>
    {
        public static readonly MethodList Empty = new MethodList();
        private readonly int _hashCode; // precomputed
        private readonly ILookup<string, MethodData> _methodNameIndex;
        private readonly SymbolInfoDataCacheKey _declaringTypeCacheKey;

        public MethodList(MethodData[] items) : this((IEnumerable<MethodData>)items)
        {
        }

        public MethodList(IEnumerable<MethodData> items)
        {
            this.Methods = items?.ToImmutableList() ?? ImmutableList<MethodData>.Empty;
            this._methodNameIndex = this.Methods.ToLookup(method => method.Name, StringComparer.Ordinal); // allow duplicate method names (overloads)

            if (this.HasItems)
            {
                this._declaringTypeCacheKey = this.Methods.First().DeclaringTypeData.CacheKey;

                ArgumentExceptionAdvanced.ThrowIfAny(
                    this.Methods,
                    methodData => methodData.DeclaringTypeData.CacheKey != this.DeclaringTypeCacheKey,
                    nameof(items),
                    $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(MethodData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All methods must belong to the same declaring type.");

            }

            this._hashCode = ComputeHashCode();
        }

        internal MethodList(IEnumerable<MethodData> items, bool isIntegrityValidationEnabled)
        {
            this.Methods = items?.ToImmutableList() ?? ImmutableList<MethodData>.Empty;

            // allow duplicate method names (overloads)
            this._methodNameIndex = this.Methods.ToLookup(method => method.Name, StringComparer.Ordinal);

            this._declaringTypeCacheKey = this.HasItems
                ? this.Methods.First().DeclaringTypeData.CacheKey
                : default;

            if (isIntegrityValidationEnabled && this.HasItems)
            {
                ArgumentExceptionAdvanced.ThrowIfAny(
                    this.Methods,
                    methodData => methodData.DeclaringTypeData.CacheKey != this.DeclaringTypeCacheKey,
                    nameof(items),
                    $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(MethodData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All methods must belong to the same declaring type.");

            }

            this._hashCode = ComputeHashCode();
        }

        private MethodList()
        {
            this.Methods = ImmutableList<MethodData>.Empty;
            this._methodNameIndex = this.Methods.ToLookup(method => method.Name, StringComparer.Ordinal);
        }

        public bool TryGetMethodsByName(string methodName, out MethodList methodList)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(methodName);
            methodList = this._methodNameIndex[methodName]
                .ToMethodList();

            return methodList.HasItems;
        }

        public int Count => this.Methods.Count;
        public bool IsEmpty => this.Methods.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<MethodData> Methods { get; }
        public SymbolInfoDataCacheKey DeclaringTypeCacheKey
            => this.HasItems
                ? this._declaringTypeCacheKey
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(this.DeclaringTypeCacheKey)));

        public TypeData DeclaringTypeData
        {
            get
            {
                SymbolInfoDataCacheKey cacheKey = this.DeclaringTypeCacheKey;
                return this.HasItems
                    ? SymbolReflectionInfoCache.GetOrCreateTypeDataCacheEntry(ref cacheKey)
                    : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(this.DeclaringTypeData)));
            }
        }

        public MethodData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Methods.Count, nameof(index));

                return this.HasItems
                    ? this.Methods[index]
                    : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), ReflectionConstants.IndexerGetMethodName));
            }
        }

        /// <summary>
        /// Gets the method data for the method with the specified name and parameter signature.
        /// </summary>
        /// <remarks>Use this indexer to retrieve method metadata when you know both the method name and
        /// the exact parameter signature. Parameter matching considers type, position, and modifier (such as ref or
        /// out).</remarks>
        /// <param name="methodName">The name of the method to retrieve. This value is case-sensitive.</param>
        /// <param name="methodParameters">An array of parameter information objects that describe the expected parameter types, positions, and
        /// modifiers for the method signature.</param>
        /// <returns>The method data that matches the specified name and parameter signature.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the method index has not been initialized.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no method with the specified name exists, or if no method with the specified name matches the
        /// provided parameter signature.</exception>
        public MethodData this[string methodName, params MethodParameterInfo[] methodParameters]
        {
            get
            {
                if (this._methodNameIndex == null)
                {
                    throw new InvalidOperationException("Method index is not initialized.");
                }

                MethodList methods = this._methodNameIndex[methodName].ToMethodList();
                if (methods.IsEmpty)
                {
                    throw new KeyNotFoundException($"Invalid key.No method named '{methodName}' could be found.");
                }

                foreach (MethodData method in methods)
                {
                    ParameterList parameters = method.Parameters;
                    bool isMethodInvalidCandidate = false;
                    int parameterIndex = 0;
                    foreach (ParameterData parameter in parameters)
                    {
                        if (parameters.Count != methodParameters.Length)
                        {
                            isMethodInvalidCandidate = true;
                            break;
                        }

                        // Parameter is valid if it matches position in method signature, parameter type and modifier.
                        foreach (MethodParameterInfo parameterInfo in methodParameters)
                        {
                            if (parameterIndex == parameterInfo.Position)
                            {
                                if (!parameter.ParameterTypeData.Handle.Equals(parameterInfo.ParameterTypeHandle)
                                    || !parameter.ParameterKind.Equals(parameterInfo.Kind))
                                {
                                    isMethodInvalidCandidate = true;
                                    break;
                                }
                            }
                        }

                        if (isMethodInvalidCandidate)
                        {
                            break;
                        }
                    }

                    if (!isMethodInvalidCandidate)
                    {
                        return method;
                    }
                }

                throw new KeyNotFoundException($"Invalid arguments. A method named '{methodName}' could be found but its signature does not match the provided parameter list of the '{nameof(methodParameters)}' argument. Ensure '{nameof(MethodParameterInfo)}.{nameof(MethodParameterInfo.ParameterTypeHandle)}' is referencing the correct type and the parameter is in the correct position expressed by collection index and the '{nameof(MethodParameterInfo)}.{nameof(MethodParameterInfo.Kind)}' describes the correct parameter modifier.");
            }
        }

        public IEnumerator<MethodData> GetEnumerator()
            => ((IEnumerable<MethodData>)this.Methods).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Methods.GetEnumerator();

        public bool Equals(MethodList? other)
        {
            if (other is null)
            {
                return false;
            }

            if (this.Count != other.Count)
            {
                return false;
            }

            if (this.DeclaringTypeCacheKey != other.DeclaringTypeCacheKey)
            {
                return false;
            }

            for (int index = 0; index < this.Count; index++)
            {
                if (!this.Methods[index].Equals(other.Methods[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
            => obj is MethodList other && Equals(other);

        public override int GetHashCode()
            => this._hashCode;

        private int ComputeHashCode()
        {
            unchecked
            {
                var hashCode = new HashCode();
                hashCode.Add(this.Count);
                hashCode.Add(this.DeclaringTypeCacheKey);
                for (int index = 0; index < this.Methods.Count; index++)
                {
                    hashCode.Add(this.Methods[index]);
                }

                return hashCode.ToHashCode();
            }
        }

        public static bool operator ==(MethodList? left, MethodList? right)
            => left?.Equals(right) ?? (right is null);
        public static bool operator !=(MethodList? left, MethodList? right)
            => !(left == right);
    }
}
