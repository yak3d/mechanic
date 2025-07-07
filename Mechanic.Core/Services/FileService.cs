namespace Mechanic.Core.Services;

using Errors;
using Infrastructure.Logging;
using LanguageExt;
using Mechanic.Core.Contracts;
using Microsoft.Extensions.Logging;

public class FileService(ILogger<FileService> logger) : IFileService
{
    public async Task<string> ReadAllText(string path) => await File.ReadAllTextAsync(path);

    public async Task WriteAllText(string path, string contents) => await File.WriteAllTextAsync(path, contents);
    public Task<string[]> GetFilesFromDirectoryAsync(string path, string searchPattern) => Task.FromResult(Directory.GetFiles(path, searchPattern));
    public Task<bool> DirectoryExists(string path) => Task.FromResult(Directory.Exists(path));
    public Task CreateDirectoryAsync(string path) => Task.FromResult(Directory.CreateDirectory(path));
    public Task<bool> FileExistsAsync(string path) => Task.FromResult(File.Exists(path));

    public Task<Either<FileError, long>> GetLastModifiedTimeAsync(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);

            return fileInfo.Exists ? Task.FromResult<Either<FileError, long>>(((DateTimeOffset)fileInfo.LastWriteTime).ToUnixTimeMilliseconds()) : Task.FromResult<Either<FileError, long>>(new FileNotFoundError(path));
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.FailedToFindLastModifiedTime(path, ex);
            return Task.FromResult<Either<FileError, long>>(new UnauthorizedError(path));
        }

    }
}
