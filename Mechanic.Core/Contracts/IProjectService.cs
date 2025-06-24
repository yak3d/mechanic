using Mechanic.Core.Models;

namespace Mechanic.Core.Contracts;

public interface IProjectService
{
    MechanicProject Initialize(string path, string projectId);
}