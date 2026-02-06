namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;

[DebuggerDisplay("{RuntimeShortSignature}")]
internal abstract class SymbolInfoData : IDisposableAdvanced
{
    protected SymbolInfoData(string name, SymbolKind symbolKind, SymbolReflectionInfoCacheKey cacheKey)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
        ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(symbolKind, [SymbolKind.Undefined], nameof(symbolKind));
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey, nameof(cacheKey));

        Name = name;
        SymbolKind = symbolKind;
        FormattingIndentation = 4;
        CacheKey = cacheKey;
    }

    internal string Name { get; }
    internal SymbolKind SymbolKind { get; }
    internal abstract string Namespace { get; }
    internal abstract IList<CustomAttributeData> AttributeData { get; }
    internal abstract SymbolAttributes SymbolAttributes { get; }
    internal abstract string AssemblyName { get; }

    /// <summary>
    /// Symbol name with namespace, the declaring type (in case of a member), and generic type parameters.
    /// </summary>
    /// <value>The fully qualified name of the symbol.
    /// <br/>For example, a method name: <c>"MyNamespace.MyClass.DoSomething&lt;T&gt;"</c>.</value>
    internal abstract string FullyQualifiedDisplayName { get; }

    /// <summary>
    /// Symbol name without namespace, but with the declaring type (in case of a member), and generic type parameters.
    /// </summary>
    /// <value>The full name of the symbol.
    /// <br/>For example, a method name: <c>"MyClass.DoSomething&lt;T&gt;"</c>.</value>
    internal abstract string DisplayName { get; }

    /// <summary>
    /// Symbol name without namespace and the declaring type (in case of a member), but with generic type parameters.
    /// </summary>
    /// <value>The short name of the symbol.
    /// <br/>For example, a method name: <c>"DoSomething;T&gt;"</c>.</value>
    internal abstract string ShortDisplayName { get; }

    /// <summary>
    /// Signature including the namespace, the declaring type (in case of a member), attributes, generic type parameters and generic type parameter constraints.
    /// </summary>
    /// <value>The fully qualified signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void MyNamespace.MyClass.DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</value>
    internal abstract string FullyQualifiedSignature { get; }

    /// <summary>
    /// Signature without namespace, but with the declaring type (in case of a member), attributes, generic type parameters and generic type parameter constraints.
    /// </summary>
    /// <value>The full signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void MyClass.DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</value>
    internal abstract string Signature { get; }

    /// <summary>
    /// Signature without namespace and the declaring type (in case of a member), but with attributes, generic type parameters and generic type parameter constraints.
    /// </summary>
    /// <value>The shortened signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</value>
    internal abstract string ShortSignature { get; }

    /// <summary>
    /// Signature without namespace, the declaring type (in case of a member), attributes and generic type parameter constraints, but with generic type parameters.
    /// </summary>
    /// <value>The compacted short signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void DoSomething&lt;T&gt;(T firstValue, string value = null)"</c>.</value>
    internal abstract string ShortCompactSignature { get; }

    /// <summary>
    /// Signature with namespace, the declaring type (in case of a member), attributes and generic type parameter constraints and with the resolved runtime generic type argument names.
    /// <br/>This returns the constructed runtime signature for generic symbols where generic type parameters are replaced by their actual constructed runtime type arguments. For example, <c>Action&lt;T&gt;</c> becomes <c>Action&lt;int&gt;</c>.
    /// </summary>
    /// <value>The fully qualified runtime signature of the symbol.
    /// <br/>For example: <c>"internal void MyNamespace.MyClass.DoSomething&lt;int&gt;(int firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</value>
    internal abstract string FullyQualifiedRuntimeSignature { get; }

    /// <summary>
    /// Signature without namespace but with the declaring type (in case of a member), attributes and generic type parameter constraints and with the resolved runtime generic type argument names.
    /// <br/>This returns the constructed runtime signature for generic symbols where generic type parameters are replaced by their actual constructed runtime type arguments. For example, <c>Action&lt;T&gt;</c> becomes <c>Action&lt;int&gt;</c>.
    /// </summary>
    /// <value>The full runtime signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void MyClass.DoSomething&lt;int&gt;(int firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</value>
    internal abstract string RuntimeSignature { get; }

    /// <summary>
    /// Signature without namespace and the declaring type (in case of a member) but with attributes and generic type parameter constraints and with the resolved runtime generic type argument names.
    /// <br/>This returns the constructed runtime signature for generic symbols where generic type parameters are replaced by their actual constructed runtime types (<c>Action&lt;T&gt;</c> becomes <c>Action&lt;int&gt;</c>)
    /// </summary>
    /// <value>The shortened runtime signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void DoSomething&lt;int&gt;(int firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</value>
    internal abstract string RuntimeShortSignature { get; }

    /// <summary>
    /// Signature without namespace, the declaring type (in case of a member), attributes and generic type parameter constraints, but with the resolved runtime generic type argument names.
    /// <br/>This returns the constructed runtime signature for generic symbols where generic type parameters are replaced by their actual constructed runtime types (<c>Action&lt;T&gt;</c> becomes <c>Action&lt;int&gt;</c>)
    /// </summary>
    /// <value>The compacted short runtime signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void DoSomething&lt;int&gt;(int firstValue, string value = null)"</c>.</value>
    internal abstract string RuntimeShortCompactSignature { get; }

    /// <summary>
    /// The individual components that make the signature.
    /// </summary>
    internal abstract SymbolComponentInfo SymbolComponentInfo { get; }
    internal bool IsDisposed { get; private set; }

    private int _indentation;

    /// <summary>
    /// Gets or sets the number of spaces to use for each indentation level when formatting the symbol signatures.
    /// </summary>
    /// <value>The number of spaces to indent a line. The default is <code>4</code>.</value>
    internal int FormattingIndentation
    {
        get => _indentation;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 0, nameof(value));

            _indentation = value;
            IndentationString = new string(' ', _indentation);
        }
    }

    /// <summary>
    /// Gets the string used to represent a single level of indentation.
    /// </summary>
    /// <value>The spaces to indent a line based on the <see cref="FormattingIndentation"/> property.</value>
    internal string IndentationString { get; private set; }

    internal SymbolReflectionInfoCacheKey CacheKey { get; }
    bool IDisposableAdvanced.IsDisposed { get; }

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            IsDisposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~SymbolInfoData()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    internal void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    void IDisposable.Dispose() => Dispose();
    #endregion IDisposable
}
