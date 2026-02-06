namespace BionicCode.Utilities.Net
{
    using System;
    using System.Threading;

    internal class ClientHandlerInfo : IDisposable
    {
        public ClientHandlerInfo(Delegate clientHandler, Action<object, object[], ClientHandlerInfo> clientAdapterHandler, SynchronizationContext clientContext)
        {
            clientHandler = clientHandler;
            ClientAdapterHandler = clientAdapterHandler;
            ClientContext = clientContext;
        }

        private void OnDisposed()
          => Disposed?.Invoke(this, EventArgs.Empty);

        public void Clear()
          => Dispose();

        public bool TryGetClientHandler(out Delegate handler)
        {
            handler = null;
            if (IsDisposed || clientHandler is null)
            {
                return false;
            }

            handler = clientHandler;

            return true;
        }

        public event EventHandler Disposed;
        public bool IsDisposed { get; private set; }
        public Action<object, object[], ClientHandlerInfo> ClientAdapterHandler { get; }
        public SynchronizationContext ClientContext { get; }
        public bool IsClientHandlerAlive => !IsDisposed && clientHandler != null;
        private Delegate clientHandler;

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    clientHandler = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                IsDisposed = true;
                OnDisposed();
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ClientHandlerInfo()
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
