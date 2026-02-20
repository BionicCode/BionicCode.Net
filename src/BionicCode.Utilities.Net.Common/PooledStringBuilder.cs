#nullable enable
namespace BionicCode.Utilities.Net;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

public class PooledStringBuilder : IDisposable
{
    private const string StringBuilderRecycledExceptionMessage = "Underlying StringBuilder has been recycled. Create a new PooledStringBuilder instance.";
    public const char DefaultIndentationChar = ' ';
    public const int DefaultIndentationLevel = 4;
    private StringBuilder? _stringBuilder;
    private StringBuilder StringBuilder
        => _stringBuilder ?? throw new InvalidOperationException(PooledStringBuilder.StringBuilderRecycledExceptionMessage);

    public char IndentationChar { get; set; } = DefaultIndentationChar;

    public bool IsDisposed => IsRecycled;

    public bool IsRecycled => _stringBuilder is null;

    public int Capacity
    {
        get => StringBuilder.Capacity;
        set => StringBuilder.Capacity = value;
    }

    public int MaxCapacity => StringBuilder.MaxCapacity;

    public int Length
    {
        get => StringBuilder.Length;
        set => StringBuilder.Length = value;
    }

    public char this[int index]
    {
        get => StringBuilder[index];
        set => StringBuilder[index] = value;
    }

    private PooledStringBuilder(StringBuilder stringBuilder)
        => _stringBuilder = stringBuilder;

    public static PooledStringBuilder GetOrCreate()
        => StringBuilderFactory.GetOrCreate();

    public static PooledStringBuilder GetOrCreate(ReadOnlySpan<char> seed)
        => StringBuilderFactory.GetOrCreate(seed);

    public static PooledStringBuilder Create(StringBuilder stringBuilder)
        => new(stringBuilder);

    internal static PooledStringBuilder CreateInternal(StringBuilder stringBuilder)
        => new(stringBuilder);

    // ============================================================================
    // CAPACITY & CONVERSION METHODS
    // ============================================================================

    public int EnsureCapacity(int capacity)
    {
        StringBuilder builder = StringBuilder;
        return builder.EnsureCapacity(capacity);
    }

    public override string ToString()
    {
        StringBuilder builder = StringBuilder;
        return builder.ToString();
    }

    public string ToString(int startIndex, int length)
    {
        StringBuilder builder = StringBuilder;
        return builder.ToString(startIndex, length);
    }

    public PooledStringBuilder Clear()
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Clear();
        return this;
    }

    // ============================================================================
    // ENUMERATION
    // ============================================================================

    public StringBuilder.ChunkEnumerator GetChunks()
    {
        StringBuilder builder = StringBuilder;
        return builder.GetChunks();
    }

    // ============================================================================
    // APPEND METHODS
    // ============================================================================

    public PooledStringBuilder Append(char value, int repeatCount)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(repeatCount);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(value, repeatCount);
        return this;
    }

    public PooledStringBuilder Append(char value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(char value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(IndentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(bool value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(bool value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(IndentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(sbyte value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(sbyte value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(IndentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(byte value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(byte value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(IndentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(short value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(short value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(IndentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(int value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(int value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(long value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(long value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(float value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(float value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(double value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(double value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(decimal value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(decimal value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(ushort value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(ushort value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(uint value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(uint value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(ulong value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(ulong value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(object? value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(object? value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(string? value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(string value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(string? value, int startIndex, int count)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value, startIndex, count);
        return this;
    }

    public PooledStringBuilder AppendIndented(string? value, int startIndex, int count, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value, startIndex, count);
        return this;
    }

    public PooledStringBuilder Append(char[]? value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(char[]? value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(char[]? value, int startIndex, int charCount)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value, startIndex, charCount);
        return this;
    }

    public PooledStringBuilder AppendIndented(char[]? value, int startIndex, int charCount, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value, startIndex, charCount);
        return this;
    }

    public PooledStringBuilder Append(ReadOnlySpan<char> value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(ReadOnlySpan<char> value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(ReadOnlyMemory<char> value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(ReadOnlyMemory<char> value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(ReadOnlySpan<char> value, int startIndex, int count)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value.Slice(startIndex, count));
        return this;
    }

    public PooledStringBuilder AppendIndented(ReadOnlySpan<char> value, int startIndex, int count, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value.Slice(startIndex, count));
        return this;
    }

    public PooledStringBuilder Append(StringBuilder? value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value);
        return this;
    }

    public PooledStringBuilder AppendIndented(StringBuilder? value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value);
        return this;
    }

    public PooledStringBuilder Append(StringBuilder? value, int startIndex, int count)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value, startIndex, count);
        return this;
    }

    public PooledStringBuilder AppendIndented(StringBuilder? value, int startIndex, int count, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value, startIndex, count);
        return this;
    }

    public unsafe PooledStringBuilder Append(char* value, int valueCount)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Append(value, valueCount);
        return this;
    }

    public unsafe PooledStringBuilder AppendIndented(char* value, int valueCount, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .Append(value, valueCount);
        return this;
    }

    public PooledStringBuilder Append([InterpolatedStringHandlerArgument("")] ref PooledStringBuilder.AppendInterpolatedStringHandler handler)
        => this;

    public PooledStringBuilder AppendIndented(int indentationLevel, char indentationChar, [InterpolatedStringHandlerArgument("", nameof(indentationLevel), nameof(indentationChar))] ref PooledStringBuilder.AppendInterpolatedStringHandler handler)
        => this;

    public PooledStringBuilder Append(IFormatProvider? provider, [InterpolatedStringHandlerArgument("", nameof(provider))] ref PooledStringBuilder.AppendInterpolatedStringHandler handler)
        => this;

    public PooledStringBuilder AppendIndented(IFormatProvider? provider, int indentationLevel, char indentationChar, [InterpolatedStringHandlerArgument("", nameof(provider), nameof(indentationLevel), nameof(indentationChar))] ref PooledStringBuilder.AppendInterpolatedStringHandler handler)
        => this;

    // ============================================================================
    // APPENDLINE METHODS
    // ============================================================================

    public PooledStringBuilder AppendLine()
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendLine();
        return this;
    }

    /// <summary>
    /// Appends the default line terminator to the end of the current <see cref="PooledStringBuilder"/> object 
    /// <br/>and indents the next line using the specified indentation character and level, followed by the default line terminator.
    /// </summary>
    /// <param name="indentationLevel">The level of indentation to apply. The default is <see cref="DefaultIndentationLevel"/>.</param>
    /// <param name="indentationChar">The character to use for indentation. The default is <see cref="DefaultIndentationChar"/>.</param>
    /// <returns>The current <see cref="PooledStringBuilder"/> instance.</returns>
    public PooledStringBuilder AppendIndentedLine(int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.AppendLine()
            .Append(indentationChar, indentationLevel);
        return this;
    }

    /// <summary>
    /// Appends the specified string followed by the default line terminator to the current instance of the
    /// PooledStringBuilder.
    /// </summary>
    /// <remarks>This method is useful for efficiently building multi-line strings. The line terminator
    /// appended is determined by the underlying StringBuilder implementation.</remarks>
    /// <param name="value">The string to append. If <see langword="null"/>, only the line terminator is appended.</param>
    /// <returns>The current instance of the <see cref="PooledStringBuilder"/>, enabling method chaining.</returns>
    public PooledStringBuilder AppendLine(string? value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendLine(value);
        return this;
    }

    /// <summary>
    /// Appends a value using the specified indentation character and level, followed by the default line terminator. The indentation is applied before the value.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <param name="indentationLevel">The level of indentation to apply. The default is <see cref="DefaultIndentationLevel"/>.</param>
    /// <param name="indentationChar">The character to use for indentation. The default is <see cref="DefaultIndentationChar"/>.</param>
    /// <returns>The current <see cref="PooledStringBuilder"/> instance.</returns>
    public PooledStringBuilder AppendIndentedLine(string value, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .AppendLine(value);
        return this;
    }

    public PooledStringBuilder AppendLine([InterpolatedStringHandlerArgument("")] ref PooledStringBuilder.AppendInterpolatedStringHandler handler)
        => AppendLine();

    public PooledStringBuilder AppendIndentedLine(int indentationLevel, char indentationChar, [InterpolatedStringHandlerArgument("", nameof(indentationLevel), nameof(indentationChar))] ref PooledStringBuilder.AppendInterpolatedStringHandler handler)
        => AppendIndentedLine(indentationLevel, indentationChar);

    public PooledStringBuilder AppendLine(IFormatProvider? provider, [InterpolatedStringHandlerArgument("", nameof(provider))] ref PooledStringBuilder.AppendInterpolatedStringHandler handler)
        => AppendLine();

    public PooledStringBuilder AppendIndentedLine(IFormatProvider? provider, int indentationLevel, char indentationChar, [InterpolatedStringHandlerArgument("", nameof(provider), nameof(indentationLevel), nameof(indentationChar))] ref PooledStringBuilder.AppendInterpolatedStringHandler handler)
        => AppendIndentedLine(indentationLevel, indentationChar);

    // ============================================================================
    // APPENDJOIN METHODS
    // ============================================================================

    public PooledStringBuilder AppendJoin(string? separator, params object?[] values)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendJoin(separator, values);
        return this;
    }

    /// <summary>
    /// Appends the specified values to the current instance, joined by a separator and preceded by a repeated
    /// indentation character.
    /// </summary>
    /// <remarks>This method is useful for formatting structured text output with indentation, such as
    /// generating code or hierarchical data representations.</remarks>
    /// <param name="separator">The string to use as a separator between the values. If null, no separator is inserted.</param>
    /// <param name="indentationLevel">The number of times to repeat the indentation character. Must be zero or greater. The default is <see cref="DefaultIndentationLevel"/>.</param>
    /// <param name="indentationChar">The character to use for indentation. Defaults to a space character if not specified. The default is <see cref="DefaultIndentationChar"/>.</param>
    /// <param name="values">An array of objects to join and append. Each <see cref="object"/> is converted to its <see cref="string"/> representation.</param>
    /// <returns>The current instance of the <see cref="PooledStringBuilder"/>, enabling method chaining.</returns>
    public PooledStringBuilder AppendIndentedJoin(string? separator, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar, params object?[] values)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .AppendJoin(separator, values);
        return this;
    }

    public PooledStringBuilder AppendJoin<T>(string? separator, IEnumerable<T> values)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendJoin(separator, values);
        return this;
    }

    /// <summary>
    /// Appends the specified values to the current instance, joined by a separator and preceded by a repeated
    /// indentation character.
    /// </summary>
    /// <remarks>This method is useful for formatting structured text output with indentation, such as
    /// generating code or hierarchical data representations.</remarks>
    /// <typeparam name="T">The type of the elements in the collection to join. Each element is converted to its string representation.</typeparam>
    /// <param name="separator">The string to use as a separator between the values. If null, no separator is inserted.</param>
    /// <param name="indentationLevel">The number of times to repeat the indentation character. Must be zero or greater. The default is <see cref="DefaultIndentationLevel"/>.</param>
    /// <param name="indentationChar">The character to use for indentation. Defaults to a space character if not specified. The default is <see cref="DefaultIndentationChar"/>.</param>
    /// <param name="values">A collection of objects to join and append. Each item is converted to its <see cref="string"/> representation.</param>
    /// <returns>The current instance of the <see cref="PooledStringBuilder"/>, enabling method chaining.</returns>
    public PooledStringBuilder AppendIndentedJoin<T>(string? separator, IEnumerable<T> values, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .AppendJoin(separator, values);
        return this;
    }

    public PooledStringBuilder AppendJoin(string? separator, params string?[] values)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendJoin(separator, values);
        return this;
    }

    /// <summary>
    /// Appends the specified values to the current instance, joined by a separator and preceded by a repeated
    /// indentation character.
    /// </summary>
    /// <remarks>This method is useful for formatting structured text output with indentation, such as
    /// generating code or hierarchical data representations.</remarks>
    /// <param name="separator">The string to use as a separator between the values. If null, no separator is inserted.</param>
    /// <param name="indentationLevel">The number of times to repeat the indentation character. Must be zero or greater. The default is <see cref="DefaultIndentationLevel"/>.</param>
    /// <param name="indentationChar">The character to use for indentation. Defaults to a space character if not specified. The default is <see cref="DefaultIndentationChar"/>.</param>
    /// <param name="values">An array of <see cref="string"/> to join and append.</param>
    /// <returns>The current instance of the <see cref="PooledStringBuilder"/>, enabling method chaining.</returns>
    public PooledStringBuilder AppendIndentedJoin(string? separator, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar, params string?[] values)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .AppendJoin(separator, values);
        return this;
    }

    public PooledStringBuilder AppendJoin(char separator, params object?[] values)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendJoin(separator, values);
        return this;
    }

    /// <summary>
    /// Appends the specified values to the current instance, joined by a separator and preceded by a repeated
    /// indentation character.
    /// </summary>
    /// <remarks>This method is useful for formatting structured text output with indentation, such as
    /// generating code or hierarchical data representations.</remarks>
    /// <param name="separator">The character to use as a separator between the values.</param>
    /// <param name="indentationLevel">The number of times to repeat the indentation character. Must be zero or greater. The default is <see cref="DefaultIndentationLevel"/>.</param>
    /// <param name="indentationChar">The character to use for indentation. Defaults to a space character if not specified. The default is <see cref="DefaultIndentationChar"/>.</param>
    /// <param name="values">An array of objects to join and append. Each <see cref="object"/> is converted to its <see cref="string"/> representation.</param>
    /// <returns>The current instance of the <see cref="PooledStringBuilder"/>, enabling method chaining.</returns>
    public PooledStringBuilder AppendIndentedJoin(char separator, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar, params object?[] values)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .AppendJoin(separator, values);
        return this;
    }

    public PooledStringBuilder AppendJoin<T>(char separator, IEnumerable<T> values)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendJoin(separator, values);
        return this;
    }

    /// <summary>
    /// Appends the specified values to the current instance, joined by a separator and preceded by a repeated
    /// indentation character.
    /// </summary>
    /// <remarks>This method is useful for formatting structured text output with indentation, such as
    /// generating code or hierarchical data representations.</remarks>
    /// <typeparam name="T">The type of the elements in the collection to join. Each element is converted to its string representation.</typeparam>
    /// <param name="separator">The character to use as a separator between the values.</param>
    /// <param name="indentationLevel">The number of times to repeat the indentation character. Must be zero or greater. The default is <see cref="DefaultIndentationLevel"/>.</param>
    /// <param name="indentationChar">The character to use for indentation. Defaults to a space character if not specified. The default is <see cref="DefaultIndentationChar"/>.</param>
    /// <param name="values">A collection of objects to join and append. Each object is converted to its <see cref="string"/> representation.</param>
    /// <returns>The current instance of the <see cref="PooledStringBuilder"/>, enabling method chaining.</returns>
    public PooledStringBuilder AppendIndentedJoin<T>(char separator, IEnumerable<T> values, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .AppendJoin(separator, values);
        return this;
    }

    public PooledStringBuilder AppendJoin(char separator, params string?[] values)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendJoin(separator, values);
        return this;
    }

    /// <summary>
    /// Appends the specified values to the current instance, joined by a separator and preceded by a repeated
    /// indentation character.
    /// </summary>
    /// <remarks>This method is useful for formatting structured text output with indentation, such as
    /// generating code or hierarchical data representations.</remarks>
    /// <param name="separator">The character to use as a separator between the values.</param>
    /// <param name="indentationLevel">The number of times to repeat the indentation character. Must be zero or greater. The default is <see cref="DefaultIndentationLevel"/>.</param>
    /// <param name="indentationChar">The character to use for indentation. Defaults to a space character if not specified. The default is <see cref="DefaultIndentationChar"/>.</param>
    /// <param name="values">An array of <see cref="string"/> to join and append.</param>
    /// <returns>The current instance of the <see cref="PooledStringBuilder"/>, enabling method chaining.</returns>
    public PooledStringBuilder AppendIndentedJoin(char separator, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar, params string?[] values)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .AppendJoin(separator, values);
        return this;
    }

#if NET9_0_OR_GREATER

    public PooledStringBuilder AppendJoin(string? separator, params ReadOnlySpan<object?> values)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendJoin(separator, values);
        return this;
    }

    /// <summary>
    /// Appends the specified values to the current instance, joined by a separator and preceded by a repeated
    /// indentation character.
    /// </summary>
    /// <remarks>This method is useful for formatting structured text output with indentation, such as
    /// generating code or hierarchical data representations.</remarks>
    /// <param name="separator">The string to use as a separator between the values. If null, no separator is inserted.</param>
    /// <param name="indentationLevel">The number of times to repeat the indentation character. Must be zero or greater. The default is <see cref="DefaultIndentationLevel"/>.</param>
    /// <param name="indentationChar">The character to use for indentation. Defaults to a space character if not specified. The default is <see cref="DefaultIndentationChar"/>.</param>
    /// <param name="values">An array of <see cref="ReadOnlySpan{T}"/> to join and append. Each <see cref="object"/> is converted to its <see cref="string"/> representation.</param>
    /// <returns>The current instance of the <see cref="PooledStringBuilder"/>, enabling method chaining.</returns>
    public PooledStringBuilder AppendIndentedJoin(string? separator, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar, params ReadOnlySpan<object?> values)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .AppendJoin(separator, values);
        return this;
    }

    public PooledStringBuilder AppendJoin(string? separator, params ReadOnlySpan<string?> values)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendJoin(separator, values);
        return this;
    }

    /// <summary>
    /// Appends the specified values to the current instance, joined by a separator and preceded by a repeated
    /// indentation character.
    /// </summary>
    /// <remarks>This method is useful for formatting structured text output with indentation, such as
    /// generating code or hierarchical data representations.</remarks>
    /// <param name="separator">The string to use as a separator between the values. If null, no separator is inserted.</param>
    /// <param name="indentationLevel">The number of times to repeat the indentation character. Must be zero or greater. The default is <see cref="DefaultIndentationLevel"/>.</param>
    /// <param name="indentationChar">The character to use for indentation. Defaults to a space character if not specified. The default is <see cref="DefaultIndentationChar"/>.</param>
    /// <param name="values">An array of <see cref="ReadOnlySpan{T}"/> to join and append.</param>
    /// <returns>The current instance of the <see cref="PooledStringBuilder"/>, enabling method chaining.</returns>
    public PooledStringBuilder AppendIndentedJoin(string? separator, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar, params ReadOnlySpan<string?> values)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .AppendJoin(separator, values);
        return this;
    }

    public PooledStringBuilder AppendJoin(char separator, params ReadOnlySpan<object?> values)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendJoin(separator, values);
        return this;
    }

    /// <summary>
    /// Appends the specified values to the current instance, joined by a separator and preceded by a repeated
    /// indentation character.
    /// </summary>
    /// <remarks>This method is useful for formatting structured text output with indentation, such as
    /// generating code or hierarchical data representations.</remarks>
    /// <param name="separator">The string to use as a separator between the values. If null, no separator is inserted.</param>
    /// <param name="indentationLevel">The number of times to repeat the indentation character. Must be zero or greater. The default is <see cref="DefaultIndentationLevel"/>.</param>
    /// <param name="indentationChar">The character to use for indentation. Defaults to a space character if not specified. The default is <see cref="DefaultIndentationChar"/>.</param>
    /// <param name="values">An array of <see cref="ReadOnlySpan{T}"/> to join and append. Each <see cref="object"/> is converted to its <see cref="string"/> representation.</param>
    /// <returns>The current instance of the <see cref="PooledStringBuilder"/>, enabling method chaining.</returns>
    public PooledStringBuilder AppendIndentedJoin(char separator, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar, params ReadOnlySpan<object?> values)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .AppendJoin(separator, values);
        return this;
    }

    public PooledStringBuilder AppendJoin(char separator, params ReadOnlySpan<string?> values)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendJoin(separator, values);
        return this;
    }

    /// <summary>
    /// Appends the specified values to the current instance, joined by a separator and preceded by a repeated
    /// indentation character.
    /// </summary>
    /// <remarks>This method is useful for formatting structured text output with indentation, such as
    /// generating code or hierarchical data representations.</remarks>
    /// <param name="separator">The character to use as a separator between the values. If null, no separator is inserted.</param>
    /// <param name="indentationLevel">The number of times to repeat the indentation character. Must be zero or greater. The default is <see cref="DefaultIndentationLevel"/>.</param>
    /// <param name="indentationChar">The character to use for indentation. Defaults to a space character if not specified. The default is <see cref="DefaultIndentationChar"/>.</param>
    /// <param name="values">An array of <see cref="ReadOnlySpan{T}"/> to join and append.</param>
    /// <returns>The current instance of the <see cref="PooledStringBuilder"/>, enabling method chaining.</returns>
    public PooledStringBuilder AppendIndentedJoin(char separator, int indentationLevel = DefaultIndentationLevel, char indentationChar = DefaultIndentationChar, params ReadOnlySpan<string?> values)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

        StringBuilder builder = StringBuilder;
        _ = builder.Append(indentationChar, indentationLevel)
            .AppendJoin(separator, values);
        return this;
    }

#endif

    // ============================================================================
    // APPENDFORMAT METHODS - String format and CompositeFormat
    // ============================================================================

    public PooledStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(format, arg0);
        return this;
    }

    public PooledStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(format, arg0, arg1);
        return this;
    }

    public PooledStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1, object? arg2)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(format, arg0, arg1, arg2);
        return this;
    }

    public PooledStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(format, args);
        return this;
    }

    public PooledStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(provider, format, arg0);
        return this;
    }

    public PooledStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(provider, format, arg0, arg1);
        return this;
    }

    public PooledStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1, object? arg2)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(provider, format, arg0, arg1, arg2);
        return this;
    }

    public PooledStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(provider, format, args);
        return this;
    }

#if NET9_0_OR_GREATER

    public PooledStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params ReadOnlySpan<object?> args)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(format, args);
        return this;
    }

    public PooledStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params ReadOnlySpan<object?> args)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(provider, format, args);
        return this;
    }

#endif

    public PooledStringBuilder AppendFormat<TArg0>(IFormatProvider? provider, CompositeFormat format, TArg0 arg0)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(provider, format, arg0);
        return this;
    }

    public PooledStringBuilder AppendFormat<TArg0, TArg1>(IFormatProvider? provider, CompositeFormat format, TArg0 arg0, TArg1 arg1)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(provider, format, arg0, arg1);
        return this;
    }

    public PooledStringBuilder AppendFormat<TArg0, TArg1, TArg2>(IFormatProvider? provider, CompositeFormat format, TArg0 arg0, TArg1 arg1, TArg2 arg2)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(provider, format, arg0, arg1, arg2);
        return this;
    }

    public PooledStringBuilder AppendFormat(IFormatProvider? provider, CompositeFormat format, params object?[] args)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(provider, format, args);
        return this;
    }

#if NET9_0_OR_GREATER

    public PooledStringBuilder AppendFormat(IFormatProvider? provider, CompositeFormat format, params ReadOnlySpan<object?> args)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.AppendFormat(provider, format, args);
        return this;
    }

#endif

    // ============================================================================
    // INSERT METHODS
    // ============================================================================

    public PooledStringBuilder Insert(int index, string? value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, string? value, int count)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value, count);
        return this;
    }

    public PooledStringBuilder Insert(int index, bool value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, sbyte value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, byte value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, short value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, char value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, char[]? value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, string? value, int startIndex, int count)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value?[startIndex..(startIndex + count)]);
        return this;
    }

    public PooledStringBuilder Insert(int index, char[]? value, int startIndex, int charCount)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value, startIndex, charCount);
        return this;
    }

    public PooledStringBuilder Insert(int index, int value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, long value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, float value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, double value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, decimal value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, ushort value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, uint value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, ulong value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, object? value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    public PooledStringBuilder Insert(int index, ReadOnlySpan<char> value)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Insert(index, value);
        return this;
    }

    // ============================================================================
    // REMOVE & REPLACE
    // ============================================================================

    public PooledStringBuilder Remove(int startIndex, int length)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Remove(startIndex, length);
        return this;
    }

    public PooledStringBuilder Replace(string oldValue, string? newValue)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Replace(oldValue, newValue);
        return this;
    }

    public PooledStringBuilder Replace(string oldValue, string? newValue, int startIndex, int count)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Replace(oldValue, newValue, startIndex, count);
        return this;
    }

    public PooledStringBuilder Replace(char oldChar, char newChar)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Replace(oldChar, newChar);
        return this;
    }

    public PooledStringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Replace(oldChar, newChar, startIndex, count);
        return this;
    }

#if NET9_0_OR_GREATER

    public PooledStringBuilder Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Replace(oldValue, newValue);
        return this;
    }

    public PooledStringBuilder Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue, int startIndex, int count)
    {
        StringBuilder builder = StringBuilder;
        _ = builder.Replace(oldValue, newValue, startIndex, count);
        return this;
    }

#endif

    // ============================================================================
    // COPY
    // ============================================================================

    public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
    {
        StringBuilder builder = StringBuilder;
        builder.CopyTo(sourceIndex, destination, destinationIndex, count);
    }

    public void CopyTo(int sourceIndex, Span<char> destination, int count)
    {
        StringBuilder builder = StringBuilder;
        builder.CopyTo(sourceIndex, destination, count);
    }

    // ============================================================================
    // EQUALS
    // ============================================================================

    public bool Equals([NotNullWhen(true)] StringBuilder? sb)
    {
        StringBuilder builder = StringBuilder;
        return builder.Equals(sb);
    }

    public bool Equals(ReadOnlySpan<char> span)
    {
        StringBuilder builder = StringBuilder;
        return builder.Equals(span);
    }

    // ----------------------------
    // Lifetime / pooling
    // ----------------------------

    public void Recycle()
    {
        if (_stringBuilder is null)
        {
            return;
        }

        StringBuilderFactory.Recycle(_stringBuilder);
        _stringBuilder = null;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Interpolated-string handler requires access to the containing instance's internals; this nested public ref struct is intentional.")]
    public ref struct AppendInterpolatedStringHandler
    {
        private StringBuilder.AppendInterpolatedStringHandler _inner;

        // Constructor for Append($"...")
        public AppendInterpolatedStringHandler(
            int literalLength,
            int formattedCount,
            PooledStringBuilder pooledBuilder)
        {
            ArgumentNullException.ThrowIfNull(pooledBuilder);

            StringBuilder wrappedStringBuilder = pooledBuilder.StringBuilder;

            // Extract the inner StringBuilder and pass it to the real handler
            _inner = new StringBuilder.AppendInterpolatedStringHandler(
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
            ArgumentNullException.ThrowIfNull(pooledBuilder);

            StringBuilder wrappedStringBuilder = pooledBuilder.StringBuilder;

            _inner = new StringBuilder.AppendInterpolatedStringHandler(
                literalLength,
                formattedCount,
                wrappedStringBuilder,
                provider);
        }

        // Constructor for Append(provider, indentationChar, indentationLevel, $"...")
        public AppendInterpolatedStringHandler(
            int literalLength,
            int formattedCount,
            PooledStringBuilder pooledBuilder,
            IFormatProvider? provider,
            char indentationChar,
            int indentationLevel)
        {
            ArgumentNullException.ThrowIfNull(pooledBuilder);
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

            StringBuilder wrappedStringBuilder = pooledBuilder.StringBuilder
                .Append(indentationChar, indentationLevel);

            _inner = new StringBuilder.AppendInterpolatedStringHandler(
                literalLength,
                formattedCount,
                wrappedStringBuilder,
                provider);
        }

        // Constructor for Append(indentationChar, indentationLevel, $"...")
        public AppendInterpolatedStringHandler(
            int literalLength,
            int formattedCount,
            PooledStringBuilder pooledBuilder,
            char indentationChar,
            int indentationLevel)
        {
            ArgumentNullException.ThrowIfNull(pooledBuilder);
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(indentationLevel);

            StringBuilder wrappedStringBuilder = pooledBuilder.StringBuilder
                .Append(indentationChar, indentationLevel);

            _inner = new StringBuilder.AppendInterpolatedStringHandler(
                literalLength,
                formattedCount,
                wrappedStringBuilder);
        }

        // Forward all calls to the inner handler
        public void AppendLiteral(string value)
            => _inner.AppendLiteral(value);

        public void AppendFormatted<T>(T value)
            => _inner.AppendFormatted(value);

        public void AppendFormatted<T>(T value, string? format)
            => _inner.AppendFormatted(value, format);

        public void AppendFormatted<T>(T value, int alignment)
            => _inner.AppendFormatted(value, alignment);

        public void AppendFormatted<T>(T value, int alignment, string? format)
            => _inner.AppendFormatted(value, alignment, format);

        public void AppendFormatted(ReadOnlySpan<char> value)
            => _inner.AppendFormatted(value);

        public void AppendFormatted(ReadOnlySpan<char> value, int alignment = 0, string? format = null)
            => _inner.AppendFormatted(value, alignment, format);

        public void AppendFormatted(string? value)
            => _inner.AppendFormatted(value);

        public void AppendFormatted(string? value, int alignment = 0, string? format = null)
            => _inner.AppendFormatted(value, alignment, format);

        public void AppendFormatted(object? value, int alignment = 0, string? format = null)
            => _inner.AppendFormatted(value, alignment, format);
    }
}
