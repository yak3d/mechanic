using Mechanic.CLI.Contracts;
using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Game = Mechanic.CLI.Models.Game;
using MechanicProject = Mechanic.CLI.Models.MechanicProject;

namespace Mechanic.CLI;

public class MechanicInitializer(IProjectService projectService) : IMechanicInitializer
{
    public MechanicProject Initialize(string projectId, Game game)
    {
        throw new NotImplementedException();
    }
}