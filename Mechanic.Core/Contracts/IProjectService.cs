namespace Mechanic.Core.Contracts;

using LanguageExt;
using Mechanic.Core.Models;
using Services.Errors;

public interface IProjectService
{
    public Task<MechanicProject> InitializeAsync(string path, string projectId, GameName gameName);
    public Task<MechanicProject> GetCurrentProjectAsync();
    public Task<MechanicProject> UpdateProjectGameAsync(GameName gameName);
    public Task<Either<SourceFileAddError, SourceFile>> AddSourceFileAsync(string path, SourceFileType fileType);
    public Task<Either<SourceFileAddError, SourceFile>> AddSourceFileAsync(string path, SourceFileType fileType,
        Guid? id);
    public Task<GameFile> AddGameFileAsync(string path, GameFileType fileType);
    public Task<GameFile?> FindGameFileByIdAsync(Guid id);
    public Task<bool> SourceFileExistsWithPath(string path);
}
