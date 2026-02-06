namespace BionicCode.Utilities.Net;

/// <summary>
/// Provides a set of constant string values that are commonly used in reflection scenarios, particularly when working with properties, indexers, events, delegates, and operator methods in .NET. These constants can be used to ensure consistency and avoid hardcoding string literals throughout the codebase when performing reflection operations or generating code dynamically.
/// </summary>
public static class ReflectionConstants
{

    /// <summary>
    /// The property genericTypeParameterIdentifier of an indexer property. This genericTypeParameterIdentifier is compiler generated and equals the typeName of the <see langword="static"/>field <see cref="System.Windows.Data.Binding.IndexerName" />.
    /// </summary>
    /// <typeName>The generated property genericTypeParameterIdentifier of an indexer is <c>Item</c>.</typeName>
    /// <remarks>This field exists to enable writing of cross-platform compatible reflection code without the requirement to import the PresentationFramework.dll.</remarks>
    /// <value>"Item"</value>
    public const string IndexerName = "Item";

    /// <summary>
    /// The name of the method used to get the value of an indexer property.
    /// </summary>
    /// <value>"get_Item"</value>
    /// <remarks>This constant can be used when generating or reflecting over code that requires the
    /// standard indexer getter method name. The value is case-sensitive and should match the method name
    /// expected by the runtime or code generation tools.</remarks>
    public const string IndexerGetMethodName = "get_Item";

    /// <summary>
    /// Represents the method name used for setting values via an indexer in .NET types.
    /// </summary>
    /// <value>"set_Item"</value>
    /// <remarks>This constant is commonly used in reflection scenarios to identify the set accessor
    /// of an indexer property, which is named "set_Item" by convention in .NET languages such as C#.</remarks>
    public const string IndexerSetMethodName = "set_Item";

    /// <summary>
    /// The prefix for event accessor methods in .NET.
    /// </summary>
    /// <value>"add_" which is the prefix for event accessor methods in .NET.</value>
    public const string EventAccessorAddMethodNamePrefix = "add_";

    /// <summary>
    /// The prefix for event remover methods in .NET.
    /// </summary>
    /// <value>"remove_" which is the prefix for event remover methods in .NET.</value>
    /// <remarks>This constant is commonly used in reflection scenarios to identify the remove accessor
    /// of an event property, which is named "remove_" by convention in .NET languages such as C#.</remarks>
    public const string EventAccessorRemoveMethodNamePrefix = "remove_";

    /// <summary>
    /// Represents the prefix used for the method name of a non-indexer property getter in .NET reflection.
    /// </summary>
    /// <remarks>This constant is typically used when working with reflection to identify or construct
    /// the method name for property getters that are not indexers. For indexer properties, a different naming
    /// convention is used.</remarks>
    /// <value>"get_" which is the prefix for non-indexer property getter methods in .NET.</value>
    public const string NonIndexerPropertyGetMethodNamePrefix = "get_";

    /// <summary>
    /// Represents the prefix used for naming property setter methods that are not indexers.
    /// </summary>
    /// <remarks>This constant is typically used when reflecting over method names to identify
    /// non-indexer property setters, which conventionally begin with this prefix in .NET.</remarks>
    /// <value>"set_" which is the prefix for non-indexer property setter methods in .NET.</value>
    public const string NonIndexerPropertySetMethodNamePrefix = "set_";

    /// <summary>
    /// Represents the name of the method used to invoke a delegate dynamically.
    /// </summary>
    /// <remarks>This constant can be used when generating or reflecting over code that requires the
    /// standard delegate invocation method name. The value is case-sensitive and should match the method name
    /// expected by the runtime or code generation tools.</remarks>
    /// <value>"Invoke</value>
    public const string DelegateInvocatorMethodName = "Invoke";

    /// <summary>
    /// Specifies the prefix used for operator method names in metadata or reflection scenarios.
    /// </summary>
    /// <remarks>This constant is typically used when identifying or constructing method names that
    /// represent operator overloads, such as addition or equality operators, in .NET type metadata. The value
    /// corresponds to the standard prefix applied to operator methods by the compiler.</remarks>
    /// <value>"op_" which is the prefix for operator methods in .NET.</value>
    public const string OperatorMethodNamePrefix = "op_";

    public const string PropertySetterValueParameterName = "value";
}
