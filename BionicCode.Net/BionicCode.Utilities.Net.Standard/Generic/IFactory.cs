namespace BionicCode.Utilities.Net.Standard.Generic
{
  /// <summary>
  /// Interface to implement Abstract Factory pattern
  /// </summary>
  /// <typeparam name="TCreate">The type of the instantiated object.</typeparam>
  public interface IFactory<out TCreate>
  {
    /// <summary>
    /// Creates the instance.
    /// </summary>
    /// <returns>An instance of <typeparamref name="TCreate"/>.</returns>
    TCreate Create();

    /// <summary>
    /// Creates the instance, allowing arguments.
    /// </summary>
    /// <param name="args">The arguments for the factory to use to instantiate the type.</param>
    /// <returns>An instance of <typeparamref name="TCreate"/>.</returns>
    TCreate Create(params object[] args);

    /// <summary>
    /// Configures the lifetime scope of the created object instances.
    /// Default is <see cref="Standard.FactoryMode.Singleton"/>
    /// </summary>
    /// <value>The lifetime scope  <see cref="Standard.FactoryMode.Singleton"/> of the created object instances</value>
    FactoryMode FactoryMode { get; set; }
  }
}
