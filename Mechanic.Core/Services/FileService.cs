namespace Mechanic.Core.Services;
using Mechanic.Core.Contracts;

public class FileService : IFileService
{
    public async Task<string> ReadAllText(string path) => await File.ReadAllTextAsync(path);

    public async Task WriteAllText(string path, string contents) => await File.WriteAllTextAsync(path, contents);
    public Task<string[]> GetFilesFromDirectoryAsync(string path, string searchPattern) => Task.FromResult(Directory.GetFiles(path, searchPattern));
    public Task<bool> DirectoryExists(string path) => Task.FromResult(Directory.Exists(path));
    public Task CreateDirectoryAsync(string path) => Task.FromResult(Directory.CreateDirectory(path));
    public Task<bool> FileExists(string path) => Task.FromResult(File.Exists(path));
}
