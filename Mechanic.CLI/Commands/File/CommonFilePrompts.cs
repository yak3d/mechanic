using Mechanic.CLI.Models;
using Mechanic.CLI.Utils;
using Mechanic.Core.Contracts;
using Spectre.Console;

namespace Mechanic.CLI.Commands.File;

public class CommonFilePrompts(IProjectService projectService)
{
    public static SourceFileType AssumeOrPromptSourceFileType(string fileName)
    {
        var fileType = FileTypeUtils.AssumeSourceFileType(Path.GetExtension(fileName));

        if (fileType == SourceFileType.Other)
        {
            fileType = AnsiConsole.Prompt(
                new SelectionPrompt<SourceFileType>()
                    .Title($"Choose a [green]source file type[/] for the file [green]{fileName}[/]")
                    .PageSize(10)
                    .AddChoices(Enum.GetValues<SourceFileType>())
                    .UseConverter(type => type.ToString())
            );
        }

        return fileType;
    }

    public static GameFileType AssumeOrPromptGameFileType(string fileName)
    {
        var fileType = FileTypeUtils.AssumeGameFileType(Path.GetExtension(fileName));

        if (fileType == GameFileType.Other)
        {
            fileType = AnsiConsole.Prompt(
                new SelectionPrompt<GameFileType>()
                    .Title($"Choose a [magenta]game file type[/] for the file [magenta]{fileName}[/]")
                    .PageSize(10)
                    .AddChoices(Enum.GetValues<GameFileType>())
                    .UseConverter(type => type.ToString())
            );
        }

        return fileType;
    }

    private static PromptChoice PromptForPossibleGameFiles(string sourceFile, List<GameFile> similarGameFiles)
    {
        var chosenGame = FilePrompts.PromptForGameFileAsPromptChoice(
            $"Found existing [purple]game files[/] that may match this [green]source file[/]: {sourceFile}",
            "Source Files",
            similarGameFiles,
            new AllPromptsChoice()
        );

        return chosenGame;
    }

    private async Task<PromptChoice> PromptForAllGameFiles()
    {
        var allGameFiles = MechanicProject.FromDomain(await projectService.GetCurrentProjectAsync()).GameFiles;
        return FilePrompts.PromptForGameFileAsPromptChoice("All game files", "Game Files", allGameFiles);
    }


    public async Task<GameFile?> FindOrPromptForMatchingGameFile(string fileName)
    {
        GameFile? gameFile = null;

        var similarGameFiles = (await projectService.FindSimilarGameFile(fileName)).Select(GameFile.FromDomain).ToList();
        var choice = PromptForPossibleGameFiles(fileName, similarGameFiles);

        return choice switch
        {
            AllPromptsChoice => ((FilePrompts.ProjectFileChoice)await this.PromptForAllGameFiles()).File as GameFile,
            FilePrompts.ProjectFileChoice gameFileChoice => gameFileChoice.File as GameFile,
            _ => gameFile
        };
    }

    private static PromptChoice PromptForPossibleSourceFiles(string sourceFile, List<SourceFile> similarSourceFiles)
    {
        var chosenSource = FilePrompts.PromptForSourceFileAsPromptChoice(
            $"Found existing [purple]source files[/] that may match this [green]source file[/]: {sourceFile}",
            "Source Files",
            similarSourceFiles,
            new AllPromptsChoice()
        );

        return chosenSource;
    }

    private async Task<PromptChoice> PromptForAllSourceFiles()
    {
        var allSourceFiles = MechanicProject.FromDomain(await projectService.GetCurrentProjectAsync()).SourceFiles;
        return FilePrompts.PromptForSourceFileAsPromptChoice("All source files", "Source Files", allSourceFiles);
    }


    public async Task<SourceFile?> FindOrPromptForMatchingSourceFile(string fileName)
    {
        SourceFile? sourceFile = null;

        var similarSourceFiles = (await projectService.FindSimilarSourceFile(fileName)).Select(SourceFile.FromDomain).ToList();
        var choice = PromptForPossibleSourceFiles(fileName, similarSourceFiles);

        return choice switch
        {
            AllPromptsChoice => ((FilePrompts.ProjectFileChoice)await this.PromptForAllSourceFiles()).File as SourceFile,
            FilePrompts.ProjectFileChoice sourceFileChoice => sourceFileChoice.File as SourceFile,
            _ => sourceFile
        };
    }

    private sealed record AllPromptsChoice() : PromptChoice("Choose from all source files");
}
