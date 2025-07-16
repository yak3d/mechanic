namespace Mechanic.Core.Models.Pyro;

using Exceptions.Pyro;

public static class GameNameExtensions
{
    public static string ToPyroGameName(this GameName gameName) => gameName switch
    {
        GameName.Tes5Skyrim => "tes5",
        GameName.SkyrimSpecialEdition => "sse",
        GameName.Fallout4 => "fo4",
        GameName.Starfield => "sf1",
        _ => throw new GameUnsupportedInPyroException(gameName)
    };
}
