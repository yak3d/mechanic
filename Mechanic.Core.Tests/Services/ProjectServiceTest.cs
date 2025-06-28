using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Mechanic.Core.Services;
using Mechanic.Core.Services.Errors;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Mechanic.Core.Tests.Services;

public class ProjectServiceTest
{
    private readonly Mock<ILogger<ProjectService>> _mockLogger;
    private readonly Mock<IProjectRepository> _mockProjectRepository;
    private readonly ProjectService _projectService;

    public ProjectServiceTest()
    {
        _mockLogger = new Mock<ILogger<ProjectService>>();
        _mockProjectRepository = new Mock<IProjectRepository>();
        _projectService = new ProjectService(_mockLogger.Object, _mockProjectRepository.Object);
    }

    [Fact]
    public async Task ProjectService_InitializeAsync_CreatesProject()
    {
        var path = "mechanic.json";
        var projectId = "com.example.MyProject";
        var game = Game.SkyrimSpecialEdition;

        _mockProjectRepository.Setup(repo => repo.InitializeProjectAsync(projectId, game));
        _mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(new MechanicProject
        {
            Id = projectId,
            Game = game
        })!);

        var result = await _projectService.InitializeAsync(path, projectId, game);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(projectId);
        result.Game.ShouldBe(game);
        result.GameFiles.ShouldBeEmpty();
        result.SourceFiles.ShouldBeEmpty();

        _mockProjectRepository.Verify(repo => repo.InitializeProjectAsync(projectId, game), Times.Once);
        _mockProjectRepository.Verify(repo => repo.GetCurrentProjectAsync(), Times.Once);
    }

    [Fact]
    public async Task ProjectService_InitializeAsync_ThrowsException_IfProjectExists()
    {
        var path = "mechanic.json";
        var projectId = "com.example.MyProject";
        var game = Game.SkyrimSpecialEdition;

        _mockProjectRepository.Setup(repo => repo.InitializeProjectAsync(projectId, game)).Throws<InvalidOperationException>();

        await _projectService.InitializeAsync(path, projectId, game).ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ProjectService_GetCurrentProjectAsync_ReturnsProject()
    {
        _mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(new MechanicProject
        {
            Id = "com.example.MyProject",
            Game = Game.SkyrimSpecialEdition
        })!);

        var result = await _projectService.GetCurrentProjectAsync();
        result.ShouldNotBeNull();
        result.Id.ShouldBe("com.example.MyProject");
        result.Game.ShouldBe(Game.SkyrimSpecialEdition);
    }

    [Fact]
    public async Task ProjectService_GetCurrentProjectAsync_ThrowsWhenProjectDoesntExist()
    {
        _mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult<MechanicProject?>(null));

        await _projectService.GetCurrentProjectAsync().ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ProjectService_UpdateProjectGameAsync_UpdatesProjectGame()
    {
        _mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(new MechanicProject
        {
            Id = "com.example.MyProject",
            Game = Game.SkyrimSpecialEdition
        })!);

        var result = await _projectService.UpdateProjectGameAsync(Game.Starfield);
        result.Game.ShouldBe(Game.Starfield);

        _mockProjectRepository.Verify(repo => repo.GetCurrentProjectAsync(), Times.Once);
        _mockProjectRepository.Verify(repo => repo.SaveCurrentProjectAsync(result), Times.Once);
    }

    [Fact]
    public async Task ProjectService_AddSourceFileAsync_AddsSourceFile()
    {
        var mechanicProject = new MechanicProject
        {
            Id = "com.example.MyProject",
            Game = Game.SkyrimSpecialEdition
        };
        _mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(mechanicProject)!);

        var pathToFileTiff = "path/to/file.tiff";
        var sourceFileType = SourceFileType.Tiff;

        var result = await _projectService.AddSourceFileAsync(pathToFileTiff, sourceFileType);

        result.Match(
            Right: file =>
            {
                file.ShouldNotBeNull();
                file.Path.ShouldBe(pathToFileTiff);
                file.FileType.ShouldBe(sourceFileType);
            },
            Left: error => throw new ShouldAssertException($"Expected success but got failure {error}")
        );

        _mockProjectRepository.Verify(repo => repo.GetCurrentProjectAsync(), Times.Once);
        _mockProjectRepository.Verify(repo => repo.SaveCurrentProjectAsync(mechanicProject), Times.Once);
    }
    
    [Fact]
    public async Task ProjectService_AddSourceFileAsync_AddsSourceFileWithId()
    {
        var existingGameFileId = Guid.NewGuid();
        var mechanicProject = new MechanicProject
        {
            Id = "com.example.MyProject",
            Game = Game.SkyrimSpecialEdition,
            GameFiles = [
                new GameFile
                {
                    Id = existingGameFileId,
                    Path = "path/to/file.dds"
                }
            ]
        };
        _mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(mechanicProject)!);

        var pathToFileTiff = "path/to/file.tiff";
        var sourceFileType = SourceFileType.Tiff;

        var result = await _projectService.AddSourceFileAsync(pathToFileTiff, sourceFileType, existingGameFileId);

        result.Match(
            Right: file =>
            {
                file.ShouldNotBeNull();
                file.Path.ShouldBe(pathToFileTiff);
                file.FileType.ShouldBe(sourceFileType);
            },
            Left: error => throw new ShouldAssertException($"Expected success but got failure {error}")
        );

        _mockProjectRepository.Verify(repo => repo.GetCurrentProjectAsync(), Times.Once);
        _mockProjectRepository.Verify(repo => repo.SaveCurrentProjectAsync(mechanicProject), Times.Once);
    }
    
    [Fact]
    public async Task ProjectService_AddSourceFileAsync_AddsSourceFileWithBadIdGetsError()
    {
        var existingGameFileId = Guid.NewGuid();
        var mechanicProject = new MechanicProject
        {
            Id = "com.example.MyProject",
            Game = Game.SkyrimSpecialEdition,
            GameFiles = [
                new GameFile
                {
                    Id = existingGameFileId,
                    Path = "path/to/file.dds"
                }
            ]
        };
        _mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(mechanicProject)!);

        var pathToFileTiff = "path/to/file.tiff";
        var sourceFileType = SourceFileType.Tiff;

        var result = await _projectService.AddSourceFileAsync(pathToFileTiff, sourceFileType, new Guid());

        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        result.Match(
            Right: file => throw new ShouldAssertException($"Expected failure, but was successful"),
            Left: error => error.ShouldBeOfType<LinkedFileDoesNotExistError>()
        );

        _mockProjectRepository.Verify(repo => repo.GetCurrentProjectAsync(), Times.Once);
        _mockProjectRepository.Verify(repo => repo.SaveCurrentProjectAsync(mechanicProject), Times.Never);
    }
}