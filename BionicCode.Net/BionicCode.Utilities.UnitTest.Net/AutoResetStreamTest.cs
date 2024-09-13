namespace BionicCode.Utilities.Net.UnitTest
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Text;
  using System.Threading;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net;
  using FluentAssertions;
  using Xunit;

  public class AutoResetStreamTest : IDisposable
  {
    private AutoResetStream AutoResetStream { get; }
    private string TestText { get; }
    private int TestTextLength { get; }


    public AutoResetStreamTest()
    {
      this.AutoResetStream = new AutoResetStream();
      this.TestText = "Test text";
      this.TestTextLength = this.TestText.Length;
    }

    public void Dispose() => this.AutoResetStream.Dispose();

    private void FillStream()
    {
      var memStream = new MemoryStream();
      using (var streamWriter = new StreamWriter(memStream, Encoding.Default, 1024, true))
      {
        streamWriter.Write(this.TestText);
        streamWriter.Flush();

        this.AutoResetStream.BaseStream = memStream;
      }
    }

    [Fact]
    public async Task ResetStreamPositionAfterReadAsync()
    {
      FillStream();

      byte[] buffer = new byte[1024];
      int bytesRead = await this.AutoResetStream.ReadAsync(buffer, 0, buffer.Length);

      _ = this.AutoResetStream.Position.Should().Be(0);
      _ = bytesRead.Should().Be(this.TestTextLength);
    }

    [Fact]
    public async Task ResetStreamPositionAfterWriteAsync()
    {
      byte[] buffer = Encoding.UTF8.GetBytes(this.TestText);
      await this.AutoResetStream.WriteAsync(buffer, 0, buffer.Length);

      _ = this.AutoResetStream.Position.Should().Be(0);
      _ = this.AutoResetStream.Length.Should().Be(this.TestTextLength);
    }

    [Fact]
    public void ResetStreamPositionAfterRead()
    {
      FillStream();
      byte[] buffer = new byte[1024];
      int bytesRead = this.AutoResetStream.Read(buffer, 0, buffer.Length);

      _ = this.AutoResetStream.Position.Should().Be(0);
      _ = bytesRead.Should().Be(this.TestTextLength);
    }

    [Fact]
    public void ResetStreamPositionAfterWrite()
    {
      byte[] buffer = Encoding.UTF8.GetBytes(this.TestText);
      this.AutoResetStream.Write(buffer, 0, buffer.Length);

      _ = this.AutoResetStream.Position.Should().Be(0);
      _ = this.AutoResetStream.Length.Should().Be(this.TestTextLength);
    }

    [Fact]
    public void ResetStreamPositionAfterReadByte()
    {
      FillStream();
      byte[] buffer = Encoding.UTF8.GetBytes(this.TestText);
      byte firstByteInBuffer = buffer.First();

      int byteRead = this.AutoResetStream.ReadByte();

      _ = this.AutoResetStream.Position.Should().Be(0);
      _ = byteRead.Should().Be(firstByteInBuffer);
    }

    [Fact]
    public void ResetStreamPositionAfterWriteByte()
    {
      byte[] buffer = Encoding.UTF8.GetBytes(this.TestText);
      this.AutoResetStream.WriteByte(buffer.First());

      _ = this.AutoResetStream.Position.Should().Be(0);
      _ = this.AutoResetStream.Length.Should().Be(1);
    }

    [Fact]
    public async Task ResetStreamPositionAfterCopyAsync()
    {
      FillStream();
      using (var destinationStream = new MemoryStream())
      {
        await this.AutoResetStream.CopyToAsync(destinationStream, this.TestTextLength, CancellationToken.None);

        _ = this.AutoResetStream.Position.Should().Be(0);
        _ = destinationStream.Length.Should().Be(this.TestTextLength);
      }
    }
  }
}
