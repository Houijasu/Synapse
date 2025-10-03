using Spectre.Console;

namespace Synapse;

/// <summary>
/// Provides detailed analysis of NNUE file format and structure.
/// Displays header information, weight distributions, and data samples.
/// </summary>
public class NNUEFormatAnalyzer
{
    /// <summary>
    /// Analyzes a NNUE file and displays detailed information about its structure.
    /// Shows header metadata, weight samples, and statistical distribution.
    /// </summary>
    /// <param name="filePath">Path to the NNUE file to analyze</param>
    public static void AnalyzeFile(string filePath)
    {
        using var reader = new BinaryReader(File.OpenRead(filePath));

        // Read NNUE header structure
        uint version = reader.ReadUInt32();           // NNUE format version
        uint hash = reader.ReadUInt32();              // Architecture hash
        uint descLength = reader.ReadUInt32();        // Description string length
        string description = System.Text.Encoding.UTF8.GetString(reader.ReadBytes((int)descLength));

        // Calculate file section sizes
        long headerSize = 12 + descLength;           // 12 bytes + description
        long dataSize = reader.BaseStream.Length - headerSize;

        AnsiConsole.MarkupLine($"[cyan]NNUE File Analysis:[/] {Path.GetFileName(filePath)}");
        AnsiConsole.MarkupLine($"[yellow]Version:[/] 0x{version:X8}");
        AnsiConsole.MarkupLine($"[yellow]Hash:[/] 0x{hash:X8}");
        AnsiConsole.MarkupLine($"[yellow]Description:[/] {description}");
        AnsiConsole.MarkupLine($"[yellow]Header size:[/] {headerSize:N0} bytes");
        AnsiConsole.MarkupLine($"[yellow]Data size:[/] {dataSize:N0} bytes");
        Console.WriteLine();

        // Display sample weights as INT16 (16-bit signed integers)
        AnsiConsole.MarkupLine("[cyan]First 20 values (as INT16):[/]");
        for (int i = 0; i < 20 && reader.BaseStream.Position < reader.BaseStream.Length - 1; i++)
        {
            short val = reader.ReadInt16();
            AnsiConsole.MarkupLine($"  [[{i}]] = {val}");
        }

        // Reset position and display same data as INT8 (8-bit signed integers)
        // This helps understand the actual data format
        reader.BaseStream.Position = headerSize;
        Console.WriteLine();
        AnsiConsole.MarkupLine("[cyan]First 20 values (as INT8):[/]");
        for (int i = 0; i < 20 && reader.BaseStream.Position < reader.BaseStream.Length; i++)
        {
            sbyte val = reader.ReadSByte();
            AnsiConsole.MarkupLine($"  [[{i}]] = {val}");
        }

        // Analyze weight distribution for statistical insights
        reader.BaseStream.Position = headerSize;
        var distribution = new Dictionary<short, int>();
        int sampleSize = Math.Min(10000, (int)(dataSize / 2)); // Limit sample for performance

        // Count frequency of each weight value
        for (int i = 0; i < sampleSize; i++)
        {
            short val = reader.ReadInt16();
            if (!distribution.ContainsKey(val))
                distribution[val] = 0;
            distribution[val]++;
        }

        Console.WriteLine();
        AnsiConsole.MarkupLine($"[cyan]Weight distribution (first {sampleSize} INT16 values):[/]");
        AnsiConsole.MarkupLine($"[yellow]Min:[/] {distribution.Keys.Min()}");
        AnsiConsole.MarkupLine($"[yellow]Max:[/] {distribution.Keys.Max()}");
        AnsiConsole.MarkupLine($"[yellow]Unique values:[/] {distribution.Count}");

        // Calculate weighted mean
        double mean = distribution.Sum(kvp => (long)kvp.Key * kvp.Value) / (double)sampleSize;
        AnsiConsole.MarkupLine($"[yellow]Mean:[/] {mean:F2}");
    }
}
