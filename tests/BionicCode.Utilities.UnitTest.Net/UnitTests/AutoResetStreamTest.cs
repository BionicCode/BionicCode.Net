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
            AutoResetStream = new AutoResetStream();
            TestText = "Test text";
            TestTextLength = TestText.Length;
        }

        public void Dispose() => AutoResetStream.Dispose();

        private void FillStream()
        {
            var memStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memStream, Encoding.Default, 1024, true);
            streamWriter.Write(TestText);
            streamWriter.Flush();

            AutoResetStream.BaseStream = memStream;
        }

        [Fact]
        public async Task ResetStreamPositionAfterReadAsync()
        {
            FillStream();

            byte[] buffer = new byte[1024];
            int bytesRead = await AutoResetStream.ReadAsync(buffer, 0, buffer.Length);

            _ = AutoResetStream.Position.Should().Be(0);
            _ = bytesRead.Should().Be(TestTextLength);
        }

        [Fact]
        public async Task ResetStreamPositionAfterWriteAsync()
        {
            byte[] buffer = Encoding.UTF8.GetBytes(TestText);
            await AutoResetStream.WriteAsync(buffer, 0, buffer.Length);

            _ = AutoResetStream.Position.Should().Be(0);
            _ = AutoResetStream.Length.Should().Be(TestTextLength);
        }

        [Fact]
        public void ResetStreamPositionAfterRead()
        {
            FillStream();
            byte[] buffer = new byte[1024];
            int bytesRead = AutoResetStream.Read(buffer, 0, buffer.Length);

            _ = AutoResetStream.Position.Should().Be(0);
            _ = bytesRead.Should().Be(TestTextLength);
        }

        [Fact]
        public void ResetStreamPositionAfterWrite()
        {
            byte[] buffer = Encoding.UTF8.GetBytes(TestText);
            AutoResetStream.Write(buffer, 0, buffer.Length);

            _ = AutoResetStream.Position.Should().Be(0);
            _ = AutoResetStream.Length.Should().Be(TestTextLength);
        }

        [Fact]
        public void ResetStreamPositionAfterReadByte()
        {
            FillStream();
            byte[] buffer = Encoding.UTF8.GetBytes(TestText);
            byte firstByteInBuffer = buffer.First();

            int byteRead = AutoResetStream.ReadByte();

            _ = AutoResetStream.Position.Should().Be(0);
            _ = byteRead.Should().Be(firstByteInBuffer);
        }

        [Fact]
        public void ResetStreamPositionAfterWriteByte()
        {
            byte[] buffer = Encoding.UTF8.GetBytes(TestText);
            AutoResetStream.WriteByte(buffer.First());

            _ = AutoResetStream.Position.Should().Be(0);
            _ = AutoResetStream.Length.Should().Be(1);
        }

        [Fact]
        public async Task ResetStreamPositionAfterCopyAsync()
        {
            FillStream();
            using var destinationStream = new MemoryStream();
            await AutoResetStream.CopyToAsync(destinationStream, TestTextLength, CancellationToken.None);

            _ = AutoResetStream.Position.Should().Be(0);
            _ = destinationStream.Length.Should().Be(TestTextLength);
        }
    }
}
