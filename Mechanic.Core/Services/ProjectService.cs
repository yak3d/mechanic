namespace Mechanic.Core.Services;

using Errors;
using Infrastructure.Logging;
using LanguageExt;
using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Microsoft.Extensions.Logging;
using Repositories.Exceptions;
using static LanguageExt.Prelude;

public class ProjectService(
    ILogger<ProjectService> logger,
    IProjectRepository projectRepository) : IProjectService
{
    public async Task<MechanicProject> InitializeAsync(string path, string projectId, GameName gameName, string gamePath)
    {
        if (await projectRepository.ProjectExistsAsync())
        {
            throw new InvalidOperationException("Project already exists. Delete it before initializing.");
        }

        logger.ProjectInitializing(projectId, path);

        await projectRepository.InitializeProjectAsync(projectId, gameName, gamePath);
        var project = await projectRepository.GetCurrentProjectAsync();
        return project ?? throw new InvalidOperationException("Project not found after initializing.");
    }

    public async Task<MechanicProject> GetCurrentProjectAsync()
    {
        var project = await projectRepository.GetCurrentProjectAsync() ?? throw new InvalidOperationException("Project not found, initialize it first.");

        return project;
    }

    public async Task<MechanicProject> UpdateProjectGameAsync(GameName gameName)
    {
        var project = await this.GetCurrentProjectAsync();
        project.ChangeGame(gameName);
        await projectRepository.SaveCurrentProjectAsync(project);

        return project;
    }

    public async Task<Either<SourceFileAddError, SourceFile>> AddSourceFileAsync(string path, SourceFileType fileType) => await this.AddSourceFileAsync(path, fileType, null);

    public async Task<Either<SourceFileAddError, SourceFile>> AddSourceFileAsync(
        string path,
        SourceFileType fileType,
        Guid? id
        )
    {
        var project = await this.GetCurrentProjectAsync();

        if (id != null && project.GameFiles.All(file => file.Id != id))
        {
            return Left<SourceFileAddError, SourceFile>(new LinkedFileDoesNotExistError { LinkedFileId = id.Value });
        }

        var sourceFile = project.AddSourceFile(path, fileType, id);
        await projectRepository.SaveCurrentProjectAsync(project);

        return sourceFile;
    }

    public async Task<Either<SourceFileDoesNotExistAtPathError, SourceFile>> RemoveSourceFileByPathAsync(string path)
    {
        try
        {
            return await projectRepository.RemoveSourceFileByPathAsync(path);
        }
        catch (ProjectSourceFileNotFoundException)
        {
            return new SourceFileDoesNotExistAtPathError(path);
        }
    }

    public async Task<Either<SourceFileDoesNotExistWithIdError, SourceFile>> RemoveSourceFileByIdAsync(Guid id)
    {
        try
        {
            return await projectRepository.RemoveSourceFileByIdAsync(id);
        }
        catch (ProjectSourceFileNotFoundException)
        {
            return new SourceFileDoesNotExistWithIdError(id);
        }
    }

    public async Task<Either<GameFileDoesNotExistAtPathError, GameFile>> RemoveGameFileByPathAsync(string path)
    {
        try
        {
            return await projectRepository.RemoveGameFileByPathAsync(path);
        }
        catch (ProjectSourceFileNotFoundException)
        {
            return new GameFileDoesNotExistAtPathError(path);
        }
    }

    public async Task<Either<GameFileDoesNotExistWithIdError, GameFile>> RemoveGameFileByIdAsync(Guid id)
    {
        try
        {
            return await projectRepository.RemoveGameFileByIdAsync(id);
        }
        catch (ProjectGameFileNotFoundException)
        {
            return new GameFileDoesNotExistWithIdError(id);
        }
    }

    public async Task<GameFile> AddGameFileAsync(string path, GameFileType fileType)
    {
        var project = await this.GetCurrentProjectAsync();
        var gameFile = project.AddGameFile(path, fileType);
        await projectRepository.SaveCurrentProjectAsync(project);

        return gameFile;
    }

    public async Task<GameFile?> FindGameFileByIdAsync(Guid id) => await projectRepository.FindGameFileByIdAsync(id);
    public async Task<bool> SourceFileExistsWithPath(string path) => await projectRepository.GetCurrentProjectAsync()
        .ContinueWith(project =>
        {
            var projectResult = project.Result;

            return projectResult != null && Enumerable.Any(projectResult.SourceFiles, sourceFile => sourceFile.Path == path);
        });
}

