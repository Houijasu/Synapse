using Synapse;

namespace Synapse.Tests;

public class NNUEAnalyzerTests
{
    [Fact]
    public void AnalyzeNetwork_WithValidData_ShouldReturnLayerInfo()
    {
        // Arrange: Create sample weight data
        var weightsData = new byte[10000]; // 5000 int16 values
        var random = new Random(42);
        random.NextBytes(weightsData);

        // Act
        var layers = NNUEAnalyzer.AnalyzeNetwork(weightsData);

        // Assert
        Assert.NotNull(layers);
        Assert.NotEmpty(layers);
        Assert.All(layers, layer =>
        {
            Assert.NotNull(layer.Name);
            Assert.True(layer.Size > 0);
            Assert.True(layer.StdDev >= 0);
        });
    }

    [Fact]
    public void AnalyzeNetwork_ShouldCalculateCorrectStatistics()
    {
        // Arrange: Create controlled data
        var weightsData = new byte[100]; // 50 int16 values
        for (int i = 0; i < 50; i++)
        {
            BitConverter.GetBytes((short)(i * 10)).CopyTo(weightsData, i * 2);
        }

        // Act
        var layers = NNUEAnalyzer.AnalyzeNetwork(weightsData);

        // Assert
        var layer = layers.First();
        Assert.True(layer.Mean >= 0); // Mean should be positive
        Assert.True(layer.Min < layer.Max); // Min should be less than Max
        Assert.True(layer.StdDev >= 0); // StdDev should be non-negative
    }

    [Fact]
    public void AnalyzeNetwork_WithEmptyData_ShouldHandleGracefully()
    {
        // Arrange
        var weightsData = new byte[0];

        // Act
        var layers = NNUEAnalyzer.AnalyzeNetwork(weightsData);

        // Assert
        Assert.NotNull(layers);
        // Should return layers even with empty data
    }
}
