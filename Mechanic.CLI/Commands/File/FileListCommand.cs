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

        var sourceFileRoot = new Tree("[green]Source Files[/]");
        foreach (var sf in project.SourceFiles)
        {
            var fileNode = new TreeNode(new Text($"{sf.Path} ({sf.Id})"));

            await BuildGameFileLinkTree(sf, fileNode);

            sourceFileRoot.AddNode(fileNode);
        }

        var gameFiles = project.GameFiles.OrderBy(static f => f.Path).Select(gf => new Text($"{gf.Path} ({gf.Id})"));
        var gameFilesRoot = new Tree("[magenta]Game Files[/]");
        gameFilesRoot.AddNodes(gameFiles);

        AnsiConsole.Write(new Rows(
                sourceFileRoot,
                Text.NewLine,
                gameFilesRoot
            )
        );

        return 0;
    }

    private async Task BuildGameFileLinkTree(SourceFile sf, TreeNode fileNode)
    {
        if (sf.DestinationPaths.Count > 0)
        {
            var gameFileIds = await Task.WhenAll(
                sf.DestinationPaths.Select(async gf =>
                {
                    var gameFile = await projectService.FindGameFileByIdAsync(gf);
                    return gameFile != null ? $"{gameFile.Path} ({gameFile.Id})" : $"Game file not found: {gf}";
                })
            );
            fileNode.AddNodes(gameFileIds);
        }
    }
}
