namespace Mechanic.CLI.Models.Settings;


public record LocalSettingsOptions(
    string? SpriggitPath,
    string? PyroPath,
    bool UseSpriggitInPath = true,
    bool UsePyroInPath = true
)
{
    public const string SettingSpriggitPath = "SpriggitPath";
    public const string SettingPyroPath = "PyroPath";
    public const string SettingUseSpriggitInPath = "UseSpriggitInPath";
    public const string SettingUsePyroInPath = "UsePyroInPath";
}
