namespace BionicCode.Utilities.Net;

/// <summary>
/// Defines the options for handling duplicate keys when adding items to a collection.
/// </summary>
/// <remarks>This enumeration allows developers to specify whether duplicate keys should result in an exception or
/// be ignored during addition operations. Use the appropriate value to control the behavior based on the requirements
/// of your collection and application logic.</remarks>
public enum AddRangeMode
{
    /// <summary>
    /// Request throwing an <see cref="ArgumentException"/> if a duplicate key is detected.
    /// </summary>
    ThrowOnDuplicateKey,
    /// <summary>
    /// Request silently skipping the duplicate key if detected.
    /// </summary>
    SkipDuplicateKey,
}

