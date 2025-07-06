using LanguageExt;
using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Mechanic.Core.Services.Errors;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mechanic.CLI.Commands;

public class CheckCommand(ILogger<CheckCommand> logger, IProjectService projectService) : AsyncCommand<CheckCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        
    }


    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[bold][green]Source files:[/][/]");
        await CheckSourceFiles();
        
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine("[bold][magenta]Game files:[/][/]");
        await CheckGameFiles();

        return 0;
    }

    private async Task CheckSourceFiles()
    {
        var checkResults = await projectService.CheckSourceFilesAsync();

        DisplayCheckedFiles(checkResults);
    }
    
    private async Task CheckGameFiles()
    {
        var checkResults = await projectService.CheckGameFilesAsync();

        DisplayCheckedFiles(checkResults);
    }

    private static void DisplayCheckedFiles<T>(Either<CheckError, Dictionary<T, bool>> checkResults) where T : ProjectFile
    {
        checkResults.Match(
            Right: results => results.ToList().ForEach(kvp =>
            {
                var symbol = kvp.Value ? ":check_mark_button:" : ":cross_mark:";
                var color = kvp.Value ? "green" : "red";
                
                AnsiConsole.MarkupLine($"[{color}]{symbol} {kvp.Key.Path}[/]");
            }),
            Left: _ => AnsiConsole.MarkupLine("[red]Failed to check source files.[/]")
        );
    }
}