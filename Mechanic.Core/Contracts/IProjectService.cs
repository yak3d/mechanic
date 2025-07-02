namespace Mechanic.Core.Contracts;

using LanguageExt;
using Mechanic.Core.Models;
using Services.Errors;

public interface IProjectService
{
    public Task<MechanicProject> InitializeAsync(string path, string projectId, GameName gameName, string gamePath);
    public Task<MechanicProject> GetCurrentProjectAsync();
    public Task<MechanicProject> UpdateProjectGameAsync(GameName gameName);
    public Task<Either<SourceFileAddError, SourceFile>> AddSourceFileAsync(string path, SourceFileType fileType);
    public Task<Either<SourceFileAddError, SourceFile>> AddSourceFileAsync(string path, SourceFileType fileType,
        Guid? id);
    public Task<Either<SourceFileDoesNotExistAtPathError, SourceFile>> RemoveSourceFileByPathAsync(string path);
    public Task<Either<SourceFileDoesNotExistWithIdError, SourceFile>> RemoveSourceFileByIdAsync(Guid id);
    public Task<Either<GameFileDoesNotExistAtPathError, GameFile>> RemoveGameFileByPathAsync(string path);
    public Task<Either<GameFileDoesNotExistWithIdError, GameFile>> RemoveGameFileByIdAsync(Guid id);
    public Task<GameFile> AddGameFileAsync(string path, GameFileType fileType);
    public Task<GameFile?> FindGameFileByIdAsync(Guid id);
    public Task<bool> SourceFileExistsWithPath(string path);
}
