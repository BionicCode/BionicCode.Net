using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BionicCode.Utilities.Net.Standard.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BionicCode.Utilities.UnitTest.Net.Standard
{
  [TestClass]
  public class AutoResetStreamTest
  {
    public AutoResetStream AutoResetStream { get; set; }
    public string TestText { get; set; }
    public int TestTextLength { get; set; }

    [TestInitialize]
    public void Initialize()
    {
      this.AutoResetStream = new AutoResetStream();
      this.TestText = "Test text";
      this.TestTextLength = this.TestText.Length;
    }

    private void FillStream()
    {
      var memStream = new MemoryStream();
      using (var streamWriter = new StreamWriter(memStream, Encoding.Default, 1024, true))
      {
        streamWriter.Write(this.TestText);
      }

      this.AutoResetStream.BaseStream = memStream;
    }

    [TestMethod]
    public async Task ResetStreamPositionAfterReadAsync()
    {
      FillStream();
      using (this.AutoResetStream)
      {
        byte[] buffer = new byte[1024];
        int bytesRead = await this.AutoResetStream.ReadAsync(buffer, 0, buffer.Length);
        Assert.AreEqual(0, this.AutoResetStream.Position);
        Assert.AreEqual(this.TestTextLength, bytesRead);
      }
    }

    [TestMethod]
    public async Task ResetStreamPositionAfterWriteAsync()
    {
      using (this.AutoResetStream)
      {
        byte[] buffer = Encoding.UTF8.GetBytes(this.TestText);
        await this.AutoResetStream.WriteAsync(buffer, 0, buffer.Length);
        Assert.AreEqual(0, this.AutoResetStream.Position);
        Assert.AreEqual(this.TestTextLength, this.AutoResetStream.Length);
      }
    }

    [TestMethod]
    public void ResetStreamPositionAfterRead()
    {
      FillStream();
      using (this.AutoResetStream)
      {
        byte[] buffer = new byte[1024];
        int bytesRead = this.AutoResetStream.Read(buffer, 0, buffer.Length);
        Assert.AreEqual(0, this.AutoResetStream.Position);
        Assert.AreEqual(this.TestTextLength, bytesRead);
      }
    }

    [TestMethod]
    public void ResetStreamPositionAfterWrite()
    {
      FillStream();
      using (this.AutoResetStream)
      {
        byte[] buffer = Encoding.UTF8.GetBytes(this.TestText);
        this.AutoResetStream.Write(buffer, 0, buffer.Length);
        Assert.AreEqual(0, this.AutoResetStream.Position);
        Assert.AreEqual(this.TestTextLength, this.AutoResetStream.Length);
      }
    }

    [TestMethod]
    public void ResetStreamPositionAfterReadByte()
    {
      FillStream();
      using (this.AutoResetStream)
      {
        byte[] buffer = Encoding.UTF8.GetBytes(this.TestText);
        int byteRead = this.AutoResetStream.ReadByte();
        Assert.AreEqual(0, this.AutoResetStream.Position);
        Assert.AreEqual(buffer.First(), byteRead);
      }
    }

    [TestMethod]
    public void ResetStreamPositionAfterWriteByte()
    {
      using (this.AutoResetStream)
      {
        byte[] buffer = Encoding.UTF8.GetBytes(this.TestText);
        this.AutoResetStream.WriteByte(buffer.First());
        Assert.AreEqual(0, this.AutoResetStream.Position);
        Assert.AreEqual(1, this.AutoResetStream.Length);
      }
    }

    [TestMethod]
    public async Task ResetStreamPositionAfterCopyAsync()
    {
      FillStream();
      using (this.AutoResetStream)
      {
        using (var destinationStream = new MemoryStream())
        {
          await this.AutoResetStream.CopyToAsync(destinationStream, this.TestTextLength, CancellationToken.None);
          Assert.AreEqual(0, this.AutoResetStream.Position);
          Assert.AreEqual(this.TestTextLength, destinationStream.Length);
        }
      }
    }
  }
}
