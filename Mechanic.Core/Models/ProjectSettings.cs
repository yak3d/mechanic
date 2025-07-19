namespace Mechanic.Core.Models;

public record ProjectSettings(
    bool UsePyro = false
);

public class ProjectSettingsBuilder
{
    private bool usePyro;

    public ProjectSettingsBuilder WithPyro(bool pyroUsage = true)
    {
        this.usePyro = pyroUsage;
        return this;
    }

    public ProjectSettingsBuilder EnablePyro()
    {
        this.usePyro = true;
        return this;
    }

    public ProjectSettingsBuilder DisablePyro()
    {
        this.usePyro = false;
        return this;
    }

    public ProjectSettings Build()
    {
        return new ProjectSettings(UsePyro: this.usePyro);
    }

    public static ProjectSettingsBuilder Create()
    {
        return new ProjectSettingsBuilder();
    }
}

public static class ProjectSettingsExtensions
{
    public static ProjectSettings ToDomain(this Core.Project.Models.Json.ProjectSettings projectSettings) =>
        new(projectSettings.UsePyro);
    public static Core.Project.Models.Json.ProjectSettings ToJsonModel(this ProjectSettings domainProjectSettings) =>
        new() { UsePyro = domainProjectSettings.UsePyro };
}
