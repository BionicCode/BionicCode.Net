using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BionicCode.Utilities.Net.Standard.IO
{
  /// <summary>
  /// Decorates a <see cref="Stream"/> instance to support auto-rewind after read/write access.
  /// </summary>
  public class AutoResetStream : Stream
  {
    private Stream baseStream;

    /// <summary>
    /// The decorated <see cref="Stream"/> instance which will be extended.
    /// </summary>
    /// <value>An instance of type <see cref="Stream"/>. This instance will be decorated to extend the default <see cref="Stream"/> features and behaviors.</value>
    public Stream BaseStream
    {
      get => this.baseStream;
      set
      {
        this.baseStream = value;
        Reset();
      }
    }

    /// <summary>
    /// Gets whether the decorated underlying <see cref="Stream"/> will be closed or disposed when the <see cref="AutoResetStream"/> instance is closed or disposed. Use constructor to set the value in order to configure the behavior.
    /// </summary>
    public bool IsDisposingDecoratedStream { get; }

    /// <summary>
    /// Default constructor. Creates an instance where the <see cref="BaseStream"/> is set to a <see cref="MemoryStream"/>.
    /// </summary>
    public AutoResetStream()
    {
      this.BaseStream = new MemoryStream();
    }

    /// <summary>
    /// Constructor which accepts the <see cref="Stream"/> instance to decorate in order to extend its behavior.
    /// </summary>
    /// <param name="baseStream">The <see cref="Stream"/> instance to decorate in order to extend its behavior.</param>
    public AutoResetStream(Stream baseStream) : this(baseStream, true)
    {
    }

    /// <summary>
    /// Constructor which accepts the <see cref="Stream"/> instance to decorate in order to extend its behavior.
    /// </summary>
    /// <param name="baseStream">The <see cref="Stream"/> instance to decorate in order to extend its behavior.</param>
    /// <param name="isDisposingDecoratedStream">When set to <c>true</c> the decorated underlying <see cref="Stream"/> will be disposed or closed too, if the <see cref="AutoResetStream"/> is disposed or closed.</param>
    public AutoResetStream(Stream baseStream, bool isDisposingDecoratedStream)
    {
      this.BaseStream = baseStream;
      this.IsDisposingDecoratedStream = isDisposingDecoratedStream;
    }

    /// <summary>
    /// Resets the <see cref="Stream.Position"/> to an offset of '0' relative to the provided <paramref name="seekOrigin"/>.
    /// </summary>
    /// <param name="seekOrigin">The optional relative position of the <see cref="Stream"/> to apply the zero offset to. The default is <see cref="SeekOrigin.Begin"/>.</param>
    public void Reset(SeekOrigin seekOrigin = SeekOrigin.Begin) => this.BaseStream.Seek(0, seekOrigin);

    #region Overrides of Stream

    /// <inheritdoc />
    public override void Flush() => this.BaseStream.Flush();

    /// <inheritdoc />
    public override Task FlushAsync(CancellationToken cancellationToken) => this.BaseStream.FlushAsync(cancellationToken);

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
      int bytesRead = this.BaseStream.Read(buffer, offset, count);
      Reset();
      return bytesRead;
    }

    /// <inheritdoc />
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      int bytesRead = await this.BaseStream.ReadAsync(buffer, offset, count, cancellationToken);
      Reset();
      return bytesRead;
    }

    /// <inheritdoc />
    public override int ReadByte()
    {
      int bytesRead = this.BaseStream.ReadByte();
      Reset();
      return bytesRead;
    }

    /// <inheritdoc />
    public override int ReadTimeout { get => this.BaseStream.ReadTimeout; set => this.BaseStream.ReadTimeout = value; }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => this.BaseStream.Seek(offset, origin);

    /// <inheritdoc />
    public override void SetLength(long value) => this.BaseStream.SetLength(value);

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
      this.BaseStream.Write(buffer, offset, count);
      Reset();
    }

    /// <inheritdoc />
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      await this.BaseStream.WriteAsync(buffer, offset, count, cancellationToken);
      Reset();
    }

    /// <inheritdoc />
    public override void WriteByte(byte value)
    {
      this.BaseStream.WriteByte(value);
      Reset();
    }

    /// <inheritdoc />
    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
      await this.BaseStream.CopyToAsync(destination, bufferSize, cancellationToken);
      Reset();
    }
    
    /// <inheritdoc />
    public override int WriteTimeout { get => this.BaseStream.WriteTimeout; set => this.BaseStream.WriteTimeout = value; }

    /// <inheritdoc />
    public override bool CanRead => this.BaseStream.CanRead;

    /// <inheritdoc />
    public override bool CanTimeout => this.BaseStream.CanTimeout;

    /// <inheritdoc />
    public override void Close()
    {
      if (this.IsDisposingDecoratedStream)
      {
        this.BaseStream.Close();
      }
      base.Close();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (this.IsDisposingDecoratedStream)
      {
        this.BaseStream.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Overrides of Object

    /// <inheritdoc />
    public override bool Equals(object obj) => this.BaseStream.Equals(obj);

    /// <inheritdoc />
    public override int GetHashCode() => this.BaseStream.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => this.BaseStream.ToString();

    #endregion

    #region Overrides of MarshalByRefObject

    /// <inheritdoc />
    public override object InitializeLifetimeService() => this.BaseStream.InitializeLifetimeService();

    #endregion

    /// <inheritdoc />
    public override bool CanSeek => this.BaseStream.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => this.BaseStream.CanWrite;

    /// <inheritdoc />
    public override long Length => this.BaseStream.Length;

    /// <inheritdoc />
    public override long Position
    {
      get => this.BaseStream.Position;
      set => this.BaseStream.Position = value;
    }

    #endregion
  }
}
