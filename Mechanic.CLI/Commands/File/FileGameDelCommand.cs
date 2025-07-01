using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mechanic.CLI.Commands.File;

public class FileGameDelCommand : AsyncCommand<FileGameDelCommand.Settings>
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

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        return Task.FromResult(0);
    }
}