using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Mechanic.Core.Services;

using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Logging;

public class JsonFileProjectSerializationService(ILogger<JsonFileProjectSerializationService> logger, IFileService fileService) : IProjectSerializationService<string>
{
    private static JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public MechanicProject DeserializeProject(string source)
    {
        var fileContents = fileService.ReadAllText(source);
        return JsonConvert.DeserializeObject<MechanicProject>(fileContents) ??
               throw new InvalidOperationException($"Attempted to deserialize project at path {source}");
    }

public void SerializeProject(MechanicProject project, string destination)
{
    logger.ProjectSerializing(project.Id, destination);

    fileService.WriteAllText(
        destination,
        JsonSerializer.Serialize(project.ToJson(), serializerOptions)
    );
}
}
