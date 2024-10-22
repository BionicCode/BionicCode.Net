namespace BionicCode.Utilities.Net
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Runtime.CompilerServices;
  using System.Runtime.Serialization;
  using JetBrains.Annotations;

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
