namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Caching.Memory;

    internal static class StringBuilderFactory
    {
        private static readonly MemoryCache StringBuilderPool;
        private static readonly Queue<object> CacheEntryKeys;
        private static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(10);

        static StringBuilderFactory()
        {
            var cacheOptions = new MemoryCacheOptions()
            {
                // Item based
                SizeLimit = 10,
            };
            StringBuilderFactory.StringBuilderPool = new MemoryCache(cacheOptions);
            StringBuilderFactory.CacheEntryKeys = new Queue<object>();
        }

        public static PooledStringBuilder GetOrCreate()
          => GetOrCreateInternal(-1);

        private static PooledStringBuilder GetOrCreateInternal(int capacity)
        {
            while (StringBuilderFactory.CacheEntryKeys.Any())
            {
                object entryKey = StringBuilderFactory.CacheEntryKeys.Dequeue();
                if (StringBuilderFactory.StringBuilderPool.TryGetValue(entryKey, out StringBuilder entry))
                {
                    StringBuilderFactory.StringBuilderPool.Remove(entryKey);

                    if (capacity > -1)
                    {
                        entry.Capacity = System.Math.Min(entry.MaxCapacity, capacity);
                    }

                    return PooledStringBuilder.Create(entry);
                }
            }

            StringBuilder stringBuilder = capacity > -1
              ? new StringBuilder(capacity)
              : new StringBuilder();

            return PooledStringBuilder.Create(stringBuilder);
        }

        public static PooledStringBuilder GetOrCreateWith(string content)
          => content is null ? throw new ArgumentNullException(nameof(content)) : GetOrCreateInternal(-1).Append(content);

        public static PooledStringBuilder GetOrCreateWith(int capacity, string content)
          => content is null ? throw new ArgumentNullException(nameof(content)) : GetOrCreateInternal(capacity).Append(content);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP || NET
        public static PooledStringBuilder GetOrCreateWith(StringBuilder content)
          => content is null ? throw new ArgumentNullException(nameof(content)) : GetOrCreateInternal(-1).Append(content);
#else
    public static PooledStringBuilder GetOrCreateWith(StringBuilder content)
      => content is null ? throw new ArgumentNullException(nameof(content)) : GetOrCreateInternal(-1).Append(content);
#endif

        public static void Recycle(PooledStringBuilder stringBuilder)
        {
            if (stringBuilder is null)
            {
                return;
            }

            stringBuilder.Recycle();
        }

        public static void AddToPool(StringBuilder stringBuilder)
        {
            _ = stringBuilder.Clear();
            Guid cacheEntryKey = Guid.NewGuid();
            ICacheEntry entry = StringBuilderFactory.StringBuilderPool.CreateEntry(cacheEntryKey)
              .SetValue(stringBuilder)
              .SetSize(1)
              .SetSlidingExpiration(StringBuilderFactory.TimeToLive);
            StringBuilderFactory.CacheEntryKeys.Enqueue(entry);
        }
    }

    internal class PooledStringBuilder
    {
        private const string StringBuilderRecycledExceptionMessage = "Underlying StringBuilder has been recycled. Create a new PooledStringBuilder instance.";
        private StringBuilder stringBuilder;
        public bool IsRecycled => this.stringBuilder is null;
        
        public int Length
        {
            get => this.stringBuilder?.Length ?? throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            set
            {
                if (this.IsRecycled)
                {
                    throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
                }

                this.stringBuilder.Length = value;
            }
        }

        public int Capacity
        {
            get
            {
                if (this.IsRecycled)
                {
                    throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
                }

                return this.stringBuilder.Capacity;
            }
            set
            {
                if (this.IsRecycled)
                {
                    throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
                }

                this.stringBuilder.Capacity = value;
            }
        }

        public int MaxCapacity
        {
            get
            {
                if (this.IsRecycled)
                {
                    throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
                }

                return this.stringBuilder.MaxCapacity;
            }
        }

        public char this[int index]
        {
            get
            {
                if (this.IsRecycled)
                {
                    throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
                }

                return this.stringBuilder[index];
            }
            set
            {
                if (this.IsRecycled)
                {
                    throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
                }

                this.stringBuilder[index] = value;
            }
        }

        public static PooledStringBuilder Create()
          => StringBuilderFactory.GetOrCreate();

        public static PooledStringBuilder Create(StringBuilder stringBuilder)
          => new PooledStringBuilder(stringBuilder);

        private PooledStringBuilder(StringBuilder stringBuilder) => this.stringBuilder = stringBuilder;

        public PooledStringBuilder Append(string value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(char value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(byte value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(sbyte value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(short value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(ushort value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(int value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(uint value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(long value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(ulong value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(float value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(double value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(decimal value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(object value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(bool value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
        public PooledStringBuilder Append(ReadOnlySpan<char> value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }
#endif

        public PooledStringBuilder Append(StringBuilder value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public PooledStringBuilder Append(StringBuilder value, int startIndex, int count)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value, startIndex, count);

            return this;
        }
#endif

#if NET6_0_OR_GREATER
        public PooledStringBuilder Append(ReadOnlyMemory<char> value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value.Span);

            return this;
        }
#endif

        public PooledStringBuilder Append(PooledStringBuilder value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value.stringBuilder);

            return this;
        }

        public PooledStringBuilder AppendFormat(string format, params object[] args)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendFormat(format, args);

            return this;
        }

        public PooledStringBuilder AppendFormat(string format, object arg0)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendFormat(format, arg0);

            return this;
        }

        public PooledStringBuilder AppendFormat(string format, object arg0, object arg1)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendFormat(format, arg0, arg1);

            return this;
        }

        public PooledStringBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendFormat(format, arg0, arg1, arg2);

            return this;
        }

        public PooledStringBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendFormat(provider, format, args);

            return this;
        }

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public PooledStringBuilder AppendJoin(string separator, params string[] values)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendJoin(separator, values);

            return this;
        }

        public PooledStringBuilder AppendJoin(string separator, params object[] values)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendJoin(separator, values);

            return this;
        }

        public PooledStringBuilder AppendJoin<T>(string separator, IEnumerable<T> values)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendJoin(separator, values);

            return this;
        }

        public PooledStringBuilder AppendJoin(char separator, params string[] values)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendJoin(separator, values);

            return this;
        }

        public PooledStringBuilder AppendJoin(char separator, params object[] values)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendJoin(separator, values);

            return this;
        }

        public PooledStringBuilder AppendJoin<T>(char separator, IEnumerable<T> values)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendJoin(separator, values);

            return this;
        }
#endif

        public PooledStringBuilder AppendLine(string value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendLine(value);

            return this;
        }

        public PooledStringBuilder AppendLine()
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendLine();

            return this;
        }

        public PooledStringBuilder Append(char[] value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(char[] value, int startIndex, int charCount)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value, startIndex, charCount);

            return this;
        }

        public PooledStringBuilder Append(char value, int repeatCount)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value, repeatCount);

            return this;
        }

        public PooledStringBuilder Append(string value, int startIndex, int count)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value, startIndex, count);

            return this;
        }

        public PooledStringBuilder Insert(int index, string value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, string value, int count)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value, count);

            return this;
        }

        public PooledStringBuilder Insert(int index, char value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, char[] value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, char[] value, int startIndex, int charCount)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value, startIndex, charCount);

            return this;
        }

        public PooledStringBuilder Insert(int index, bool value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, byte value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, sbyte value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, short value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, int value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, long value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, float value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, double value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, decimal value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, ushort value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, uint value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, ulong value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Insert(int index, object value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
        public PooledStringBuilder Insert(int index, ReadOnlySpan<char> value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }
#endif

        public PooledStringBuilder Remove(int startIndex, int length)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Remove(startIndex, length);

            return this;
        }

        public PooledStringBuilder Replace(string oldValue, string newValue)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Replace(oldValue, newValue);

            return this;
        }

        public PooledStringBuilder Replace(string oldValue, string newValue, int startIndex, int count)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Replace(oldValue, newValue, startIndex, count);

            return this;
        }

        public PooledStringBuilder Replace(char oldChar, char newChar)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Replace(oldChar, newChar);

            return this;
        }

        public PooledStringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Replace(oldChar, newChar, startIndex, count);

            return this;
        }

        public PooledStringBuilder Clear()
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Clear();

            return this;
        }

        public new string ToString()
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            return this.stringBuilder.ToString();
        }

        public string ToString(int startIndex, int length)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            return this.stringBuilder.ToString(startIndex, length);
        }

        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            this.stringBuilder.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public void CopyTo(int sourceIndex, Span<char> destination, int count)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            this.stringBuilder.CopyTo(sourceIndex, destination, count);
        }
#endif

        public int EnsureCapacity(int capacity)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            return this.stringBuilder.EnsureCapacity(capacity);
        }

        public void Recycle()
        {
            StringBuilderFactory.AddToPool(this.stringBuilder);
            this.stringBuilder = null;
        }
    }
}
