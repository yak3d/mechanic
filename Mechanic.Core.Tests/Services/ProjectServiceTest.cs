using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Mechanic.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Mechanic.Core.Tests.Services;

public class ProjectServiceTest
{
        private readonly Mock<ILogger<ProjectService>> _mockLogger;
    private readonly Mock<IProjectSerializationService<string>> _mockSerializationService;
    private readonly ProjectService _projectService;

    public ProjectServiceTest()
    {
        _mockLogger = new Mock<ILogger<ProjectService>>();
        _mockSerializationService = new Mock<IProjectSerializationService<string>>();
        _projectService = new ProjectService(_mockLogger.Object, _mockSerializationService.Object);
    }

    [Fact]
    public void Initialize_ShouldReturnMechanicProjectWithCorrectProperties()
    {
        var path = "/test/path";
        var projectId = "test-project-id";
        var game = Game.Tes5Skyrim;

        var result = _projectService.Initialize(path, projectId, game);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(projectId);
        result.Game.ShouldBe(game);
    }

    [Theory]
    [InlineData("project-1", "/path/one", Game.Tes4Oblivion)]
    [InlineData("project-2", "/path/two", Game.Tes5Skyrim)]
    [InlineData("project-3", "/path/three", Game.SkyrimSpecialEdition)]
    [InlineData("project-4", "/path/four", Game.Fallout3)]
    [InlineData("project-5", "/path/five", Game.FalloutNewVegas)]
    [InlineData("project-6", "/path/six", Game.Fallout4)]
    [InlineData("project-7", "/path/seven", Game.Starfield)]
    public void Initialize_WithDifferentParameters_ShouldReturnCorrectProject(string projectId, string path, Game game)
    {
        var result = _projectService.Initialize(path, projectId, game);

        result.Id.ShouldBe(projectId);
        result.Game.ShouldBe(game);
    }

    [Fact]
    public void Initialize_ShouldCallSerializationServiceWithCorrectParameters()
    {
        var path = "/test/path";
        var projectId = "test-project-id";
        var game = Game.Fallout4;

        _projectService.Initialize(path, projectId, game);

        _mockSerializationService.Verify(
            x => x.SerializeProject(
                It.Is<MechanicProject>(p => p.Id == projectId && p.Game == game),
                path),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Initialize_WithEmptyOrNullProjectId_ShouldStillCreateProject(string projectId)
    {
        var path = "/test/path";
        var game = Game.Tes5Skyrim;

        var result = _projectService.Initialize(path, projectId, game);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(projectId);
        result.Game.ShouldBe(game);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Initialize_WithEmptyOrNullPath_ShouldStillCallSerializationService(string path)
    {
        var projectId = "test-project-id";
        var game = Game.Fallout3;

        _projectService.Initialize(path, projectId, game);

        _mockSerializationService.Verify(
            x => x.SerializeProject(It.IsAny<MechanicProject>(), path),
            Times.Once);
    }

    [Fact]
    public void Initialize_WhenSerializationServiceThrows_ShouldNotCatchException()
    {
        var path = "/test/path";
        var projectId = "test-project-id";
        var game = Game.Tes5Skyrim;
        var expectedException = new InvalidOperationException("Serialization failed");

        _mockSerializationService
            .Setup(x => x.SerializeProject(It.IsAny<MechanicProject>(), It.IsAny<string>()))
            .Throws(expectedException);

        Should.Throw<InvalidOperationException>(() => _projectService.Initialize(path, projectId, game))
            .ShouldBe(expectedException);
    }

    [Fact]
    public void Initialize_ShouldCreateNewProjectInstanceEachTime()
    {
        var path = "/test/path";
        var projectId = "test-project-id";
        var game = Game.Tes5Skyrim;

        var result1 = _projectService.Initialize(path, projectId, game);
        var result2 = _projectService.Initialize(path, projectId, game);

        result1.ShouldNotBeSameAs(result2);
        result1.Id.ShouldBe(result2.Id);
        result1.Game.ShouldBe(result2.Game);
    }
}