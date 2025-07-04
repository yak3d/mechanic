using System.ComponentModel;
using FuzzySharp;
using Mechanic.CLI.Infrastructure.Logging;
using Mechanic.CLI.Models;
using Mechanic.CLI.Utils;
using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Mechanic.Core.Services;
using Mechanic.Core.Services.Errors;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Spectre.Console;
using Spectre.Console.Cli;
using GameFile = Mechanic.CLI.Models.GameFile;
using GameFileType = Mechanic.CLI.Models.GameFileType;
using SourceFileType = Mechanic.CLI.Models.SourceFileType;

namespace Mechanic.CLI.Commands.File;

public partial class FileSrcAddCommand(ILogger<FileSrcAddCommand> logger, IProjectService projectService) : AsyncCommand<FileSrcAddCommand.Settings>
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

    private record PromptFileChoiceHeader() : PromptChoice("Game Files");

    private record ActionsChoiceHeader() : PromptChoice("If none match:");
    private record CancelChoice() : PromptChoice("Cancel");

    private record AllPromptsChoice() : PromptChoice("Choose from all game files");

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var fileType = string.IsNullOrEmpty(settings.Type)
            ? AssumeFileType(Path.GetExtension(settings.SourcePath))
            : Enum.Parse<SourceFileType>(settings.Type);

        if (await projectService.SourceFileExistsWithPathAsync(settings.SourcePath))
        {
            logger.SourceFileAlreadyExists(settings.SourcePath);
            return -3;
        }

        GameFile? selectedGameFile = null;
        if (settings.GameFile == null)
        {
            var similarGameFiles = (await FindSimilarGameFile(settings.SourcePath)).ToList();
            if (similarGameFiles.Count != 0)
            {
                var choice = PromptForPossibleGameFiles(settings, similarGameFiles);

                selectedGameFile = choice switch
                {
                    AllPromptsChoice => ((FilePrompts.ProjectFileChoice) await PromptForAllGameFiles()).File as GameFile,
                    FilePrompts.ProjectFileChoice gameFileChoice => gameFileChoice.File as GameFile,
                    _ => selectedGameFile
                };
            }
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

    private PromptChoice PromptForPossibleGameFiles(Settings settings, List<GameFile> similarGameFiles)
    {
        var chosenGame = FilePrompts.PromptForGameFileAsPromptChoice(
            $"Found existing [purple]game files[/] that may match this [green]source file[/]: {settings.SourcePath}",
            "Source Files",
            similarGameFiles,
            new AllPromptsChoice()
        );

        return chosenGame;
    }

    private async Task<PromptChoice> PromptForAllGameFiles()
    {
        var allGameFiles = Models.MechanicProject.FromDomain(await projectService.GetCurrentProjectAsync()).GameFiles;
        return FilePrompts.PromptForGameFileAsPromptChoice("All game files", "Game Files", allGameFiles);
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        return string.IsNullOrEmpty(settings.SourcePath)
            ? ValidationResult.Error("You must enter a [b]source path[/].")
            : ValidationResult.Success();
    }

    private async Task<IEnumerable<GameFile>> FindSimilarGameFile(string sourceFilePath)
    {
        var gameFiles = (await projectService.GetCurrentProjectAsync()).GameFiles.Select(GameFile.FromDomain);
        var sourceFile = Path.GetFileNameWithoutExtension(sourceFilePath);
        var bestMatches = ProjectFileFuzzyMatcher.FuzzyMatch(gameFiles, sourceFile)
            .Select(file => file.File);

        return bestMatches;
    }

    private static SourceFileType AssumeFileType(string fileExtension) => fileExtension switch
    {
        ".fbx" => SourceFileType.Fbx,
        ".blend" => SourceFileType.Blend,
        ".tiff" => SourceFileType.Tiff,
        ".wav" => SourceFileType.Wav,
        ".psc" => SourceFileType.Psc,
        _ => SourceFileType.Other
    };
}
