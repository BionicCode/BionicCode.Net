namespace BionicCode.Utilities.Net
{
  using System.Threading.Tasks;

  /// <summary>
  /// An interface to enable polymorphism for types like view model classes etc. that require an explicit (asynchronous) initialization.
  /// </summary>
  /// <remarks>It's considered bad practice to have a longrunning cobstructor. Also constructors don't have an async context. 
  /// <br/>Deferring the initilaization of the instance is a convenient way to avoid the shortcommings and code smells.
  /// <br/>It's also a clean way to "lazy" initialize expensive resources.</remarks>
  public interface IInitializable
  {
    /// <summary>
    /// Asynchronous method that executes the insatnce initialization routine.
    /// </summary>
    /// <returns>Return <c>true</c> when initialized successfully, otherwise <c>false</c>.</returns>
    Task<bool> InitializeAsync();

    /// <summary>
    /// Returns whether the instance is already initialized.
    /// </summary>
    /// <value><c>true</c> when <see cref="InitializeAsync"/> was already called, otherwise <c>false</c>.</value>
    bool IsInitialized { get; }
  }
}