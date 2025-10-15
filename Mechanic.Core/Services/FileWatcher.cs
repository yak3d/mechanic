namespace Mechanic.Core.Services;

using Contracts;
using Infrastructure.Logging;
using Infrastructure.Observer;
using Microsoft.Extensions.Logging;
using Models.FileWatcher;

public class FileWatcher(ILogger<FileWatcher> logger, string? gamePath = null, string? sourcePath = null) : IFileWatcher, IDisposable
{
    private readonly HashSet<IObserver<FileEvent>> observers = [];
    private FileSystemWatcher? sourceFileWatcher;
    private FileSystemWatcher? gameFileWatcher;

    public IDisposable Subscribe(IObserver<FileEvent> observer)
    {
        if (this.observers.Add(observer))
        {
            logger.FileWatcherRegisteredSubscriber(observer.GetType().ToString(), observer);
        }

        return new Unsubscriber<FileEvent>(this.observers, observer);
    }

    public void StartFileWatcher()
    {
        if (sourcePath != null)
        {
            logger.WatchingSourceFile(sourcePath);
            this.sourceFileWatcher = new FileSystemWatcher(sourcePath);
            this.sourceFileWatcher.NotifyFilter = NotifyFilters.Attributes
                                                  | NotifyFilters.CreationTime
                                                  | NotifyFilters.DirectoryName
                                                  | NotifyFilters.FileName
                                                  | NotifyFilters.LastWrite
                                                  | NotifyFilters.Size;

            this.sourceFileWatcher.Changed += this.SourceFileWatcherOnChanged;
            this.sourceFileWatcher.Created += this.SourceFileWatcherOnChanged;
            this.sourceFileWatcher.Deleted += this.SourceFileWatcherOnChanged;
            this.sourceFileWatcher.Renamed += this.SourceFileWatcherOnChanged;
            this.sourceFileWatcher.Error += this.FileWatcherError;

            // this.sourceFileWatcher.Filter = "*.tiff,*.fbx,*.wav";
            this.sourceFileWatcher.IncludeSubdirectories = true;
            this.sourceFileWatcher.EnableRaisingEvents = true;
        }

        if (gamePath != null)
        {
            logger.WatchingGameFile(gamePath);
            this.gameFileWatcher = new FileSystemWatcher(gamePath);
            this.gameFileWatcher.NotifyFilter = NotifyFilters.Attributes
                                                  | NotifyFilters.CreationTime
                                                  | NotifyFilters.DirectoryName
                                                  | NotifyFilters.FileName
                                                  | NotifyFilters.LastWrite
                                                  | NotifyFilters.Size;

            this.gameFileWatcher.Changed += this.GameFileWatcherOnChanged;
            this.gameFileWatcher.Created += this.GameFileWatcherOnChanged;
            this.gameFileWatcher.Deleted += this.GameFileWatcherOnChanged;
            this.gameFileWatcher.Renamed += this.GameFileWatcherOnChanged;
            this.gameFileWatcher.Error += this.FileWatcherError;

            //this.gameFileWatcher.Filter = "*.tiff,*.fbx,*.wav";
            this.gameFileWatcher.IncludeSubdirectories = true;
            this.gameFileWatcher.EnableRaisingEvents = true;
        }
    }

    private void FileWatcherError(object sender, ErrorEventArgs e)
    {
        logger.ErrorWatchingFile(e.GetException());
    }

    private void SourceFileWatcherOnChanged(object sender, FileSystemEventArgs e)
    {
        logger.FileWatcherDetected(e.ChangeType.ToString(), e.FullPath);
        var fileEvent = new FileEvent(FilePath: e.FullPath, EventType: e.ChangeType.ToFileEventType(), FileType.Source);
        this.EmitEvent(fileEvent);
    }

    private void GameFileWatcherOnChanged(object sender, FileSystemEventArgs e)
    {
        logger.FileWatcherDetected(e.ChangeType.ToString(), e.FullPath);

        var fileType = sender.Equals(this.sourceFileWatcher) ? FileType.Source : FileType.Game;

        var fileEvent = e is RenamedEventArgs renamed
            ? new FileRenamedEvent(renamed.OldFullPath, renamed.FullPath, fileType)
            : new FileEvent(e.FullPath, e.ChangeType.ToFileEventType(), fileType);

        this.EmitEvent(fileEvent);
    }

    private void EmitEvent(FileEvent fileEvent)
    {
        foreach (var observer in this.observers)
        {
            observer.OnNext(fileEvent);
        }
    }

    public void Dispose()
    {
        this.sourceFileWatcher?.Dispose();
        this.gameFileWatcher?.Dispose();
        GC.SuppressFinalize(this);
    }
}
