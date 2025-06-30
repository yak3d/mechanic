namespace Mechanic.Core.Contracts;

using Models;

public interface IProjectRepository
{
    public Task<MechanicProject?> GetCurrentProjectAsync();
    public Task SaveCurrentProjectAsync(MechanicProject project);
    public Task<bool> ProjectExistsAsync();
    public Task InitializeProjectAsync(string id, GameName gameName, string gamePath);
    public Task<GameFile?> FindGameFileByIdAsync(Guid id);
}
