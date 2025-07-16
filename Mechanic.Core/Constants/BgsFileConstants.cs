namespace Mechanic.Core.Constants;

public static class BgsFileConstants
{
    public const string GameDataDirectoryName = "Data";
    public const string ScriptsDirectoryName = "Scripts";
}

public static class SkyrimFileConstants
{
    public static readonly string ScriptSourceDirectory = Path.Combine(
        BgsFileConstants.GameDataDirectoryName,
        BgsFileConstants.ScriptsDirectoryName,
        "Source"
    );
}

public static class FalloutFileConstants
{
    public static readonly string ScriptSourceDirectory = Path.Combine(
        BgsFileConstants.GameDataDirectoryName,
        BgsFileConstants.ScriptsDirectoryName,
        "Source",
        "Base"
    );
}
