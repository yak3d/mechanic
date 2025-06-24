using Mechanic.Core.Models;

namespace Mechanic.Core.Contracts;

public interface IProjectService
{
    public MechanicProject Initialize(string path, string projectId, Game game);
}
