using System;
using System.Text;
using Framework.IO;
using Xunit;

namespace HermesProxy.Tests.Framework;

public class ByteBufferReadCStringTests
{
    [Fact]
    public void ReadCString_WithEmptyString_ReturnsEmpty()
    {
        // Arrange - just a null terminator
        var data = new byte[] { 0x00 };
        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ReadCString_WithSimpleAscii_ReturnsCorrectString()
    {
        // Arrange - "Hello" + null terminator
        var data = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x00 };
        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();

        // Assert
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void ReadCString_WithUtf8_ReturnsCorrectString()
    {
        // Arrange - UTF-8 encoded string with multi-byte characters
        var testString = "Héllo Wörld";
        var stringBytes = Encoding.UTF8.GetBytes(testString);
        var data = new byte[stringBytes.Length + 1];
        stringBytes.CopyTo(data, 0);
        data[^1] = 0x00; // null terminator

        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();

        // Assert
        Assert.Equal(testString, result);
    }

    [Fact]
    public void ReadCString_WithMultipleStrings_ReadsSequentially()
    {
        // Arrange - "One" + null + "Two" + null
        var data = new byte[] { 0x4F, 0x6E, 0x65, 0x00, 0x54, 0x77, 0x6F, 0x00 };
        using var buffer = new ByteBuffer(data);

        // Act
        var result1 = buffer.ReadCString();
        var result2 = buffer.ReadCString();

        // Assert
        Assert.Equal("One", result1);
        Assert.Equal("Two", result2);
    }

    [Fact]
    public void ReadCString_WithLongString_ReturnsCorrectString()
    {
        // Arrange - string longer than 256 bytes (stackalloc threshold)
        var testString = new string('A', 300);
        var stringBytes = Encoding.UTF8.GetBytes(testString);
        var data = new byte[stringBytes.Length + 1];
        stringBytes.CopyTo(data, 0);
        data[^1] = 0x00;

        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();

        // Assert
        Assert.Equal(testString, result);
        Assert.Equal(300, result.Length);
    }

    [Fact]
    public void ReadCString_With256ByteString_UsesStackalloc()
    {
        // Arrange - exactly 256 bytes (boundary case for stackalloc)
        var testString = new string('B', 256);
        var stringBytes = Encoding.UTF8.GetBytes(testString);
        var data = new byte[stringBytes.Length + 1];
        stringBytes.CopyTo(data, 0);
        data[^1] = 0x00;

        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();

        // Assert
        Assert.Equal(testString, result);
        Assert.Equal(256, result.Length);
    }

    [Fact]
    public void ReadCString_WithSpecialCharacters_ReturnsCorrectString()
    {
        // Arrange - string with special UTF-8 characters (emoji, CJK)
        var testString = "Test 🎮 游戏";
        var stringBytes = Encoding.UTF8.GetBytes(testString);
        var data = new byte[stringBytes.Length + 1];
        stringBytes.CopyTo(data, 0);
        data[^1] = 0x00;

        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();

        // Assert
        Assert.Equal(testString, result);
    }

    [Fact]
    public void ReadCString_PositionAdvancesCorrectly()
    {
        // Arrange - "ABC" + null + extra bytes
        var data = new byte[] { 0x41, 0x42, 0x43, 0x00, 0xFF, 0xFF };
        using var buffer = new ByteBuffer(data);

        // Act
        var result = buffer.ReadCString();
        var nextByte = buffer.ReadUInt8();

        // Assert
        Assert.Equal("ABC", result);
        Assert.Equal(0xFF, nextByte);
    }
}

public class ByteBufferGetDataTests
{
    [Fact]
    public void GetData_WithEmptyBuffer_ReturnsEmptyArray()
    {
        // Arrange
        using var buffer = new ByteBuffer();

        // Act
        var result = buffer.GetData();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetData_WithWrittenData_ReturnsCorrectBytes()
    {
        // Arrange
        using var buffer = new ByteBuffer();
        buffer.WriteUInt8(0x01);
        buffer.WriteUInt8(0x02);
        buffer.WriteUInt8(0x03);

        // Act
        var result = buffer.GetData();

        // Assert
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, result);
    }

    [Fact]
    public void GetData_WithLargeData_ReturnsAllBytes()
    {
        // Arrange
        using var buffer = new ByteBuffer();
        var testData = new byte[4096];
        for (int i = 0; i < testData.Length; i++)
            testData[i] = (byte)(i & 0xFF);

        buffer.WriteBytes(testData);

        // Act
        var result = buffer.GetData();

        // Assert
        Assert.Equal(testData.Length, result.Length);
        Assert.Equal(testData, result);
    }

    [Fact]
    public void GetData_PreservesBufferState()
    {
        // Arrange
        using var buffer = new ByteBuffer();
        buffer.WriteUInt32(0x12345678);
        buffer.WriteUInt32(0xDEADBEEF);

        // Act - GetData should not affect the buffer's write position
        var result = buffer.GetData();

        // Write more data after GetData to verify position is preserved
        buffer.WriteUInt32(0xCAFEBABE);
        var resultAfterWrite = buffer.GetData();

        // Assert
        Assert.Equal(8, result.Length);
        Assert.Equal(12, resultAfterWrite.Length); // Original 8 + new 4 bytes
    }

    [Fact]
    public void GetData_WithReadBuffer_ReturnsAllBytes()
    {
        // Arrange
        var sourceData = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55 };
        using var buffer = new ByteBuffer(sourceData);

        // Read some data first
        buffer.ReadUInt8();
        buffer.ReadUInt8();

        // Act
        var result = buffer.GetData();

        // Assert - should return ALL data, not just unread portion
        Assert.Equal(sourceData, result);
    }

    [Fact]
    public void GetData_CalledMultipleTimes_ReturnsSameData()
    {
        // Arrange
        using var buffer = new ByteBuffer();
        buffer.WriteUInt32(0xCAFEBABE);

        // Act
        var result1 = buffer.GetData();
        var result2 = buffer.GetData();

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void GetData_WithMixedTypes_ReturnsCorrectBytes()
    {
        // Arrange
        using var buffer = new ByteBuffer();
        buffer.WriteUInt8(0xFF);
        buffer.WriteUInt16(0x1234);
        buffer.WriteUInt32(0xDEADBEEF);
        buffer.WriteFloat(1.5f);

        // Act
        var result = buffer.GetData();

        // Assert
        Assert.Equal(11, result.Length); // 1 + 2 + 4 + 4
    }

    [Theory]
    [InlineData(16)]
    [InlineData(256)]
    [InlineData(1024)]
    [InlineData(4096)]
    [InlineData(65536)]
    public void GetData_VariousSizes_ReturnsCorrectLength(int size)
    {
        // Arrange
        using var buffer = new ByteBuffer();
        var testData = new byte[size];
        new Random(42).NextBytes(testData);
        buffer.WriteBytes(testData);

        // Act
        var result = buffer.GetData();

        // Assert
        Assert.Equal(size, result.Length);
        Assert.Equal(testData, result);
    }
}
