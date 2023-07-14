namespace BionicCode.Utilities.Net
{
  using System;

  internal class ScopedFactory<TCreate> : Factory<TCreate>, IDisposable
  {
    private bool disposedValue;

    new public FactoryMode FactoryMode { get => FactoryMode.Scoped; }

    protected override TCreate CreateInstance() => this.Factory.CreateInstanceBase();

    protected override TCreate CreateInstance(params object[] args) => this.Factory.CreateInstanceBase(args);

    private Factory<TCreate> Factory { get; }

    public ScopedFactory(Factory<TCreate> factory) : base(FactoryMode.Scoped)
    {
      this.Factory = factory;
      this.Factory.IsScoped = true;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposedValue)
      {
        if (disposing)
        {
          this.Factory.IsScoped = false;
          if (this.SharedProductInstance is IDisposable disposable)
          {
            disposable.Dispose();
          }
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        this.disposedValue = true;
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~FactoryScope()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}