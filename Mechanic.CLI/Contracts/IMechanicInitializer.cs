using Mechanic.Core.Models;
using Game = Mechanic.CLI.Models.Game;
using MechanicProject = Mechanic.CLI.Models.MechanicProject;

namespace Mechanic.CLI.Contracts;

public interface IMechanicInitializer
{
    public MechanicProject Initialize(string projectId, Game game);
}