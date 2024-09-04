using System;
using System.Runtime.Serialization;

namespace BionicCode.Utilities.Net
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
