namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class MethodList : IReadOnlyList<MethodData>, IEquatable<MethodList>
    {
        public static readonly MethodList Empty = new MethodList(Array.Empty<MethodData>());
        private readonly int _hashCode; // precomputed

        public MethodList(MethodData[] items) : this((IEnumerable<MethodData>)items)
        {
        }

        public MethodList(IEnumerable<MethodData> items)
        {
            this.Methods = items.ToImmutableList();
            ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(this.Methods, nameof(items));
            this._methodNameIndex = this.Methods.ToLookup(method => method.Name); // allow duplicate method names (overloads)

            this.DeclaringTypeHandle = this.Methods.FirstOrDefault()!.DeclaringTypeHandle;

            ArgumentExceptionAdvanced.ThrowIfAny(
                this.Methods,
                methodData => !methodData.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(MethodData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All methods must belong to the same declaring type.");

            this._hashCode = ComputeHashCode();
        }

        private readonly ILookup<string, MethodData>? _methodNameIndex;
        public int Count => this.Methods.Count;
        public bool IsEmpty => this.Methods.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<MethodData> Methods { get; }
        public RuntimeTypeHandle DeclaringTypeHandle { get; }

        public MethodData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Methods.Count, nameof(index));

                return this.Methods[index];
            }
        }

        public IEnumerable<MethodData> this[string methodName]
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

                return methods;
            }
        }

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

                            // Parameter is valid if it matches position in method signature, parameter type and modifier.
                            // Using the parameter type's type handle simplifies the equality check.
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

            if (!this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle))
            {
                return false;
            }

            bool isEqual = false;
            for (int index = 0; index < this.Count && !isEqual; index++)
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

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode()
        {
            unchecked
            {
                var hashCode = new HashCode();
                hashCode.Add(this.Count);
                hashCode.Add(this.DeclaringTypeHandle);
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
