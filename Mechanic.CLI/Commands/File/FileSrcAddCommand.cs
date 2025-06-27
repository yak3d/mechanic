using System.ComponentModel;
using FuzzySharp;
using Mechanic.CLI.Models;
using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Mechanic.Core.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using GameFile = Mechanic.CLI.Models.GameFile;
using GameFileType = Mechanic.CLI.Models.GameFileType;
using SourceFileType = Mechanic.CLI.Models.SourceFileType;

namespace Mechanic.CLI.Commands.File;

public class FileSrcAddCommand(IProjectService projectService) : AsyncCommand<FileSrcAddCommand.Settings>
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

        [Description("The path or Mechanic GUID of the [b]game file[/] it points to.")]
        [CommandOption("-g|--game-file")]
        public string? GameFile { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var fileType = string.IsNullOrEmpty(settings.Type)
            ? AssumeFileType(Path.GetExtension(settings.SourcePath))
            : Enum.Parse<SourceFileType>(settings.Type);

        var similarGameFiles = (await FindSimilarGameFile(settings.SourcePath)).ToList();
    
        Guid? selectedGameFileId = null;
    
        if (similarGameFiles.Count != 0)
        {
            var gameFileChoices = similarGameFiles.Select(gf => gf.Id.ToString()).ToList();
        
            var chosenGameFileId = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(
                        $"Found existing [purple]game files[/] that may match this [green]source file[/]: {settings.SourcePath}")
                    .PageSize(10)
                    .MoreChoicesText("[grey](More)[/]")
                    .AddChoiceGroup("Game Files", gameFileChoices)
                    .AddChoiceGroup("If none match: ", new[] { "Cancel" })
                    .UseConverter(id => 
                    {
                        if (id == "Cancel") return "Cancel";
                        var gameFile = similarGameFiles.FirstOrDefault(gf => gf.Id.ToString() == id);
                        return gameFile != null ? $"{gameFile.Path} ({gameFile.Id})" : id;
                    })
            );

            if (!chosenGameFileId.Equals("Cancel"))
            {
                selectedGameFileId = Guid.Parse(chosenGameFileId);
            }
        }

        await projectService.AddSourceFileAsync(
            settings.SourcePath, 
            fileType.ToDomain(), 
            selectedGameFileId
        );

        return 0;
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
