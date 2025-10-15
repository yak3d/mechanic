namespace Mechanic.Core.Models.FileWatcher;

public enum FileEventType
{
    Created,
    Deleted,
    Changed,
    Renamed
}

public static class FileEventTypeExtensions
{
    public static FileEventType ToFileEventType(this WatcherChangeTypes changeType) => changeType switch
    {
        WatcherChangeTypes.Created => FileEventType.Created,
        WatcherChangeTypes.Deleted => FileEventType.Deleted,
        WatcherChangeTypes.Changed => FileEventType.Changed,
        WatcherChangeTypes.Renamed => FileEventType.Renamed,
        _ => throw new ArgumentOutOfRangeException(nameof(changeType), changeType, null)
    };
}
