using Mechanic.Core.Contracts;
using Mechanic.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Mechanic.Core.Tests.Services;

public class OSServiceTest : IDisposable
{
    private readonly Mock<ILogger<OSService>> _mockLogger;
    private readonly Mock<IFileService> _mockFileService;
    private readonly OSService _osService;
    private readonly string _originalPathVariable;

    public OSServiceTest()
    {
        _mockLogger = new Mock<ILogger<OSService>>();
        _mockFileService = new Mock<IFileService>();
        _osService = new OSService(_mockLogger.Object, _mockFileService.Object);
        
        _originalPathVariable = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("PATH", _originalPathVariable);
    }

    [Fact]
    public async Task ExecutableIsInPathAsync_WhenPathVariableIsNull_ReturnsFalse()
    {
        Environment.SetEnvironmentVariable("PATH", null);

        var result = await _osService.ExecutableIsInPathAsync("test.exe");
        
        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBeFalse());
    }
    
    [Fact]
    public async Task ExecutableIsInPathAsync_WhenPathVariableIsEmpty_ReturnsFalse()
    {
        Environment.SetEnvironmentVariable("PATH", string.Empty);

        var result = await _osService.ExecutableIsInPathAsync("test.exe");
        
        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBeFalse());
    }
    
    
    [Theory]
    [InlineData("C:\\Windows\\System32", "cmd.exe", true)]
    [InlineData("C:\\NonExistent\\Dir", "missing.exe", false)]
    public async Task ExecutableIsInPathAsync_WindowsExecutable_ReturnsExpectedResult(
        string pathDirectory, 
        string executableName, 
        bool shouldExist)
    {
        if (!OperatingSystem.IsWindows()) return;

        Environment.SetEnvironmentVariable("PATH", pathDirectory);
        
        _mockFileService.Setup(fs => fs.DirectoryExists(pathDirectory))
            .ReturnsAsync(shouldExist);
        
        if (shouldExist)
        {
            var fullPath = Path.Combine(pathDirectory, executableName);
            _mockFileService.Setup(fs => fs.FileExistsAsync(fullPath))
                .ReturnsAsync(true);
        }

        var result = await _osService.ExecutableIsInPathAsync(executableName);

        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBe(shouldExist));
    }
    
    
    [Theory]
    [InlineData("/usr/bin", "ls", true)]
    [InlineData("/nonexistent/dir", "missing", false)]
    public async Task ExecutableIsInPathAsync_UnixExecutable_ReturnsExpectedResult(
        string pathDirectory,
        string executableName,
        bool shouldExist)
    {
        if (OperatingSystem.IsWindows()) return;

        Environment.SetEnvironmentVariable("PATH", pathDirectory);
        
        _mockFileService.Setup(fs => fs.DirectoryExists(pathDirectory))
            .ReturnsAsync(shouldExist);
        
        if (shouldExist)
        {
            var fullPath = Path.Combine(pathDirectory, executableName);
            _mockFileService.Setup(fs => fs.FileExistsAsync(fullPath))
                .ReturnsAsync(true);
        }

        var result = await _osService.ExecutableIsInPathAsync(executableName);

        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBe(shouldExist));
    }
    
    [Fact]
    public async Task ExecutableIsInPathAsync_WindowsWithMultipleExtensions_ChecksAllExtensions()
    {
        if (!OperatingSystem.IsWindows()) return;

        var testDir = "C:\\TestDir";
        var executableBaseName = "testapp";
        
        Environment.SetEnvironmentVariable("PATH", testDir);
        _mockFileService.Setup(fs => fs.DirectoryExists(testDir)).ReturnsAsync(true);
        
        _mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(testDir, "testapp.exe")))
            .ReturnsAsync(true);
        _mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(testDir, "testapp.cmd")))
            .ReturnsAsync(false);
        _mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(testDir, "testapp.bat")))
            .ReturnsAsync(false);
        _mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(testDir, "testapp.com")))
            .ReturnsAsync(false);

        var result = await _osService.ExecutableIsInPathAsync(executableBaseName);

        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBeTrue());

        _mockFileService.Verify(fs => fs.FileExistsAsync(Path.Combine(testDir, "testapp.exe")), Times.Once);
    }
    
    [Theory]
    [InlineData("C:\\Dir1;C:\\Dir2;C:\\Dir3", ';')]
    [InlineData("/usr/bin:/usr/local/bin:/opt/bin", ':')]
    public async Task ExecutableIsInPathAsync_MultiplePathDirectories_ChecksAllDirectories(
        string pathValue,
        char separator)
    {
        if ((separator == ';' && !OperatingSystem.IsWindows()) || 
            (separator == ':' && OperatingSystem.IsWindows())) 
            return;

        var directories = pathValue.Split(separator);
        var executableName = "testexe";
        
        Environment.SetEnvironmentVariable("PATH", pathValue);
        
        foreach (var dir in directories)
        {
            _mockFileService.Setup(fs => fs.DirectoryExists(dir.Trim())).ReturnsAsync(true);
            
            if (OperatingSystem.IsWindows())
            {
                _mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(dir.Trim(), "testexe.exe")))
                    .ReturnsAsync(false);
                _mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(dir.Trim(), "testexe.cmd")))
                    .ReturnsAsync(false);
                _mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(dir.Trim(), "testexe.bat")))
                    .ReturnsAsync(false);
                _mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(dir.Trim(), "testexe.com")))
                    .ReturnsAsync(false);
            }
            else
            {
                _mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(dir.Trim(), executableName)))
                    .ReturnsAsync(false);
            }
        }

        var result = await _osService.ExecutableIsInPathAsync(executableName);

        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBeFalse());

        foreach (var dir in directories)
        {
            _mockFileService.Verify(fs => fs.DirectoryExists(dir.Trim()), Times.Once);
        }
    }

    [Fact]
    public async Task ExecutableIsInPathAsync_FirstDirectoryContainsExecutable_ReturnsImmediately()
    {
        var pathDirs = OperatingSystem.IsWindows() 
            ? "C:\\Dir1;C:\\Dir2;C:\\Dir3" 
            : "/dir1:/dir2:/dir3";
        var executableName = "foundexe";
        var directories = pathDirs.Split(OperatingSystem.IsWindows() ? ';' : ':');
        
        Environment.SetEnvironmentVariable("PATH", pathDirs);
        _mockFileService.Setup(fs => fs.DirectoryExists(directories[0])).ReturnsAsync(true);
        
        if (OperatingSystem.IsWindows())
        {
            _mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(directories[0], "foundexe.exe")))
                .ReturnsAsync(true);
        }
        else
        {
            _mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(directories[0], executableName)))
                .ReturnsAsync(true);
        }

        var result = await _osService.ExecutableIsInPathAsync(executableName);

        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBeTrue());

        for (int i = 1; i < directories.Length; i++)
        {
            _mockFileService.Verify(fs => fs.DirectoryExists(directories[i]), Times.Never);
        }
    }
    
    [Fact]
    public async Task ExecutableIsInPathAsync_EmptyStringInPath_HandlesGracefully()
    {
        var pathWithEmpty = OperatingSystem.IsWindows() ? "C:\\Dir1;;C:\\Dir2" : "/dir1::/dir2";
        var executableName = "testexe";
        
        Environment.SetEnvironmentVariable("PATH", pathWithEmpty);

        var result = await _osService.ExecutableIsInPathAsync(executableName);

        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBeFalse());
    }
}