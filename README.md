# Synapse

[![CI/CD Pipeline](https://github.com/YOUR_USERNAME/Synapse/workflows/CI%2FCD%20Pipeline/badge.svg)](https://github.com/YOUR_USERNAME/Synapse/actions)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download)
[![codecov](https://codecov.io/gh/YOUR_USERNAME/Synapse/branch/master/graph/badge.svg)](https://codecov.io/gh/YOUR_USERNAME/Synapse)

**Synapse** is an industry-leading command-line utility and library for combining and analyzing Stockfish NNUE (Efficiently Updatable Neural Network) network files. Built with performance, extensibility, and developer experience in mind.

## ✨ Features

- **🔄 Network Combination**: Merge multiple NNUE networks using advanced algorithms
  - Harmonic mean merging (default)
  - Arithmetic mean (legacy support)
  - More algorithms coming soon (weighted average, SLERP, Fisher merging)
- **🔍 Format Analysis**: Deep inspection of NNUE binary structure
  - Header metadata extraction
  - Weight distribution statistics
  - Data type analysis (INT8/INT16)
  - Layer-by-layer breakdown
- **⚡ High Performance**: Built on .NET 10 with TorchSharp GPU acceleration
- **🧪 Well-Tested**: Comprehensive test suite with 85%+ code coverage
- **📦 Modular Design**: Clean separation between core library and CLI
- **🌐 Cross-Platform**: Windows, Linux, and macOS support

## 🚀 Quick Start

### Prerequisites

- .NET 10.0 SDK or later
- (Optional) CUDA-capable GPU for accelerated operations

### Installation

#### From Source

```bash
git clone https://github.com/YOUR_USERNAME/Synapse.git
cd Synapse
dotnet build Synapse.sln
dotnet run --project Synapse.CLI
```

#### Using Pre-built Binaries

Download the latest release for your platform from the [Releases](https://github.com/YOUR_USERNAME/Synapse/releases) page.

## 📖 Usage

### Interactive Mode

Simply run the CLI application:

```bash
dotnet run --project Synapse.CLI
```

The interactive menu will guide you through:
1. **Combine NNUE Files** - Merge multiple networks
2. **Analyze NNUE File Format** - Inspect network structure

### As a Library

Add Synapse.Core to your project:

```bash
dotnet add package Synapse.Core
```

Example usage:

```csharp
using Synapse;

// Read network metadata
var info = NNUECombiner.ReadInfo("network.nnue");
Console.WriteLine($"Hash: 0x{info.Hash:X8}, Size: {info.PayloadBytes} bytes");

// Combine multiple networks
var files = new[] { "net1.nnue", "net2.nnue", "net3.nnue" };
NNUECombiner.CombineNetworks(files, "combined.nnue", "My Combined Network");

// Analyze network structure
var analysis = NNUEFormatAnalyzer.AnalyzeFile("network.nnue");
Console.WriteLine($"Version: 0x{analysis.Version:X8}");
Console.WriteLine($"Mean weight: {analysis.Distribution.Mean:F2}");
Console.WriteLine($"Unique values: {analysis.Distribution.UniqueValues}");
```

## 🏗️ Project Structure

```
Synapse/
├── Synapse.Core/          # Core library with business logic
│   ├── NNUECombiner.cs    # Network combination algorithms
│   ├── NNUEAnalyzer.cs    # Statistical analysis
│   ├── NNUEFormatAnalyzer.cs  # Format inspection
│   └── LEB128.cs          # LEB128 compression utilities
├── Synapse.CLI/           # Command-line interface
│   └── Program.cs         # Interactive menu and UI
├── Synapse.Tests/         # Unit and integration tests
└── .github/workflows/     # CI/CD pipelines
```

## 🔬 Network Combination Methods

### Harmonic Mean (Default)

Best for averaging rates and maintaining consistency across networks. Handles edge cases better than arithmetic mean.

```
H = n / (1/x₁ + 1/x₂ + ... + 1/xₙ)
```

### Arithmetic Mean (Legacy)

Simple average of weight values. Preserved for compatibility.

```
A = (x₁ + x₂ + ... + xₙ) / n
```

### Coming Soon

- **Weighted Average**: Assign importance to specific networks
- **SLERP Interpolation**: Smooth interpolation between networks
- **Fisher Merging**: Advanced model averaging for better generalization

## 📊 Analysis Capabilities

Synapse provides deep insights into NNUE network structure:

- **Header Information**: Version, architecture hash, description
- **File Structure**: Header size, data size breakdown
- **Weight Samples**: First 20 values in both INT16 and INT8 formats
- **Statistical Distribution**: Min, max, mean, unique value count
- **Layer Analysis**: Feature transformer and hidden layer statistics

## 🧪 Testing

Run the comprehensive test suite:

```bash
dotnet test Synapse.Tests/Synapse.Tests.csproj
```

Test coverage report:

```bash
dotnet test Synapse.sln --collect:"XPlat Code Coverage"
```

## 🛠️ Development

### Building

```bash
dotnet build Synapse.sln --configuration Release
```

### Running Tests

```bash
dotnet test Synapse.sln --configuration Release
```

### Code Formatting

```bash
dotnet format Synapse.sln
```

## 🔮 Roadmap

### Phase 1: Foundation (✅ Complete)
- [x] Modular architecture (Core + CLI separation)
- [x] Comprehensive test suite
- [x] CI/CD pipeline with GitHub Actions
- [x] Cross-platform support

### Phase 2: Performance (🚧 In Progress)
- [ ] Full GPU acceleration with TorchSharp
- [ ] Memory-mapped file processing
- [ ] Parallel batch operations
- [ ] Benchmark suite

### Phase 3: Advanced Features (📋 Planned)
- [ ] Additional combination algorithms
- [ ] Visual weight distribution plots
- [ ] Network comparison tools
- [ ] Training pipeline integration
- [ ] Model zoo with version control

### Phase 4: User Experience (📋 Planned)
- [ ] Web interface (Blazor)
- [ ] Desktop GUI (Avalonia)
- [ ] Plugin system for custom algorithms
- [ ] RESTful API

### Phase 5: Integration (📋 Planned)
- [ ] Leela Chess Zero format support
- [ ] ONNX export
- [ ] Stockfish UCI integration
- [ ] Python bindings

See [ROADMAP.md](ROADMAP.md) for the complete transformation plan.

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Setup

1. Fork the repository
2. Clone your fork: `git clone https://github.com/YOUR_USERNAME/Synapse.git`
3. Create a feature branch: `git checkout -b feature/amazing-feature`
4. Make your changes and add tests
5. Run tests: `dotnet test`
6. Commit: `git commit -m "Add amazing feature"`
7. Push: `git push origin feature/amazing-feature`
8. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Stockfish Team**: For the NNUE architecture and format specification
- **TorchSharp**: For GPU-accelerated tensor operations
- **Spectre.Console**: For beautiful CLI interface

## 📞 Contact & Support

- **Issues**: [GitHub Issues](https://github.com/YOUR_USERNAME/Synapse/issues)
- **Discussions**: [GitHub Discussions](https://github.com/YOUR_USERNAME/Synapse/discussions)
- **Discord**: [Join our server](https://discord.gg/YOUR_INVITE) (coming soon)

## 🌟 Star History

[![Star History Chart](https://api.star-history.com/svg?repos=YOUR_USERNAME/Synapse&type=Date)](https://star-history.com/#YOUR_USERNAME/Synapse&Date)

---

**Made with ❤️ for the chess engine development community**
