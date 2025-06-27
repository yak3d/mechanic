namespace Mechanic.Core.Services;
using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Microsoft.Extensions.Logging;

using Infrastructure.Logging;

public class ProjectService(
    ILogger<ProjectService> logger,
    IProjectRepository projectRepository) : IProjectService
{
    public async Task<MechanicProject> InitializeAsync(string path, string projectId, Game game)
    {
        if (await projectRepository.ProjectExistsAsync())
        {
            throw new InvalidOperationException("Project already exists. Delete it before initializing.");
        }

        logger.ProjectInitializing(projectId, path);

        await projectRepository.InitializeProjectAsync(projectId, game);
        var project = await projectRepository.GetCurrentProjectAsync();
        return project ?? throw new InvalidOperationException("Project not found after initializing.");
    }

    public async Task<MechanicProject> GetCurrentProjectAsync()
    {
        var project = await projectRepository.GetCurrentProjectAsync() ?? throw new InvalidOperationException("Project not found, initialize it first.");

        return project;
    }

    public async Task<MechanicProject> UpdateProjectGameAsync(Game game)
    {
        var project = await this.GetCurrentProjectAsync();
        project.ChangeGame(game);
        await projectRepository.SaveCurrentProjectAsync(project);

        return project;
    }

    public async Task<SourceFile> AddSourceFileAsync(string path, SourceFileType fileType) => await this.AddSourceFileAsync(path, fileType, null);

    public async Task<SourceFile> AddSourceFileAsync(string path, SourceFileType fileType, Guid? id)
    {
        var project = await this.GetCurrentProjectAsync();
        var sourceFile = project.AddSourceFile(path, fileType, id);
        await projectRepository.SaveCurrentProjectAsync(project);

        return sourceFile;
    }

    public async Task<GameFile> AddGameFileAsync(string path, GameFileType fileType)
    {
        var project = await this.GetCurrentProjectAsync();
        var gameFile = project.AddGameFile(path, fileType);
        await projectRepository.SaveCurrentProjectAsync(project);

        return gameFile;
    }

    public async Task<GameFile?> FindGameFileByIdAsync(Guid id) => await projectRepository.FindGameFileByIdAsync(id);
}
