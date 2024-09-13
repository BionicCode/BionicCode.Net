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

    public static StringBuilder GetOrCreate()
      => GetOrCreateInternal(-1);

    private static StringBuilder GetOrCreateInternal(int capacity)
    {
      while (StringBuilderFactory.CacheEntryKeys.Any())
      {
        object entryKey = StringBuilderFactory.CacheEntryKeys.Dequeue();
        if (StringBuilderFactory.StringBuilderPool.TryGetValue(entryKey, out StringBuilder entry))
        {
          StringBuilderFactory.StringBuilderPool.Remove(entryKey);

          if (capacity > -1)
          {
            entry.Capacity = System.Math.Max(entry.MaxCapacity, capacity);
          }

          return entry;
        }
      }

      return new StringBuilder();
    }

    public static StringBuilder GetOrCreateWith(string content)
      => GetOrCreate().Append(content);

    public static StringBuilder GetOrCreateWith(int capacity, string content)
      => GetOrCreateInternal(capacity).Append(content);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP || NET
    public static StringBuilder GetOrCreateWith(StringBuilder content)
      => GetOrCreate().Append(content);
#else
    public static StringBuilder GetOrCreateWith(StringBuilder content)
    {
      char[] tempArray = new char[content.Length];
      content.CopyTo(0,  tempArray, 0, content.Length);
      return GetOrCreateInternal(-1).Append(content);
    }
#endif

    public static void Recycle(StringBuilder stringBuilder)
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
}