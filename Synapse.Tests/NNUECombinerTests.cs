using Synapse;

namespace Synapse.Tests;

public class NNUECombinerTests
{
    [Fact]
    public void ReadInfo_ShouldParseHeaderCorrectly()
    {
        // Arrange: Create a minimal NNUE file in memory
        var tempFile = Path.GetTempFileName();
        using (var writer = new BinaryWriter(File.Create(tempFile)))
        {
            writer.Write(0x7AF32F20u); // version
            writer.Write(0x12345678u); // hash
            var desc = System.Text.Encoding.UTF8.GetBytes("Test Network");
            writer.Write((uint)desc.Length);
            writer.Write(desc);
            writer.Write(new byte[100]); // dummy payload
        }

        try
        {
            // Act
            var info = NNUECombiner.ReadInfo(tempFile);

            // Assert
            Assert.Equal(0x7AF32F20u, info.Version);
            Assert.Equal(0x12345678u, info.Hash);
            Assert.Equal("Test Network", info.Description);
            Assert.Equal(100, info.PayloadBytes);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ReadNNUE_ShouldLoadCompleteNetwork()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var testData = new byte[] { 1, 2, 3, 4, 5 };
        using (var writer = new BinaryWriter(File.Create(tempFile)))
        {
            writer.Write(0x7AF32F20u);
            writer.Write(0xABCDEF00u);
            writer.Write(0u); // empty description
            writer.Write(testData);
        }

        try
        {
            // Act
            var network = NNUECombiner.ReadNNUE(tempFile);

            // Assert
            Assert.Equal(0x7AF32F20u, network.Version);
            Assert.Equal(0xABCDEF00u, network.Hash);
            Assert.Equal(string.Empty, network.Description);
            Assert.Equal(testData, network.WeightsData);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void WriteNNUE_ShouldCreateValidFile()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var network = new NNUECombiner.NNUENetwork
        {
            Version = 0x7AF32F20,
            Hash = 0x11223344,
            Description = "Test",
            WeightsData = new byte[] { 10, 20, 30 }
        };

        try
        {
            // Act
            NNUECombiner.WriteNNUE(tempFile, network);
            var loaded = NNUECombiner.ReadNNUE(tempFile);

            // Assert
            Assert.Equal(network.Version, loaded.Version);
            Assert.Equal(network.Hash, loaded.Hash);
            Assert.Equal(network.Description, loaded.Description);
            Assert.Equal(network.WeightsData, loaded.WeightsData);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void CombineNetworks_WithIncompatibleArchitectures_ShouldThrow()
    {
        // Arrange
        var file1 = Path.GetTempFileName();
        var file2 = Path.GetTempFileName();
        
        CreateTestNetwork(file1, 0x11111111, new byte[50]);
        CreateTestNetwork(file2, 0x22222222, new byte[50]); // Different hash

        var outputFile = Path.GetTempFileName();

        try
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                NNUECombiner.CombineNetworks(new[] { file1, file2 }, outputFile, "Test"));
            Assert.Contains("different architecture", ex.Message);
        }
        finally
        {
            File.Delete(file1);
            File.Delete(file2);
            if (File.Exists(outputFile)) File.Delete(outputFile);
        }
    }

    [Fact]
    public void CombineNetworks_WithCompatibleNetworks_ShouldSucceed()
    {
        // Arrange
        var file1 = Path.GetTempFileName();
        var file2 = Path.GetTempFileName();
        var outputFile = Path.GetTempFileName();

        CreateTestNetwork(file1, 0x12345678, new byte[] { 100, 150, 200 });
        CreateTestNetwork(file2, 0x12345678, new byte[] { 50, 100, 150 });

        try
        {
            // Act
            NNUECombiner.CombineNetworks(new[] { file1, file2 }, outputFile, "Combined");

            // Assert
            Assert.True(File.Exists(outputFile));
            var result = NNUECombiner.ReadNNUE(outputFile);
            Assert.Equal(0x12345678u, result.Hash);
            Assert.Equal("Combined", result.Description);
            Assert.Equal(3, result.WeightsData.Length);
        }
        finally
        {
            File.Delete(file1);
            File.Delete(file2);
            File.Delete(outputFile);
        }
    }

    private void CreateTestNetwork(string filePath, uint hash, byte[] data)
    {
        var network = new NNUECombiner.NNUENetwork
        {
            Version = 0x7AF32F20,
            Hash = hash,
            Description = "Test",
            WeightsData = data
        };
        NNUECombiner.WriteNNUE(filePath, network);
    }
}
