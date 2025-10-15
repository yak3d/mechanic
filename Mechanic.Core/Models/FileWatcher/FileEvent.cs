namespace Mechanic.Core.Models.FileWatcher;

public record FileEvent(string FilePath, FileEventType EventType, FileType FileType);
public record FileRenamedEvent(string OldFilePath, string FilePath, FileType FileType) : FileEvent(FilePath, FileEventType.Renamed, FileType);
