namespace Mechanic.Core.Models;

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Project.Models.Json;

public enum Game
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
    public static string GetDisplayName(this Game game)
    {
        var displayAttribute = game.GetType()
            .GetField(game.ToString())
            ?.GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.Name ?? game.ToString();
    }

    public static MechanicProjectGame ToJson(this Game game) => game switch
    {
        Game.Tes4Oblivion => MechanicProjectGame.TES4Oblivion,
        Game.Tes5Skyrim => MechanicProjectGame.TES5Skyrim,
        Game.SkyrimSpecialEdition => MechanicProjectGame.SkyrimSpecialEdition,
        Game.Fallout3 => MechanicProjectGame.Fallout3,
        Game.FalloutNewVegas => MechanicProjectGame.FalloutNewVegas,
        Game.Fallout4 => MechanicProjectGame.Fallout4,
        Game.Starfield => MechanicProjectGame.Starfield,
        _ => throw new ArgumentOutOfRangeException(nameof(game), game, null)
    };

}

public static class MechanicProjectGameExtensions
{
    public static Game FromJsonGame(this MechanicProjectGame game) => game switch
    {
        MechanicProjectGame.TES4Oblivion => Game.Tes4Oblivion,
        MechanicProjectGame.TES5Skyrim => Game.Tes5Skyrim,
        MechanicProjectGame.SkyrimSpecialEdition => Game.SkyrimSpecialEdition,
        MechanicProjectGame.Fallout3 => Game.Fallout3,
        MechanicProjectGame.FalloutNewVegas => Game.FalloutNewVegas,
        MechanicProjectGame.Fallout4 => Game.Fallout4,
        MechanicProjectGame.Starfield => Game.Starfield,
        _ => throw new ArgumentOutOfRangeException(nameof(game), game, null)
    };
}

