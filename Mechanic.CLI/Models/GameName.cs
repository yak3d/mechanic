using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Mechanic.CLI.Models;

public enum GameName
{
    [Display(Name = "The Elder Scrolls IV: Oblivion")]
    Tes4Oblivion = 0,

    [Display(Name = "The Elder Scrolls V: Skyrim")]
    Tes5Skyrim = 1,

    [Display(Name = "The Elder Scrolls V: Skyrim Special Edition")]
    SkyrimSpecialEdition = 2,

    [Display(Name = "Fallout 3")]
    Fallout3 = 3,

    [Display(Name = "Fallout: New Vegas")]
    FalloutNewVegas = 4,

    [Display(Name = "Fallout 4")]
    Fallout4 = 5,

    [Display(Name = "Starfield")]
    Starfield = 6
}

public static class GameExtensions
{
    public static string GetDisplayName(this GameName gameName)
    {
        var displayAttribute = gameName.GetType()
            .GetField(gameName.ToString())
            ?.GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.Name ?? gameName.ToString();
    }
    
    public static Core.Models.GameName ToDomain(this GameName gameName) => gameName switch
    {
        GameName.Tes4Oblivion => Core.Models.GameName.Tes4Oblivion,
        GameName.Tes5Skyrim => Core.Models.GameName.Tes5Skyrim,
        GameName.SkyrimSpecialEdition => Core.Models.GameName.SkyrimSpecialEdition,
        GameName.Fallout3 => Core.Models.GameName.Fallout3,
        GameName.FalloutNewVegas => Core.Models.GameName.FalloutNewVegas,
        GameName.Fallout4 => Core.Models.GameName.Fallout4,
        GameName.Starfield => Core.Models.GameName.Starfield,
        _ => throw new ArgumentOutOfRangeException(nameof(gameName), gameName, null)
    };
    
    public static GameName FromDomain(this Core.Models.GameName domainGameName) => domainGameName switch
    {
        Core.Models.GameName.Tes4Oblivion => GameName.Tes4Oblivion,
        Core.Models.GameName.Tes5Skyrim => GameName.Tes5Skyrim,
        Core.Models.GameName.SkyrimSpecialEdition => GameName.SkyrimSpecialEdition,
        Core.Models.GameName.Fallout3 => GameName.Fallout3,
        Core.Models.GameName.FalloutNewVegas => GameName.FalloutNewVegas,
        Core.Models.GameName.Fallout4 => GameName.Fallout4,
        Core.Models.GameName.Starfield => GameName.Starfield,
        _ => throw new ArgumentOutOfRangeException(nameof(domainGameName), domainGameName, null)
    };
}

