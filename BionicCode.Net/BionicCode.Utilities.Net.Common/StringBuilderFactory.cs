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
        SizeLimit = 100,
      };
      StringBuilderFactory.StringBuilderPool = new MemoryCache(cacheOptions);
      StringBuilderFactory.CacheEntryKeys = new Queue<object>();
    }

    public static StringBuilder GetOrCreate()
    {
      while (StringBuilderFactory.CacheEntryKeys.Any())
      {
        object entryKey = StringBuilderFactory.CacheEntryKeys.Dequeue();
        if (StringBuilderFactory.StringBuilderPool.TryGetValue(entryKey, out StringBuilder entry))
        {
          StringBuilderFactory.StringBuilderPool.Remove(entryKey);
          return entry;
        }
      }

      return new StringBuilder();
    }

    public static StringBuilder GetOrCreateWith(string content)
      => GetOrCreate().Append(content);

    public static StringBuilder GetOrCreateWith(StringBuilder content)
      => GetOrCreate().Append(content);

    public static void Recycle(StringBuilder stringBuilder)
    {
      _ = stringBuilder.Clear();

      ICacheEntry entry = StringBuilderFactory.StringBuilderPool.CreateEntry(stringBuilder)
        .SetSize(1)
        .SetSlidingExpiration(StringBuilderFactory.TimeToLive);
      StringBuilderFactory.CacheEntryKeys.Enqueue(entry);
    }
  }
}