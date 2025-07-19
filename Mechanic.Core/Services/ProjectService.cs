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
    IFileService fileService,
    IProjectRepository projectRepository) : IProjectService
{
    public async Task<MechanicProject> InitializeAsync(
        string path,
        string projectId,
        GameName gameName,
        ProjectSettings projectSettings,
        string gamePath,
        List<SourceFile> sourceFiles,
        List<GameFile> gameFiles)
    {
        if (await projectRepository.ProjectExistsAsync())
        {
            throw new InvalidOperationException("Project already exists. Delete it before initializing.");
        }

        logger.ProjectInitializing(projectId, path);

        await projectRepository.InitializeProjectAsync(projectId, gameName, projectSettings, gamePath, sourceFiles, gameFiles);
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

    public async Task<Either<GameFileError, GameFile>> AddGameFileAsync(string path, GameFileType fileType)
    {
        var project = await this.GetCurrentProjectAsync();
        var gameFile = project.AddGameFile(path, fileType);
        await projectRepository.SaveCurrentProjectAsync(project);

        return gameFile;
    }

    public async Task<Either<ProjectError, GameFile>> AddGameFileAsync(string path, GameFileType fileType, Guid? sourceFileId)
    {
        var project = await this.GetCurrentProjectAsync();

        var gameFile = sourceFileId != null
            ? project.AddGameFile(path, fileType, sourceFileId.Value)
            : project.AddGameFile(path, fileType);

        await projectRepository.SaveCurrentProjectAsync(project);
        return gameFile;
    }

    public async Task LinkGameFileToSourceFileAsync(Guid gameFileId, Guid sourceFileId)
    {
        var project = await this.GetCurrentProjectAsync();
        project.SourceFiles.Where(f => f.Id == sourceFileId).ToList().ForEach(f => f.GameFileLinks.Add(gameFileId));
        await projectRepository.SaveCurrentProjectAsync(project);
    }

    public async Task<GameFile?> FindGameFileByIdAsync(Guid id) => await projectRepository.FindGameFileByIdAsync(id);
    public async Task<SourceFile?> FindSourceFileByIdAsync(Guid id) => await projectRepository.FindSourceFileByIdAsync(id);
    public async Task<bool> SourceFileExistsWithPathAsync(string path) => await projectRepository.GetCurrentProjectAsync()
        .ContinueWith(project =>
        {
            var projectResult = project.Result;

            return projectResult != null && Enumerable.Any(projectResult.SourceFiles, sourceFile => sourceFile.Path == path);
        });

    public async Task<bool> GameFileExistsWithPathAsync(string path) => await projectRepository.GetCurrentProjectAsync()
        .ContinueWith(project =>
        {
            var projectResult = project.Result;

            return projectResult != null && Enumerable.Any(projectResult.GameFiles, gameFile => gameFile.Path == path);
        });

    public Task<Either<CheckError, Dictionary<SourceFile, bool>>> CheckSourceFilesAsync<T>() => throw new NotImplementedException();

    public async Task<Either<ProjectError, Dictionary<SourceFile, FileCheckStatus>>> CheckSourceFilesAsync()
    {
        var project = await this.GetCurrentProjectAsync();
        var files = project.SourceFiles;

        return await this.CheckFileExistence(files);
    }

    public async Task<Either<ProjectError, Dictionary<GameFile, FileCheckStatus>>> CheckGameFilesAsync()
    {
        var project = await this.GetCurrentProjectAsync();
        var files = project.GameFiles;

        return await this.CheckFileExistence(files);
    }

    private async Task<Either<ProjectError, Dictionary<T, FileCheckStatus>>> CheckFileExistence<T>(IEnumerable<T> files) where T : ProjectFile
    {
        try
        {
            var project = await this.GetCurrentProjectAsync();
            var results = await Task.WhenAll(files.Select(async file =>
            {
                var checkPath = file is GameFile ? Path.Combine(project.GamePath, "Data", file.Path) : file.Path; // todo make this more concrete with a service or something idk
                var fileExists = await fileService.FileExistsAsync(checkPath);

                if (file is GameFile)
                {
                    var pairedSourceFile = await projectRepository.FindSourceFileForGameFileIdAsync(file.Id);

                    if (pairedSourceFile != null)
                    {
                        var sourceFileLastModified = await fileService.GetLastModifiedTimeAsync(pairedSourceFile.Path);
                        var gameFileLastModified = await fileService.GetLastModifiedTimeAsync(checkPath);

                        if (sourceFileLastModified.IsRight && gameFileLastModified.IsRight)
                        {
                            if (sourceFileLastModified >= gameFileLastModified)
                            {
                                return new { File = file, Status = FileCheckStatus.OutOfDate };
                            }
                        }
                    }
                }

                var existenceStatus = fileExists ? FileCheckStatus.Exists : FileCheckStatus.DoesNotExist;
                return new { File = file, Status = existenceStatus };
            }));

            return results.ToDictionary(
                result => result.File,
                result => result.Status
            );
        }
        catch (Exception)
        {
            return new CheckError();
        }
    }
}

