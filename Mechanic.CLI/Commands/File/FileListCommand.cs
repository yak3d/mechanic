using System.ComponentModel;
using Mechanic.CLI.Models;
using Mechanic.Core.Contracts;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mechanic.CLI.Commands.File;

public class FileListCommand(IProjectService projectService) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        var project = MechanicProject.FromDomain(await projectService.GetCurrentProjectAsync());
        var files = project.SourceFiles.Select(ProjectFile (file) => file)
            .Concat(project.DestinationFiles.Select(file => (ProjectFile) file));

        var treeRoot = new Tree(project.Id);
        treeRoot.AddNodes(files.Select(file => $"{file.Path}"));
        
        AnsiConsole.Write(treeRoot);

        return 0;
    }
}