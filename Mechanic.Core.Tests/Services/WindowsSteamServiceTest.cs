using Mechanic.Core.Contracts;
using Mechanic.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Mechanic.Core.Tests.Services;

public class WindowsSteamServiceTest : IDisposable
{
    private readonly Mock<ILogger<WindowsSteamService>> _mockLogger;
    private readonly WindowsSteamService _steamService;
    private readonly string _testDirectory;
    private readonly Mock<IRegistryService> _mockRegistry;
    private const string SteamRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam";
    private const string SteamRegistryKey64Bit = @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam";

    public WindowsSteamServiceTest()
    {
        _mockLogger = new Mock<ILogger<WindowsSteamService>>();
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _mockRegistry = new Mock<IRegistryService>();
        Directory.CreateDirectory(_testDirectory);
        _steamService = new WindowsSteamService(_mockLogger.Object, _mockRegistry.Object);
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
    
    #endregion
}