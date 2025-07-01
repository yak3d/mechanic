using System.ComponentModel;
using Mechanic.CLI.Commands.File;
using Mechanic.CLI.Models;
using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Mechanic.Core.Models.Steam;
using Spectre.Console;
using Spectre.Console.Cli;
using GameName = Mechanic.CLI.Models.GameName;

namespace Mechanic.CLI.Commands;

[Description("Initializes a project in the current directory. This will create the project file in the current directory with the specified project ID. The project ID must be in reverse DNS format. For example: com.example.myproject.")]
public class InitializeCommand(IProjectService projectService, SteamService steamService) : AsyncCommand<InitializeCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("The project ID in reverse DNS order.")]
        [CommandOption("-p|--project-id")]
        public string? ProjectId { get; init; }

        [Description("The game the mod project is for.")]
        [CommandOption("-n|--game-name")]
        public string? GameName { get; init; }

        [Description("The path to the game the mod project is for.")]
        [CommandOption("-g|--game-path")]
        public string? GamePath { get; init; }

        public override ValidationResult Validate()
        {
            return GameName != null && Enum.TryParse<GameName>(GameName, out _)
                ? ValidationResult.Error(
                    $"Game must be one of the valid games: {string.Join(",", Enum.GetNames<GameName>())}")
                : ValidationResult.Success();
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var projectId = settings.ProjectId ?? PromptForProjectId();

        var gameName = settings.GameName == null ? PromptForGame() : Enum.Parse<GameName>(settings.GameName);

        var gamePath = settings.GamePath ?? await PromptForGamePath();

        await projectService.InitializeAsync(Path.Join(Directory.GetCurrentDirectory(), "mechanic.json"), projectId, gameName.ToDomain(), gamePath);

        return 0;
    }

    private async Task<string> PromptForGamePath()
    {
        var games = await steamService.GetInstalledGamesAsync();

        return games.Match(
            Right: steamGames =>
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<PromptChoice>()
                        .Title("Choose the game to set the game path")
                        .PageSize(10)
                        .MoreChoicesText("[grey](More)[/]")
                        .AddChoiceGroup(new SteamGameChoiceHeader(), steamGames.Select(g => new SteamGameChoice(g)))
                        .AddChoiceGroup(new ActionsHeader(), new NoGamesMatchChoice())
                        .UseConverter(choice => choice.ToString())
                );

                if (choice is SteamGameChoice steamGameChoice)
                {
                    return steamGameChoice.game.FullPath;
                }

                return PromptForGamePathText();
            },
            Left: _ => PromptForGamePathText());
    }

    private static string PromptForGamePathText()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>(
                "Enter the path to the root game directory:"
            )
        );
    }

    private string PromptForProjectId()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>(
                "Enter a project ID in reverse DNS format. [dim]For example: com.example.myproject[/]:"
                )
            );
    }

    private GameName PromptForGame()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<GameName>()
                .Title("Which game is this project for?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more games)[/]")
                .AddChoices(Enum.GetValues<GameName>())
                .UseConverter(game => game.GetDisplayName())
        );
    }

    private record SteamGameChoiceHeader() : PromptChoice("Installed Steam Games");

    private record SteamGameChoice(SteamGame game) : PromptChoice($"{game.Name} ({game.FullPath})");

    private record ActionsHeader() : PromptChoice("Actions");
    private record NoGamesMatchChoice() : PromptChoice("None of these");
}