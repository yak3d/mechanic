using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Microsoft.Extensions.Logging;

namespace Mechanic.Core.Services;

public class ProjectService(
    ILogger<ProjectService> logger,
    IProjectSerializationService<string> serializationService) : IProjectService
{
    public MechanicProject Initialize(string path, string projectId)
    {
        logger.LogInformation("Initializing project {ProjectId} in path {path}", projectId, path);
        var project = new MechanicProject
        {
            Id = projectId
        };

        serializationService.SerializeProject(project, path);
        return project;
    }
}