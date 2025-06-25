using Mechanic.Core.Models;

namespace Mechanic.Core.Contracts;

public interface IProjectService
{
    public MechanicProject Initialize(string path, string projectId, Game game);
    public MechanicProject Load(string path);
    public MechanicProject AddSourceFileToProject(MechanicProject mechanicProject, string path, SourceFile sourceFile);
}
