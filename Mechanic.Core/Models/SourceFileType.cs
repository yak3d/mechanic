namespace Mechanic.Core.Models;
using Mechanic.Core.Project.Models.Json;

public enum SourceFileType
{
    Fbx = 0,
    Blend = 1,
    Tiff = 2,
    Wav = 3,
    Psc = 4,
    Other = 5
}

public static class SourceFileTypeExtensions
{
    public static SourceFilesFileType ToJson(this SourceFileType fileType) => fileType switch
    {
        SourceFileType.Fbx => SourceFilesFileType.FBX,
        SourceFileType.Blend => SourceFilesFileType.BLEND,
        SourceFileType.Tiff => SourceFilesFileType.TIFF,
        SourceFileType.Wav => SourceFilesFileType.WAV,
        SourceFileType.Psc => SourceFilesFileType.PSC,
        SourceFileType.Other => SourceFilesFileType.OTHER,
        _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null)
    };
}
public static class SourceFilesFileTypeExtensions
{
    public static SourceFileType ToSourceFileType(this SourceFilesFileType jsonSourceFileType) =>
        jsonSourceFileType switch
        {
            SourceFilesFileType.FBX => SourceFileType.Fbx,
            SourceFilesFileType.BLEND => SourceFileType.Blend,
            SourceFilesFileType.TIFF => SourceFileType.Tiff,
            SourceFilesFileType.WAV => SourceFileType.Wav,
            SourceFilesFileType.PSC => SourceFileType.Psc,
            SourceFilesFileType.OTHER => SourceFileType.Other,
            _ => throw new ArgumentOutOfRangeException(nameof(jsonSourceFileType), jsonSourceFileType, null)
        };
}
