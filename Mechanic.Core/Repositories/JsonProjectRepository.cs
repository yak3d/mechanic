namespace Mechanic.Core.Repositories;

using System.Text.Json;
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

    public async Task SaveCurrentProjectAsync(MechanicProject project) => await fileService.WriteAllText(projectFilePath, JsonSerializer.Serialize(project, serializerOptions));

    public async Task<bool> ProjectExistsAsync() => await Task.FromResult(File.Exists(projectFilePath));

    public async Task InitializeProjectAsync(string id, Game game) =>
        await this.SaveCurrentProjectAsync(new MechanicProject
        {
            Id = id,
            Game = Game.Tes4Oblivion
        });
}
