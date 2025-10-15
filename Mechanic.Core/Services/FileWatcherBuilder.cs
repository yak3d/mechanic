namespace Mechanic.Core.Services;

using Contracts;
using Microsoft.Extensions.Logging;

public class FileWatcherBuilder(ILogger<FileWatcher> logger) : IFileWatcherBuilder
{
    private string? gamePath;
    private string? sourcePath;

    public IFileWatcherBuilder SetGamePath(string newGamePath)
    {
        this.gamePath = newGamePath;
        return this;
    }

    public IFileWatcherBuilder SetSourcePath(string newSourcePath)
    {
        this.sourcePath = newSourcePath;
        return this;
    }

    public IFileWatcher Build() => new FileWatcher(logger, this.gamePath, this.sourcePath);
}
