using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Mechanic.Core.Services;

using Newtonsoft.Json.Serialization;

public class JsonFileProjectSerializationService(ILogger<JsonFileProjectSerializationService> logger, IFileService fileService) : IProjectSerializationService<string>
{
    public MechanicProject DeserializeProject(string source)
    {
        var fileContents = fileService.ReadAllText(source);
        return JsonConvert.DeserializeObject<MechanicProject>(fileContents) ??
               throw new InvalidOperationException($"Attempted to deserialize project at path {source}");
    }

    public void SerializeProject(MechanicProject project, string destination)
    {
        // todo: needs to convert to the generated types first before writing
        logger.LogInformation("Serializing project {id} to {destination}", project.Id, destination);
        fileService.WriteAllText(
            destination,
            JsonConvert.SerializeObject(
                project.ToJson(),
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            ));
    }
}
