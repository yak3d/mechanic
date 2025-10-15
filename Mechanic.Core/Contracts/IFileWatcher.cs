namespace Mechanic.Core.Contracts;

using Models.FileWatcher;

public interface IFileWatcher : IObservable<FileEvent>
{
    public void StartFileWatcher();
}
