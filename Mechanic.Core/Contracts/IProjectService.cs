namespace Mechanic.Core.Contracts;
using Mechanic.Core.Models;

public interface IProjectService
{
    public Task<MechanicProject> InitializeAsync(string path, string projectId, Game game);
    public Task<MechanicProject> GetCurrentProjectAsync();
    public Task<MechanicProject> UpdateProjectGameAsync(Game game);
    public Task<SourceFile> AddSourceFileAsync(string path, SourceFileType fileType);
}
