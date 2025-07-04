using System.Text.Json;
using Mechanic.CLI.Contracts;
using Mechanic.CLI.Models.Settings;
using Mechanic.CLI.Services;
using Mechanic.Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;
using LanguageExt;
using Mechanic.CLI.Errors;

namespace Mechanic.CLI.Tests.Services;

public class JsonLocalSettingsServiceTest
{
    private readonly Mock<IFileService> _mockFileService;
    private readonly JsonLocalSettingsService _service;
    private readonly string _expectedConfigPath;

    public JsonLocalSettingsServiceTest()
    {
        Mock<ILogger<JsonLocalSettingsService>> mockLogger = new();
        _mockFileService = new Mock<IFileService>();
        Mock<IOptions<LocalSettingsOptions>> mockOptions = new();
        
        mockOptions.Setup(x => x.Value).Returns(new LocalSettingsOptions(
            SpriggitPath: null,
            PyroPath: null
        ));
        
        _expectedConfigPath = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mechanic", "config", "config.json")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "mechanic", "config.json");
        
        _service = new JsonLocalSettingsService(mockLogger.Object, _mockFileService.Object, mockOptions.Object);
    }

    #region ReadSettingAsync Tests

    [Fact]
    public async Task ReadSettingAsync_WhenFileDoesNotExist_ShouldReturnDefault()
    {
        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ThrowsAsync(new FileNotFoundException());

        var result = await _service.ReadSettingAsync<string>("testKey");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ReadSettingAsync_WhenFileIsEmpty_ShouldReturnDefault()
    {
        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync(string.Empty);

        var result = await _service.ReadSettingAsync<string>("testKey");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ReadSettingAsync_WhenFileIsWhitespace_ShouldReturnDefault()
    {
        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync("   \n\t   ");

        var result = await _service.ReadSettingAsync<string>("testKey");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ReadSettingAsync_WhenKeyNotFound_ShouldReturnDefault()
    {
        var jsonContent = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["otherKey"] = "otherValue"
        });
        
        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync(jsonContent);

        var result = await _service.ReadSettingAsync<string>("testKey");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ReadSettingAsync_WhenKeyExistsAsJsonElement_ShouldReturnDeserializedValue()
    {
        var testValue = new { Name = "TestName", Age = 25 };
        var jsonContent = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["testKey"] = testValue
        });
        
        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync(jsonContent);

        var result = await _service.ReadSettingAsync<JsonElement>("testKey");

        result.ValueKind.ShouldNotBe(JsonValueKind.Undefined);
        result.ValueKind.ShouldNotBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task ReadSettingAsync_WhenKeyExistsAsString_ShouldReturnDeserializedValue()
    {
        var testObject = new { Name = "TestName", Age = 25 };
        var serializedTestObject = JsonSerializer.Serialize(testObject);
        var jsonContent = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["testKey"] = serializedTestObject
        });
        
        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync(jsonContent);

        var result = await _service.ReadSettingAsync<string>("testKey");

        result.ShouldBe(serializedTestObject);
    }

    [Fact]
    public async Task ReadSettingAsync_WhenJsonIsCorrupted_ShouldReturnDefault()
    {
        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync("{ invalid json content");

        var result = await _service.ReadSettingAsync<string>("testKey");

        result.ShouldBeNull();
    }

    [Theory]
    [InlineData("stringValue", "testKey")]
    [InlineData(42, "intKey")]
    [InlineData(true, "boolKey")]
    public async Task ReadSettingAsync_WithDifferentTypes_ShouldReturnCorrectValues<T>(T expectedValue, string key)
    {
        var jsonContent = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            [key] = expectedValue
        });
        
        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync(jsonContent);

        var result = await _service.ReadSettingAsync<T>(key);

        result.ShouldBe(expectedValue);
    }

    #endregion

    #region SaveSettingAsync Tests

    [Fact]
    public async Task SaveSettingAsync_WhenDirectoryDoesNotExist_ShouldCreateDirectory_AndReturnNone()
    {
        var testKey = "testKey";
        var testValue = "testValue";
        var expectedDirectory = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mechanic", "config")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "mechanic");

        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync("{}");
        _mockFileService.Setup(x => x.DirectoryExists(expectedDirectory))
            .ReturnsAsync(false);

        var result = await _service.SaveSettingAsync(testKey, testValue);

        result.IsNone.ShouldBeTrue();
        _mockFileService.Verify(x => x.DirectoryExists(expectedDirectory), Times.Once);
    }

    [Fact]
    public async Task SaveSettingAsync_ShouldSerializeAndWriteToFile_AndReturnNone()
    {
        var testKey = "testKey";
        var testValue = "testValue";
        var expectedDirectory = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mechanic", "config")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "mechanic");

        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync("{}");
        _mockFileService.Setup(x => x.DirectoryExists(expectedDirectory))
            .ReturnsAsync(true);

        var result = await _service.SaveSettingAsync(testKey, testValue);

        result.IsNone.ShouldBeTrue();
        _mockFileService.Verify(x => x.WriteAllText(_expectedConfigPath, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SaveSettingAsync_WhenExistingSettings_ShouldMergeWithExisting_AndReturnNone()
    {
        var existingSettings = new Dictionary<string, object>
        {
            ["existingKey"] = "existingValue"
        };
        var existingJson = JsonSerializer.Serialize(existingSettings);
        
        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync(existingJson);
        _mockFileService.Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .ReturnsAsync(true);

        var capturedJson = string.Empty;
        _mockFileService.Setup(x => x.WriteAllText(_expectedConfigPath, It.IsAny<string>()))
            .Callback<string, string>((_, content) => capturedJson = content);

        var result = await _service.SaveSettingAsync("newKey", "newValue");

        result.IsNone.ShouldBeTrue();
        _mockFileService.Verify(x => x.WriteAllText(_expectedConfigPath, It.IsAny<string>()), Times.Once);
        capturedJson.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task SaveSettingAsync_WhenFileServiceThrowsException_ShouldReturnError()
    {
        var testKey = "testKey";
        var testValue = "testValue";
        
        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied to config"));
        _mockFileService.Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.WriteAllText(_expectedConfigPath, It.IsAny<string>()))
            .ThrowsAsync(new UnauthorizedAccessException("Write access denied"));

        var result = await _service.SaveSettingAsync(testKey, testValue);

        result.IsSome.ShouldBeTrue();
        result.Match(
            Some: error => error.ShouldBeOfType<FailedToSaveSettingsError>(),
            None: () => throw new Exception("Expected error but got None")
        );
    }

    [Theory]
    [InlineData("stringValue")]
    [InlineData(42)]
    [InlineData(true)]
    [InlineData(3.14)]
    public async Task SaveSettingAsync_WithDifferentTypes_ShouldSerializeCorrectly_AndReturnNone<T>(T value)
    {
        var testKey = "testKey";
        
        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync("{}");
        _mockFileService.Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .ReturnsAsync(true);

        var result = await _service.SaveSettingAsync(testKey, value);

        result.IsNone.ShouldBeTrue();
        _mockFileService.Verify(x => x.WriteAllText(_expectedConfigPath, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SaveSettingAsync_WhenDirectoryCreationFails_ShouldReturnError()
    {
        var testKey = "testKey";
        var testValue = "testValue";
        var expectedDirectory = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mechanic", "config")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "mechanic");

        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync("{}");
        _mockFileService.Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockFileService.Setup(x => x.CreateDirectoryAsync(expectedDirectory))
            .ThrowsAsync(new UnauthorizedAccessException("Cannot create directory"));

        var result = await _service.SaveSettingAsync(testKey, testValue);

        result.IsSome.ShouldBeTrue();
        result.Match(
            Some: error => error.ShouldBeOfType<FailedToMakeSettingsDirectoryError>(),
            None: () => throw new Exception("Expected error but got None")
        );
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task SaveAndRead_ShouldMaintainDataIntegrity_AndReturnProperResults()
    {
        var testKey = "integrationTestKey";
        var testValue = "simple test value";
        var settings = new Dictionary<string, object>();
        
        _mockFileService.Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .ReturnsAsync(true);
        
        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync(() => JsonSerializer.Serialize(settings));
        
        _mockFileService.Setup(x => x.WriteAllText(_expectedConfigPath, It.IsAny<string>()))
            .Callback<string, string>((_, content) => 
            {
                settings[testKey] = JsonSerializer.Serialize(testValue);
            });

        var saveResult = await _service.SaveSettingAsync(testKey, testValue);
        var readResult = await _service.ReadSettingAsync<string>(testKey);

        saveResult.IsNone.ShouldBeTrue();
        readResult.ShouldBe($"\"{testValue}\"");
        _mockFileService.Verify(x => x.WriteAllText(_expectedConfigPath, It.IsAny<string>()), Times.Once);
        _mockFileService.Verify(x => x.ReadAllText(_expectedConfigPath), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SaveAndRead_WhenSaveFails_ShouldHandleErrorsGracefully()
    {
        var testKey = "failureTestKey";
        var testValue = "test value that shall not pass";
        
        _mockFileService.Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllText(_expectedConfigPath))
            .ReturnsAsync("{}");
        _mockFileService.Setup(x => x.WriteAllText(_expectedConfigPath, It.IsAny<string>()))
            .ThrowsAsync(new IOException("Unable to write to file"));

        var saveResult = await _service.SaveSettingAsync(testKey, testValue);
        
        saveResult.IsSome.ShouldBeTrue();
        saveResult.Match(
            Some: error => 
            {
                error.ShouldBeOfType<FailedToSaveSettingsError>();
            },
            None: () => throw new Exception("Expected error but operation succeeded")
        );
    }

    #endregion
}