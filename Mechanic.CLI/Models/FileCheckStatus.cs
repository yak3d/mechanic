namespace Mechanic.CLI.Models;

public enum FileCheckStatus
{
    Exists,
    DoesNotExist,
    OutOfDate
}

public static class FileCheckStatusExtensions
{
    public static FileCheckStatus FromDomain(this Mechanic.Core.Models.FileCheckStatus status) => status switch
        {
            Mechanic.Core.Models.FileCheckStatus.Exists => FileCheckStatus.Exists,
            Mechanic.Core.Models.FileCheckStatus.DoesNotExist => FileCheckStatus.DoesNotExist,
            Mechanic.Core.Models.FileCheckStatus.OutOfDate => FileCheckStatus.OutOfDate,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

    public static string ToStatusEmoji(this FileCheckStatus status) => status switch
    {
        FileCheckStatus.Exists => ":check_mark_button:",
        FileCheckStatus.DoesNotExist => ":cross_mark:",
        FileCheckStatus.OutOfDate => ":alarm_clock:",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static string ToStatusColor(this FileCheckStatus status, bool isGameFile) => status switch
    {
        FileCheckStatus.Exists => "green",
        FileCheckStatus.DoesNotExist => isGameFile ? "orange3" : "red",
        FileCheckStatus.OutOfDate => "yellow",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static string ToStatusDescription(this FileCheckStatus status, bool isGameFile) => status switch
    {
        FileCheckStatus.Exists => "File exists and is up to date",
        FileCheckStatus.DoesNotExist => isGameFile ? "File does not exist but should be generated" : "File does not exist",
        FileCheckStatus.OutOfDate => "File is out of date compared to its source file",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };
}
