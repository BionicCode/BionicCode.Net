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

          return new PooledStringBuilder(entry);
        }
      }

      StringBuilder stringBuilder = capacity > -1 
        ? new StringBuilder(capacity) 
        : new StringBuilder();
      return new PooledStringBuilder(stringBuilder);
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

    public PooledStringBuilder(StringBuilder stringBuilder) => this.stringBuilder = stringBuilder;

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

      _ = this.stringBuilder.AppendReadOnlySpan(value);

      return this;
    }

    public PooledStringBuilder Append(StringBuilder value)
    {
      if (this.IsRecycled)
      {
        throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
      }

      _ = this.stringBuilder.AppendStringBuilder(value);

      return this;
    }

    public PooledStringBuilder Append(PooledStringBuilder value)
    {
      if (this.IsRecycled)
      {
        throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
      }

      _ = this.stringBuilder.AppendStringBuilder(value.stringBuilder);

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

    public PooledStringBuilder AppendJoin(string separator, params string[] values)
    {
      if (this.IsRecycled)
      {
        throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);
      }

      _ = this.stringBuilder.AppendJoin(separator, values);

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

    public string ToString()
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
  }
}