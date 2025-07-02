using Mechanic.CLI.Models;
using Mechanic.Core.Contracts;
using Mechanic.Core.Project.Models.Json;
using Spectre.Console;

namespace Mechanic.CLI.Commands.File;

public static class FilePrompts
{
    public static async Task<SourceFile?> PromptForSourceFile(
        string title,
        string choiceHeader,
        IProjectService projectService)
    {
        var sourceFiles = (await projectService.GetCurrentProjectAsync()).SourceFiles.Select(SourceFile.FromDomain);
        var chosenGameFileId = AnsiConsole.Prompt(
            new SelectionPrompt<PromptChoice>()
                .Title(title)
                .PageSize(10)
                .MoreChoicesText("[grey](More)[/]")
                .AddChoiceGroup(new PromptFileChoiceHeader(choiceHeader), sourceFiles.Select(sf => new SourceFileChoice(sf)))
                .AddChoiceGroup(new ActionsChoiceHeader(), new CancelChoice())
                .UseConverter(id => id.ToString())
        );

        return chosenGameFileId switch
        {
            SourceFileChoice sourceFileChoice => sourceFileChoice.File,
            _ => null
        };
    }
    
    // ReSharper disable once NotAccessedPositionalProperty.Global
    private record PromptFileChoiceHeader(string Header) : PromptChoice(Header);

    private record SourceFileChoice(SourceFile File) : PromptChoice($"{File.Path} ({File.Id})");
    private record ActionsChoiceHeader() : PromptChoice("Actions:");
    private record CancelChoice() : PromptChoice("Cancel");
}