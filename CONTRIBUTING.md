# Contributing to Synapse

Thank you for considering contributing to Synapse! This document provides guidelines and instructions for contributing.

## Code of Conduct

This project adheres to a Code of Conduct that all contributors are expected to follow. Please be respectful and constructive in all interactions.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/Synapse.git
   cd Synapse
   ```
3. **Add upstream remote**:
   ```bash
   git remote add upstream https://github.com/ORIGINAL_OWNER/Synapse.git
   ```

## Development Setup

### Prerequisites

- .NET 10.0 SDK or later
- Git
- Your favorite IDE (Visual Studio, Rider, VS Code)

### Building the Project

```bash
dotnet restore Synapse.sln
dotnet build Synapse.sln
```

### Running Tests

```bash
dotnet test Synapse.sln
```

### Code Formatting

We use `dotnet format` for consistent code style:

```bash
dotnet format Synapse.sln
```

## Making Changes

### Branch Naming

Use descriptive branch names:
- `feature/add-weighted-average` - for new features
- `fix/network-size-validation` - for bug fixes
- `docs/update-readme` - for documentation changes
- `perf/optimize-memory-usage` - for performance improvements
- `test/add-leb128-tests` - for test additions

### Commit Messages

Write clear, concise commit messages:

```
feat: Add weighted average combination method

- Implement WeightedAverageCombiner class
- Add unit tests for weighted combinations
- Update documentation

Closes #123
```

Use conventional commit prefixes:
- `feat:` - new feature
- `fix:` - bug fix
- `docs:` - documentation changes
- `test:` - test additions/modifications
- `refactor:` - code refactoring
- `perf:` - performance improvements
- `chore:` - maintenance tasks

### Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and small
- Use async/await for I/O operations
- Prefer immutability where possible

Example:

```csharp
/// <summary>
/// Combines multiple NNUE networks using weighted average.
/// </summary>
/// <param name="networks">List of networks to combine</param>
/// <param name="weights">Weights for each network (must sum to 1.0)</param>
/// <returns>Combined network</returns>
public static NNUENetwork CombineWeighted(
    IReadOnlyList<NNUENetwork> networks,
    IReadOnlyList<double> weights)
{
    ArgumentNullException.ThrowIfNull(networks);
    ArgumentNullException.ThrowIfNull(weights);
    
    if (networks.Count != weights.Count)
        throw new ArgumentException("Network count must match weight count");
    
    // Implementation...
}
```

### Testing

All new features and bug fixes must include tests:

```csharp
[Fact]
public void CombineWeighted_WithValidInputs_ShouldSucceed()
{
    // Arrange
    var network1 = CreateTestNetwork(0x12345678, new byte[] { 100, 150 });
    var network2 = CreateTestNetwork(0x12345678, new byte[] { 50, 100 });
    var weights = new[] { 0.7, 0.3 };

    // Act
    var result = NNUECombiner.CombineWeighted(
        new[] { network1, network2 },
        weights);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(0x12345678u, result.Hash);
}

[Theory]
[InlineData(new[] { 0.5, 0.5 }, 75)]  // Equal weights
[InlineData(new[] { 0.8, 0.2 }, 90)]  // Favor first network
[InlineData(new[] { 0.2, 0.8 }, 60)]  // Favor second network
public void CombineWeighted_CalculatesCorrectValues(double[] weights, byte expected)
{
    // Arrange & Act & Assert...
}
```

### Documentation

- Update README.md if adding user-facing features
- Add XML comments to all public APIs
- Include code examples in comments
- Update CHANGELOG.md

## Pull Request Process

1. **Update your fork** with the latest upstream changes:
   ```bash
   git fetch upstream
   git rebase upstream/master
   ```

2. **Create a pull request** with:
   - Clear title describing the change
   - Detailed description of what and why
   - Reference to related issues (e.g., "Fixes #123")
   - Screenshots/examples if applicable

3. **Ensure CI passes**:
   - All tests pass on all platforms
   - Code formatting is correct
   - No merge conflicts

4. **Respond to feedback**:
   - Address reviewer comments
   - Make requested changes
   - Be open to suggestions

5. **Squash commits** if requested:
   ```bash
   git rebase -i HEAD~n  # where n is number of commits
   ```

## Types of Contributions

### Bug Reports

Use the bug report template and include:
- Clear description of the issue
- Steps to reproduce
- Expected vs actual behavior
- Environment details (OS, .NET version)
- Relevant logs or error messages

### Feature Requests

Use the feature request template and describe:
- The problem you're trying to solve
- Proposed solution
- Alternative solutions considered
- Additional context

### Code Contributions

We welcome:
- Bug fixes
- New features (discuss in an issue first)
- Performance improvements
- Test additions
- Documentation improvements
- Code refactoring

### Documentation

Help improve:
- README and getting started guides
- API documentation
- Code examples
- Tutorials and how-tos

## Review Process

1. **Automated checks** run on all PRs
2. **Maintainer review** within 48 hours
3. **Feedback and iteration** as needed
4. **Approval and merge** when ready

## Project Architecture

Understanding the codebase:

```
Synapse.Core/           # Core business logic (no UI dependencies)
├── NNUECombiner.cs     # Network combination algorithms
├── NNUEAnalyzer.cs     # Statistical analysis
├── NNUEFormatAnalyzer.cs  # Format inspection
└── LEB128.cs           # Compression utilities

Synapse.CLI/            # Command-line interface
└── Program.cs          # UI logic using Spectre.Console

Synapse.Tests/          # Test suite
├── *Tests.cs           # Unit and integration tests
└── TestHelpers.cs      # Shared test utilities (when needed)
```

### Design Principles

- **Separation of Concerns**: Core library has no UI dependencies
- **Testability**: All business logic is testable
- **Performance**: Optimize hot paths, measure with benchmarks
- **Extensibility**: Use interfaces and dependency injection
- **Documentation**: Code should be self-documenting with good names

## Questions?

- Open a [Discussion](https://github.com/YOUR_USERNAME/Synapse/discussions)
- Ask in the [Discord server](https://discord.gg/YOUR_INVITE)
- Comment on related issues

## Recognition

Contributors will be:
- Listed in CONTRIBUTORS.md
- Mentioned in release notes
- Given credit in relevant documentation

Thank you for contributing to Synapse! 🎉
