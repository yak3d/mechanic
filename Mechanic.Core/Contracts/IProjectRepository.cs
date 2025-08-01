namespace Mechanic.Core.Contracts;

using Models;

public interface IProjectRepository
{
    public Task<MechanicProject?> GetCurrentProjectAsync();
    public Task SaveCurrentProjectAsync(MechanicProject project);
    public Task<bool> ProjectExistsAsync();

    public Task InitializeProjectAsync(string id,
        string projectNamespace,
        GameName gameName,
        ProjectSettings projectSettings,
        string gamePath,
        List<SourceFile>? sourceFiles = default,
        List<GameFile>? gameFiles = default
    );
    public Task<GameFile?> FindGameFileByIdAsync(Guid id);
    public Task<SourceFile?> FindSourceFileByIdAsync(Guid id);
    public Task<SourceFile> RemoveSourceFileByIdAsync(Guid id);
    public Task<SourceFile> RemoveSourceFileByPathAsync(string path);
    public Task<GameFile> RemoveGameFileByIdAsync(Guid id);
    public Task<GameFile> RemoveGameFileByPathAsync(string path);
    public Task<SourceFile?> FindSourceFileForGameFileIdAsync(Guid id);
}
