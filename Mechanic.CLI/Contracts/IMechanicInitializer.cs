using Mechanic.Core.Models;
using GameName = Mechanic.CLI.Models.GameName;
using MechanicProject = Mechanic.CLI.Models.MechanicProject;

namespace Mechanic.CLI.Contracts;

public interface IMechanicInitializer
{
    public MechanicProject Initialize(string projectId, GameName gameName);
}