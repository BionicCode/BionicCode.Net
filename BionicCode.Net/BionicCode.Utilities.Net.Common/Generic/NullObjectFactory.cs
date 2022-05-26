#region Info
// //  
// BionicUtilities.Net.Standard
#endregion

using System;
using System.Threading;

namespace BionicCode.Utilities.Net
{
  /// <summary>
  /// Implementation of <see cref="INullObject"/> and <see cref="IFactory{TCreate}"/>. Used to create an instance of a <see cref="INullObject"/> where <see cref="INullObject.IsNull"/> is set to <c>true</c> by default.
  /// </summary>
  /// <typeparam name="TObject"></typeparam>
  public class NullObjectFactory<TObject> : Factory<TObject> where TObject : INullObject
  {
    private Func<TObject> FactoryMethod { get; }

    /// <summary>
    /// Initializes the <see cref="NullObjectFactory{TObject}"/> using a factory method delegate using <see cref="FactoryMode.Singleton"/>.
    /// </summary>
    /// <param name="factoryMethod">The delegate to create instances of type <typeparamref name="TObject"/>.</param>
    public NullObjectFactory(Func<TObject> factoryMethod) : this(factoryMethod, FactoryMode.Singleton)
    {
      
    }

    /// <summary>
    /// Initializes the <see cref="NullObjectFactory{TObject}"/> using a factory method delegate using <see cref="FactoryMode.Singleton"/>.
    /// </summary>
    /// <param name="factoryMethod">The delegate to create instances of type <typeparamref name="TObject"/>.</param>
    /// <param name="factoryMode">The lifetime scope of the created instance.</param>
    public NullObjectFactory(Func<TObject> factoryMethod, FactoryMode factoryMode) : base(factoryMode)
    {
      this.FactoryMethod = factoryMethod;
    }

    /// <summary>
    /// Initializes the <see cref="NullObjectFactory{TObject}"/> using a <see cref="IFactory{TCreate}"/> and its <see cref="IFactory{TCreate}.FactoryMode"/> to set the <see cref="FactoryMode"/>.
    /// </summary>
    /// <param name="factory">The <see cref="IFactory{TCreate}"/> to create instances of type <typeparamref name="TObject"/>.</param>
    public NullObjectFactory(IFactory<TObject> factory) : this(factory.Create, factory.FactoryMode)
    {
    }

    #region Overrides of Factory<TObject>

    /// <inheritdoc />
    protected override TObject CreateInstance()
    {
      TObject instance = this.FactoryMethod.Invoke();
      instance.IsNull = true;
      return instance;
    }

    /// <inheritdoc />
    protected override TObject CreateInstance(params object[] args) => CreateInstance();

    #endregion
  }
}