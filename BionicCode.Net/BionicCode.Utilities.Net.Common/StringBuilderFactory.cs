#nullable enable
namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal static class StringBuilderFactory
    {
        private const int MaxPoolSize = 10;
        private static readonly ConcurrentBag<StringBuilder> StringBuilderPool;
        private static readonly object SyncLock;

        static StringBuilderFactory()
        {
            StringBuilderFactory.StringBuilderPool = new ConcurrentBag<StringBuilder>();
            StringBuilderFactory.SyncLock = new object();
        }

        public static PooledStringBuilder GetOrCreate()
            => GetOrCreateInternal(0, ReadOnlySpan<char>.Empty);

        public static PooledStringBuilder GetOrCreate(ReadOnlySpan<char> content)
            => GetOrCreateInternal(0, content);

        public static PooledStringBuilder GetOrCreate(int capacity, ReadOnlySpan<char> content)
            => GetOrCreateInternal(capacity, content);

        private static PooledStringBuilder GetOrCreateInternal(int capacity, ReadOnlySpan<char> content)
        {
            if (!StringBuilderFactory.StringBuilderPool.TryTake(out StringBuilder? stringBuilder))
            {
                stringBuilder = new StringBuilder(capacity);
                if (!content.IsEmpty)
                {
                    _ = stringBuilder.Append(content);
                }
            }

            return PooledStringBuilder.CreateInternal(stringBuilder);
        }

        public static void Recycle(StringBuilder stringBuilder)
        {
            lock (StringBuilderFactory.SyncLock)
            {
                if (stringBuilder is null)
                {
                    return;
                }

                AddToPool(stringBuilder);
            }
        }

        private static void AddToPool(StringBuilder stringBuilder)
        {
            _ = stringBuilder.Clear();
            if (StringBuilderFactory.StringBuilderPool.Count < StringBuilderFactory.MaxPoolSize)
            {
                StringBuilderFactory.StringBuilderPool.Add(stringBuilder);
            }
        }
    }

    internal class PooledStringBuilder : IDisposable
    {
        private const string StringBuilderRecycledExceptionMessage = "Underlying StringBuilder has been recycled. Create a new PooledStringBuilder instance.";
        private StringBuilder? stringBuilder;

        public bool IsDisposed => this.IsRecycled;

        public bool IsRecycled => this.stringBuilder is null;

        public int Capacity
        {
            get => GetStringBuilderOrThrowIfRecycled().Capacity;
            set => GetStringBuilderOrThrowIfRecycled().Capacity = value;
        }

        public int MaxCapacity => GetStringBuilderOrThrowIfRecycled().MaxCapacity;

        public int Length
        {
            get => GetStringBuilderOrThrowIfRecycled().Length;
            set => GetStringBuilderOrThrowIfRecycled().Length = value;
        }

        public char this[int index]
        {
            get => GetStringBuilderOrThrowIfRecycled()[index];
            set => GetStringBuilderOrThrowIfRecycled()[index] = value;
        }

        public static PooledStringBuilder Create()
            => StringBuilderFactory.GetOrCreate();

        public static PooledStringBuilder Create(StringBuilder stringBuilder)
            => new PooledStringBuilder(stringBuilder);

        internal static PooledStringBuilder CreateInternal(StringBuilder stringBuilder)
            => new PooledStringBuilder(stringBuilder);

        private PooledStringBuilder(StringBuilder stringBuilder) => this.stringBuilder = stringBuilder;

        private StringBuilder GetStringBuilderOrThrowIfRecycled()
            => this.stringBuilder ?? throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);

        // ============================================================================
        // CAPACITY & CONVERSION METHODS
        // ============================================================================

        public int EnsureCapacity(int capacity)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            return builder.EnsureCapacity(capacity);
        }

        public override string ToString()
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            return builder.ToString();
        }

        public string ToString(int startIndex, int length)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            return builder.ToString(startIndex, length);
        }

        public PooledStringBuilder Clear()
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Clear();
            return this;
        }

        // ============================================================================
        // ENUMERATION
        // ============================================================================

        public StringBuilder.ChunkEnumerator GetChunks()
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            return builder.GetChunks();
        }

        // ============================================================================
        // APPEND METHODS
        // ============================================================================

        public PooledStringBuilder Append(char value, int repeatCount)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value, repeatCount);
            return this;
        }

        public PooledStringBuilder Append(char value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(bool value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(sbyte value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(byte value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(short value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(int value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(long value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(float value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(double value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(decimal value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(ushort value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(uint value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(ulong value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(object? value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(string? value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(string? value, int startIndex, int count)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value, startIndex, count);
            return this;
        }

        public PooledStringBuilder Append(char[]? value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(char[]? value, int startIndex, int charCount)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value, startIndex, charCount);
            return this;
        }

        public PooledStringBuilder Append(ReadOnlySpan<char> value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(ReadOnlyMemory<char> value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(ReadOnlySpan<char> value, int startIndex, int count)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value.Slice(startIndex, count));
            return this;
        }

        public PooledStringBuilder Append(StringBuilder? value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value);
            return this;
        }

        public PooledStringBuilder Append(StringBuilder? value, int startIndex, int count)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value, startIndex, count);
            return this;
        }

        public unsafe PooledStringBuilder Append(char* value, int valueCount)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Append(value, valueCount);
            return this;
        }

        public PooledStringBuilder Append([InterpolatedStringHandlerArgument("")] ref PooledStringBuilder.AppendInterpolatedStringHandler handler)
            => this;

        public PooledStringBuilder Append(IFormatProvider? provider, [InterpolatedStringHandlerArgument("", nameof(provider))] ref PooledStringBuilder.AppendInterpolatedStringHandler handler)
            => this;

        // ============================================================================
        // APPENDLINE METHODS
        // ============================================================================

        public PooledStringBuilder AppendLine()
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendLine();
            return this;
        }

        public PooledStringBuilder AppendLine(string? value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendLine(value);
            return this;
        }

        public PooledStringBuilder AppendLine([InterpolatedStringHandlerArgument("")] ref PooledStringBuilder.AppendInterpolatedStringHandler handler)
            => AppendLine();

        public PooledStringBuilder AppendLine(IFormatProvider? provider, [InterpolatedStringHandlerArgument("", nameof(provider))] ref PooledStringBuilder.AppendInterpolatedStringHandler handler)
            => AppendLine();

        // ============================================================================
        // APPENDJOIN METHODS
        // ============================================================================

        public PooledStringBuilder AppendJoin(string? separator, params object?[] values)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendJoin(separator, values);
            return this;
        }

        public PooledStringBuilder AppendJoin<T>(string? separator, IEnumerable<T> values)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendJoin(separator, values);
            return this;
        }

        public PooledStringBuilder AppendJoin(string? separator, params string?[] values)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendJoin(separator, values);
            return this;
        }

        public PooledStringBuilder AppendJoin(char separator, params object?[] values)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendJoin(separator, values);
            return this;
        }

        public PooledStringBuilder AppendJoin<T>(char separator, IEnumerable<T> values)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendJoin(separator, values);
            return this;
        }

        public PooledStringBuilder AppendJoin(char separator, params string?[] values)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendJoin(separator, values);
            return this;
        }

#if NET9_0_OR_GREATER

        public PooledStringBuilder AppendJoin(string? separator, params ReadOnlySpan<object?> values)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendJoin(separator, values);
            return this;
        }

        public PooledStringBuilder AppendJoin(string? separator, params ReadOnlySpan<string?> values)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendJoin(separator, values);
            return this;
        }

        public PooledStringBuilder AppendJoin(char separator, params ReadOnlySpan<object?> values)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendJoin(separator, values);
            return this;
        }

        public PooledStringBuilder AppendJoin(char separator, params ReadOnlySpan<string?> values)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendJoin(separator, values);
            return this;
        }

#endif

        // ============================================================================
        // APPENDFORMAT METHODS - String format and CompositeFormat
        // ============================================================================

        public PooledStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(format, arg0);
            return this;
        }

        public PooledStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(format, arg0, arg1);
            return this;
        }

        public PooledStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1, object? arg2)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(format, arg0, arg1, arg2);
            return this;
        }

        public PooledStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(format, args);
            return this;
        }

        public PooledStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(provider, format, arg0);
            return this;
        }

        public PooledStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(provider, format, arg0, arg1);
            return this;
        }

        public PooledStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1, object? arg2)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(provider, format, arg0, arg1, arg2);
            return this;
        }

        public PooledStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(provider, format, args);
            return this;
        }

#if NET9_0_OR_GREATER

        public PooledStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params ReadOnlySpan<object?> args)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(format, args);
            return this;
        }

        public PooledStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params ReadOnlySpan<object?> args)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(provider, format, args);
            return this;
        }

#endif

        public PooledStringBuilder AppendFormat<TArg0>(IFormatProvider? provider, CompositeFormat format, TArg0 arg0)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(provider, format, arg0);
            return this;
        }

        public PooledStringBuilder AppendFormat<TArg0, TArg1>(IFormatProvider? provider, CompositeFormat format, TArg0 arg0, TArg1 arg1)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(provider, format, arg0, arg1);
            return this;
        }

        public PooledStringBuilder AppendFormat<TArg0, TArg1, TArg2>(IFormatProvider? provider, CompositeFormat format, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(provider, format, arg0, arg1, arg2);
            return this;
        }

        public PooledStringBuilder AppendFormat(IFormatProvider? provider, CompositeFormat format, params object?[] args)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(provider, format, args);
            return this;
        }

#if NET9_0_OR_GREATER

        public PooledStringBuilder AppendFormat(IFormatProvider? provider, CompositeFormat format, params ReadOnlySpan<object?> args)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.AppendFormat(provider, format, args);
            return this;
        }

#endif

        // ============================================================================
        // INSERT METHODS
        // ============================================================================

        public PooledStringBuilder Insert(int index, string? value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, string? value, int count)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value, count);
            return this;
        }

        public PooledStringBuilder Insert(int index, bool value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, sbyte value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, byte value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, short value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, char value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, char[]? value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, string? value, int startIndex, int count)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value?[startIndex..(startIndex + count)]);
            return this;
        }

        public PooledStringBuilder Insert(int index, char[]? value, int startIndex, int charCount)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value, startIndex, charCount);
            return this;
        }

        public PooledStringBuilder Insert(int index, int value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, long value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, float value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, double value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, decimal value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, ushort value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, uint value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, ulong value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, object? value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        public PooledStringBuilder Insert(int index, ReadOnlySpan<char> value)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Insert(index, value);
            return this;
        }

        // ============================================================================
        // REMOVE & REPLACE
        // ============================================================================

        public PooledStringBuilder Remove(int startIndex, int length)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Remove(startIndex, length);
            return this;
        }

        public PooledStringBuilder Replace(string oldValue, string? newValue)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Replace(oldValue, newValue);
            return this;
        }

        public PooledStringBuilder Replace(string oldValue, string? newValue, int startIndex, int count)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Replace(oldValue, newValue, startIndex, count);
            return this;
        }

        public PooledStringBuilder Replace(char oldChar, char newChar)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Replace(oldChar, newChar);
            return this;
        }

        public PooledStringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Replace(oldChar, newChar, startIndex, count);
            return this;
        }

#if NET9_0_OR_GREATER

        public PooledStringBuilder Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Replace(oldValue, newValue);
            return this;
        }

        public PooledStringBuilder Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue, int startIndex, int count)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            _ = builder.Replace(oldValue, newValue, startIndex, count);
            return this;
        }

#endif

        // ============================================================================
        // COPY
        // ============================================================================

        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            builder.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

        public void CopyTo(int sourceIndex, Span<char> destination, int count)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            builder.CopyTo(sourceIndex, destination, count);
        }

        // ============================================================================
        // EQUALS
        // ============================================================================

        public bool Equals([NotNullWhen(true)] StringBuilder? sb)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            return builder.Equals(sb);
        }

        public bool Equals(ReadOnlySpan<char> span)
        {
            StringBuilder builder = GetStringBuilderOrThrowIfRecycled();
            return builder.Equals(span);
        }

        // ----------------------------
        // Lifetime / pooling
        // ----------------------------

        public void Recycle()
        {
            if (this.stringBuilder is null)
            {
                return;
            }

            StringBuilderFactory.Recycle(this.stringBuilder);
            this.stringBuilder = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    Recycle();
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // Nested handler that wraps StringBuilder's handler
        [InterpolatedStringHandler]
        public ref struct AppendInterpolatedStringHandler
        {
            private StringBuilder.AppendInterpolatedStringHandler _inner;

            // Constructor for Append($"...")
            public AppendInterpolatedStringHandler(
                int literalLength,
                int formattedCount,
                PooledStringBuilder pooledBuilder)
            {
                StringBuilder wrappedStringBuilder = pooledBuilder.GetStringBuilderOrThrowIfRecycled();

                // Extract the inner StringBuilder and pass it to the real handler
                this._inner = new StringBuilder.AppendInterpolatedStringHandler(
                    literalLength,
                    formattedCount,
                    wrappedStringBuilder);
            }

            // Constructor for Append(provider, $"...")
            public AppendInterpolatedStringHandler(
                int literalLength,
                int formattedCount,
                PooledStringBuilder pooledBuilder,
                IFormatProvider? provider)
            {
                StringBuilder wrappedStringBuilder = pooledBuilder.GetStringBuilderOrThrowIfRecycled();

                this._inner = new StringBuilder.AppendInterpolatedStringHandler(
                    literalLength,
                    formattedCount,
                    wrappedStringBuilder,
                    provider);
            }

            // Forward all calls to the inner handler
            public void AppendLiteral(string value)
                => this._inner.AppendLiteral(value);

            public void AppendFormatted<T>(T value)
                => this._inner.AppendFormatted(value);

            public void AppendFormatted<T>(T value, string? format)
                => this._inner.AppendFormatted(value, format);

            public void AppendFormatted<T>(T value, int alignment)
                => this._inner.AppendFormatted(value, alignment);

            public void AppendFormatted<T>(T value, int alignment, string? format)
                => this._inner.AppendFormatted(value, alignment, format);

            public void AppendFormatted(ReadOnlySpan<char> value)
                => this._inner.AppendFormatted(value);

            public void AppendFormatted(ReadOnlySpan<char> value, int alignment = 0, string? format = null)
                => this._inner.AppendFormatted(value, alignment, format);

            public void AppendFormatted(string? value)
                => this._inner.AppendFormatted(value);

            public void AppendFormatted(string? value, int alignment = 0, string? format = null)
                => this._inner.AppendFormatted(value, alignment, format);

            public void AppendFormatted(object? value, int alignment = 0, string? format = null)
                => this._inner.AppendFormatted(value, alignment, format);
        }
    }
}
