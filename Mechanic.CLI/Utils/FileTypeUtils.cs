using Mechanic.CLI.Models;

namespace Mechanic.CLI.Utils;

public class FileTypeUtils
{
    public static SourceFileType AssumeSourceFileType(string fileExtension) => fileExtension switch
    {
        ".fbx" => SourceFileType.Fbx,
        ".blend" => SourceFileType.Blend,
        ".tiff" => SourceFileType.Tiff,
        ".wav" => SourceFileType.Wav,
        ".psc" => SourceFileType.Psc,
        _ => SourceFileType.Other
    };

    public static GameFileType AssumeGameFileType(string fileExtension) => fileExtension switch
    {
        ".mat" => GameFileType.Material,
        ".dds" => GameFileType.DirectDrawSurface,
        ".wem" => GameFileType.WwiseEncodedMedia,
        ".pex" => GameFileType.PapyrusExecutable,
        ".ppj" => GameFileType.PapyrusProject,
        ".nif" => GameFileType.NetImmerseFile,
        _ => GameFileType.Other
    };
}