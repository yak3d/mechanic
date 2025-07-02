using System.ComponentModel;
using Mechanic.CLI.Infrastructure.Logging;
using Mechanic.Core.Contracts;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mechanic.CLI.Commands.File;

public class FileGameFileRemoveCommand(IProjectService projectService, ILogger<FileGameFileRemoveCommand> logger) : AsyncCommand<FileGameFileRemoveCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Relative path to the file to be deleted")]
        [CommandOption("-p|--path <PATH>")]
        public string? Path { get; init; }
        
        [Description("Id of the file to be deleted")]
        [CommandOption("-i|--id <ID>")]
        public string? Id { get; init; }
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        return !string.IsNullOrEmpty(settings.Path) && !string.IsNullOrEmpty(settings.Id)
            ? ValidationResult.Error("You cannot specify both Id and Path")
            : ValidationResult.Success();
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (settings.Path != null)
        {
            var removedFile = await projectService.RemoveGameFileByPathAsync(settings.Path);
            return removedFile.Match(
                Right: file =>
                {
                    logger.GameFileRemovedFromProjectByPath(file.Path);
                    return 0;
                },
                Left: _ =>
                {
                    logger.GameFileNotFoundByPathWhenRemoving(settings.Path);
                    return -1;
                });
        }

        if (settings.Id != null)
        {
            var id = Guid.Parse(settings.Id);
            var removedFile = await projectService.RemoveGameFileByIdAsync(id);
            return removedFile.Match(
                Right: file =>
                {
                    logger.GameFileRemovedFromProjectById(file.Id);
                    return 0;
                },
                Left: _ =>
                {
                    logger.GameFileNotFoundByIdWhenRemoving(id);
                    return -2;
                });
        }

        var chosenFile = await FilePrompts.PromptForGameFile("Pick a source file to remove", "Source files", projectService);
        if (chosenFile == null) return 1;
        {
            var removedFile = await projectService.RemoveGameFileByIdAsync(chosenFile.Id);
            return removedFile.Match(
                Right: file =>
                {
                    logger.GameFileRemovedFromProjectById(file.Id);
                    return 0;
                },
                Left: _ =>
                {
                    logger.GameFileNotFoundByIdWhenRemoving(chosenFile.Id);
                    return -3;
                }
            );
        }
    }
}
