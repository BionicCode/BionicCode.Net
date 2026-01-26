namespace BionicCode.Utilities.Net
{
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
    internal class SymbolComponentInfo : IDisposable
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
            => this.IsCompleted
                ? throw new InvalidOperationException($"Cannot access '{nameof(this.NameBuilder)}' after the '{nameof(SymbolComponentInfo)}' has been marked as completed.")
                : this._nameBuilder;

        private readonly PooledStringBuilder _valueNameBuilder;
        public PooledStringBuilder ValueNameBuilder
            => this.IsCompleted
                ? throw new InvalidOperationException($"Cannot access '{nameof(this.ValueNameBuilder)}' after the '{nameof(SymbolComponentInfo)}' has been marked as completed.")
                : this._valueNameBuilder;

        public bool IsKeyword { get; set; }
        public bool IsExtensionMethodParameter { get; set; }
        public bool IsSymbol { get; set; }
        public SymbolComponentInfo ReturnType
        {
            get => this.returnType;
            set
            {
                this.returnType = value;
                if (this.returnType == null)
                {
                    return;
                }

                this.returnType.IsSymbol = false;
            }
        }

        private int indentation;
        /// <summary>
        /// Gets or sets the number of spaces to use for each indentation level when formatting the symnbol signatures.
        /// </summary>
        /// <value>The number of spaces to indent a line. The default is <code>4</code>.</value>
        public int Indentation
        {
            get => this.indentation;
            set
            {
                this.indentation = value;
                this.IndentationString = new string(' ', this.indentation);
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

        private readonly List<string> modifiersInternal;
        private readonly List<SymbolComponentInfo> genericTypeParametersInternal;
        private readonly List<SymbolComponentInfo> inheritedTypesInternal;
        private readonly List<SymbolComponentInfo> genericTypeConstraintsInternal;
        private readonly List<SymbolComponentInfo> customAttributes;
        private readonly List<string> customAttributeConstructorArgs;
        private readonly List<(string PropertyName, string PropertyValue)> customAttributeNamedArgs;
        private readonly List<SymbolComponentInfo> parametersInternal;
        private string html;
        private SymbolComponentInfo returnType;

        public SymbolComponentInfo(bool isKeyword)
        {
            this.modifiersInternal = new List<string>();
            this.Modifiers = new ReadOnlyCollection<string>(this.modifiersInternal);
            this.genericTypeParametersInternal = new List<SymbolComponentInfo>();
            this.GenericTypeParameters = new ReadOnlyCollection<SymbolComponentInfo>(this.genericTypeParametersInternal);
            this.inheritedTypesInternal = new List<SymbolComponentInfo>();
            this.InheritedTypes = new ReadOnlyCollection<SymbolComponentInfo>(this.inheritedTypesInternal);
            this.genericTypeConstraintsInternal = new List<SymbolComponentInfo>();
            this.GenericTypeConstraints = new ReadOnlyCollection<SymbolComponentInfo>(this.genericTypeConstraintsInternal);
            this.parametersInternal = new List<SymbolComponentInfo>();
            this.Parameters = new ReadOnlyCollection<SymbolComponentInfo>(this.parametersInternal);
            this.customAttributes = new List<SymbolComponentInfo>();
            this.CustomAttributes = new ReadOnlyCollection<SymbolComponentInfo>(this.customAttributes);
            this.customAttributeConstructorArgs = new List<string>();
            this.CustomAttributeConstructorArgs = new ReadOnlyCollection<string>(this.customAttributeConstructorArgs);
            this.customAttributeNamedArgs = new List<(string PropertyName, string PropertyValue)>();
            this.CustomAttributeNamedArgs = new ReadOnlyCollection<(string PropertyName, string PropertyValue)>(this.customAttributeNamedArgs);
            this._nameBuilder = StringBuilderFactory.GetOrCreate();
            this._valueNameBuilder = StringBuilderFactory.GetOrCreate();
            this.Signature = string.Empty;
            this.ReturnType = null;
            this.IsKeyword = isKeyword;
            this.Indentation = 4;
        }

        public SymbolComponentInfo(string name, bool isKeyword = false) : this(isKeyword)
            => _ = this.NameBuilder.Append(name);

        public void AddModifier(string modifier)
          => this.modifiersInternal.Add(modifier);

        public void AddCustomAttribute(SymbolComponentInfo attribute)
          => this.customAttributes.Add(attribute);

        public void AddCustomAttributeConstructorArg(string attributeConstructorArg)
          => this.customAttributeConstructorArgs.Add(attributeConstructorArg);

        public void AddCustomAttributeNamedArg((string PropertyName, string PropertyValue) attributeNamedArg)
          => this.customAttributeNamedArgs.Add(attributeNamedArg);

        public void AddGenericTypeParameter(SymbolComponentInfo typeParameter)
          => this.genericTypeParametersInternal.Add(typeParameter);

        public void AddGenericTypeParameterRange(IEnumerable<SymbolComponentInfo> typeParameters)
          => this.genericTypeParametersInternal.AddRange(typeParameters);

        public void AddGenericTypeConstraint(SymbolComponentInfo typeConstraint)
          => this.genericTypeConstraintsInternal.Add(typeConstraint);

        public void AddGenericTypeConstraintRange(IEnumerable<SymbolComponentInfo> typeConstraints)
          => this.genericTypeConstraintsInternal.AddRange(typeConstraints);

        public void AddInheritedType(SymbolComponentInfo type)
          => this.inheritedTypesInternal.Add(type);

        public void AddInheritedTypeRange(IEnumerable<SymbolComponentInfo> types)
          => this.inheritedTypesInternal.AddRange(types);

        public void AddParameter(SymbolComponentInfo parameter)
          => this.parametersInternal.Add(parameter);

        public void Complete()
        {
            if (!this.IsCompleted)
            {
                this.Name = this.NameBuilder.ToString();
                this.ValueName = this.ValueNameBuilder.ToString();
                this.NameBuilder.Recycle();
                this.ValueNameBuilder.Recycle();
                this.IsCompleted = true;
            }
        }

        //TODO::Implement to signature
        public override string ToString() => this.Signature;

        public string ToHtml()
        {
            if (this.html is null)
            {
                using PooledStringBuilder signatureBuilder = StringBuilderFactory.GetOrCreate()
                  .Append("<div style=\"display: block; width: 100%;\">")
                  .AppendInlineHtml(this)
                  .Append("</div>");

                this.html = signatureBuilder.ToString();
            }

            return this.html;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsCompleted)
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
            Dispose(disposing: true);
        }
    }
}
