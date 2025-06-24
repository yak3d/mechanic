using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Mechanic.Core.Services;

public class JsonFileProjectSerializationService(ILogger logger, IFileService fileService) : IProjectSerializationService<string>
{
    public MechanicProject DeserializeProject(string source)
    {
        var fileContents = fileService.ReadAllText(source);
        return JsonConvert.DeserializeObject<MechanicProject>(fileContents) ??
               throw new InvalidOperationException($"Attempted to deserialize project at path {source}");
    }

    public void SerializeProject(MechanicProject project, string destination)
    {
        logger.LogInformation("Serializing project {id} to {destination}", project.Id, destination);
        fileService.WriteAllText(destination, JsonConvert.SerializeObject(project));
    }
}