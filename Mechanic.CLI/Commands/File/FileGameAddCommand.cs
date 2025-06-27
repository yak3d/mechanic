using System.ComponentModel;
using Mechanic.CLI.Infrastructure.Logging;
using Mechanic.CLI.Models;
using Mechanic.Core.Contracts;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mechanic.CLI.Commands.File;

public class FileGameAddCommand(ILogger<FileGameAddCommand> logger, IProjectService projectService) : AsyncCommand<FileGameAddCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Relative path to the [b]game[/] file to be added")]
        [CommandArgument(0, "<GAME_PATH>")]
        public string GamePath { get; set; } = string.Empty;

        [Description(
            "The type of file to add. If not specified, Mechanic will attempt to figure it out by file extension.")]
        [CommandOption("-t|--type")]
        public string? Type { get; init; }
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        return string.IsNullOrEmpty(settings.GamePath)
            ? ValidationResult.Error("You must enter a [b]game file path[/].")
            : ValidationResult.Success();
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var fileType = string.IsNullOrEmpty(settings.Type)
            ? AssumeFileType(settings.GamePath)
            : Enum.Parse<GameFileType>(settings.Type, true);

        await projectService.AddGameFileAsync(settings.GamePath, fileType.ToDomain());
        
        return 0;
    }

    private GameFileType AssumeFileType(string file)
    {
        var gameFileType = Path.GetExtension(file).ToLowerInvariant() switch
        {
            ".mat" => GameFileType.Material,
            ".dds" => GameFileType.DirectDrawSurface,
            ".wem" => GameFileType.WwiseEncodedMedia,
            ".pex" => GameFileType.PapyrusExecutable,
            _ => GameFileType.Other
        };
        
        logger.AssumingGameFileType(gameFileType, Path.GetExtension(file));
        
        return gameFileType;
    }
}