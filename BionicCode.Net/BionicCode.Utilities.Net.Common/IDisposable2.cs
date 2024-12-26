namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  internal interface IDisposable2 : IDisposable
  {
    bool IsDisposed { get; }
  }
}
