namespace Synapse;

/// <summary>
/// Analyzes NNUE network structure and weight distributions
/// </summary>
public class NNUEAnalyzer
{
    /// <summary>
    /// Contains statistical information about a neural network layer.
    /// </summary>
    public class LayerInfo
    {
        /// <summary>Name of the layer</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Starting byte offset in the weights data</summary>
        public int StartOffset { get; set; }

        /// <summary>Ending byte offset in the weights data</summary>
        public int EndOffset { get; set; }

        /// <summary>Size of the layer in bytes</summary>
        public int Size => EndOffset - StartOffset;

        /// <summary>Mean value of weights in this layer</summary>
        public double Mean { get; set; }

        /// <summary>Standard deviation of weights in this layer</summary>
        public double StdDev { get; set; }

        /// <summary>Minimum weight value in this layer</summary>
        public short Min { get; set; }

        /// <summary>Maximum weight value in this layer</summary>
        public short Max { get; set; }
    }

    /// <summary>
    /// Analyzes weight distribution for a NNUE network
    /// Based on typical Stockfish NNUE architecture (HalfKAv2_hm)
    /// </summary>
    public static List<LayerInfo> AnalyzeNetwork(byte[] weightsData)
    {
        var layers = new List<LayerInfo>();
        int numWeights = weightsData.Length / 2;

        // Typical Stockfish NNUE architecture (approximate):
        // 1. Feature Transformer (largest layer)
        // 2. Hidden Layer 1
        // 3. Hidden Layer 2
        // 4. Output Layer

        // We'll detect layers by analyzing weight distribution changes
        // For now, create a simple segmentation
        // Feature transformer is usually ~45M params, rest is smaller

        int featureTransformerSize = (int)(numWeights * 0.98); // ~98% of weights

        var featureLayer = new LayerInfo
        {
            Name = "FeatureTransformer",
            StartOffset = 0,
            EndOffset = featureTransformerSize * 2 // Convert to bytes
        };
        CalculateStats(weightsData, featureLayer);
        layers.Add(featureLayer);

        var hiddenLayer = new LayerInfo
        {
            Name = "HiddenLayers",
            StartOffset = featureTransformerSize * 2,
            EndOffset = weightsData.Length
        };
        CalculateStats(weightsData, hiddenLayer);
        layers.Add(hiddenLayer);

        return layers;
    }

    /// <summary>
    /// Calculates statistical properties for a layer's weights.
    /// Computes mean, standard deviation, min, and max values.
    /// </summary>
    /// <param name="data">Raw weights data</param>
    /// <param name="layer">Layer to calculate statistics for</param>
    private static void CalculateStats(byte[] data, LayerInfo layer)
    {
        int numWeights = (layer.EndOffset - layer.StartOffset) / 2;
        if (numWeights == 0) return;

        // Calculate basic statistics
        long sum = 0;
        short min = short.MaxValue;
        short max = short.MinValue;

        for (int i = layer.StartOffset; i < layer.EndOffset; i += 2)
        {
            short val = BitConverter.ToInt16(data, i);
            sum += val;
            if (val < min) min = val;
            if (val > max) max = val;
        }

        layer.Mean = (double)sum / numWeights;
        layer.Min = min;
        layer.Max = max;

        // Calculate standard deviation using the formula: sqrt(sum((x - mean)^2) / n)
        double sumSquaredDiff = 0;
        for (int i = layer.StartOffset; i < layer.EndOffset; i += 2)
        {
            short val = BitConverter.ToInt16(data, i);
            double diff = val - layer.Mean;
            sumSquaredDiff += diff * diff;
        }

        layer.StdDev = Math.Sqrt(sumSquaredDiff / numWeights);
    }
}
