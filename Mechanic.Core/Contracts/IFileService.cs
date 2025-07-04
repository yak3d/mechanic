namespace Mechanic.Core.Contracts;

public interface IFileService
{
    public Task<string> ReadAllText(string path);
    public Task WriteAllText(string path, string contents);
    public Task<string[]> GetFilesFromDirectoryAsync(string path, string searchPattern);
    public Task<bool> DirectoryExists(string path);
    public Task CreateDirectoryAsync(string path);
}
