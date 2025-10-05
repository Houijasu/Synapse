namespace Synapse;

/// <summary>
/// Provides detailed analysis of NNUE file format and structure.
/// </summary>
public class NNUEFormatAnalyzer
{
    public record AnalysisResult(
        string FileName,
        uint Version,
        uint Hash,
        string Description,
        long HeaderSize,
        long DataSize,
        short[] SampleInt16Values,
        sbyte[] SampleInt8Values,
        WeightDistribution Distribution
    );

    public record WeightDistribution(
        int SampleSize,
        short Min,
        short Max,
        int UniqueValues,
        double Mean
    );

    /// <summary>
    /// Analyzes a NNUE file and returns detailed information about its structure.
    /// </summary>
    /// <param name="filePath">Path to the NNUE file to analyze</param>
    public static AnalysisResult AnalyzeFile(string filePath)
    {
        using var reader = new BinaryReader(File.OpenRead(filePath));

        // Read NNUE header structure
        uint version = reader.ReadUInt32();
        uint hash = reader.ReadUInt32();
        uint descLength = reader.ReadUInt32();
        string description = System.Text.Encoding.UTF8.GetString(reader.ReadBytes((int)descLength));

        // Calculate file section sizes
        long headerSize = 12 + descLength;
        long dataSize = reader.BaseStream.Length - headerSize;

        // Read sample weights as INT16
        var sampleInt16 = new List<short>();
        for (int i = 0; i < 20 && reader.BaseStream.Position < reader.BaseStream.Length - 1; i++)
        {
            sampleInt16.Add(reader.ReadInt16());
        }

        // Read sample weights as INT8
        reader.BaseStream.Position = headerSize;
        var sampleInt8 = new List<sbyte>();
        for (int i = 0; i < 20 && reader.BaseStream.Position < reader.BaseStream.Length; i++)
        {
            sampleInt8.Add(reader.ReadSByte());
        }

        // Analyze weight distribution
        reader.BaseStream.Position = headerSize;
        var distribution = new Dictionary<short, int>();
        int sampleSize = Math.Min(10000, (int)(dataSize / 2));

        for (int i = 0; i < sampleSize; i++)
        {
            short val = reader.ReadInt16();
            if (!distribution.ContainsKey(val))
                distribution[val] = 0;
            distribution[val]++;
        }

        double mean = distribution.Sum(kvp => (long)kvp.Key * kvp.Value) / (double)sampleSize;

        var dist = new WeightDistribution(
            sampleSize,
            distribution.Keys.Min(),
            distribution.Keys.Max(),
            distribution.Count,
            mean
        );

        return new AnalysisResult(
            Path.GetFileName(filePath),
            version,
            hash,
            description,
            headerSize,
            dataSize,
            sampleInt16.ToArray(),
            sampleInt8.ToArray(),
            dist
        );
    }
}
