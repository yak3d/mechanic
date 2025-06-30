namespace Mechanic.Core.Models;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

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

    public static Project.Models.Json.GameName ToJson(this GameName gameName) => gameName switch
    {
        GameName.Tes4Oblivion => Project.Models.Json.GameName.TES4Oblivion,
        GameName.Tes5Skyrim => Project.Models.Json.GameName.TES5Skyrim,
        GameName.SkyrimSpecialEdition => Project.Models.Json.GameName.SkyrimSpecialEdition,
        GameName.Fallout3 => Project.Models.Json.GameName.Fallout3,
        GameName.FalloutNewVegas => Project.Models.Json.GameName.FalloutNewVegas,
        GameName.Fallout4 => Project.Models.Json.GameName.Fallout4,
        GameName.Starfield => Project.Models.Json.GameName.Starfield,
        _ => throw new ArgumentOutOfRangeException(nameof(gameName), gameName, null)
    };

}

public static class MechanicProjectGameExtensions
{
    public static GameName FromJsonGame(this Project.Models.Json.GameName game) => game switch
    {
        Project.Models.Json.GameName.TES4Oblivion => GameName.Tes4Oblivion,
        Project.Models.Json.GameName.TES5Skyrim => GameName.Tes5Skyrim,
        Project.Models.Json.GameName.SkyrimSpecialEdition => GameName.SkyrimSpecialEdition,
        Project.Models.Json.GameName.Fallout3 => GameName.Fallout3,
        Project.Models.Json.GameName.FalloutNewVegas => GameName.FalloutNewVegas,
        Project.Models.Json.GameName.Fallout4 => GameName.Fallout4,
        Project.Models.Json.GameName.Starfield => GameName.Starfield,
        _ => throw new ArgumentOutOfRangeException(nameof(game), game, null)
    };
}

