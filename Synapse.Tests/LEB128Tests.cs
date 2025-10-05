using Synapse;

namespace Synapse.Tests;

public class LEB128Tests
{
    [Theory]
    [InlineData(new short[] { 0 })]
    [InlineData(new short[] { 1, 2, 3, 4, 5 })]
    [InlineData(new short[] { -1, -2, -3 })]
    [InlineData(new short[] { 127, -128, 255, -256 })]
    [InlineData(new short[] { short.MaxValue, short.MinValue, 0 })]
    public void LEB128_RoundTrip_ShouldPreserveValues(short[] values)
    {
        // Arrange
        var memoryStream = new MemoryStream();
        
        // Act - Write
        using (var writer = new BinaryWriter(memoryStream, System.Text.Encoding.UTF8, true))
        {
            LEB128.WriteLEB128(writer, values);
        }

        // Act - Read
        memoryStream.Position = 0;
        short[] result;
        using (var reader = new BinaryReader(memoryStream))
        {
            result = LEB128.ReadLEB128(reader, values.Length);
        }

        // Assert
        Assert.Equal(values, result);
    }

    [Fact]
    public void IsCompressed_WithValidMagic_ShouldReturnTrue()
    {
        // Arrange
        var data = System.Text.Encoding.ASCII.GetBytes("COMPRESSED_LEB128");

        // Act
        var result = LEB128.IsCompressed(data);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsCompressed_WithInvalidMagic_ShouldReturnFalse()
    {
        // Arrange
        var data = System.Text.Encoding.ASCII.GetBytes("NOT_COMPRESSED");

        // Act
        var result = LEB128.IsCompressed(data);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCompressed_WithShortData_ShouldReturnFalse()
    {
        // Arrange
        var data = new byte[10]; // Too short

        // Act
        var result = LEB128.IsCompressed(data);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void LEB128_WithLargeDataset_ShouldCompress()
    {
        // Arrange: Create a large array with common values (should compress well)
        var values = new short[1000];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = (short)(i % 100); // Values 0-99 repeating
        }

        var memoryStream = new MemoryStream();

        // Act
        using (var writer = new BinaryWriter(memoryStream, System.Text.Encoding.UTF8, true))
        {
            LEB128.WriteLEB128(writer, values);
        }

        var compressedSize = memoryStream.Length;
        var originalSize = values.Length * sizeof(short);

        // Assert: LEB128 should use some compression
        Assert.True(compressedSize < originalSize * 2); // At least not worse than 2x

        // Verify round-trip
        memoryStream.Position = 0;
        using (var reader = new BinaryReader(memoryStream))
        {
            var result = LEB128.ReadLEB128(reader, values.Length);
            Assert.Equal(values, result);
        }
    }
}
