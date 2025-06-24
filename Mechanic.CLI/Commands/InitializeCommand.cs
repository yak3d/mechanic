using System.ComponentModel;
using Mechanic.Core.Contracts;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mechanic.CLI.Commands;

[Description("Initializes a project in the current directory. This will create the project file in the current directory with the specified project ID. The project ID must be in reverse DNS format. For example: com.example.myproject.")]
public class InitializeCommand(IProjectService projectService) : Command<InitializeCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("The project ID in reverse DNS order.")]
        [CommandArgument(0, "[projectId]")]
        public string? ProjectId { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        string projectId = settings.ProjectId ?? PromptForProjectId();
        
        projectService.Initialize(Path.Join(Directory.GetCurrentDirectory(), "mechanic.json"), projectId);

        return 0;
    }

    private string PromptForProjectId()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>(
                "Enter a project ID in reverse DNS format. [dim]For example: com.example.myproject[/]:"
                )
            );
    }
}