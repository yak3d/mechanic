using Mechanic.Core.Contracts;
using Mechanic.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Mechanic.Core.Tests.Services;

public class OsServiceTest : IDisposable
{
    private readonly Mock<IFileService> mockFileService;
    private readonly OSService osService;
    private readonly string originalPathVariable;

    public OsServiceTest()
    {
        Mock<ILogger<OSService>> mockLogger = new();
        this.mockFileService = new Mock<IFileService>();
        this.osService = new OSService(mockLogger.Object, this.mockFileService.Object);

        this.originalPathVariable = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Environment.SetEnvironmentVariable("PATH", this.originalPathVariable);
    }

    [Fact]
    public async Task ExecutableIsInPathAsync_WhenPathVariableIsNull_ReturnsFalse()
    {
        Environment.SetEnvironmentVariable("PATH", null);

        var result = await this.osService.ExecutableIsInPathAsync("test.exe");

        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBeFalse());
    }

    [Fact]
    public async Task ExecutableIsInPathAsync_WhenPathVariableIsEmpty_ReturnsFalse()
    {
        Environment.SetEnvironmentVariable("PATH", string.Empty);

        var result = await this.osService.ExecutableIsInPathAsync("test.exe");

        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBeFalse());
    }


    [Theory]
    [InlineData(@"C:\Windows\System32", "cmd.exe", true)]
    [InlineData(@"C:\NonExistent\Dir", "missing.exe", false)]
    public async Task ExecutableIsInPathAsync_WindowsExecutable_ReturnsExpectedResult(
        string pathDirectory,
        string executableName,
        bool shouldExist)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        Environment.SetEnvironmentVariable("PATH", pathDirectory);

        this.mockFileService.Setup(fs => fs.DirectoryExists(pathDirectory))
            .ReturnsAsync(shouldExist);

        if (shouldExist)
        {
            var fullPath = Path.Combine(pathDirectory, executableName);
            this.mockFileService.Setup(fs => fs.FileExistsAsync(fullPath))
                .ReturnsAsync(true);
        }

        var result = await this.osService.ExecutableIsInPathAsync(executableName);

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
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        Environment.SetEnvironmentVariable("PATH", pathDirectory);

        this.mockFileService.Setup(fs => fs.DirectoryExists(pathDirectory))
            .ReturnsAsync(shouldExist);

        if (shouldExist)
        {
            var fullPath = Path.Combine(pathDirectory, executableName);
            this.mockFileService.Setup(fs => fs.FileExistsAsync(fullPath))
                .ReturnsAsync(true);
        }

        var result = await this.osService.ExecutableIsInPathAsync(executableName);

        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBe(shouldExist));
    }

    [Fact]
    public async Task ExecutableIsInPathAsync_WindowsWithMultipleExtensions_ChecksAllExtensions()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var testDir = "C:\\TestDir";
        var executableBaseName = "testapp";

        Environment.SetEnvironmentVariable("PATH", testDir);
        this.mockFileService.Setup(fs => fs.DirectoryExists(testDir)).ReturnsAsync(true);

        this.mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(testDir, "testapp.exe")))
            .ReturnsAsync(true);
        this.mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(testDir, "testapp.cmd")))
            .ReturnsAsync(false);
        this.mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(testDir, "testapp.bat")))
            .ReturnsAsync(false);
        this.mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(testDir, "testapp.com")))
            .ReturnsAsync(false);

        var result = await this.osService.ExecutableIsInPathAsync(executableBaseName);

        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBeTrue());

        this.mockFileService.Verify(fs => fs.FileExistsAsync(Path.Combine(testDir, "testapp.exe")), Times.Once);
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
        {
            return;
        }

        var directories = pathValue.Split(separator);
        var executableName = "testexe";

        Environment.SetEnvironmentVariable("PATH", pathValue);

        foreach (var dir in directories)
        {
            this.mockFileService.Setup(fs => fs.DirectoryExists(dir.Trim())).ReturnsAsync(true);

            if (OperatingSystem.IsWindows())
            {
                this.mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(dir.Trim(), "testexe.exe")))
                    .ReturnsAsync(false);
                this.mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(dir.Trim(), "testexe.cmd")))
                    .ReturnsAsync(false);
                this.mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(dir.Trim(), "testexe.bat")))
                    .ReturnsAsync(false);
                this.mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(dir.Trim(), "testexe.com")))
                    .ReturnsAsync(false);
            }
            else
            {
                this.mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(dir.Trim(), executableName)))
                    .ReturnsAsync(false);
            }
        }

        var result = await this.osService.ExecutableIsInPathAsync(executableName);

        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBeFalse());

        foreach (var dir in directories)
        {
            this.mockFileService.Verify(fs => fs.DirectoryExists(dir.Trim()), Times.Once);
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
        this.mockFileService.Setup(fs => fs.DirectoryExists(directories[0])).ReturnsAsync(true);

        if (OperatingSystem.IsWindows())
        {
            this.mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(directories[0], "foundexe.exe")))
                .ReturnsAsync(true);
        }
        else
        {
            this.mockFileService.Setup(fs => fs.FileExistsAsync(Path.Combine(directories[0], executableName)))
                .ReturnsAsync(true);
        }

        var result = await this.osService.ExecutableIsInPathAsync(executableName);

        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBeTrue());

        for (var i = 1; i < directories.Length; i++)
        {
            this.mockFileService.Verify(fs => fs.DirectoryExists(directories[i]), Times.Never);
        }
    }

    [Fact]
    public async Task ExecutableIsInPathAsync_EmptyStringInPath_HandlesGracefully()
    {
        var pathWithEmpty = OperatingSystem.IsWindows() ? "C:\\Dir1;;C:\\Dir2" : "/dir1::/dir2";
        var executableName = "testexe";

        Environment.SetEnvironmentVariable("PATH", pathWithEmpty);

        var result = await this.osService.ExecutableIsInPathAsync(executableName);

        result.IsRight.ShouldBeTrue();
        result.IfRight(found => found.ShouldBeFalse());
    }
}
