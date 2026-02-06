namespace BionicCode.Utilities.Net
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

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
            get => baseStream;
            set
            {
                baseStream = value;
                Reset();
            }
        }

        /// <summary>
        /// Defines the position to which the stream should reset to.
        /// </summary>
        /// <value>A <see cref="SeekOrigin"/> value.</value>
        public SeekOrigin ResetOrigin { get; set; }

        /// <summary>
        /// Gets whether the decorated underlying <see cref="Stream"/> will be closed or disposed when the <see cref="AutoResetStream"/> instance is closed or disposed. Use constructor to set the value in order to configure the behavior.
        /// </summary>
        public bool IsDisposingDecoratedStream { get; }

        /// <summary>
        /// Default constructor. Creates an instance where the <see cref="BaseStream"/> is set to a <see cref="MemoryStream"/>.
        /// </summary>
        public AutoResetStream() => BaseStream = new MemoryStream();

        /// <summary>
        /// MemberConstructor which accepts the <see cref="Stream"/> instance to decorate in order to extend its behavior.
        /// </summary>
        /// <param name="baseStream">The <see cref="Stream"/> instance to decorate in order to extend its behavior.</param>
        public AutoResetStream(Stream baseStream) : this(baseStream, SeekOrigin.Begin, true)
        {
        }

        /// <summary>
        /// MemberConstructor which accepts the <see cref="Stream"/> instance to decorate in order to extend its behavior.
        /// </summary>
        /// <param name="baseStream">The <see cref="Stream"/> instance to decorate in order to extend its behavior.</param>
        /// <param name="leaveDecoratedStreamOpen">When set to <see langword="true"/> the decorated underlying <see cref="Stream"/> will be disposed or closed too, if the <see cref="AutoResetStream"/> is disposed or closed.</param>
        public AutoResetStream(Stream baseStream, bool leaveDecoratedStreamOpen) : this(baseStream, SeekOrigin.Begin, leaveDecoratedStreamOpen)
        {
            BaseStream = baseStream;
            IsDisposingDecoratedStream = leaveDecoratedStreamOpen;
        }

        /// <summary>
        /// MemberConstructor which accepts the <see cref="Stream"/> instance to decorate in order to extend its behavior.
        /// </summary>
        /// <param name="baseStream">The <see cref="Stream"/> instance to decorate in order to extend its behavior.</param>
        /// <param name="resetOrigin">The origin to which the stream should be reset to.</param>
        /// <param name="leaveDecoratedStreamOpen">When set to <see langword="true"/> the decorated underlying <see cref="Stream"/> will be disposed or closed too, if the <see cref="AutoResetStream"/> is disposed or closed.</param>
        public AutoResetStream(Stream baseStream, SeekOrigin resetOrigin, bool leaveDecoratedStreamOpen)
        {
            BaseStream = baseStream;
            IsDisposingDecoratedStream = leaveDecoratedStreamOpen;
            ResetOrigin = resetOrigin;
        }

        /// <summary>
        /// Resets the <see cref="Stream.Position"/> to an offset of '0' relative to the provided <paramref name="seekOrigin"/>.
        /// </summary>
        /// <param name="seekOrigin">The optional relative position of the <see cref="Stream"/> to apply the zero offset to. The default is <see cref="SeekOrigin.Begin"/>.</param>
        public void Reset(SeekOrigin seekOrigin = SeekOrigin.Current) => BaseStream.Seek(0, seekOrigin == SeekOrigin.Current ? ResetOrigin : seekOrigin);

        #region Overrides of Stream

        /// <inheritdoc />
        public override void Flush() => BaseStream.Flush();

        /// <inheritdoc />
        public override Task FlushAsync(CancellationToken cancellationToken) => BaseStream.FlushAsync(cancellationToken);

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = BaseStream.Read(buffer, offset, count);
            Reset();
            return bytesRead;
        }

        /// <inheritdoc />
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int bytesRead = await BaseStream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            Reset();
            return bytesRead;
        }

        /// <inheritdoc />
        public override int ReadByte()
        {
            int bytesRead = BaseStream.ReadByte();
            Reset();
            return bytesRead;
        }

        /// <inheritdoc />
        public override int ReadTimeout { get => BaseStream.ReadTimeout; set => BaseStream.ReadTimeout = value; }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

        /// <inheritdoc />
        public override void SetLength(long value) => BaseStream.SetLength(value);

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
            Reset();
        }

        /// <inheritdoc />
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await BaseStream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            Reset();
        }

        /// <inheritdoc />
        public override void WriteByte(byte value)
        {
            BaseStream.WriteByte(value);
            Reset();
        }

        /// <inheritdoc />
        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            await BaseStream.CopyToAsync(destination, bufferSize, cancellationToken).ConfigureAwait(false);
            Reset();
        }

        /// <inheritdoc />
        public override int WriteTimeout { get => BaseStream.WriteTimeout; set => BaseStream.WriteTimeout = value; }

        /// <inheritdoc />
        public override bool CanRead => BaseStream.CanRead;

        /// <inheritdoc />
        public override bool CanTimeout => BaseStream.CanTimeout;

        /// <inheritdoc />
        public override void Close()
        {
            if (IsDisposingDecoratedStream)
            {
                BaseStream.Close();
            }

            base.Close();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (IsDisposingDecoratedStream)
            {
                BaseStream.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Overrides of Object

        /// <inheritdoc />
        public override bool Equals(object obj) => BaseStream.Equals(obj);

        /// <inheritdoc />
        public override int GetHashCode() => BaseStream.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => BaseStream.ToString();

        #endregion

        /// <inheritdoc />
        public override bool CanSeek => BaseStream.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => BaseStream.CanWrite;

        /// <inheritdoc />
        public override long Length => BaseStream.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        #endregion
    }
}
