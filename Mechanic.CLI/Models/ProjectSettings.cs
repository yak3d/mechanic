namespace Mechanic.CLI.Models;

public record ProjectSettings(bool usePyro);

public static class ProjectSettingsExtensions
{
    public static ProjectSettings ToCliModel(this Core.Models.ProjectSettings projectSettings) => new(projectSettings.UsePyro);

    public static Core.Models.ProjectSettings ToDomain(this ProjectSettings projectSettings) =>
        new(projectSettings.usePyro);
}