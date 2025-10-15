namespace Mechanic.Core.Contracts;

public interface IFileWatcherBuilder
{
    IFileWatcherBuilder SetGamePath(string newGamePath);
    IFileWatcherBuilder SetSourcePath(string newSourcePath);
    IFileWatcher Build();
}
