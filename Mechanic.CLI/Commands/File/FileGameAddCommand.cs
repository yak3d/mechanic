using System.ComponentModel;
using Mechanic.CLI.Infrastructure.Logging;
using Mechanic.CLI.Models;
using Mechanic.CLI.Utils;
using Mechanic.Core.Contracts;
using Mechanic.Core.Utils;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mechanic.CLI.Commands.File;

public class FileGameAddCommand(
    ILogger<FileGameAddCommand> logger,
    IProjectService projectService,
    CommonFilePrompts commonFilePrompts
    ) : AsyncCommand<FileGameAddCommand.Settings>
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

        [Description("The Mechanic ID of the [green]source file[/] it points to.")]
        [CommandOption("-g|--game-file")]
        public string? SourceFile { get; init; }
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        return string.IsNullOrEmpty(settings.GamePath)
            ? ValidationResult.Error("You must enter a [b]game file path[/].")
            : ValidationResult.Success();
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (await projectService.GameFileExistsWithPathAsync(settings.GamePath))
        {
            logger.GameFileAlreadyExists(settings.GamePath);

            return -3;
        }
        var fileType = string.IsNullOrEmpty(settings.Type)
            ? AssumeFileType(settings.GamePath)
            : Enum.Parse<GameFileType>(settings.Type, true);

        SourceFile? selectedSourceFile = null;
        if (settings.SourceFile == null)
        {
            selectedSourceFile = await commonFilePrompts.FindOrPromptForMatchingSourceFile(settings.GamePath);
        }
        else
        {
            var sourceFile = await projectService.FindSourceFileByIdAsync(Guid.Parse(settings.SourceFile));

            if (sourceFile  != null)
            {
                selectedSourceFile = SourceFile.FromDomain(sourceFile);
            }
        }

        var result = await projectService.AddGameFileAsync(
            settings.GamePath,
            fileType.ToDomain(),
            selectedSourceFile?.Id);

        return result.Match(
            Right: file =>
            {
                if (selectedSourceFile != null)
                {
                    logger.AddedGameFileWithLink(file.Path, file.Id, selectedSourceFile.Path, selectedSourceFile.Id);
                }
                else
                {
                    logger.AddedGameFileWithId(file.Path, file.Id);
                }

                return 0;
            },
            Left: _ => -1
        );
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

        if (gameFileType == GameFileType.Other)
        {
            logger.FileTypeSetToOther();
        }

        return gameFileType;
    }
}
