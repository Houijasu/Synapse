using System.Text;

namespace Synapse;

/// <summary>
/// LEB128 (Little Endian Base 128) compression/decompression for NNUE files
/// Based on Stockfish implementation: src/nnue/nnue_common.h
/// </summary>
public static class LEB128
{
    private const string MagicString = "COMPRESSED_LEB128";
    private const int MagicStringSize = 17;
    private const int BufferSize = 4096;

    /// <summary>
    /// Decompresses LEB128-encoded signed integers
    /// If count is -1, reads until all compressed bytes are consumed
    /// </summary>
    public static short[] ReadLEB128(BinaryReader reader, int count = -1)
    {
        // Read and verify magic string
        byte[] magicBytes = reader.ReadBytes(MagicStringSize);
        string magic = Encoding.ASCII.GetString(magicBytes);

        if (magic != MagicString)
        {
            throw new InvalidDataException($"Invalid LEB128 magic string: {magic}");
        }

        // Read total compressed byte count
        uint bytesLeft = reader.ReadUInt32();
        uint totalBytes = bytesLeft;

        var result = new List<short>();
        byte[] buffer = new byte[BufferSize];
        int bufferPos = BufferSize; // Force initial read

        // If count specified, use it; otherwise read until bytesLeft is exhausted
        int targetCount = count;
        int i = 0;

        while ((targetCount == -1 && bytesLeft > 0) || (targetCount != -1 && i < targetCount))
        {
            int value = 0;
            int shift = 0;

            while (true)
            {
                if (bytesLeft == 0)
                {
                    if (targetCount == -1)
                        goto done; // All compressed data consumed
                    throw new InvalidDataException($"LEB128 compressed data exhausted at index {i}/{targetCount}");
                }

                // Refill buffer if needed
                if (bufferPos >= BufferSize || bufferPos == 0)
                {
                    int toRead = (int)Math.Min(bytesLeft, BufferSize);
                    if (toRead == 0)
                        break; // No more data to read

                    int actualRead = reader.Read(buffer, 0, toRead);
                    if (actualRead == 0)
                        throw new InvalidDataException($"Unexpected end of stream at index {i}");

                    bufferPos = 0;
                }

                byte b = buffer[bufferPos++];
                bytesLeft--;

                // Extract 7 bits of data
                value |= (b & 0x7F) << shift;
                shift += 7;

                // Check if this is the last byte (MSB = 0)
                if ((b & 0x80) == 0)
                {
                    // Sign extend if necessary
                    if (shift < 16 && (b & 0x40) != 0)
                    {
                        // Negative number - sign extend
                        value |= ~0 << shift;
                    }

                    result.Add((short)value);
                    break;
                }

                if (shift >= 16)
                {
                    throw new InvalidDataException($"LEB128 value too large at index {i}");
                }
            }
            i++;
        }

done:
        return result.ToArray();
    }

    /// <summary>
    /// Compresses signed integers using LEB128 encoding
    /// </summary>
    public static void WriteLEB128(BinaryWriter writer, short[] values)
    {
        // Write magic string
        byte[] magicBytes = Encoding.ASCII.GetBytes(MagicString);
        writer.Write(magicBytes);

        // Collect all compressed bytes first to know total size
        var compressedData = new List<byte>();

        foreach (short value in values)
        {
            int v = value;
            bool more = true;

            while (more)
            {
                byte b = (byte)(v & 0x7F);
                v >>= 7;

                // Check if we need more bytes
                // For positive: v must be 0 and sign bit must be 0
                // For negative: v must be -1 and sign bit must be 1
                if ((v == 0 && (b & 0x40) == 0) || (v == -1 && (b & 0x40) != 0))
                {
                    more = false;
                }
                else
                {
                    b |= 0x80; // Set continuation bit
                }

                compressedData.Add(b);
            }
        }

        // Write total byte count
        writer.Write((uint)compressedData.Count);

        // Write compressed data
        writer.Write(compressedData.ToArray());
    }

    /// <summary>
    /// Checks if data starts with LEB128 magic string
    /// </summary>
    public static bool IsCompressed(byte[] data, int offset = 0)
    {
        if (data.Length - offset < MagicStringSize)
            return false;

        string magic = Encoding.ASCII.GetString(data, offset, MagicStringSize);
        return magic == MagicString;
    }
}
