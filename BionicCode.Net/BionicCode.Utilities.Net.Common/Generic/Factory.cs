namespace BionicCode.Utilities.Net
{
  #region Info

  // 2020/09/19  17:17
  // BionicCode.Utilities.Net

  #endregion

  using System;

  /// <summary>
  /// Provides a base class of <see cref="IFactory{TCreate}"/> that supports lifetime management using scopes and <see cref="FactoryMode"/>.
  /// </summary>
  /// <typeparam name="TObject">The type of the instance to create.</typeparam>
  /// <remarks>
  /// <para>This base class is provided to make it easier for implementers of <see cref="IFactory{TCreate}"/> to create a custom factory. Implementers are encouraged to extend this base class instead of creating their own.
  /// <br/><see cref="Factory{TObject}"/> provides useful features like factory scopes and other lifetime managment modes like shared instances.</para>
  /// <para>The internals will handle the lifetime management based on the value of the <see cref="FactoryMode"/> property. To control instantiation of the factory products, simply implement the abstract <see cref="CreateInstance()"/> and <see cref="CreateInstance(object[])"/> members.</para>
  /// </remarks>
  /// <example>
  /// The following example shows how to implement an <see cref="IFactory{TCreate}"/> by extending the anstract <see cref="Factory{TObject}"/> clas.
  /// <br/>It also shows how to create and use a factory scope.
  /// <code>
  ///public class PersonFactoryExample
  ///{
  ///  PersonFactory PersonFactory { get; }
  ///  public FactoryTest()
  ///  {
  ///    this.PersonFactory = new PersonFactory();
  ///  }
  ///  
  ///  public void FactoryExample()
  ///  {
  ///    // Configure factory to produce shared instances
  ///    this.PersonFactory.FactoryMode = FactoryMode.Singleton;
  ///    
  ///    Person firstSharedInstanceInScope = null;
  ///    Person secondSharedInstanceInScope = null;
  ///    Person sharedInstanceCreatedBeforeScope = this.PersonFactory.Create();
  ///    
  ///    // Create a new factory scope
  ///    using (IDisposable factoryScope = this.PersonFactory.CreateScope())
  ///    {
  ///      firstSharedInstanceInScope = this.PersonFactory.Create();
  ///      secondSharedInstanceInScope = this.PersonFactory.Create();
  ///    }
  ///    
  ///    Person sharedInstanceCreatedAfterScope = this.PersonFactory.Create();
  ///      
  ///    bool isReferenceEqual = ReferenceEquals(firstSharedInstanceInScope, secondShartedInstanceInScope); // true
  ///    isReferenceEqual = ReferenceEquals(firstSharedInstanceInScope, sharedInstanceCreatedBeforeScope); // false
  ///    isReferenceEqual = ReferenceEquals(secondSharedInstanceInScope, sharedInstanceCreatedBeforeScope); // false
  ///    isReferenceEqual = ReferenceEquals(firstSharedInstanceInScope, sharedInstanceCreatedAfterScope); // false
  ///    isReferenceEqual = ReferenceEquals(secondSharedInstanceInScope, sharedInstanceCreatedAfterScope); // false
  ///    isReferenceEqual = ReferenceEquals(sharedInstanceCreatedBeforeScope, sharedInstanceCreatedAfterScope); // true
  ///    
  ///    // Configure factory to produce transient instances
  ///    this.PersonFactory.FactoryMode = FactoryMode.Transient;
  ///    
  ///    Person firstTransientInstance = this.PersonFactory.Create();
  ///    Person secondTransientInstance = this.PersonFactory.Create();
  ///    
  ///    isReferenceEqual = ReferenceEquals(firstTransientInstance, secondTransientInstance); // false
  ///  }
  ///}
  ///
  ///public class PersonFactory : Factory{Person}
  ///{
  ///  protected override Person CreateInstance() 
  ///    => new Person("DefaultPersonFirstName", "DefaultPersonLastName", -1);
  ///    
  ///  protected override Person CreateInstance(params object[] args) 
  ///    => new Person(args[0] as string, args[1] as string, (int)args[2]);
  ///}
  ///
  ///public class Person
  ///{
  ///  public Person(string firstName, string lastName, int id)
  ///  {
  ///    this.FirstName = firstName;
  ///    this.LastName = lastName;
  ///    this.Id = id;
  ///  }
  ///  
  ///  public string FirstName { get; }
  ///  public string LastName { get; }
  ///  public int Id { get; }
  ///}
  /// </code>
  /// </example>
  public abstract class Factory<TObject> : IFactory<TObject>
  {
    /// <summary>
    /// Initializes the <c>Factory</c> to create instances using <see cref="FactoryMode.Singleton"/>. 
    /// </summary>
    protected Factory() : this(FactoryMode.Singleton)
    {
    }

    /// <summary>
    /// Initializes instance. 
    /// </summary>
    /// <param name="factoryMode">Describes the created objects lifetime using <see cref="FactoryMode"/>.</param>
    protected Factory(FactoryMode factoryMode) => this.FactoryMode = factoryMode;

    /// <summary>
    /// The shared instance which <see cref="Factory{TObject}"/> returns when <see cref="FactoryMode"/>
    /// is set to <see cref="FactoryMode.Singleton"/> or <see cref="FactoryMode.Scoped"/>.
    /// </summary>
    protected TObject SharedProductInstance { get; private set; }

    private FactoryMode factoryMode;
    /// <inheritdoc />
    public FactoryMode FactoryMode
    {
      get => this.IsScoped
        ? this.ScopedFactory.FactoryMode
        : this.factoryMode;
      set
      {
        if (this.IsScoped)
        {
          throw new InvalidOperationException(ExceptionMessages.GetInvalidOperationExceptionMessage_SetFactoryModeOnScopedFactory());
        }

        this.factoryMode = value;
      }
    }

    #region (Implementation of IFactoy<T>)

    /// <inheritdoc />
    public TObject Create()
    {
      if (this.IsScoped)
      {
        return this.ScopedFactory.Create();
      }

      if (this.FactoryMode is FactoryMode.Singleton
        || this.FactoryMode is FactoryMode.Scoped)
      {
        if (!this.IsInitialized)
        {
          this.IsInitialized = true;
          this.SharedProductInstance = CreateInstance();
        }

        return this.SharedProductInstance;
      }

      return CreateInstance();
    }

    /// <inheritdoc />
    public TObject Create(params object[] args)
    {
      if (this.IsScoped)
      {
        return this.ScopedFactory.Create(args);
      }

      if (this.FactoryMode is FactoryMode.Singleton
        || this.FactoryMode is FactoryMode.Scoped)
      {
        if (!this.IsInitialized)
        {
          this.IsInitialized = true;
          this.SharedProductInstance = CreateInstance(args);
        }

        return this.SharedProductInstance;
      }

      return CreateInstance(args);
    }

    #endregion

    /// <summary>
    /// Creates a scope in which created instances of the current <see cref="Factory{TObject}"/> are shared instances.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> to define the scope.</returns>
    /// <remarks>The instances returned from the current <see cref="Factory{TObject}"/> instance are shared instances within this scope.
    /// <br/>This means the returned instances are the same whithin the current scope, but different from the enclosing scope.
    /// <para>To define the scope, simply call <see cref="CreateScope"/> and wrap the returned <see cref="IDisposable"/> inside a <see langword="using"/> statement or expression.</para></remarks>
    /// <example>
    /// The following example shows how to create a factory scope. See <see cref="Factory{TObject}"/> for the complete example.
    /// <code>
    ///public class PersonFactoryExample
    ///{
    ///  PersonFactory PersonFactory { get; }
    ///  public FactoryTest()
    ///  {
    ///    this.PersonFactory = new PersonFactory();
    ///  }
    ///  
    ///  public void FactoryExample()
    ///  {
    ///    // Configure factory to produce shared instances
    ///    this.PersonFactory.FactoryMode = FactoryMode.Singleton;
    ///    
    ///    Person firstSharedInstanceInScope = null;
    ///    Person secondSharedInstanceInScope = null;
    ///    Person sharedInstanceCreatedBeforeScope = this.PersonFactory.Create();
    ///    
    ///    // Create a new factory scope
    ///    using (IDisposable factoryScope = this.PersonFactory.CreateScope())
    ///    {
    ///      firstSharedInstanceInScope = this.PersonFactory.Create();
    ///      secondSharedInstanceInScope = this.PersonFactory.Create();
    ///    }
    ///    
    ///    Person sharedInstanceCreatedAfterScope = this.PersonFactory.Create();
    ///      
    ///    bool isReferenceEqual = ReferenceEquals(firstSharedInstanceInScope, secondShartedInstanceInScope); // true
    ///    isReferenceEqual = ReferenceEquals(firstSharedInstanceInScope, sharedInstanceCreatedBeforeScope); // false
    ///    isReferenceEqual = ReferenceEquals(secondSharedInstanceInScope, sharedInstanceCreatedBeforeScope); // false
    ///    isReferenceEqual = ReferenceEquals(firstSharedInstanceInScope, sharedInstanceCreatedAfterScope); // false
    ///    isReferenceEqual = ReferenceEquals(secondSharedInstanceInScope, sharedInstanceCreatedAfterScope); // false
    ///    isReferenceEqual = ReferenceEquals(sharedInstanceCreatedBeforeScope, sharedInstanceCreatedAfterScope); // true
    ///    
    ///    // Configure factory to produce transient instances
    ///    this.PersonFactory.FactoryMode = FactoryMode.Transient;
    ///    
    ///    Person firstTransientInstance = this.PersonFactory.Create();
    ///    Person secondTransientInstance = this.PersonFactory.Create();
    ///    
    ///    isReferenceEqual = ReferenceEquals(firstTransientInstance, secondTransientInstance); // false
    ///  }
    ///}
    /// </code>
    /// </example>
    public IDisposable CreateScope()
    {
      this.ScopedFactory = new ScopedFactory<TObject>(this);
      return this.ScopedFactory;
    }

    internal TObject CreateInstanceBase() => CreateInstance();
    internal TObject CreateInstanceBase(params object[] args) => CreateInstance(args);

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

    internal ScopedFactory<TObject> ScopedFactory { get; set; }
    internal bool IsScoped { get; set; }
    private bool IsInitialized { get; set; }
  }
}