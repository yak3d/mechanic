using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Microsoft.Extensions.Logging;

namespace Mechanic.Core.Services;

using Infrastructure.Logging;

public class ProjectService(
    ILogger<ProjectService> logger,
    IProjectSerializationService<string> serializationService) : IProjectService
{
    public MechanicProject Initialize(string path, string projectId, Game game)
    {
        logger.ProjectInitializing(projectId, path);
        var project = new MechanicProject
        {
            Id = projectId,
            Game = game
        };

        serializationService.SerializeProject(project, path);
        return project;
    }

    public MechanicProject AddSourceFileToProject(MechanicProject mechanicProject, string path, SourceFile sourceFile)
    {
        logger.ProjectAddingSourceFile(sourceFile.Path, sourceFile.FileType.ToString(), mechanicProject.Id);
        mechanicProject.AddSourceFile(sourceFile);
        this.Save(mechanicProject, path);

        return mechanicProject;
    }

    public MechanicProject Load(string path)
    {
        logger.ProjectLoading(path);
        var mechanicProject = serializationService.DeserializeProject(path);
        logger.ProjectLoaded(mechanicProject.Id);

        return mechanicProject;
    }

    public void Save(MechanicProject mechanicProject, string path)
    {
        logger.ProjectSaving(mechanicProject.Id, path);
        serializationService.SerializeProject(mechanicProject, path);
        logger.ProjectSaved(mechanicProject.Id, path);
    }
}
