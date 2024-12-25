namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;

  internal class ClientHandlerInfoCollection : IEnumerable<ClientHandlerInfo>
  {
    public int Count => this.items.Count;
    private readonly List<ClientHandlerInfo> items;

    public ClientHandlerInfoCollection()
    {
      this.items = new List<ClientHandlerInfo>();
    }

    public IEnumerable<ClientHandlerInfo> EnumerateSafe()
    {
      var itemsCopy = this.items.ToList();
      for (int index = itemsCopy.Count - 1; index >= 0; index--)
      {
        ClientHandlerInfo item = itemsCopy[index];
        if (item.IsClientHandlerAlive)
        {
          yield return item;
        }
      }
    }

    public void Add(ClientHandlerInfo clientHandlerInfo)
    {
      StartListeningToItem(clientHandlerInfo);
      this.items.Add(clientHandlerInfo);
    }

    public void Remove(ClientHandlerInfo clientHandlerInfo)
      => clientHandlerInfo.Dispose();

    public void Clear()
    {
      for (int index = this.items.Count - 1; index >= 0; index--)
      {
        ClientHandlerInfo item = this.items[index];
        StopListeningToItem(item);
        item.Dispose();
        this.items.RemoveAt(index);
      }
    }

    private void OnItemDisposed(object sender, EventArgs e)
    {
      var item = (ClientHandlerInfo)sender;
      StopListeningToItem(item);
      _ = this.items.Remove(item);
    }

    private void StartListeningToItem(ClientHandlerInfo clientHandlerInfo)
      => clientHandlerInfo.Disposed += OnItemDisposed;

    private void StopListeningToItem(ClientHandlerInfo clientHandlerInfo)
      => clientHandlerInfo.Disposed -= OnItemDisposed;

    IEnumerator<ClientHandlerInfo> IEnumerable<ClientHandlerInfo>.GetEnumerator()
    {
      foreach (ClientHandlerInfo item in EnumerateSafe())
      {
        yield return item;
      }
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ClientHandlerInfo>)this).GetEnumerator();
  }
}