using System.ComponentModel;
using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Mechanic.Core.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Mechanic.CLI.Commands.File;

public class FileSrcAddCommand(IProjectService projectService) : Command<FileSrcAddCommand.Settings>
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

    public override int Execute(CommandContext context, Settings settings)
    {
        var mechanicProjectPath = Path.Join(Directory.GetCurrentDirectory(), "mechanic.json");
        var mechanicProject = projectService.Load(mechanicProjectPath);
        var fileType = string.IsNullOrEmpty(settings.Type) 
            ? AssumeFileType(Path.GetExtension(settings.SourcePath))
            : Enum.Parse<SourceFileType>(settings.Type);
        var sourceFile = new SourceFile
        {
            Path = settings.SourcePath,
            FileType = fileType,
        };

        projectService.AddSourceFileToProject(mechanicProject, mechanicProjectPath, sourceFile);
        
        return 0;
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        return string.IsNullOrEmpty(settings.SourcePath)
            ? ValidationResult.Error("You must enter a [b]source path[/].")
            : ValidationResult.Success();
    }

    public SourceFileType AssumeFileType(string fileExtension) => fileExtension switch
    {
        ".fbx" => SourceFileType.Fbx,
        ".blend" => SourceFileType.Blend,
        ".tiff" => SourceFileType.Tiff,
        ".wav" => SourceFileType.Wav,
        ".psc" => SourceFileType.Psc,
        _ => SourceFileType.Other
    };
}