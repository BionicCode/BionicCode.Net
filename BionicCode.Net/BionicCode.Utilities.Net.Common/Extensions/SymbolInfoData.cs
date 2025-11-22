namespace BionicCode.Utilities.Net
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    [DebuggerDisplay("{RuntimeShortSignature}")]
    internal abstract class SymbolInfoData
    {
        protected SymbolInfoData(string name)
        {
            this.Name = name;
            this.Indentation = 4;
        }

        internal const BindingFlags AllMembersFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        public abstract IList<CustomAttributeData> AttributeData { get; }
        public abstract SymbolAttributes SymbolAttributes { get; }
        public string Name { get; }
        public abstract string AssemblyName { get; }

        /// <summary>
        /// Symbol name without namespace, but with the declaring type (in case of a member), and generic type parameters.
        /// </summary>
        /// <value>The full name of the symbol. For example: "MyClass.DoSomething&lt;T&gt;".</value>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Symbol name without namespace and the declaring type (in case of a member), but with generic type parameters.
        /// </summary>
        /// <value>The short name of the symbol. For example: "MyClass&lt;T&gt;".</value>
        public abstract string ShortDisplayName { get; }

        /// <summary>
        /// Symbol name with namespace, the declaring type (in case of a member), and generic type parameters.
        /// </summary>
        /// <value>The fully qualified name of the symbol. For example: "MyNamespace.MyClass.DoSomething&lt;T&gt;".</value>
        public abstract string FullyQualifiedDisplayName { get; }

        /// <summary>
        /// Signature including the namespace, the declaring type (in case of a member), attributes, generic type parameters and generic type parameter constraints.
        /// </summary>
        /// <value>The fully qualified signature of the symbol. For example: "public void MyNamespace.MyClass.DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : class".</value>
        public abstract string FullyQualifiedSignature { get; }

        /// <summary>
        /// Signature without namespace, but with the declaring type (in case of a member), attributes, generic type parameters and generic type parameter constraints.
        /// </summary>
        /// <value>The full signature of the symbol. For example: "public void MyClass.DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : class".</value>
        public abstract string Signature { get; }

        /// <summary>
        /// Signature without namespace and the declaring type (in case of a member), but with attributes, generic type parameters and generic type parameter constraints.
        /// </summary>
        /// <value>The short signature of the symbol. For example: "public void DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : class".</value>
        public abstract string ShortSignature { get; }

        /// <summary>
        /// Signature without namespace and the declaring type (in case of a member), attributes and generic type parameter constraints, but with generic type parameters.
        /// </summary>
        /// <value>The short signature of the symbol. For example: "public void DoSomething&lt;T&gt;(T firstValue, string value = null)".</value>
        public abstract string ShortCompactSignature { get; }

        /// <summary>
        /// Signature without namespace and the declaring type (in case of a member), attributes and generic type parameter constraints, but with generic type parameters.
        /// <br/>This returns the constructed runtime signature for generic symbols where generic type parameters are replaced by their actual constructed runtime types (<c>Action&lt;T&gt;</c> becomes <c>Action&lt;int&gt;</c>)
        /// </summary>
        /// <value>The short signature of the symbol. For example: "public void DoSomething&lt;int&gt;(int firstValue, string value = null)".</value>
        public abstract string RuntimeShortSignature { get; }

        ///// <summary>
        ///// Signature including the namespace and the declaring type (in case of a member), but without attributes and generic type parameter constraints, but with generic type parameters.
        ///// <br/>This returns the constructed runtime signature for generic symbols where generic type parameters are replaced by their actual constructed runtime types (<c>Action&lt;T&gt;</c> becomes <c>Action&lt;int&gt;</c>)
        ///// </summary>
        //public abstract string RuntimeSignature { get; }

        /// <summary>
        /// The individual components that make the signature.
        /// </summary>
        public abstract SymbolComponentInfo SymbolComponentInfo { get; }

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
    }
}
