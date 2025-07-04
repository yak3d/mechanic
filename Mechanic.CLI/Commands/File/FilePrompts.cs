using Mechanic.CLI.Models;
using Mechanic.Core.Contracts;
using Spectre.Console;

namespace Mechanic.CLI.Commands.File;

public static class FilePrompts
{
    public static PromptChoice PromptForProjectFile<T>(
        string title,
        string choiceHeader,
        IEnumerable<T> files,
        params PromptChoice[] actionChoices
    )
        where T : ProjectFile
    {
        var chosenFileId = AnsiConsole.Prompt(
            new SelectionPrompt<PromptChoice>()
                .Title(title)
                .PageSize(10)
                .MoreChoicesText("[grey](More)[/]")
                .AddChoiceGroup(new PromptFileChoiceHeader(choiceHeader), files.Select(f => new ProjectFileChoice(f)))
                .AddChoiceGroup(new ActionsChoiceHeader(), [new CancelChoice(), ..actionChoices])
                .UseConverter(id => id.ToString())
        );

        return chosenFileId;
    }

    public static PromptChoice PromptForSourceFileAsPromptChoice(
        string title,
        string choiceHeader,
        IEnumerable<SourceFile> files,
        params PromptChoice[] actionChoices
    )
    {
        return PromptForProjectFile(
            title,
            choiceHeader,
            files,
           actionChoices 
        );
    }
    
    public static SourceFile? PromptForSourceFile(
        string title,
        string choiceHeader,
        IEnumerable<SourceFile> files,
        params PromptChoice[] actionChoices
    )
    {
        return PromptForSourceFileAsPromptChoice(
                title,
                choiceHeader,
                files,
                actionChoices
            ) switch
            {
                ProjectFileChoice file => file.File as SourceFile,
                _ => null
            };
    }
    
    public static PromptChoice PromptForGameFileAsPromptChoice(
        string title,
        string choiceHeader,
        IEnumerable<GameFile> files,
        params PromptChoice[] actionChoices
    )
    {
        return PromptForProjectFile(
            title,
            choiceHeader,
            files,
            actionChoices
        );
    }

    public static GameFile? PromptForGameFile(
        string title,
        string choiceHeader,
        IEnumerable<GameFile> files,
        params PromptChoice[] actionChoices
    )
    {
        return PromptForGameFileAsPromptChoice(
                title,
                choiceHeader,
                files,
                actionChoices
            ) switch
            {
                ProjectFileChoice file => file.File as GameFile,
                _ => null
            };
    }
    
    // ReSharper disable once NotAccessedPositionalProperty.Global
    private record PromptFileChoiceHeader(string Header) : PromptChoice(Header);

    public record ProjectFileChoice(ProjectFile File) : PromptChoice($"{File.Path} ({File.Id})");
    private record ActionsChoiceHeader() : PromptChoice("Actions:");
    private record CancelChoice() : PromptChoice("Cancel");
}