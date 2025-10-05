using Synapse;

namespace Synapse.Tests;

public class NNUEFormatAnalyzerTests
{
    [Fact]
    public void AnalyzeFile_ShouldReturnCompleteAnalysis()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        CreateTestNNUEFile(tempFile, "Test Network v1.0");

        try
        {
            // Act
            var result = NNUEFormatAnalyzer.AnalyzeFile(tempFile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Path.GetFileName(tempFile), result.FileName);
            Assert.Equal(0x7AF32F20u, result.Version);
            Assert.NotEqual(0u, result.Hash);
            Assert.Equal("Test Network v1.0", result.Description);
            Assert.True(result.HeaderSize > 0);
            Assert.True(result.DataSize > 0);
            Assert.NotEmpty(result.SampleInt16Values);
            Assert.NotEmpty(result.SampleInt8Values);
            Assert.NotNull(result.Distribution);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void AnalyzeFile_Distribution_ShouldHaveValidStatistics()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        CreateTestNNUEFile(tempFile, "Stats Test");

        try
        {
            // Act
            var result = NNUEFormatAnalyzer.AnalyzeFile(tempFile);

            // Assert
            var dist = result.Distribution;
            Assert.True(dist.SampleSize > 0);
            Assert.True(dist.Min <= dist.Max);
            Assert.True(dist.UniqueValues > 0);
            Assert.True(dist.UniqueValues <= dist.SampleSize);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void AnalyzeFile_WithEmptyDescription_ShouldHandleCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        CreateTestNNUEFile(tempFile, "");

        try
        {
            // Act
            var result = NNUEFormatAnalyzer.AnalyzeFile(tempFile);

            // Assert
            Assert.Equal(string.Empty, result.Description);
            Assert.NotNull(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void AnalyzeFile_SampleValues_ShouldNotExceed20()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        CreateTestNNUEFile(tempFile, "Large File Test", dataSize: 10000);

        try
        {
            // Act
            var result = NNUEFormatAnalyzer.AnalyzeFile(tempFile);

            // Assert
            Assert.True(result.SampleInt16Values.Length <= 20);
            Assert.True(result.SampleInt8Values.Length <= 20);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private void CreateTestNNUEFile(string filePath, string description, int dataSize = 1000)
    {
        using var writer = new BinaryWriter(File.Create(filePath));
        
        // Write header
        writer.Write(0x7AF32F20u); // version
        writer.Write(0xABCD1234u); // hash
        
        var descBytes = System.Text.Encoding.UTF8.GetBytes(description);
        writer.Write((uint)descBytes.Length);
        if (descBytes.Length > 0)
        {
            writer.Write(descBytes);
        }

        // Write test weight data
        var random = new Random(42);
        for (int i = 0; i < dataSize / 2; i++)
        {
            writer.Write((short)(random.Next(-1000, 1000)));
        }
    }
}
