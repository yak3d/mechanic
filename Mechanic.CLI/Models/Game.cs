using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Mechanic.CLI.Models;

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
    
    public static Core.Models.Game ToDomain(this Game game) => game switch
    {
        Game.Tes4Oblivion => Core.Models.Game.Tes4Oblivion,
        Game.Tes5Skyrim => Core.Models.Game.Tes5Skyrim,
        Game.SkyrimSpecialEdition => Core.Models.Game.SkyrimSpecialEdition,
        Game.Fallout3 => Core.Models.Game.Fallout3,
        Game.FalloutNewVegas => Core.Models.Game.FalloutNewVegas,
        Game.Fallout4 => Core.Models.Game.Fallout4,
        Game.Starfield => Core.Models.Game.Starfield,
        _ => throw new ArgumentOutOfRangeException(nameof(game), game, null)
    };
    
    public static Game FromDomain(this Core.Models.Game domainGame) => domainGame switch
    {
        Core.Models.Game.Tes4Oblivion => Game.Tes4Oblivion,
        Core.Models.Game.Tes5Skyrim => Game.Tes5Skyrim,
        Core.Models.Game.SkyrimSpecialEdition => Game.SkyrimSpecialEdition,
        Core.Models.Game.Fallout3 => Game.Fallout3,
        Core.Models.Game.FalloutNewVegas => Game.FalloutNewVegas,
        Core.Models.Game.Fallout4 => Game.Fallout4,
        Core.Models.Game.Starfield => Game.Starfield,
        _ => throw new ArgumentOutOfRangeException(nameof(domainGame), domainGame, null)
    };
}

