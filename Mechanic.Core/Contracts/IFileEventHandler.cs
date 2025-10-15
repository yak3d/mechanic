namespace Mechanic.Core.Contracts;

using Models.FileWatcher;

#pragma warning disable CA1711
public abstract class IFileEventHandler : IObserver<FileEvent>
#pragma warning restore CA1711
{
    public void OnCompleted() => throw new NotImplementedException();

    public void OnError(Exception error) => throw new NotImplementedException();

    public void OnNext(FileEvent value) => this.ProcessFileEvent(value);

    public abstract IDisposable Subscribe(IObserver<FileEvent> observer);

    protected abstract void ProcessFileEvent(FileEvent fileEvent);
}
