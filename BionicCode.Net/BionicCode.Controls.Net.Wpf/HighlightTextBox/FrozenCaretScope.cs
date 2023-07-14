namespace BionicCode.Controls.Net.Wpf
{
  using System;

  internal class FrozenCaretScope : IDisposable
  {
    private bool disposedValue;

    public FrozenCaretScope(CaretAdorner caret)
    {
      this.Caret = caret;
      this.Caret.Freeze();
    }

    public CaretAdorner Caret { get; }

    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposedValue)
      {
        if (disposing)
        {
          this.Caret.Unfreeze();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        this.disposedValue = true;
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~FrozenCaretScope()
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
