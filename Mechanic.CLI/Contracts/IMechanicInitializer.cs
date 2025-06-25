using Mechanic.Core.Models;

namespace Mechanic.CLI.Contracts;

public interface IMechanicInitializer
{
    public MechanicProject Initialize(string projectId, Game game);
}