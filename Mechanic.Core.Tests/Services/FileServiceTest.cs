using Mechanic.Core.Contracts;
using Mechanic.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Mechanic.Core.Tests.Services;

public class FileServiceTest
{
    private readonly Mock<ILogger<FileService>> _logger = new();
    [Fact]
    public async Task FileService_Can_Write_File()
    {
        var fileService = new FileService(_logger.Object);
        var path = $"{Path.GetTempPath()}\\file.txt";
        var helloWorld = "Hello World!";
        await fileService.WriteAllText(path, helloWorld);

        var result = await fileService.ReadAllText(path);
        Assert.Equal(helloWorld, result);
    }

    [Fact]
    public async Task FileService_Can_Read_File()
    {
        var fileService = new FileService(_logger.Object);
        var path = $"{Path.GetTempPath()}\\file.txt";
        var helloWorld = "Hello World!";
        await fileService.WriteAllText(path, helloWorld);

        var result = await fileService.ReadAllText(path);
        Assert.Equal(helloWorld, result);
    }
}