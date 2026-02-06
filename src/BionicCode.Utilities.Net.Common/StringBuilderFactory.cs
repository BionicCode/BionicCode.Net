#nullable enable
namespace BionicCode.Utilities.Net;

using System;
using System.Collections.Concurrent;
using System.Text;

internal static class StringBuilderFactory
{
    private const int MaxPoolSize = 10;
    private static readonly ConcurrentBag<StringBuilder> s_stringBuilderPool = new ConcurrentBag<StringBuilder>();
    private static readonly object s_syncLock = new object();

    public static PooledStringBuilder GetOrCreate()
        => GetOrCreateInternal(0, ReadOnlySpan<char>.Empty);

    public static PooledStringBuilder GetOrCreate(ReadOnlySpan<char> content)
        => GetOrCreateInternal(0, content);

    public static PooledStringBuilder GetOrCreate(int capacity, ReadOnlySpan<char> content)
        => GetOrCreateInternal(capacity, content);

    private static PooledStringBuilder GetOrCreateInternal(int capacity, ReadOnlySpan<char> content)
    {
        if (!StringBuilderFactory.s_stringBuilderPool.TryTake(out StringBuilder? stringBuilder))
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
        lock (StringBuilderFactory.s_syncLock)
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
        if (StringBuilderFactory.s_stringBuilderPool.Count < StringBuilderFactory.MaxPoolSize)
        {
            StringBuilderFactory.s_stringBuilderPool.Add(stringBuilder);
        }
    }
}
