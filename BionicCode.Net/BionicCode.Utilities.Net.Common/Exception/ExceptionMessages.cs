[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BionicCode.Controls.Net.Wpf")]
namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class ExceptionMessages
    {
        public static string GetIndexOutOfRangeExceptionMessage(int index, string collectionName) => $"The index '{index}' is out of range of collection {collectionName}.";
        public static string GetInvalidOperationExceptionMessage_CollectionEmpty() => "The sequence is empty.";
        public static string GetInvalidOperationExceptionMessage_IInitializableFailed(Type initializableImplementationType) => $"IInitializable.InitializeAsync for type {initializableImplementationType.FullName}  returned 'false'.";
        public static string GetInvalidOperationExceptionMessage_ItemNotFound(string predicateName) => $"The sequence contains no item that matches the predicate '{predicateName}'.";
        public static string GetArgumentNullExceptionMessage_ValidationPropertyName() => "Please provide a valid property name. A validation error must always map to a property.";
        public static string GetArgumentExceptionMessage_ValidationFailed() => "Property validation failed.";
        public static string GetInvalidOperationExceptionMessage_SetFactoryModeOnScopedFactory() => $"Modifying the lifetime of instances produced by a scoped factory is not allowed. Instances are automatically treated as {FactoryMode.Singleton} for the current scope. You can change the IFactory.FactoryMode property after leaving the scope.";
        public static string GetValueNotSupportedExceptionMessage(object value) => $"The {(value is Enum ? "enum " : string.Empty)}value '{(value is Enum enumValue ? $"{enumValue.GetType().FullName}.{enumValue}" : value)}' is not supported.";
        public static string GetModificationOfReadOnlyCollectionNotSupportedExceptionMessage(IEnumerable collection) => $"The {collection.GetType().ToDisplayName()} is read-only.";
        public static string GetModificationOfImmutableCollectionNotSupportedExceptionMessage(IEnumerable collection) => $"The {collection.GetType().ToDisplayName()} is immutable.";
        public static string GetHandlerDelegateSignatureMismatchExceptionMessage(EventInfo eventInfo, MethodInfo eventHandlerMethodInfo, string because) => $"Event handler delegate signature mismatch. Expected signature as required by event source: '{eventInfo.EventHandlerType.ToSignatureName()}'. Found signature on provided event handler: '{eventHandlerMethodInfo.ToSignatureName()}'. Because: {because}";
    }
}
