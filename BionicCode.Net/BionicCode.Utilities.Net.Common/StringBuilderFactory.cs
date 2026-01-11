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

    internal class PooledStringBuilder : IDisposable
    {
        private const string StringBuilderRecycledExceptionMessage = "Underlying StringBuilder has been recycled. Create a new PooledStringBuilder instance.";
        private StringBuilder stringBuilder;
        public bool IsDisposed => this.IsRecycled;

        public bool IsRecycled => this.stringBuilder is null;
        public int Length => this.stringBuilder?.Length ?? throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);

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

        public PooledStringBuilder Append(int value)
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

        public PooledStringBuilder Append(bool value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(ReadOnlySpan<char> value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(StringBuilder value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value);

            return this;
        }

        public PooledStringBuilder Append(PooledStringBuilder value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(value.stringBuilder);

            return this;
        }

        public PooledStringBuilder AppendFormat(ReadOnlySpan<char> format, IFormatProvider? formatProvider, ReadOnlySpan<object?> values)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            IFormatProvider provider = formatProvider ?? System.Globalization.CultureInfo.CurrentCulture;
            CompositeFormat compositeFormat = CompositeFormat.Parse(format.ToString());
            _ = this.stringBuilder.AppendFormat(provider, compositeFormat, values);

            return this;
        }

        public PooledStringBuilder AppendJoin(string separator, ReadOnlySpan<string> values)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Append(values.JoinToString(separator));

            return this;
        }

        public PooledStringBuilder AppendJoin(string separator, IEnumerable<string> values)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.AppendJoin(separator, values);

            return this;
        }

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

        public PooledStringBuilder Insert(int index, string value)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Insert(index, value);

            return this;
        }

        public PooledStringBuilder Remove(int startIndex, int length)
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            _ = this.stringBuilder.Remove(startIndex, length);

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

        public override string ToString()
        {
            if (this.IsRecycled)
            {
                throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
            }

            return this.stringBuilder.ToString();
        }

        public void Recycle()
        {
            StringBuilderFactory.AddToPool(this.stringBuilder);
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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PooledStringBuilder()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
