namespace Mechanic.Core.FileHandlers;

using Contracts;
using Infrastructure.Logging;
using Infrastructure.Observer;
using Microsoft.Extensions.Logging;
using Models.FileWatcher;

public class DefaultFileEventHandler(ILogger<DefaultFileEventHandler> logger, IProjectService projectService) : IFileEventHandler, IObservable<FileEvent>
{
    private readonly HashSet<IObserver<FileEvent>> observers = [];

    protected override void ProcessFileEvent(FileEvent fileEvent)
    {
        logger.ReceivedFileEvent(fileEvent.EventType.ToString(), fileEvent.FilePath);
        switch (fileEvent.EventType)
        {
            case FileEventType.Created:
                this.HandleCreatedFile(fileEvent);
                break;
            case FileEventType.Deleted:
                this.HandleDeletedFile(fileEvent);
                break;
            case FileEventType.Changed:
                break;
            case FileEventType.Renamed:
                if (fileEvent is FileRenamedEvent fileRenamedEvent)
                {
                    this.HandleRenamedFile(fileRenamedEvent);
                }
                break;
        }
    }

    private void HandleRenamedFile(FileRenamedEvent fileEvent)
    {
        if (fileEvent.FileType == FileType.Source)
        {
            projectService.ChangeSourceFilePath(fileEvent.OldFilePath, fileEvent.FilePath);
        }
    }

    private void HandleDeletedFile(FileEvent fileEvent) => this.NotifyObservers(fileEvent);

    private void HandleCreatedFile(FileEvent fileEvent) => this.NotifyObservers(fileEvent);

    public override IDisposable Subscribe(IObserver<FileEvent> observer)
    {
        this.observers.Add(observer);

        return new Unsubscriber<FileEvent>(this.observers, observer);
    }

    private void NotifyObservers(FileEvent fileEvent)
    {
        foreach (var observer in this.observers)
        {
            observer.OnNext(fileEvent);
        }
    }
}
