using Mechanic.CLI.Models;
using Mechanic.Core.Contracts;
using Spectre.Console;

namespace Mechanic.CLI.Commands.File;

public static class FilePrompts
{
    public static async Task<T?> PromptForProjectFile<T>(
        string title,
        string choiceHeader,
        IProjectService projectService,
        Func<Core.Models.MechanicProject, IEnumerable<T>> fileSelector,
        Func<T, ProjectFile> fileConverter)
        where T : ProjectFile
    {
        var project = await projectService.GetCurrentProjectAsync();
        var files = fileSelector(project);
    
        var chosenFileId = AnsiConsole.Prompt(
            new SelectionPrompt<PromptChoice>()
                .Title(title)
                .PageSize(10)
                .MoreChoicesText("[grey](More)[/]")
                .AddChoiceGroup(new PromptFileChoiceHeader(choiceHeader), files.Select(f => new ProjectFileChoice(fileConverter(f))))
                .AddChoiceGroup(new ActionsChoiceHeader(), new CancelChoice())
                .UseConverter(id => id.ToString())
        );

        return chosenFileId switch
        {
            ProjectFileChoice fileChoice => fileChoice.File as T,
            _ => null
        };
    }

    public static async Task<SourceFile?> PromptForSourceFile(
        string title,
        string choiceHeader,
        IProjectService projectService)
    {
        return await PromptForProjectFile<SourceFile>(
            title,
            choiceHeader,
            projectService,
            project => project.SourceFiles.Select(SourceFile.FromDomain),
            file => file);
    }

    public static async Task<GameFile?> PromptForGameFile(
        string title,
        string choiceHeader,
        IProjectService projectService)
    {
        return await PromptForProjectFile<GameFile>(
            title,
            choiceHeader,
            projectService,
            project => project.GameFiles.Select(GameFile.FromDomain),
            file => file);
    }    
    // ReSharper disable once NotAccessedPositionalProperty.Global
    private record PromptFileChoiceHeader(string Header) : PromptChoice(Header);

    private record ProjectFileChoice(ProjectFile File) : PromptChoice($"{File.Path} ({File.Id})");
    private record ActionsChoiceHeader() : PromptChoice("Actions:");
    private record CancelChoice() : PromptChoice("Cancel");
}