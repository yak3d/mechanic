using System.ComponentModel;
using Mechanic.Core.Contracts;
using Mechanic.Core.Extensions;
using Mechanic.Core.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mechanic.CLI.Commands;

[Description("Initializes a project in the current directory. This will create the project file in the current directory with the specified project ID. The project ID must be in reverse DNS format. For example: com.example.myproject.")]
public class InitializeCommand(IProjectService projectService) : Command<InitializeCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("The project ID in reverse DNS order.")]
        [CommandOption("-i|--project-id")]
        public string? ProjectId { get; init; }
        
        [Description("The game the mod project is for.")]
        [CommandOption("-g|--game")]
        public string? Game { get; init; }

        public override ValidationResult Validate()
        {
            return Game != null && Enum.TryParse<Game>(Game, out _)
                ? ValidationResult.Error(
                    $"Game must be one of the valid games: {string.Join(",", Enum.GetNames<Game>())}")
                : ValidationResult.Success();
        }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        string projectId = settings.ProjectId ?? PromptForProjectId();

        Game game = settings.Game == null ? PromptForGame() : Enum.Parse<Game>(settings.Game);
        
        projectService.Initialize(Path.Join(Directory.GetCurrentDirectory(), "mechanic.json"), projectId, game);

        return 0;
    }

    private string PromptForProjectId()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>(
                "Enter a project ID in reverse DNS format. [dim]For example: com.example.myproject[/]:"
                )
            );
    }

    private Game PromptForGame()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<Game>()
                .Title("Which game is this project for?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more games)[/]")
                .AddChoices(Enum.GetValues<Game>())
                .UseConverter(game => game.GetDisplayName())
        );
    }
}