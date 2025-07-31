namespace Mechanic.Core.Repositories;

using System.Text.Json;
using System.Text.Json.Nodes;
using Contracts;
using Exceptions;
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

    public async Task InitializeProjectAsync(string id,
        string projectNamespace,
        GameName gameName,
        ProjectSettings projectSettings,
        string gamePath,
        List<SourceFile>? sourceFiles = default,
        List<GameFile>? gameFiles = default) => await this.SaveCurrentProjectAsync(new MechanicProject
        {
            Id = id,
            Namespace = projectNamespace,
            GameName = gameName,
            GamePath = gamePath,
            ProjectSettings = projectSettings,
            SourceFiles = sourceFiles ?? [],
            GameFiles = gameFiles ?? []
        });

    public async Task<GameFile?> FindGameFileByIdAsync(Guid id)
    {
        var project = await this.GetCurrentProjectAsync();
        return project?.GameFiles.FirstOrDefault(x => x.Id == id);
    }

    public async Task<SourceFile?> FindSourceFileByIdAsync(Guid id)
    {
        var project = await this.GetCurrentProjectAsync();
        return project?.SourceFiles.FirstOrDefault(x => x.Id == id);
    }

    public async Task<SourceFile> RemoveSourceFileByIdAsync(Guid id)
    {
        var project = await this.GetCurrentProjectAsync();

        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        var fileToRemove = project.SourceFiles.FirstOrDefault(x => x.Id == id);

        if (fileToRemove == null)
        {
            throw new ProjectSourceFileNotFoundException(new IdIdentifier(id));
        }

        project.SourceFiles.Remove(fileToRemove);
        await this.SaveCurrentProjectAsync(project);

        return fileToRemove;
    }

    public async Task<SourceFile> RemoveSourceFileByPathAsync(string path)
    {
        var project = await this.GetCurrentProjectAsync();

        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        var fileToRemove = project.SourceFiles.FirstOrDefault(x => x.Path == path);

        if (fileToRemove == null)
        {
            throw new ProjectSourceFileNotFoundException(new PathIdentifier(path));
        }

        project.SourceFiles.Remove(fileToRemove);
        await this.SaveCurrentProjectAsync(project);

        return fileToRemove;
    }

    public async Task<GameFile> RemoveGameFileByIdAsync(Guid id)
    {
        var project = await this.GetCurrentProjectAsync();

        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        var fileToRemove = project.GameFiles.FirstOrDefault(x => x.Id == id);

        if (fileToRemove == null)
        {
            throw new ProjectGameFileNotFoundException(new IdIdentifier(id));
        }

        project.GameFiles.Remove(fileToRemove);
        project.SourceFiles
            .Where(sf => sf.GameFileLinks.Contains(fileToRemove.Id))
            .ToList()
            .ForEach(sf =>
            {
                logger.UnlinkingGameFile(fileToRemove.Path, fileToRemove.Id, sf.Path, sf.Id);
                project.SourceFiles.Remove(sf);
            });

        await this.SaveCurrentProjectAsync(project);

        return fileToRemove;
    }

    public async Task<GameFile> RemoveGameFileByPathAsync(string path)
    {
        var project = await this.GetCurrentProjectAsync();

        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        var fileToRemove = project.GameFiles.FirstOrDefault(x => x.Path == path);

        if (fileToRemove == null)
        {
            throw new ProjectGameFileNotFoundException(new PathIdentifier(path));
        }

        project.GameFiles.Remove(fileToRemove);
        project.SourceFiles
            .Where(sf => sf.GameFileLinks.Contains(fileToRemove.Id))
            .ToList()
            .ForEach(sf => project.SourceFiles.Remove(sf));

        await this.SaveCurrentProjectAsync(project);

        return fileToRemove;
    }

    public async Task<SourceFile?> FindSourceFileForGameFileIdAsync(Guid id)
    {
        var project = await this.GetCurrentProjectAsync();

        if (project == null)
        {
            throw new ProjectNotFoundException();
        }

        return project.SourceFiles.FirstOrDefault(x => x.GameFileLinks.Contains(id));
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

public class ProjectNotFoundException : Exception;
