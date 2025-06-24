using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Microsoft.Extensions.Logging;

namespace Mechanic.Core.Services;

public class ProjectService(
    ILogger<ProjectService> logger,
    IProjectSerializationService<string> serializationService) : IProjectService
{
    public MechanicProject Initialize(string path, string projectId, Game game)
    {
        logger.LogInformation("Initializing project {ProjectId} in path {Path}", projectId, path);
        var project = new MechanicProject
        {
            Id = projectId,
            Game = game
        };

        serializationService.SerializeProject(project, path);
        return project;
    }
}
