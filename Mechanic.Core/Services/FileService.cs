namespace Mechanic.Core.Services;
using Mechanic.Core.Contracts;

public class FileService : IFileService
{
    public async Task<string> ReadAllText(string path) => await File.ReadAllTextAsync(path);

    public async Task WriteAllText(string path, string contents) => await File.WriteAllTextAsync(path, contents);
}
