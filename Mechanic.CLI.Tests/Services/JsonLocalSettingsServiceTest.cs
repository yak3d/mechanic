using System.Text.Json;
using Mechanic.CLI.Models.Settings;
using Mechanic.CLI.Services;
using Mechanic.Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Mechanic.CLI.Errors;

namespace Mechanic.CLI.Tests.Services;

public class JsonLocalSettingsServiceTest
{
    private readonly Mock<IFileService> mockFileService;
    private readonly JsonLocalSettingsService service;
    private readonly string expectedConfigPath;

    public JsonLocalSettingsServiceTest()
    {
        Mock<ILogger<JsonLocalSettingsService>> mockLogger = new();
        this.mockFileService = new Mock<IFileService>();
        Mock<IOptions<LocalSettingsOptions>> mockOptions = new();

        mockOptions.Setup(x => x.Value).Returns(new LocalSettingsOptions(
            SpriggitPath: null,
            PyroPath: null
        ));

        this.expectedConfigPath = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mechanic", "config", "config.json")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "mechanic", "config.json");

        this.service = new JsonLocalSettingsService(mockLogger.Object, this.mockFileService.Object, mockOptions.Object);
    }

    #region ReadSettingAsync Tests

    [Fact]
    public async Task ReadSettingAsync_WhenFileDoesNotExist_ShouldReturnDefault()
    {
        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ThrowsAsync(new FileNotFoundException());

        var result = await this.service.ReadSettingAsync<string>("testKey");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ReadSettingAsync_WhenFileIsEmpty_ShouldReturnDefault()
    {
        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync(string.Empty);

        var result = await this.service.ReadSettingAsync<string>("testKey");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ReadSettingAsync_WhenFileIsWhitespace_ShouldReturnDefault()
    {
        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync("   \n\t   ");

        var result = await this.service.ReadSettingAsync<string>("testKey");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ReadSettingAsync_WhenKeyNotFound_ShouldReturnDefault()
    {
        var jsonContent = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["otherKey"] = "otherValue"
        });

        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync(jsonContent);

        var result = await this.service.ReadSettingAsync<string>("testKey");

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

        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync(jsonContent);

        var result = await this.service.ReadSettingAsync<JsonElement>("testKey");

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

        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync(jsonContent);

        var result = await this.service.ReadSettingAsync<string>("testKey");

        result.ShouldBe(serializedTestObject);
    }

    [Fact]
    public async Task ReadSettingAsync_WhenJsonIsCorrupted_ShouldReturnDefault()
    {
        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync("{ invalid json content");

        var result = await this.service.ReadSettingAsync<string>("testKey");

        result.ShouldBeNull();
    }

    [Theory]
    [InlineData("stringValue", "testKey")]
    [InlineData(42, "intKey")]
    [InlineData(true, "boolKey")]
    public async Task ReadSettingAsync_WithDifferentTypes_ShouldReturnCorrectValues<T>(T expectedValue, string key)
    {
        var jsonContent = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            [key] = expectedValue
        });

        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync(jsonContent);

        var result = await this.service.ReadSettingAsync<T>(key);

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

        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync("{}");
        this.mockFileService.Setup(x => x.DirectoryExists(expectedDirectory))
            .ReturnsAsync(false);

        var result = await this.service.SaveSettingAsync(testKey, testValue);

        result.IsNone.ShouldBeTrue();
        this.mockFileService.Verify(x => x.DirectoryExists(expectedDirectory), Times.Once);
    }

    [Fact]
    public async Task SaveSettingAsync_ShouldSerializeAndWriteToFile_AndReturnNone()
    {
        var testKey = "testKey";
        var testValue = "testValue";
        var expectedDirectory = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mechanic", "config")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "mechanic");

        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync("{}");
        this.mockFileService.Setup(x => x.DirectoryExists(expectedDirectory))
            .ReturnsAsync(true);

        var result = await this.service.SaveSettingAsync(testKey, testValue);

        result.IsNone.ShouldBeTrue();
        this.mockFileService.Verify(x => x.WriteAllText(this.expectedConfigPath, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SaveSettingAsync_WhenExistingSettings_ShouldMergeWithExisting_AndReturnNone()
    {
        var existingSettings = new Dictionary<string, object>
        {
            ["existingKey"] = "existingValue"
        };
        var existingJson = JsonSerializer.Serialize(existingSettings);

        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync(existingJson);
        this.mockFileService.Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .ReturnsAsync(true);

        var capturedJson = string.Empty;
        this.mockFileService.Setup(x => x.WriteAllText(this.expectedConfigPath, It.IsAny<string>()))
            .Callback<string, string>((_, content) => capturedJson = content);

        var result = await this.service.SaveSettingAsync("newKey", "newValue");

        result.IsNone.ShouldBeTrue();
        this.mockFileService.Verify(x => x.WriteAllText(this.expectedConfigPath, It.IsAny<string>()), Times.Once);
        capturedJson.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task SaveSettingAsync_WhenFileServiceThrowsException_ShouldReturnError()
    {
        var testKey = "testKey";
        var testValue = "testValue";

        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied to config"));
        this.mockFileService.Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .ReturnsAsync(true);
        this.mockFileService.Setup(x => x.WriteAllText(this.expectedConfigPath, It.IsAny<string>()))
            .ThrowsAsync(new UnauthorizedAccessException("Write access denied"));

        var result = await this.service.SaveSettingAsync(testKey, testValue);

        result.IsSome.ShouldBeTrue();
        result.Match(
            Some: error => Assert.IsType<FailedToSaveSettingsError>(error),
            None: () => Assert.Fail()
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

        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync("{}");
        this.mockFileService.Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .ReturnsAsync(true);

        var result = await this.service.SaveSettingAsync(testKey, value);

        result.IsNone.ShouldBeTrue();
        this.mockFileService.Verify(x => x.WriteAllText(this.expectedConfigPath, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SaveSettingAsync_WhenDirectoryCreationFails_ShouldReturnError()
    {
        var testKey = "testKey";
        var testValue = "testValue";
        var expectedDirectory = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mechanic", "config")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "mechanic");

        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync("{}");
        this.mockFileService.Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .ReturnsAsync(false);
        this.mockFileService.Setup(x => x.CreateDirectoryAsync(expectedDirectory))
            .ThrowsAsync(new UnauthorizedAccessException("Cannot create directory"));

        var result = await this.service.SaveSettingAsync(testKey, testValue);

        result.IsSome.ShouldBeTrue();
        result.Match(
            Some: error => error.ShouldBeOfType<FailedToMakeSettingsDirectoryError>(),
            None: () => Assert.Fail()
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

        this.mockFileService.Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .ReturnsAsync(true);

        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync(() => JsonSerializer.Serialize(settings));

        this.mockFileService.Setup(x => x.WriteAllText(this.expectedConfigPath, It.IsAny<string>()))
            .Callback<string, string>((_, _) =>
            {
                settings[testKey] = JsonSerializer.Serialize(testValue);
            });

        var saveResult = await this.service.SaveSettingAsync(testKey, testValue);
        var readResult = await this.service.ReadSettingAsync<string>(testKey);

        saveResult.IsNone.ShouldBeTrue();
        readResult.ShouldBe($"\"{testValue}\"");
        this.mockFileService.Verify(x => x.WriteAllText(this.expectedConfigPath, It.IsAny<string>()), Times.Once);
        this.mockFileService.Verify(x => x.ReadAllText(this.expectedConfigPath), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SaveAndRead_WhenSaveFails_ShouldHandleErrorsGracefully()
    {
        var testKey = "failureTestKey";
        var testValue = "test value that shall not pass";

        this.mockFileService.Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .ReturnsAsync(true);
        this.mockFileService.Setup(x => x.ReadAllText(this.expectedConfigPath))
            .ReturnsAsync("{}");
        this.mockFileService.Setup(x => x.WriteAllText(this.expectedConfigPath, It.IsAny<string>()))
            .ThrowsAsync(new IOException("Unable to write to file"));

        var saveResult = await this.service.SaveSettingAsync(testKey, testValue);

        saveResult.IsSome.ShouldBeTrue();
        saveResult.Match(
            Some: error =>
            {
                error.ShouldBeOfType<FailedToSaveSettingsError>();
            },
            None: () => Assert.Fail()
        );
    }

    #endregion
}
