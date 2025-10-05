using System.Text;

namespace Synapse;

/// <summary>
/// Combines Stockfish NNUE networks by averaging their compressed data.
/// This is a simplified approach that works with the compressed format directly.
/// </summary>
public static class NNUECombiner
{
    private const uint NNUE_VERSION = 0x7AF32F20;

    /// <summary>
    /// Represents metadata information about a NNUE network file.
    /// </summary>
    /// <param name="FilePath">Path to the NNUE file</param>
    /// <param name="Version">NNUE format version</param>
    /// <param name="Hash">Architecture hash identifier</param>
    /// <param name="Description">Network description text</param>
    /// <param name="PayloadBytes">Size of the weights data in bytes</param>
    public sealed record NetworkInfo(string FilePath, uint Version, uint Hash, string Description, long PayloadBytes);

    /// <summary>
    /// Represents a complete NNUE network with header and weights data.
    /// </summary>
    public class NNUENetwork
    {
        /// <summary>NNUE format version</summary>
        public uint Version { get; set; }

        /// <summary>Architecture hash identifier</summary>
        public uint Hash { get; set; }

        /// <summary>Network description text</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Raw weights data (may be compressed)</summary>
        public byte[] WeightsData { get; set; } = [];
    }

    /// <summary>
    /// Reads header metadata from a NNUE file without loading the full payload.
    /// </summary>
    public static NetworkInfo ReadInfo(string filePath)
    {
        using var reader = new BinaryReader(File.OpenRead(filePath));

        uint version = reader.ReadUInt32();
        uint hash = reader.ReadUInt32();
        uint descLength = reader.ReadUInt32();
        string description = descLength > 0
            ? Encoding.UTF8.GetString(reader.ReadBytes((int)descLength))
            : string.Empty;

        long payloadBytes = reader.BaseStream.Length - reader.BaseStream.Position;
        return new NetworkInfo(filePath, version, hash, description, payloadBytes);
    }

    /// <summary>
    /// Reads a complete NNUE file.
    /// </summary>
    public static NNUENetwork ReadNNUE(string filePath)
    {
        using var reader = new BinaryReader(File.OpenRead(filePath));

        var network = new NNUENetwork
        {
            Version = reader.ReadUInt32(),
            Hash = reader.ReadUInt32()
        };

        uint descLength = reader.ReadUInt32();
        if (descLength > 0)
        {
            network.Description = Encoding.UTF8.GetString(reader.ReadBytes((int)descLength));
        }

        // Read all remaining data as-is (keep compressed format)
        network.WeightsData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));

        return network;
    }

    /// <summary>
    /// Writes a NNUE file.
    /// </summary>
    public static void WriteNNUE(string filePath, NNUENetwork network)
    {
        using var writer = new BinaryWriter(File.Create(filePath));

        writer.Write(network.Version);
        writer.Write(network.Hash);

        byte[] descBytes = Encoding.UTF8.GetBytes(network.Description);
        writer.Write((uint)descBytes.Length);
        if (descBytes.Length > 0)
        {
            writer.Write(descBytes);
        }

        writer.Write(network.WeightsData);
    }

    /// <summary>
    /// Methods for combining multiple NNUE networks.
    /// </summary>
    public enum CombinationMethod
    {
        /// <summary>Uses harmonic mean to combine weight values</summary>
        HarmonicMean
    }

    /// <summary>
    /// Combines multiple NNUE networks using Harmonic Mean.
    /// Note: This combines the compressed data directly, which may not produce optimal results.
    /// For best results, networks should be combined using Stockfish's training tools.
    /// </summary>
    public static void CombineNetworks(IReadOnlyList<string> inputFiles, string outputFile, string description)
    {
        if (inputFiles.Count == 0)
            throw new ArgumentException("Must provide at least one network", nameof(inputFiles));

        if (inputFiles.Count == 1)
        {
            // Just copy the single file
            var single = ReadNNUE(inputFiles[0]);
            single.Description = description;
            WriteNNUE(outputFile, single);
            return;
        }

        // Read all networks
        var networks = new List<NNUENetwork>();
        foreach (var file in inputFiles)
        {
            networks.Add(ReadNNUE(file));
        }

        // Verify compatibility
        uint refHash = networks[0].Hash;
        int refLength = networks[0].WeightsData.Length;

        for (int i = 1; i < networks.Count; i++)
        {
            if (networks[i].Hash != refHash)
            {
                throw new InvalidOperationException(
                    $"Network {i} has different architecture: 0x{networks[i].Hash:X8} vs 0x{refHash:X8}");
            }

            if (networks[i].WeightsData.Length != refLength)
            {
                throw new InvalidOperationException(
                    $"Network {i} has different data size: {networks[i].WeightsData.Length} vs {refLength}");
            }
        }

        // Create combined network
        var combined = new NNUENetwork
        {
            Version = NNUE_VERSION,
            Hash = refHash,
            Description = description,
            WeightsData = new byte[refLength]
        };

        // Combine using Harmonic Mean
        for (int i = 0; i < refLength; i++)
        {
            double result = CalculateHarmonicMean(networks, i);
            combined.WeightsData[i] = (byte)Math.Clamp((int)Math.Round(result), 0, 255);
        }

        WriteNNUE(outputFile, combined);
    }

    /// <summary>
    /// Calculates harmonic mean for a specific weight index across all networks.
    /// Harmonic mean is better for averaging rates and ratios.
    /// </summary>
    /// <param name="networks">List of networks to combine</param>
    /// <param name="index">Weight index to calculate</param>
    /// <returns>Harmonic mean value</returns>
    private static double CalculateHarmonicMean(List<NNUENetwork> networks, int index)
    {
        double sum = 0;
        foreach (var net in networks)
        {
            byte value = net.WeightsData[index];
            if (value == 0) return 0; // Avoid division by zero
            sum += 1.0 / value;
        }
        return networks.Count / sum;
    }



    /// <summary>
    /// Combines multiple NNUE networks using arithmetic mean.
    /// This is a legacy method kept for compatibility.
    /// </summary>
    public static NNUENetwork CombineMultipleNetworksArithmetic(List<NNUENetwork> networks, string description)
    {
        if (networks.Count == 0)
            throw new ArgumentException("Must provide at least one network");

        if (networks.Count == 1)
            return networks[0];

        // Create temp file list and use CombineNetworks
        var tempFiles = new List<string>();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            for (int i = 0; i < networks.Count; i++)
            {
                var tempFile = Path.Combine(tempDir, $"net{i}.nnue");
                WriteNNUE(tempFile, networks[i]);
                tempFiles.Add(tempFile);
            }

            var outputFile = Path.Combine(tempDir, "combined.nnue");
            CombineNetworks(tempFiles, outputFile, description);

            return ReadNNUE(outputFile);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
