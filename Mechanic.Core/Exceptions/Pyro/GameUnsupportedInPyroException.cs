namespace Mechanic.Core.Exceptions.Pyro;

using Models;

public class GameUnsupportedInPyroException(GameName inputGame) : Exception($"Game {inputGame.ToString()} is unsupported by Pyro.")
{

}
