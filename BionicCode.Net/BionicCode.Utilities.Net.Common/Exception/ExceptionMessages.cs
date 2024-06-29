[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BionicCode.Controls.Net.Wpf")]
namespace BionicCode.Utilities.Net
{
  using System;

  internal class ExceptionMessages
  {
    public static string GetInvalidOperationExceptionMessage_CollectionEmpty() => "The sequence is empty.";
    public static string GetInvalidOperationExceptionMessage_IInitializableFailed(Type initializableImplementationType) => $"IInitializable.InitializeAsync for type {initializableImplementationType.FullName}  returned 'false'.";
    public static string GetInvalidOperationExceptionMessage_ItemNotFound(string predicateName) => $"The sequence contains no item that matches the predicate '{predicateName}'.";
    public static string GetArgumentNullExceptionMessage_ValidationPropertyName() => "Please provide a valid property name. A validation error must always map to a property.";
    public static string GetArgumentExceptionMessage_ValidationFailed() => "Property validation failed.";
    public static string GetInvalidOperationExceptionMessage_SetFactoryModeOnScopedFactory() => $"Modifying the lifetime of instances produced by a scoped factory is not allowed. Instances are automatically treated as {FactoryMode.Singleton} for the current scope. You can change the IFactory.FactoryMode property after leaving the scope.";
    public static string GetArgumentExceptionMessage_ProfilerRunCount() => "The value must be between '0' and 'ulong.MaxValue'.";
    public static string GetArgumentExceptionMessage_ProfilerWarmupCount() => "The value must be between '0' and 'ulong.MaxValue'.";
    public static string GetIndexOutOfRangeExceptionMessage(int index, string collectionName) => $"The index '{index}' is out of range of collection {collectionName}.";
    public static string GetMissingProfiledArgumentAttributeExceptionMessage_Property() => $"The profiled property requires 1 parameter but no argument list was provided.Please decorate the profiled member with at least one 'ProfilerArgumentAttribute' to specify a set of arguments to enable the invocation of the member.";
    public static string GetMissingProfiledArgumentAttributeExceptionMessage_IndexerProperty() => $"The profiled indexer property requires 2 parameters (index and value) but no argument list was provided.Please decorate the profiled member with at least one 'ProfilerArgumentAttribute' to specify a set of arguments to enable the invocation of the member.";
    public static string GetMissingProfiledArgumentAttributeExceptionMessage_Method(int parameterCount) => $"The profiled method requires {parameterCount} {(parameterCount == 1 ? "parameter" : "parameters")} but no argument list was provided.Please decorate the profiled member with at least one 'ProfilerArgumentAttribute' to specify a set of arguments to enable the invocation of the member.";
    public static string GetMissingProfiledArgumentAttributeExceptionMessage_Constructor(int parameterCount) => $"The profiled constructor requires {parameterCount} {(parameterCount == 1 ? "parameter" : "parameters")} but no argument list was provided.Please decorate the profiled member with at least one 'ProfilerArgumentAttribute' to specify a set of arguments to enable the invocation of the member.";
    public static string GetMissingProfiledArgumentAttributeExceptionMessage_Delegate(int parameterCount) => $"The profiled delegate requires {parameterCount} {(parameterCount == 1 ? "parameter" : "parameters")} but no argument list was provided.Please decorate the profiled member with at least one 'ProfilerArgumentAttribute' to specify a set of arguments to enable the invocation of the member.";
    public static string GetMissingCreationMemberOfProfiledTypeExceptionMessage(Type profiledType) => $@"Unable to create an instance of the type {profiledType.FullName} that declares the profiled member.{System.Environment.NewLine}The profiled/declaring type must define a parameterless constructor. Alternatively, provide a factory (a class member that provides an initialized instance) that satisfies all of the following conditions: it must return a properly initialized instance of the declaring owner type or a delegate that returns this instance, and that member must be a static field, property or method or a non-static constructor. By convention, the first member that satisfies those conditions is taken in the following order: method, property, field, constructor. To override this convention decorate a member that satisfies the aforementioned conditions with the '{nameof(ProfilerFactoryAttribute)}' and provide the argument list to the attribute's constructor if required. The access modifier (e.g. private, internal, public) can be of any kind and is not relevant, this mens even private members are selected as instance provider.";
    public static string GetArgumentListMismatchExceptionMessage() => $"The argument list privided by the '{nameof(ProfilerArgumentAttribute)}' does not match the signature of the profiled member. The argument list must define the arguments in the same order and of the same type as the parameter list of the profiled member.";

  }
}

