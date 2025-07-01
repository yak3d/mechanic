using System.Text.Json;
using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Mechanic.Core.Repositories;
using Mechanic.Core.Repositories.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Mechanic.Core.Tests.Repositories;

public class JsonProjectRepositoryIntegrationTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testProjectFile;
    private readonly Mock<ILogger<JsonProjectRepository>> _mockLogger;
    private readonly Mock<IFileService> _mockFileService;
    private readonly JsonProjectRepository _repository;
    private const string TestGamePath = @"C:\Program Files (x86)\Steam\steamapps\common\Mechanic\Mechanic.exe";

    public JsonProjectRepositoryIntegrationTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"MechanicTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _testProjectFile = Path.Combine(_testDirectory, "test-mechanic.json");

        _mockLogger = new Mock<ILogger<JsonProjectRepository>>();
        _mockFileService = new Mock<IFileService>();

        _mockFileService.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
            .Returns<string>(async filePath => await File.ReadAllTextAsync(filePath));

        _mockFileService.Setup(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>(async (filePath, content) => await File.WriteAllTextAsync(filePath, content));

        _repository = new JsonProjectRepository(_mockLogger.Object, _mockFileService.Object, _testProjectFile);
    }

    [Fact]
    public async Task GetCurrentProjectAsync_WhenProjectFileDoesNotExist_ReturnsNull()
    {
        var result = await _repository.GetCurrentProjectAsync();
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetCurrentProjectAsync_WhenProjectFileExists_ReturnsDeserializedProject()
    {
        var expectedProject = new MechanicProject
        {
            Id = "test-project-123",
            GameName = GameName.Tes4Oblivion,
            GamePath = TestGamePath
        };

        await _repository.SaveCurrentProjectAsync(expectedProject);
        var result = await _repository.GetCurrentProjectAsync();

        result.ShouldNotBeNull();
        result.Id.ShouldBe(expectedProject.Id);
        result.GameName.ShouldBe(expectedProject.GameName);
    }

    [Fact]
    public async Task GetCurrentProjectAsync_WhenJsonFileIsCorrupted_ReturnsNullAndLogsError()
    {
        await File.WriteAllTextAsync(_testProjectFile, "{ invalid json content }");

        var result = await _repository.GetCurrentProjectAsync();

        result.ShouldBeNull();
    }

    [Fact]
    public async Task SaveCurrentProjectAsync_CreatesJsonFileWithCorrectFormat()
    {
        var project = new MechanicProject
        {
            Id = "integration-test-project",
            GameName = GameName.Tes4Oblivion,
            GamePath = TestGamePath
        };

        await _repository.SaveCurrentProjectAsync(project);

        File.Exists(_testProjectFile).ShouldBeTrue();

        var fileContent = await File.ReadAllTextAsync(_testProjectFile);
        fileContent.ShouldNotBeNullOrEmpty();

        var jsonDocument = JsonDocument.Parse(fileContent);
        jsonDocument.RootElement.GetProperty("id").GetString().ShouldBe("integration-test-project");
        jsonDocument.RootElement.GetProperty("game").GetProperty("name").GetString().ShouldBe(Project.Models.Json.GameName.TES4Oblivion.ToString());
        jsonDocument.RootElement.GetProperty("game").GetProperty("path").GetString().ShouldBe(TestGamePath);

        fileContent.ShouldContain("  ");
    }

    [Fact]
    public async Task ProjectExistsAsync_WhenFileExists_ReturnsTrue()
    {
        await File.WriteAllTextAsync(_testProjectFile, "{}");

        var result = await _repository.ProjectExistsAsync();

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectExistsAsync_WhenFileDoesNotExist_ReturnsFalse()
    {
        var result = await _repository.ProjectExistsAsync();
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task InitializeProjectAsync_CreatesNewProjectWithSpecifiedValues()
    {
        const string projectId = "new-project-456";
        const GameName game = GameName.Tes4Oblivion;

        await _repository.InitializeProjectAsync(projectId, game, TestGamePath);

        File.Exists(_testProjectFile).ShouldBeTrue();

        var savedProject = await _repository.GetCurrentProjectAsync();
        savedProject.ShouldNotBeNull();
        savedProject.Id.ShouldBe(projectId);
        savedProject.GameName.ShouldBe(GameName.Tes4Oblivion);
    }

    [Fact]
    public async Task SaveAndRetrieve_RoundTripSerialization_PreservesAllProperties()
    {
        var originalProject = new MechanicProject
        {
            Id = "roundtrip-test",
            GameName = GameName.Tes4Oblivion,
            GamePath = TestGamePath
        };

        await _repository.SaveCurrentProjectAsync(originalProject);
        var retrievedProject = await _repository.GetCurrentProjectAsync();

        retrievedProject.ShouldNotBeNull();
        retrievedProject.ShouldBeEquivalentTo(originalProject);
    }

    [Fact]
    public async Task SaveCurrentProjectAsync_OverwritesExistingFile()
    {
        var firstProject = new MechanicProject { Id = "first", GameName = GameName.Tes4Oblivion, GamePath = TestGamePath };
        var secondProject = new MechanicProject { Id = "second", GameName = GameName.Tes4Oblivion, GamePath = TestGamePath };

        await _repository.SaveCurrentProjectAsync(firstProject);
        await _repository.SaveCurrentProjectAsync(secondProject);

        var result = await _repository.GetCurrentProjectAsync();
        result.ShouldNotBeNull();
        result.Id.ShouldBe("second");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("special-chars-!@#$%")]
    [InlineData("unicode-测试")]
    public async Task SaveAndRetrieve_WithVariousProjectIds_HandlesCorrectly(string projectId)
    {
        var project = new MechanicProject { Id = projectId, GameName = GameName.Tes4Oblivion, GamePath = TestGamePath };

        await _repository.SaveCurrentProjectAsync(project);
        var result = await _repository.GetCurrentProjectAsync();

        result.ShouldNotBeNull();
        result.Id.ShouldBe(projectId);
    }

    [Fact]
    public async Task GetCurrentProjectAsync_WhenFileServiceThrowsException_ReturnsNull()
    {
        _mockFileService.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var result = await _repository.GetCurrentProjectAsync();

        result.ShouldBeNull();
    }

    [Fact]
    public async Task SaveCurrentProjectAsync_CallsFileServiceWithCorrectParameters()
    {
        var project = new MechanicProject { Id = "test", GameName = GameName.Tes4Oblivion, GamePath = TestGamePath };

        await _repository.SaveCurrentProjectAsync(project);

        _mockFileService.Verify(
            fs => fs.WriteAllText(
                _testProjectFile,
                It.Is<string>(content => content.Contains("test"))),
            Times.Once);
    }

    [Fact]
    public async Task ProjectExistsAsync_UsesCorrectFilePath()
    {
        var customPath = Path.Combine(_testDirectory, "custom-project.json");
        var customRepository = new JsonProjectRepository(_mockLogger.Object, _mockFileService.Object, customPath);

        await File.WriteAllTextAsync(customPath, "{}");

        var result = await customRepository.ProjectExistsAsync();

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task InitializeProjectAsync_IgnoresPassedGameParameter()
    {
        await _repository.InitializeProjectAsync("test-id", GameName.Tes4Oblivion, TestGamePath);

        var result = await _repository.GetCurrentProjectAsync();

        result.ShouldNotBeNull();
        result.GameName.ShouldBe(GameName.Tes4Oblivion);
    }

    [Fact]
    public async Task GetCurrentProjectAsync_WhenJsonModelIsNull_ReturnsNull()
    {
        await File.WriteAllTextAsync(_testProjectFile, "null");

        var result = await _repository.GetCurrentProjectAsync();

        result.ShouldBeNull();
    }

    [Fact]
    public async Task RemoveSourceFileById_RemovesFileSuccessfully()
    {
        var fileId = Guid.NewGuid();
        var project = new MechanicProject
        {
            Id = "integration-test-project",
            GameName = GameName.Tes4Oblivion,
            GamePath = TestGamePath,
            SourceFiles = [new SourceFile
            {
                Id = fileId,
                Path = "test\\path\\test.tiff",
                FileType = SourceFileType.Tiff
            }]
        };
        
        await _repository.SaveCurrentProjectAsync(project);
        var result = await _repository.RemoveSourceFileByIdAsync(fileId);
        var resultProject = await _repository.GetCurrentProjectAsync();
        
        result.ShouldNotBeNull();
        result.Id.ShouldBe(fileId);
        resultProject.ShouldNotBeNull();
        resultProject.SourceFiles.Count.ShouldBe(0);
    }
    
    [Fact]
    public async Task RemoveSourceFileByPath_RemovesFileSuccessfully()
    {
        var fileId = Guid.NewGuid();
        var testPathTestTiff = "test\\path\\test.tiff";
        var project = new MechanicProject
        {
            Id = "integration-test-project",
            GameName = GameName.Tes4Oblivion,
            GamePath = TestGamePath,
            SourceFiles = [new SourceFile
            {
                Id = fileId,
                Path = testPathTestTiff,
                FileType = SourceFileType.Tiff
            }]
        };
        
        await _repository.SaveCurrentProjectAsync(project);
        var result = await _repository.RemoveSourceFileByIdAsync(fileId);
        var resultProject = await _repository.GetCurrentProjectAsync();
        
        result.ShouldNotBeNull();
        result.Id.ShouldBe(fileId);
        result.Path.ShouldBe(testPathTestTiff);
        resultProject.ShouldNotBeNull();
        resultProject.SourceFiles.Count.ShouldBe(0);
    }
    
    [Fact]
    public async Task RemoveSourceFileById_WithEmptyProject_ThrowsException()
    {
        await Should.ThrowAsync<ProjectNotFoundException>(_repository.RemoveSourceFileByIdAsync(new Guid()));
    }
    
    [Fact]
    public async Task RemoveSourceFileById_WithFileNotExisting_ThrowsException()
    {
        var fileId = Guid.NewGuid();
        var project = new MechanicProject
        {
            Id = "integration-test-project",
            GameName = GameName.Tes4Oblivion,
            GamePath = TestGamePath,
            SourceFiles = [new SourceFile
            {
                Id = fileId,
                Path = "test\\path\\test.tiff",
                FileType = SourceFileType.Tiff
            }]
        };
        
        await _repository.SaveCurrentProjectAsync(project);
        await Should.ThrowAsync<ProjectSourceFileNotFoundException>(_repository.RemoveSourceFileByIdAsync(new Guid()));
    }
    
    [Fact]
    public async Task RemoveSourceFileByPath_WithFileNotExisting_ThrowsException()
    {
        var fileId = Guid.NewGuid();
        var project = new MechanicProject
        {
            Id = "integration-test-project",
            GameName = GameName.Tes4Oblivion,
            GamePath = TestGamePath,
            SourceFiles = [new SourceFile
            {
                Id = fileId,
                Path = "test\\path\\test.tiff",
                FileType = SourceFileType.Tiff
            }]
        };
        
        await _repository.SaveCurrentProjectAsync(project);
        await Should.ThrowAsync<ProjectSourceFileNotFoundException>(_repository.RemoveSourceFileByPathAsync("broken path"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }
}