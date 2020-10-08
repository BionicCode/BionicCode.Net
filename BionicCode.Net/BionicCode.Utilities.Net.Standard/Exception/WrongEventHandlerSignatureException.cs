using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BionicCode.Utilities.Net.Standard.Exception
{
  /// <inheritdoc />
  [Serializable]
  public class WrongEventHandlerSignatureException : System.Exception
  {
    /// <inheritdoc />
    public WrongEventHandlerSignatureException()
    {
    }

    /// <inheritdoc />
    public WrongEventHandlerSignatureException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public WrongEventHandlerSignatureException(string message, System.Exception inner) : base(message, inner)
    {
    }

    /// <inheritdoc />
    protected WrongEventHandlerSignatureException(
      SerializationInfo info,
      StreamingContext context) : base(info, context)
    {
    }
  }
}
