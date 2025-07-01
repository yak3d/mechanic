namespace Mechanic.Core.Repositories;

using System.Text.Json;
using System.Text.Json.Nodes;
using Contracts;
using Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using Models;

public class JsonProjectRepository(
    ILogger<JsonProjectRepository> logger,
    IFileService fileService,
    string projectFilePath = "mechanic.json"
    ) : IProjectRepository
{
    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<MechanicProject?> GetCurrentProjectAsync()
    {
        if (!await this.ProjectExistsAsync())
        {
            return null;
        }

        try
        {
            var json = await fileService.ReadAllText(projectFilePath);
            var jsonModel = JsonSerializer.Deserialize<Project.Models.Json.MechanicProject>(json, serializerOptions);
            return jsonModel != null ? MechanicProject.FromJsonObject(jsonModel) : null;
        }
        catch (JsonException jsonException)
        {
            logger.ProjectDeserializingError(projectFilePath, jsonException);
            return null;
        }
    }

    public async Task SaveCurrentProjectAsync(MechanicProject project)
    {
        var projectJson = project.ToJson();
        var jsonString = JsonSerializer.Serialize(projectJson, serializerOptions);

        var jsonNode = JsonNode.Parse(jsonString)!;
        var jsonWithSchema = PrependSchema(jsonNode, "https://raw.githubusercontent.com/yak3d/mechanic/refs/heads/main/Mechanic.Core/ProjectFileSchema.json");

        await fileService.WriteAllText(projectFilePath, jsonWithSchema.ToJsonString(serializerOptions));
    }

    public async Task<bool> ProjectExistsAsync() => await Task.FromResult(File.Exists(projectFilePath));

    public async Task InitializeProjectAsync(string id, GameName gameName, string gamePath) =>
        await this.SaveCurrentProjectAsync(new MechanicProject
        {
            Id = id,
            GameName = GameName.Tes4Oblivion,
            GamePath = gamePath
        });

    public async Task<GameFile?> FindGameFileByIdAsync(Guid id)
    {
        var project = await this.GetCurrentProjectAsync();
        return project?.GameFiles.FirstOrDefault(x => x.Id == id);
    }

    private static JsonObject PrependSchema(JsonNode jsonNode, string schemaUrl)
    {
        var originalObject = jsonNode.AsObject();
        var newJsonObject = new JsonObject
        {
            ["$schema"] = schemaUrl
        };

        foreach (var kvp in originalObject)
        {
            newJsonObject[kvp.Key] = kvp.Value?.DeepClone();
        }

        return newJsonObject;
    }
}
