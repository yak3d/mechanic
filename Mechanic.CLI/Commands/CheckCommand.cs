using System.ComponentModel;
using System.Text;
using LanguageExt;
using Mechanic.CLI.Models;
using Mechanic.Core.Contracts;
using Mechanic.Core.Services.Errors;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mechanic.CLI.Commands;

public class CheckCommand(IProjectService projectService) : AsyncCommand<CheckCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Display all fields in the status table")]
        [CommandOption("-a|--all")]
        public bool All { get; set; }
    }


    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[bold][green]Source files:[/][/]");
        await CheckSourceFiles(settings.All);
        
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine("[bold][magenta]Game files:[/][/]");
        await CheckGameFiles(settings.All);

        return 0;
    }

    private async Task CheckSourceFiles(bool all)
    {
        var checkResults = await projectService.CheckSourceFilesAsync();
        checkResults.Match(
            Right: dict =>
            {
                var cliCheckResults = dict.Select(
                    kvp => new KeyValuePair<SourceFile, FileCheckStatus>(SourceFile.FromDomain(kvp.Key), kvp.Value.FromDomain())
                    ).ToDictionary();
                DisplayCheckedFiles<SourceFile>(cliCheckResults, all);
            },
            Left: e => AnsiConsole.MarkupLine(
                $"[red]Failed to retrieve check results for game files due to error {e}[/]")
        );
    }
    
    private async Task CheckGameFiles(bool all)
    {
        var checkResults = await projectService.CheckGameFilesAsync();

        checkResults.Match(
            Right: dict =>
            {
                var cliCheckResults = dict.Select(
                    kvp => new KeyValuePair<GameFile, FileCheckStatus>(GameFile.FromDomain(kvp.Key), kvp.Value.FromDomain())
                    ).ToDictionary();
                DisplayCheckedFiles<GameFile>(cliCheckResults, all);
            },
            Left: e => AnsiConsole.MarkupLine(
                $"[red]Failed to retrieve check results for game files due to error {e}[/]")
        );
    }

    private static void DisplayCheckedFiles<T>(Either<ProjectError, Dictionary<T, FileCheckStatus>> checkResults, bool all) where T : ProjectFile
    {
        var table = new Table();
        table.Border(TableBorder.None);
        table.AddColumn("ID");
        table.AddColumn("Path");
        table.AddColumn("Status");

        if (all)
        {
            table.AddColumn("Description").Alignment(Justify.Left);
        }
        
        checkResults.Match(
            Right: results => results.OrderBy(kvp => kvp.Key.Path).ToList().ForEach(kvp =>
            {
                var symbol = kvp.Value.ToStatusEmoji();
                var color = kvp.Value.ToStatusColor(kvp.Key is GameFile);
                var description = kvp.Value.ToStatusDescription(kvp.Key is GameFile);

                var rowBuilder = new List<string>
                {
                    $"[{color}]{kvp.Key.Id}[/]",
                    $"[{color}]{kvp.Key.Path}[/]",
                    $"[{color}]{symbol}[/]",
                };

                if (all)
                {
                    rowBuilder.Add($"[{color}]{description}[/]");
                }
                
                table.AddRow(rowBuilder.ToArray());
            }),
            Left: _ => AnsiConsole.MarkupLine("[red]Failed to check source files.[/]")
        );
        
        AnsiConsole.Write(table);
    }
}