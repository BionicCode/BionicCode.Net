namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

/// <summary>
/// Represents a component of a symbol, such as a type, member, parameter, or attribute, including its modifiers,
/// generic parameters, attributes, and related metadata.
/// </summary>
/// <remarks>This class is used to model the structure and metadata of code symbols for analysis or code
/// generation scenarios. It provides access to modifiers, generic type parameters and constraints, custom
/// attributes, parameters, and other symbol characteristics. Instances of this class are typically constructed and
/// populated as part of a larger symbol processing workflow.</remarks>
[DebuggerDisplay("Symbol name = {NameBuilder}")]
public class SymbolComponentInfo : IDisposable
{
    public ReadOnlyCollection<string> Modifiers { get; }
    public ReadOnlyCollection<SymbolComponentInfo> GenericTypeParameters { get; }
    public ReadOnlyCollection<SymbolComponentInfo> InheritedTypes { get; }
    public ReadOnlyCollection<SymbolComponentInfo> GenericTypeConstraints { get; }
    public ReadOnlyCollection<SymbolComponentInfo> CustomAttributes { get; }
    public ReadOnlyCollection<string> CustomAttributeConstructorArgs { get; }
    public ReadOnlyCollection<(string PropertyName, string PropertyValue)> CustomAttributeNamedArgs { get; }
    public ReadOnlyCollection<SymbolComponentInfo> Parameters { get; }

    public string Name { get; private set; } = string.Empty;

    public string ValueName { get; private set; } = string.Empty;

    public bool IsCompleted { get; private set; }

    private readonly PooledStringBuilder _nameBuilder;
    public PooledStringBuilder NameBuilder
        => IsCompleted
            ? throw new InvalidOperationException($"Cannot access '{nameof(NameBuilder)}' after the '{nameof(SymbolComponentInfo)}' has been marked as completed.")
            : _nameBuilder;

    private readonly PooledStringBuilder _valueNameBuilder;
    public PooledStringBuilder ValueNameBuilder
        => IsCompleted
            ? throw new InvalidOperationException($"Cannot access '{nameof(ValueNameBuilder)}' after the '{nameof(SymbolComponentInfo)}' has been marked as completed.")
            : _valueNameBuilder;

    public bool IsKeyword { get; set; }
    public bool IsExtensionMethodParameter { get; set; }
    public bool IsSymbol { get; set; }
    public SymbolComponentInfo? ReturnType
    {
        get => _returnType;
        set
        {
            _returnType = value;
            if (_returnType == null)
            {
                return;
            }

            _returnType.IsSymbol = false;
        }
    }

    private int _indentation;
    /// <summary>
    /// Gets or sets the number of spaces to use for each indentation level when formatting the symbol signatures.
    /// </summary>
    /// <value>The number of spaces to indent a line. The default is <code>4</code>.</value>
    public int Indentation
    {
        get => _indentation;
        set
        {
            _indentation = value;
            IndentationString = new string(' ', _indentation);
        }
    }

    /// <summary>
    /// Gets the string used to represent a single level of indentation.
    /// </summary>
    /// <value>The spaces to indent a line based on the <see cref="Indentation"/> property.</value>
    public string IndentationString { get; private set; }

    public SymbolComponentInfo PropertyGet { get; set; }
    public SymbolComponentInfo PropertySet { get; set; }
    public string Signature { get; set; }
    public bool HasExpressionTerminator { get; set; }
    public bool IsIndexer { get; set; }
    public bool IsParameter { get; set; }
    public bool HasInlineAttributes { get; set; }

    private readonly List<string> _modifiersInternal;
    private readonly List<SymbolComponentInfo> _genericTypeParametersInternal;
    private readonly List<SymbolComponentInfo> _inheritedTypesInternal;
    private readonly List<SymbolComponentInfo> _genericTypeConstraintsInternal;
    private readonly List<SymbolComponentInfo> _customAttributes;
    private readonly List<string> _customAttributeConstructorArgs;
    private readonly List<(string PropertyName, string PropertyValue)> _customAttributeNamedArgs;
    private readonly List<SymbolComponentInfo> _parametersInternal;
    private string _html;
    private SymbolComponentInfo _returnType;

    public SymbolComponentInfo(bool isKeyword)
    {
        _modifiersInternal = [];
        Modifiers = new ReadOnlyCollection<string>(_modifiersInternal);
        _genericTypeParametersInternal = [];
        GenericTypeParameters = new ReadOnlyCollection<SymbolComponentInfo>(_genericTypeParametersInternal);
        _inheritedTypesInternal = [];
        InheritedTypes = new ReadOnlyCollection<SymbolComponentInfo>(_inheritedTypesInternal);
        _genericTypeConstraintsInternal = [];
        GenericTypeConstraints = new ReadOnlyCollection<SymbolComponentInfo>(_genericTypeConstraintsInternal);
        _parametersInternal = [];
        Parameters = new ReadOnlyCollection<SymbolComponentInfo>(_parametersInternal);
        _customAttributes = [];
        CustomAttributes = new ReadOnlyCollection<SymbolComponentInfo>(_customAttributes);
        _customAttributeConstructorArgs = [];
        CustomAttributeConstructorArgs = new ReadOnlyCollection<string>(_customAttributeConstructorArgs);
        _customAttributeNamedArgs = [];
        CustomAttributeNamedArgs = new ReadOnlyCollection<(string PropertyName, string PropertyValue)>(_customAttributeNamedArgs);
        _nameBuilder = PooledStringBuilder.GetOrCreate();
        _valueNameBuilder = PooledStringBuilder.GetOrCreate();
        Signature = string.Empty;
        ReturnType = null;
        IsKeyword = isKeyword;
        Indentation = 4;
    }

    public SymbolComponentInfo(string name, bool isKeyword = false) : this(isKeyword)
        => _ = NameBuilder.Append(name);

    public void AddModifier(string modifier)
      => _modifiersInternal.Add(modifier);

    public void AddCustomAttribute(SymbolComponentInfo attribute)
      => _customAttributes.Add(attribute);

    public void AddCustomAttributeConstructorArg(string attributeConstructorArg)
      => _customAttributeConstructorArgs.Add(attributeConstructorArg);

    public void AddCustomAttributeNamedArg((string PropertyName, string PropertyValue) attributeNamedArg)
      => _customAttributeNamedArgs.Add(attributeNamedArg);

    public void AddGenericTypeParameter(SymbolComponentInfo typeParameter)
      => _genericTypeParametersInternal.Add(typeParameter);

    public void AddGenericTypeParameterRange(IEnumerable<SymbolComponentInfo> typeParameters)
      => _genericTypeParametersInternal.AddRange(typeParameters);

    public void AddGenericTypeConstraint(SymbolComponentInfo typeConstraint)
      => _genericTypeConstraintsInternal.Add(typeConstraint);

    public void AddGenericTypeConstraintRange(IEnumerable<SymbolComponentInfo> typeConstraints)
      => _genericTypeConstraintsInternal.AddRange(typeConstraints);

    public void AddInheritedType(SymbolComponentInfo type)
      => _inheritedTypesInternal.Add(type);

    public void AddInheritedTypeRange(IEnumerable<SymbolComponentInfo> types)
      => _inheritedTypesInternal.AddRange(types);

    public void AddParameter(SymbolComponentInfo parameter)
      => _parametersInternal.Add(parameter);

    public void Complete()
    {
        if (!IsCompleted)
        {
            Name = NameBuilder.ToString();
            ValueName = ValueNameBuilder.ToString();
            NameBuilder.Recycle();
            ValueNameBuilder.Recycle();
            IsCompleted = true;
        }
    }

    //TODO::Implement to signature
    public override string ToString() => Signature;

    public string ToHtml()
    {
        if (_html is null)
        {
            using PooledStringBuilder signatureBuilder = PooledStringBuilder.GetOrCreate()
              .Append("<div style=\"display: block; width: 100%;\">")
              .AppendInlineHtml(this)
              .Append("</div>");

            _html = signatureBuilder.ToString();
        }

        return _html;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!IsCompleted)
        {
            Complete();
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~SymbolComponentInfo()
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
