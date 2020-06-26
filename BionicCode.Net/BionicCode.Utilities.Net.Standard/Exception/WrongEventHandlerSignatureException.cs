using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BionicCode.Utilities.Net.Standard.Exception
{
  [Serializable]
  public class WrongEventHandlerSignatureException : System.Exception
  {
    public WrongEventHandlerSignatureException()
    {
    }

    public WrongEventHandlerSignatureException(string message) : base(message)
    {
    }

    public WrongEventHandlerSignatureException(string message, System.Exception inner) : base(message, inner)
    {
    }

    protected WrongEventHandlerSignatureException(
      SerializationInfo info,
      StreamingContext context) : base(info, context)
    {
    }
  }
}
