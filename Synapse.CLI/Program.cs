using Spectre.Console;

using TorchSharp;

using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace Synapse;

/// <summary>
/// Main entry point for the Synapse NNUE network utility.
/// Provides a command-line interface for combining and analyzing Stockfish NNUE files.
/// </summary>
class Program
{
    /// <summary>
    /// Application entry point. Displays the main menu and handles user selection.
    /// </summary>
    /// <param name="_">Command line arguments (not used)</param>
    /// <returns>Task representing the async operation</returns>
    static async Task Main(string[] _)
    {
        AnsiConsole.Write(
            new FigletText("Synapse")
                .LeftJustified()
                .Color(Color.Cyan1));

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]What do you want to do?[/]")
                .PageSize(10)
                .AddChoices(new[] {
                    "Combine NNUE Files",
                    "Analyze NNUE File Format",
                    "Exit"
                }));

        switch (choice)
        {
            case "Combine NNUE Files":
                await CombineNNUEFiles();
                break;
            case "Analyze NNUE File Format":
                AnalyzeNNUEFormat();
                break;
            case "Exit":
                return;
        }
    }

    /// <summary>
    /// Handles the NNUE file combination workflow.
    /// Allows users to select multiple NNUE files and combines them using harmonic mean.
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    static async Task CombineNNUEFiles()
    {
        AnsiConsole.MarkupLine("\n[bold cyan]NNUE File Combiner[/]");
        AnsiConsole.MarkupLine("[grey]Combines multiple Stockfish NNUE files into one[/]\n");

        // Create Networks folder if it doesn't exist
        string networksFolder = Path.Combine(Directory.GetCurrentDirectory(), "Networks");
        if (!Directory.Exists(networksFolder))
        {
            Directory.CreateDirectory(networksFolder);
            AnsiConsole.MarkupLine($"[yellow]Created Networks folder at:[/] {networksFolder}");
            AnsiConsole.MarkupLine("[grey]Please add NNUE files to this folder and restart.[/]\n");
            AnsiConsole.MarkupLine("[green]Press any key to continue...[/]");
            Console.ReadKey();
            return;
        }

        // Scan for NNUE files - support both .nnue and .bin extensions
        var nnueFiles = Directory.GetFiles(networksFolder, "*.nnue")
            .Concat(Directory.GetFiles(networksFolder, "*.bin"))
            .ToList();

        if (nnueFiles.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]No NNUE files found in:[/] {networksFolder}");
            AnsiConsole.MarkupLine("[grey]Please add .nnue or .bin files to the Networks folder.[/]\n");
            AnsiConsole.MarkupLine("[green]Press any key to continue...[/]");
            Console.ReadKey();
            return;
        }

        // Display found networks in a formatted table
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("[cyan]#[/]");
        table.AddColumn("[cyan]Filename[/]");
        table.AddColumn("[cyan]Size[/]");

        for (int i = 0; i < nnueFiles.Count; i++)
        {
            var fileInfo = new FileInfo(nnueFiles[i]);
            table.AddRow(
                $"[yellow]{i + 1}[/]",
                $"[white]{Path.GetFileName(nnueFiles[i])}[/]",
                $"[grey]{fileInfo.Length / 1024.0:F2} KB[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[green]Found {nnueFiles.Count} network(s)[/]\n");

        // Select networks to combine using multi-selection prompt
        var selectedIndices = AnsiConsole.Prompt(
            new MultiSelectionPrompt<int>()
                .Title("[cyan]Select networks to combine (use Space to select, Enter to confirm):[/]")
                .PageSize(10)
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to confirm)[/]")
                .AddChoices(Enumerable.Range(0, nnueFiles.Count))
                .UseConverter(i => $"{i + 1}. {Path.GetFileName(nnueFiles[i])}")
        );

        // Validate selection - need at least 2 networks for combination
        if (selectedIndices.Count < 2)
        {
            AnsiConsole.MarkupLine("[red]Please select at least 2 networks to combine![/]");
            AnsiConsole.MarkupLine("\n[green]Press any key to continue...[/]");
            Console.ReadKey();
            return;
        }

        var selectedFiles = selectedIndices.Select(i => nnueFiles[i]).ToList();

        // Generate unique output filename with timestamp
        string outputFile = Path.Combine(networksFolder, $"combined_harmonicmean_{DateTime.Now:yyyyMMdd_HHmmss}.nnue");

        AnsiConsole.MarkupLine($"\n[yellow]Combining {selectedFiles.Count} networks using Harmonic Mean[/]");

        try
        {
            string combinationDescription = string.Empty;

            // Process networks with status spinner and progress updates
            await AnsiConsole.Status()
                .StartAsync("Processing NNUE files...", async ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);

                    var infos = new List<NNUECombiner.NetworkInfo>();

                    // Read metadata from each selected network
                    for (int i = 0; i < selectedFiles.Count; i++)
                    {
                        ctx.Status($"[yellow]Reading network {i + 1}/{selectedFiles.Count}...[/]");
                        var info = NNUECombiner.ReadInfo(selectedFiles[i]);
                        infos.Add(info);

                        // Display network information
                        AnsiConsole.MarkupLine($"[green]✓[/] Loaded: {Path.GetFileName(selectedFiles[i])}");
                        AnsiConsole.MarkupLine($"  [grey]Hash: 0x{info.Hash:X8}, Payload: {info.PayloadBytes:N0} bytes[/]");

                        await Task.Delay(50);
                    }

                    // Combine networks using harmonic mean
                    ctx.Status($"[yellow]Combining {infos.Count} networks...[/]");
                    combinationDescription = $"Combined {infos.Count} networks - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                    await Task.Run(() => NNUECombiner.CombineNetworks(selectedFiles, outputFile, combinationDescription));

                    ctx.Status("[bold green]Combination complete![/]");
                });

            AnsiConsole.MarkupLine($"\n[green]✓ Successfully created:[/] [yellow]{outputFile}[/]");
            AnsiConsole.MarkupLine($"[cyan]{combinationDescription}[/]");
            AnsiConsole.MarkupLine($"[grey]Method: Harmonic Mean[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"\n[red]Error: {ex.Message}[/]");
        }

        AnsiConsole.MarkupLine("\n[green]Press any key to continue...[/]");
        Console.ReadKey();
    }

    /// <summary>
    /// Handles the NNUE file format analysis workflow.
    /// Allows users to select a NNUE file and displays detailed information about its structure.
    /// </summary>
    static void AnalyzeNNUEFormat()
    {
        AnsiConsole.MarkupLine("\n[bold cyan]NNUE File Format Analyzer[/]");
        AnsiConsole.MarkupLine("[grey]Analyzes the binary structure of NNUE files[/]\n");

        string networksFolder = Path.Combine(Directory.GetCurrentDirectory(), "Networks");

        if (!Directory.Exists(networksFolder))
        {
            AnsiConsole.MarkupLine($"[red]Networks folder not found![/]");
            AnsiConsole.MarkupLine("[green]Press any key to continue...[/]");
            Console.ReadKey();
            return;
        }

        var nnueFiles = Directory.GetFiles(networksFolder, "*.nnue")
            .Concat(Directory.GetFiles(networksFolder, "*.bin"))
            .ToList();

        if (nnueFiles.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]No NNUE files found in:[/] {networksFolder}");
            AnsiConsole.MarkupLine("[green]Press any key to continue...[/]");
            Console.ReadKey();
            return;
        }

        var selectedFile = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Select NNUE file to analyze:[/]")
                .PageSize(10)
                .AddChoices(nnueFiles.Select(f => Path.GetFileName(f) ?? f).ToList())
        );

        string filePath = nnueFiles.First(f => Path.GetFileName(f) == selectedFile);

        Console.WriteLine();
        var result = NNUEFormatAnalyzer.AnalyzeFile(filePath);

        // Display analysis results
        AnsiConsole.MarkupLine($"[cyan]NNUE File Analysis:[/] {result.FileName}");
        AnsiConsole.MarkupLine($"[yellow]Version:[/] 0x{result.Version:X8}");
        AnsiConsole.MarkupLine($"[yellow]Hash:[/] 0x{result.Hash:X8}");
        AnsiConsole.MarkupLine($"[yellow]Description:[/] {result.Description}");
        AnsiConsole.MarkupLine($"[yellow]Header size:[/] {result.HeaderSize:N0} bytes");
        AnsiConsole.MarkupLine($"[yellow]Data size:[/] {result.DataSize:N0} bytes");
        Console.WriteLine();

        // Display sample weights as INT16
        AnsiConsole.MarkupLine("[cyan]First 20 values (as INT16):[/]");
        for (int i = 0; i < result.SampleInt16Values.Length; i++)
        {
            AnsiConsole.MarkupLine($"  [[{i}]] = {result.SampleInt16Values[i]}");
        }

        Console.WriteLine();
        // Display sample weights as INT8
        AnsiConsole.MarkupLine("[cyan]First 20 values (as INT8):[/]");
        for (int i = 0; i < result.SampleInt8Values.Length; i++)
        {
            AnsiConsole.MarkupLine($"  [[{i}]] = {result.SampleInt8Values[i]}");
        }

        Console.WriteLine();
        AnsiConsole.MarkupLine($"[cyan]Weight distribution (first {result.Distribution.SampleSize} INT16 values):[/]");
        AnsiConsole.MarkupLine($"[yellow]Min:[/] {result.Distribution.Min}");
        AnsiConsole.MarkupLine($"[yellow]Max:[/] {result.Distribution.Max}");
        AnsiConsole.MarkupLine($"[yellow]Unique values:[/] {result.Distribution.UniqueValues}");
        AnsiConsole.MarkupLine($"[yellow]Mean:[/] {result.Distribution.Mean:F2}");

        AnsiConsole.MarkupLine("\n[green]Press any key to continue...[/]");
        Console.ReadKey();
    }
}
