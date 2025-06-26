namespace Mechanic.CLI.Models;

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
    public static Core.Models.SourceFileType ToDomain(this SourceFileType sourceFileType) => sourceFileType switch
    {
        SourceFileType.Fbx => Core.Models.SourceFileType.Fbx,
        SourceFileType.Blend => Core.Models.SourceFileType.Blend,
        SourceFileType.Tiff => Core.Models.SourceFileType.Tiff,
        SourceFileType.Wav => Core.Models.SourceFileType.Wav,
        SourceFileType.Psc => Core.Models.SourceFileType.Psc,
        SourceFileType.Other => Core.Models.SourceFileType.Other,
        _ => throw new ArgumentOutOfRangeException(nameof(sourceFileType), sourceFileType, null)
    };
    
    public static SourceFileType FromDomain(this Core.Models.SourceFileType domainSourceFileType) => domainSourceFileType switch
    {
        Core.Models.SourceFileType.Fbx => SourceFileType.Fbx,
        Core.Models.SourceFileType.Blend => SourceFileType.Blend,
        Core.Models.SourceFileType.Tiff => SourceFileType.Tiff,
        Core.Models.SourceFileType.Wav => SourceFileType.Wav,
        Core.Models.SourceFileType.Psc => SourceFileType.Psc,
        Core.Models.SourceFileType.Other => SourceFileType.Other,
        _ => throw new ArgumentOutOfRangeException(nameof(domainSourceFileType), domainSourceFileType, null)
    };
}
