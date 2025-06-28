using System.ComponentModel;
using FuzzySharp;
using Mechanic.CLI.Infrastructure.Logging;
using Mechanic.CLI.Models;
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

public class FileSrcAddCommand(ILogger<FileSrcAddCommand> logger, IProjectService projectService) : AsyncCommand<FileSrcAddCommand.Settings>
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

    private abstract record GameChoice(string DisplayName)
    {
        public sealed override string ToString() => DisplayName;
    }

    private record GameFileChoiceHeader() : GameChoice("Game Files");
    private record GameFileChoice(GameFile GameFile) : GameChoice($"{GameFile.Path} ({GameFile.Id})");

    private record ActionsChoiceHeader() : GameChoice("If none match:");
    private record CancelChoice() : GameChoice("Cancel");

    private record AllGamesChoice() : GameChoice("Choose from all game files");

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var fileType = string.IsNullOrEmpty(settings.Type)
            ? AssumeFileType(Path.GetExtension(settings.SourcePath))
            : Enum.Parse<SourceFileType>(settings.Type);

        if (await projectService.SourceFileExistsWithPath(settings.SourcePath))
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
                var gameFileChoices = similarGameFiles.Select(gf => gf.Id.ToString()).ToList();

                var choice = PromptForPossibleGameFiles(settings, gameFileChoices, similarGameFiles);

                selectedGameFile = choice switch
                {
                    AllGamesChoice => ((GameFileChoice)await PromptForAllGameFiles()).GameFile,
                    GameFileChoice gameFileChoice => gameFileChoice.GameFile,
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

    private GameChoice PromptForPossibleGameFiles(Settings settings, List<string> gameFileChoices, List<GameFile> similarGameFiles)
    {
        var chosenGameFileId = AnsiConsole.Prompt(
            new SelectionPrompt<GameChoice>()
                .Title(
                    $"Found existing [purple]game files[/] that may match this [green]source file[/]: {settings.SourcePath}")
                .PageSize(10)
                .MoreChoicesText("[grey](More)[/]")
                .AddChoiceGroup(new GameFileChoiceHeader(), similarGameFiles.Select(gf => new GameFileChoice(gf)))
                .AddChoiceGroup(new ActionsChoiceHeader(), new CancelChoice(), new AllGamesChoice())
                .UseConverter(id => 
                {
                    if (id is AllGamesChoice)
                    {
                        return id.ToString();
                    }
                    if (id is CancelChoice) return id.ToString();
                    return id.ToString();
                })
        );
        
        return chosenGameFileId;
    }

    private async Task<GameChoice> PromptForAllGameFiles()
    {
        var allGameFiles = Models.MechanicProject.FromDomain(await projectService.GetCurrentProjectAsync()).GameFiles;
        return AnsiConsole.Prompt(
            new SelectionPrompt<GameChoice>()
                .Title("All game files")
                .PageSize(10)
                .MoreChoicesText("[grey](More)[/]")
                .AddChoiceGroup(new GameFileChoiceHeader(), allGameFiles.OrderByDescending(gf => gf.Path).Select(gf => new GameFileChoice(gf)))
            );
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        return string.IsNullOrEmpty(settings.SourcePath)
            ? ValidationResult.Error("You must enter a [b]source path[/].")
            : ValidationResult.Success();
    }

    private async Task<IEnumerable<GameFile>> FindSimilarGameFile(string sourceFilePath)
    {
        var gameFiles = (await projectService.GetCurrentProjectAsync()).GameFiles;
        var sourceFile = Path.GetFileNameWithoutExtension(sourceFilePath);

        var bestMatch = gameFiles
            .Select(gf => new
            {
                GameFile = gf,
                Score = Fuzz.Ratio(sourceFile, Path.GetFileNameWithoutExtension(gf.Path))
            })
            .Where(match => match.Score >= 70)
            .OrderByDescending(match => match.Score)
            .Select(file => GameFile.FromDomain(file.GameFile));

        return bestMatch;
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
