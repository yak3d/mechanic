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
    private const string TestGamePath = @"C:\Program Files (x86)\Steam\steamapps\common\Mechanic\Mechanic.exe";
    private readonly Mock<ILogger<ProjectService>> _mockLogger;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<IProjectRepository> _mockProjectRepository;
    private readonly ProjectService _projectService;

    public ProjectServiceTest()
    {
        _mockLogger = new Mock<ILogger<ProjectService>>();
        _mockFileService = new Mock<IFileService>();
        _mockProjectRepository = new Mock<IProjectRepository>();
        _projectService = new ProjectService(_mockLogger.Object, _mockFileService.Object, _mockProjectRepository.Object);
    }

    [Fact]
    public async Task ProjectService_InitializeAsync_CreatesProject()
    {
        var path = "mechanic.json";
        var projectId = "com.example.MyProject";
        var game = GameName.SkyrimSpecialEdition;

        _mockProjectRepository.Setup(repo => repo.InitializeProjectAsync(projectId, game, TestGamePath));
        _mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(new MechanicProject
        {
            Id = projectId,
            GameName = game,
            GamePath = TestGamePath
        })!);

        var result = await _projectService.InitializeAsync(path, projectId, game, TestGamePath);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(projectId);
        result.GameName.ShouldBe(game);
        result.GamePath.ShouldBe(TestGamePath);
        result.GameFiles.ShouldBeEmpty();
        result.SourceFiles.ShouldBeEmpty();

        _mockProjectRepository.Verify(repo => repo.InitializeProjectAsync(projectId, game, TestGamePath), Times.Once);
        _mockProjectRepository.Verify(repo => repo.GetCurrentProjectAsync(), Times.Once);
    }

    [Fact]
    public async Task ProjectService_InitializeAsync_ThrowsException_IfProjectExists()
    {
        var path = "mechanic.json";
        var projectId = "com.example.MyProject";
        var game = GameName.SkyrimSpecialEdition;

        _mockProjectRepository.Setup(repo => repo.InitializeProjectAsync(projectId, game, TestGamePath)).Throws<InvalidOperationException>();

        await _projectService.InitializeAsync(path, projectId, game, TestGamePath).ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ProjectService_GetCurrentProjectAsync_ReturnsProject()
    {
        _mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(new MechanicProject
        {
            Id = "com.example.MyProject",
            GameName = GameName.SkyrimSpecialEdition,
            GamePath = TestGamePath
        })!);

        var result = await _projectService.GetCurrentProjectAsync();
        result.ShouldNotBeNull();
        result.Id.ShouldBe("com.example.MyProject");
        result.GameName.ShouldBe(GameName.SkyrimSpecialEdition);
        result.GamePath.ShouldBe(TestGamePath);
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
            GameName = GameName.SkyrimSpecialEdition,
            GamePath = TestGamePath
        })!);

        var result = await _projectService.UpdateProjectGameAsync(GameName.Starfield);
        result.GameName.ShouldBe(GameName.Starfield);

        _mockProjectRepository.Verify(repo => repo.GetCurrentProjectAsync(), Times.Once);
        _mockProjectRepository.Verify(repo => repo.SaveCurrentProjectAsync(result), Times.Once);
    }

    [Fact]
    public async Task ProjectService_AddSourceFileAsync_AddsSourceFile()
    {
        var mechanicProject = new MechanicProject
        {
            Id = "com.example.MyProject",
            GameName = GameName.SkyrimSpecialEdition,
            GamePath = TestGamePath
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
            GameName = GameName.SkyrimSpecialEdition,
            GamePath = TestGamePath,
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
            GameName = GameName.SkyrimSpecialEdition,
            GamePath = TestGamePath,
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