namespace BionicCode.Utilities.Net.Profiling
{
    using System;

    public static partial class HelperExtensionsCommon
    {
        /// <summary>
        /// Converts the value of <see cref="ProfiledTargetType"/> to a string representation.
        /// </summary>
        /// <param name="profiledTargetType"></param>
        /// <param name="toUpperCase">Controls whether to convert the string's first character to uppercase (<paramref name="toUpperCase"/>=<c>true</c>). The default is lowercase (<paramref name="toUpperCase"/>=<c>true</c>.</param>
        /// <returns>The readable string representation of the enum value.</returns>
        /// <exception cref="NotSupportedException">The enum value is not supported. (This exception is only intended for internal maintenance and will never be thrown in production code)</exception>
        internal static string ToDisplayStringValue(this ProfiledTargetType profiledTargetType, bool toUpperCase = false, bool toBaseType = false)
        {
            switch (profiledTargetType)
            {
                case ProfiledTargetType.None:
                    return toUpperCase ? "None" : "none";
                case ProfiledTargetType.PropertyGet:
                    {
                        string result = toUpperCase ? "Property" : "property";
                        return toBaseType ? result : $"{result} get()";
                    }
                case ProfiledTargetType.PropertySet:
                    {
                        string result = toUpperCase ? "Property" : "property";
                        return toBaseType ? result : $"{result} set()";
                    }
                case ProfiledTargetType.Property:
                    return toUpperCase ? "Property" : "property";
                case ProfiledTargetType.Constructor:
                    return toUpperCase ? "Constructor" : "constructor";
                case ProfiledTargetType.Event:
                    return toUpperCase ? "Event" : "event";
                case ProfiledTargetType.Delegate:
                    return toUpperCase ? "Delegate" : "delegate";
                case ProfiledTargetType.Method:
                    return toUpperCase ? "Method" : "method";
                case ProfiledTargetType.Indexer:
                    return toUpperCase ? "Indexer" : "indexer";
                case ProfiledTargetType.IndexerGet:
                    {
                        string result = toUpperCase ? "Indexer" : "indexer";
                        return toBaseType ? result : $"{result} get()";
                    }
                case ProfiledTargetType.IndexerSet:
                    {
                        string result = toUpperCase ? "Indexer" : "indexer";
                        return toBaseType ? result : $"{result} set()";
                    }
                case ProfiledTargetType.Scope:
                    return toUpperCase ? "Scope" : "scope";
                default:
                    throw new NotSupportedException(ExceptionMessages.GetValueNotSupportedExceptionMessage(profiledTargetType));
            }
        }
    }
}
