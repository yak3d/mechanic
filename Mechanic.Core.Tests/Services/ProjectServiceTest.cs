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
    private readonly Mock<IProjectRepository> mockProjectRepository;
    private readonly ProjectService projectService;
    private const string TestSourcePath = "./Source";

    public ProjectServiceTest()
    {
        Mock<ILogger<ProjectService>> mockLogger = new();
        Mock<IFileService> mockFileService = new();
        this.mockProjectRepository = new Mock<IProjectRepository>();
        this.projectService = new ProjectService(mockLogger.Object, mockFileService.Object, this.mockProjectRepository.Object, TestSourcePath);
    }

    [Fact]
    public async Task ProjectService_InitializeAsync_CreatesProject()
    {
        var path = "mechanic.json";
        var projectId = "com.example.MyProject";
        var game = GameName.SkyrimSpecialEdition;
        var settings = new ProjectSettingsBuilder().EnablePyro().Build();

        this.mockProjectRepository.Setup(repo => repo.InitializeProjectAsync(projectId, "TEST", game, settings, TestGamePath, default, default));
        this.mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(new MechanicProject
        {
            Id = projectId,
            GameName = game,
            ProjectSettings = settings,
            GamePath = TestGamePath,
            Namespace = "TEST"
        })!);

        var result = await this.projectService.InitializeAsync(path, projectId, "TEST", game, settings, TestGamePath, [], []);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(projectId);
        result.GameName.ShouldBe(game);
        result.GamePath.ShouldBe(TestGamePath);
        result.GameFiles.ShouldBeEmpty();
        result.SourceFiles.ShouldBeEmpty();

        this.mockProjectRepository.Verify(repo => repo.InitializeProjectAsync(projectId, "TEST", game, settings, TestGamePath, It.IsAny<List<SourceFile>>(), It.IsAny<List<GameFile>>()), Times.Once);
        this.mockProjectRepository.Verify(repo => repo.GetCurrentProjectAsync(), Times.Once);
    }

    [Fact]
    public async Task ProjectService_InitializeAsync_ThrowsException_IfProjectExists()
    {
        var path = "mechanic.json";
        var projectId = "com.example.MyProject";
        var game = GameName.SkyrimSpecialEdition;
        var settings = new ProjectSettingsBuilder().EnablePyro().Build();

        this.mockProjectRepository.Setup(repo => repo.InitializeProjectAsync(projectId, "TEST", game, settings, TestGamePath, default, default)).Throws<InvalidOperationException>();

        await this.projectService.InitializeAsync(path, projectId, "TEST", game, settings, TestGamePath, [], []).ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ProjectService_GetCurrentProjectAsync_ReturnsProject()
    {
        this.mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(new MechanicProject
        {
            Id = "com.example.MyProject",
            GameName = GameName.SkyrimSpecialEdition,
            ProjectSettings = new ProjectSettingsBuilder().EnablePyro()
                .Build(),
            GamePath = TestGamePath,
            Namespace = "TEST"
        })!);

        var result = await this.projectService.GetCurrentProjectAsync();
        result.ShouldNotBeNull();
        result.Id.ShouldBe("com.example.MyProject");
        result.GameName.ShouldBe(GameName.SkyrimSpecialEdition);
        result.GamePath.ShouldBe(TestGamePath);
    }

    [Fact]
    public async Task ProjectService_GetCurrentProjectAsync_ThrowsWhenProjectDoesntExist()
    {
        this.mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult<MechanicProject?>(null));

        await this.projectService.GetCurrentProjectAsync().ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ProjectService_UpdateProjectGameAsync_UpdatesProjectGame()
    {
        this.mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(result: new MechanicProject
        {
            Id = "com.example.MyProject",
            GameName = GameName.SkyrimSpecialEdition,
            ProjectSettings = new ProjectSettingsBuilder().EnablePyro()
                .Build(),
            GamePath = TestGamePath,
            Namespace = "TEST"
        })!);

        var result = await this.projectService.UpdateProjectGameAsync(GameName.Starfield);
        result.GameName.ShouldBe(GameName.Starfield);

        this.mockProjectRepository.Verify(repo => repo.GetCurrentProjectAsync(), Times.Once);
        this.mockProjectRepository.Verify(repo => repo.SaveCurrentProjectAsync(result), Times.Once);
    }

    [Fact]
    public async Task ProjectService_AddSourceFileAsync_AddsSourceFile()
    {
        var mechanicProject = new MechanicProject
        {
            Id = "com.example.MyProject",
            GameName = GameName.SkyrimSpecialEdition,
            ProjectSettings = new ProjectSettingsBuilder().EnablePyro()
                .Build(),
            GamePath = TestGamePath,
            Namespace = "TEST"
        };
        this.mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(mechanicProject)!);

        var pathToFileTiff = "./Source/path/to/file.tiff";
        var sourceFileType = SourceFileType.Tiff;

        var result = await this.projectService.AddSourceFileAsync(pathToFileTiff, sourceFileType);

        result.Match(
            Right: file =>
            {
                file.ShouldNotBeNull();
                file.Path.ShouldBe(@"path\to\file.tiff");
                file.FileType.ShouldBe(sourceFileType);
            },
            Left: error => throw new ShouldAssertException($"Expected success but got failure {error}")
        );

        this.mockProjectRepository.Verify(repo => repo.GetCurrentProjectAsync(), Times.Once);
        this.mockProjectRepository.Verify(repo => repo.SaveCurrentProjectAsync(mechanicProject), Times.Once);
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
            ProjectSettings = new ProjectSettingsBuilder().EnablePyro()
                .Build(),
            GameFiles =
            [
                new GameFile
                {
                    Id = existingGameFileId,
                    Path = "path/to/file.dds"
                }
            ],
            Namespace = "TEST"
        };
        this.mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(mechanicProject)!);

        var pathToFileTiff = "./Source/path/to/file.tiff";
        var sourceFileType = SourceFileType.Tiff;

        var result = await this.projectService.AddSourceFileAsync(pathToFileTiff, sourceFileType, existingGameFileId);

        result.Match(
            Right: file =>
            {
                file.ShouldNotBeNull();
                file.Path.ShouldBe(@"path\to\file.tiff");
                file.FileType.ShouldBe(sourceFileType);
            },
            Left: error => throw new ShouldAssertException($"Expected success but got failure {error}")
        );

        this.mockProjectRepository.Verify(repo => repo.GetCurrentProjectAsync(), Times.Once);
        this.mockProjectRepository.Verify(repo => repo.SaveCurrentProjectAsync(mechanicProject), Times.Once);
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
            ProjectSettings = new ProjectSettingsBuilder().EnablePyro()
                .Build(),
            GameFiles =
            [
                new GameFile
                {
                    Id = existingGameFileId,
                    Path = "path/to/file.dds"
                }
            ],
            Namespace = "TEST"
        };
        this.mockProjectRepository.Setup(repo => repo.GetCurrentProjectAsync()).Returns(Task.FromResult(mechanicProject)!);

        var pathToFileTiff = "path/to/file.tiff";
        var sourceFileType = SourceFileType.Tiff;

        var result = await this.projectService.AddSourceFileAsync(pathToFileTiff, sourceFileType, new Guid());

        Assert.IsType<LinkedFileDoesNotExistError>(result.Match(
            Right: _ => throw new ShouldAssertException("Expected failure, but was successful"),
            Left: error => error.ShouldBeOfType<LinkedFileDoesNotExistError>()
        ));

        this.mockProjectRepository.Verify(repo => repo.GetCurrentProjectAsync(), Times.Once);
        this.mockProjectRepository.Verify(repo => repo.SaveCurrentProjectAsync(mechanicProject), Times.Never);
    }
}
