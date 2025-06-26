using Mechanic.Core.Models;

namespace Mechanic.CLI.Models;

public class GameFile : ProjectFile
{
    public GameFileType GameFileType { get; set; }

    public Core.Models.GameFile ToDomain()
    {
        return new Core.Models.GameFile
        {
            Id = Id,
            Path = Path,
            GameFileType = GameFileType.ToDomain()
        };
    }
    
    public static GameFile FromDomain(Core.Models.GameFile domainGameFile)
    {
        return new GameFile
        {
            Id = domainGameFile.Id,
            Path = domainGameFile.Path,
            GameFileType = domainGameFile.GameFileType.FromDomain()
        };
    }
}
