namespace BionicCode.Utilities.Net
{
  using System;

  /// <summary>
  /// Interface to implement Abstract Factory pattern.
  /// <para>Implementers are encouraged to extend the abstract <see cref="Factory{TObject}"/> class instead of creating their own.</para>
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
    /// Default is <see cref=".FactoryMode.Singleton"/>
    /// </summary>
    /// <value>The lifetime scope  <see cref=".FactoryMode.Singleton"/> of the created object instances</value>
    FactoryMode FactoryMode { get; set; }
  }
}
