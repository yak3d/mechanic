using Mechanic.Core.Contracts;
using Mechanic.Core.Services;
using Mechanic.Core.Services.Errors;
using Mechanic.Core.Tests.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Mechanic.Core.Tests.Services;

public class WindowsSteamServiceTest : IDisposable
{
    private readonly Mock<ILogger<WindowsSteamService>> _mockLogger;
    private readonly Mock<IFileService> _mockFileService;
    private readonly WindowsSteamService _steamService;
    private readonly string _testDirectory;
    private readonly Mock<IRegistryService> _mockRegistry;
    private const string SteamRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam";
    private const string SteamRegistryKey64Bit = @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam";
    private const string DefaultSteamDirectory = @"C:\Program Files (x86)\Steam";

    public WindowsSteamServiceTest()
    {
        _mockLogger = new Mock<ILogger<WindowsSteamService>>();
        _mockFileService = new Mock<IFileService>();
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _mockRegistry = new Mock<IRegistryService>();
        Directory.CreateDirectory(_testDirectory);
        _steamService = new WindowsSteamService(_mockLogger.Object, _mockRegistry.Object, _mockFileService.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    #region GetSteamInstallPath Tests

    [Fact]
    public void GetSteamInstallPath_WhenWow6432NodeKeyExists_ReturnsRegistryPath()
    {
        var expectedPath = @"C:\Program Files (x86)\Steam";
        _mockRegistry.Setup(service => service.GetValue(SteamRegistryKey, "InstallPath", null)).Returns(expectedPath);

        var result = _steamService.GetSteamInstallPath();

        result.ShouldBe(expectedPath);
    }
   
    [Fact]
    public void GetSteamInstallPath_When64BitKeyExists_And32BitDoesNot_Returns64BitPath()
    {
        var expected64BitPath = @"C:\Program Files\Steam";
        _mockRegistry.Setup(service => service.GetValue(SteamRegistryKey, "InstallPath", null))
            .Returns((string?)null);
        _mockRegistry.Setup(service => service.GetValue(SteamRegistryKey64Bit, "InstallPath", null))
            .Returns(expected64BitPath);

        var result = _steamService.GetSteamInstallPath();

        result.ShouldBe(expected64BitPath);
    }
    
    [Fact]
    public void GetSteamInstallPath_WhenNoRegistryKeysExist_ReturnsDefaultPath()
    {
        _mockRegistry.Setup(service => service.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .Returns((string?)null);

        var result = _steamService.GetSteamInstallPath();

        result.ShouldBe(DefaultSteamDirectory);
    }
    #endregion
    
    #region GetInstalledGames Tests
    [Fact]
    public async Task GetInstalledGames_WhenNoLibraryPaths_ReturnsEmptyList()
    {
        _mockRegistry.Setup(service => service.GetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .Returns((string?)null);

        var result = await _steamService.GetInstalledGamesAsync();

        result.IsLeft.ShouldBeTrue();
    }
    
    
    [Fact]
    public async Task GetInstalledGames_WithValidManifests_ReturnsGameList()
    {
        var steamPath = @"C:\Program Files (x86)\Steam";
        var steamAppsPath = Path.Combine(steamPath, "steamapps");

        var libraryContent = VdfTestUtils.BuildSimpleLibraryFolder(steamPath);
        var manifestContent = VdfTestUtils.BuildFullAppState("12345", "Test Game", "testgame");

        var manifestFile = Path.Combine(steamAppsPath, "appmanifest_12345.acf");

        var commonPath = Path.Combine(steamAppsPath, "common");
        Directory.CreateDirectory(commonPath);
        Directory.CreateDirectory(Path.Combine(commonPath, "testgame"));

        _mockFileService.Setup(service => service.DirectoryExists(It.IsAny<string>())).Returns(Task.FromResult(true));
        _mockRegistry.Setup(service => service.GetValue(SteamRegistryKey, "InstallPath", null))
            .Returns(steamPath);
        _mockFileService.Setup(service => service.ReadAllText(Path.Combine(steamAppsPath, "libraryfolders.vdf")))
            .Returns(Task.FromResult(libraryContent));
        _mockFileService.Setup(service => service.GetFilesFromDirectoryAsync(steamAppsPath, "appmanifest_*.acf"))
            .Returns(Task.FromResult((string[])[manifestFile]));
        _mockFileService.Setup(service => service.ReadAllText(manifestFile)).Returns(Task.FromResult(manifestContent));

        var result = await _steamService.GetInstalledGamesAsync();

        result.IsRight.ShouldBeTrue();
        result.IfRight(games =>
        {
            games.Count.ShouldBe(1);
            games[0].AppId.ShouldBe("12345");
            games[0].Name.ShouldBe("Test Game");
            games[0].InstallDir.ShouldBe("testgame");
        });
    }
    
    [Fact]
    public async Task GetInstalledGames_WithMultipleLibraries_CombinesResults()
    {
        var steamPath1 = "C:\\Program Files (x86)\\Steam";
        var steamAppsPath1 = Path.Combine(steamPath1, "steamapps");
        var steamPath2 = "C:\\Program Files (x86)\\Steam2";
        var steamAppsPath2 = Path.Combine(steamPath2, "steamapps");

        var libraryFoldersContent = $@"""libraryfolders""
                     {{
                     	""0""
                     	{{
                     		""path""		""{steamPath1.Replace("\\", "\\\\")}""
                     	}}
                     	""1""
                     	{{
                     		""path""		""{steamPath2.Replace("\\", "\\\\")}""
                     	}}
                     }}
                     """"";;
        
        var manifestFile1 = Path.Combine(steamAppsPath1, "appmanifest_12345.acf");
        var manifestFileContents1 = VdfTestUtils.BuildFullAppState("12345", "Game One", "testgame");
        var manifestFile2 = Path.Combine(steamAppsPath2, "appmanifest_54321.acf");
        var manifestFileContents2 = VdfTestUtils.BuildFullAppState("54321", "Game Two", "testgame");

        var libraryFoldersPath = Path.Combine(steamPath1, "steamapps", "libraryfolders.vdf");
        File.WriteAllText(libraryFoldersPath, libraryFoldersContent);

        _mockRegistry.Setup(service => service.GetValue(SteamRegistryKey, "InstallPath", null))
            .Returns(steamPath1);
        _mockFileService.Setup(service => service.ReadAllText(Path.Combine(steamAppsPath1, "libraryfolders.vdf")))
            .Returns(Task.FromResult(libraryFoldersContent));
        _mockFileService.Setup(service => service.DirectoryExists(It.IsAny<string>())).Returns(Task.FromResult(true));
        _mockFileService.Setup(service => service.GetFilesFromDirectoryAsync(steamAppsPath1, "appmanifest_*.acf"))
            .Returns(Task.FromResult((string[])[manifestFile1]));
        _mockFileService.Setup(service => service.ReadAllText(manifestFile1)).Returns(Task.FromResult(manifestFileContents1));
        _mockFileService.Setup(service => service.ReadAllText(Path.Combine(steamAppsPath2, "libraryfolders.vdf")))
            .Returns(Task.FromResult(libraryFoldersContent));
        _mockFileService.Setup(service => service.GetFilesFromDirectoryAsync(steamAppsPath2, "appmanifest_*.acf"))
            .Returns(Task.FromResult((string[])[manifestFile2]));
        _mockFileService.Setup(service => service.ReadAllText(manifestFile2)).Returns(Task.FromResult(manifestFileContents2));

        var result = await _steamService.GetInstalledGamesAsync();

        result.IsRight.ShouldBeTrue();
        result.IfRight(games =>
        {
            games.Count.ShouldBe(2);
            games.Any(g => g.Name == "Game One").ShouldBeTrue();
            games.Any(g => g.Name == "Game Two").ShouldBeTrue();
        });
    }
    
    [Fact]
    public async Task GetInstalledGames_WithCorruptedManifest_ReturnsError()
    {
        var steamPath = @"C:\Program Files (x86)\Steam";
        var steamAppsPath = Path.Combine(steamPath, "steamapps");

        var libraryContent = VdfTestUtils.BuildSimpleLibraryFolder(steamPath);

        var manifestFile = Path.Combine(steamAppsPath, "appmanifest_12345.acf");

        var commonPath = Path.Combine(steamAppsPath, "common");
        Directory.CreateDirectory(commonPath);
        Directory.CreateDirectory(Path.Combine(commonPath, "testgame"));

        _mockFileService.Setup(service => service.DirectoryExists(It.IsAny<string>())).Returns(Task.FromResult(true));
        _mockRegistry.Setup(service => service.GetValue(SteamRegistryKey, "InstallPath", null))
            .Returns(steamPath);
        _mockFileService.Setup(service => service.ReadAllText(Path.Combine(steamAppsPath, "libraryfolders.vdf")))
            .Returns(Task.FromResult(libraryContent));
        _mockFileService.Setup(service => service.GetFilesFromDirectoryAsync(steamAppsPath, "appmanifest_*.acf"))
            .Returns(Task.FromResult((string[])[manifestFile]));
        _mockFileService.Setup(service => service.ReadAllText(manifestFile)).Returns(Task.FromResult("{ corrupted vdf content }"));

        _mockRegistry.Setup(service => service.GetValue(SteamRegistryKey, "InstallPath", null))
            .Returns(steamPath);

        var result = await _steamService.GetInstalledGamesAsync();

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ShouldBeOfType<SteamManifestError.VdfParseError>());
    }

    #endregion
}