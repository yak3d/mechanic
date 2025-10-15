namespace Mechanic.Core.Infrastructure.Observer;

public sealed class Unsubscriber<TFileEvent> : IDisposable
{
    private readonly ISet<IObserver<TFileEvent>> observers;
    private readonly IObserver<TFileEvent> observer;

    internal Unsubscriber(
        ISet<IObserver<TFileEvent>> observers,
        IObserver<TFileEvent> observer
    ) => (this.observers, this.observer) = (observers, observer);

    public void Dispose() => this.observers.Remove(this.observer);
}
