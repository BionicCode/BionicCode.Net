#region Info

// 2020/09/19  17:17
// BionicCode.Utilities.Net

#endregion

using System;
using System.Threading;

namespace BionicCode.Utilities.Net
{
  /// <summary>
  /// Abstract implementation of <see cref="IFactory{TCreate}"/> that supports lifetime management using <see cref=".FactoryMode"/>.
  /// </summary>
  /// <typeparam name="TObject">The type of the instance to create.</typeparam>
  /// <remarks>The internals will handle the lifetime management based on the value of the <see cref="FactoryMode"/> property. Instances are actually created using the abstract <see cref="CreateInstance()"/> and <see cref="CreateInstance(object[])"/> members, which needs to  be implemented by the inheritor.</remarks>
  public abstract class Factory<TObject> : IFactory<TObject>
  {
    /// <summary>
    /// Initializes the <c>Factory</c> to create instances using <see cref=".FactoryMode.Singleton"/>. 
    /// </summary>
    protected Factory() : this(FactoryMode.Singleton)
    {
    }


    /// <summary>
    /// Initializes instance. 
    /// </summary>
    /// <param name="factoryMode">Describes the created objects lifetime using <see cref=".FactoryMode"/>.</param>
    protected Factory(FactoryMode factoryMode)
    {
      this.SharedProductInstance = new Lazy<TObject>(CreateInstance, LazyThreadSafetyMode.ExecutionAndPublication);
      this.FactoryMode = factoryMode;
    }

    private Lazy<TObject> SharedProductInstance { get; }
    private TObject SharedConfigurableProductInstance { get; set; }

    /// <inheritdoc />
    public FactoryMode FactoryMode { get; set; }


    #region (Implementation of IFactoy<T>)

    /// <inheritdoc />
    public TObject Create() => this.FactoryMode == FactoryMode.Singleton
      ? this.SharedProductInstance.Value
      : CreateInstance();

    /// <inheritdoc />
    public TObject Create(params object[] args) =>
      this.FactoryMode == FactoryMode.Singleton
        ? this.SharedConfigurableProductInstance.Equals(default) 
          ? this.SharedConfigurableProductInstance = CreateInstance(args) 
          : this.SharedConfigurableProductInstance
        : CreateInstance(args);

    #endregion

    /// <summary>
    /// Implementation to create and initialize instances of type <typeparamref name="TObject"/>.
    /// </summary>
    /// <returns>An instance of type <typeparamref name="TObject"/>.</returns>
    protected abstract TObject CreateInstance();
    /// <summary>
    /// Implementation to create and initialize instances of type <typeparamref name="TObject"/>.
    /// </summary>
    /// <returns>An instance of type <typeparamref name="TObject"/>.</returns>
    /// <param name="args">A variable list of arguments.</param>
    protected abstract TObject CreateInstance(params object[] args);
  }
}