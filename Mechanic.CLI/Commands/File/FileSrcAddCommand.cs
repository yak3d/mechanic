using System.ComponentModel;
using Mechanic.CLI.Infrastructure.Logging;
using Mechanic.CLI.Models;
using Mechanic.Core.Contracts;
using Mechanic.Core.Services.Errors;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using GameFile = Mechanic.CLI.Models.GameFile;
using SourceFileType = Mechanic.CLI.Models.SourceFileType;

namespace Mechanic.CLI.Commands.File;

public class FileSrcAddCommand(ILogger<FileSrcAddCommand> logger, IProjectService projectService, CommonFilePrompts commonFilePrompts) : AsyncCommand<FileSrcAddCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Relative path to the [b]source[/] file to be added")]
        [CommandArgument(0, "<SOURCE_PATH>")]
        public string SourcePath { get; set; } = string.Empty;

        [Description(
            "The type of file to add. If not specified, Mechanic will attempt to figure it out by file extension.")]
        [CommandOption("-t|--type")]
        public string? Type { get; init; }

        [Description("The Mechanic GUID of the [b]game file[/] it points to.")]
        [CommandOption("-g|--game-file")]
        public string? GameFile { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var fileType = string.IsNullOrEmpty(settings.Type)
            ? CommonFilePrompts.AssumeOrPromptSourceFileType(Path.GetFileName(settings.SourcePath))
            : Enum.Parse<SourceFileType>(settings.Type);

        if (await projectService.SourceFileExistsWithPathAsync(settings.SourcePath))
        {
            logger.SourceFileAlreadyExists(settings.SourcePath);
            return -3;
        }

        GameFile? selectedGameFile = null;
        if (settings.GameFile == null)
        {
            selectedGameFile = await commonFilePrompts.FindOrPromptForMatchingGameFile(settings.SourcePath);
        }
        else
        {
            var gameFile = await projectService.FindGameFileByIdAsync(Guid.Parse(settings.GameFile));

            if (gameFile != null)
            {
                selectedGameFile = GameFile.FromDomain(gameFile);
            }
        }

        var result = await projectService.AddSourceFileAsync(
            settings.SourcePath,
            fileType.ToDomain(),
            selectedGameFile?.Id
        );

        return result.Match(
            Right: file =>
            {
                if (selectedGameFile != null)
                {
                    logger.AddedSourceFileWithLink(file.Path, file.Id, selectedGameFile.Path, selectedGameFile.Id);
                }
                else
                {
                    logger.AddedSourceFile(file.Path, file.Id);
                }

                return 0;
            },
            Left: error =>
            {
                if (error is LinkedFileDoesNotExistError existError)
                {
                    logger.FailedToAddSourceFileWithBadLink(settings.SourcePath, existError.LinkedFileId);
                    return -1;
                }

                logger.FailedToAddSourceFileWithError(error.GetType());
                return -2;
            });
    }


    public override ValidationResult Validate(CommandContext context, Settings settings) =>
        string.IsNullOrEmpty(settings.SourcePath)
            ? ValidationResult.Error("You must enter a [b]source path[/].")
            : ValidationResult.Success();
}
