# Synapse

A command-line utility for combining and analyzing Stockfish NNUE (Efficiently Updatable Neural Network) network files.

## Features

- **Combine NNUE Files**: Merge multiple NNUE networks using harmonic mean
- **Analyze NNUE Format**: Inspect binary structure, header metadata, and weight distributions

## Requirements

- .NET 10.0 or later
- TorchSharp
- Spectre.Console

## Getting Started

1. **Create Networks Folder**: On first run, the application will create a `Networks` folder in the working directory
2. **Add NNUE Files**: Place your `.nnue` or `.bin` files in the Networks folder
3. **Run the Application**: Launch Synapse and select your desired operation

## Usage

### Combining Networks

1. Select "Combine NNUE Files" from the main menu
2. Choose 2 or more networks using the multi-selection prompt (Space to select, Enter to confirm)
3. The combined network will be saved with a timestamp in the Networks folder

The combination uses the **Harmonic Mean** method to merge network weights.

### Analyzing Networks

1. Select "Analyze NNUE File Format" from the main menu
2. Choose a network file to analyze
3. View detailed information including:
   - Version and architecture hash
   - Description metadata
   - Weight samples (INT16 and INT8 views)
   - Statistical distribution (min, max, mean, unique values)

## File Structure

- [Program.cs](Synapse/Program.cs) - Main entry point and CLI interface
- [NNUECombiner.cs](Synapse/NNUECombiner.cs) - Network combination logic
- [NNUEFormatAnalyzer.cs](Synapse/NNUEFormatAnalyzer.cs) - Format analysis and inspection
- [LEB128.cs](Synapse/LEB128.cs) - LEB128 encoding/decoding utilities

## Output

Combined networks are named: `combined_harmonicmean_YYYYMMDD_HHmmss.nnue`

## License

See LICENSE file for details.
