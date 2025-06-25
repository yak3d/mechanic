using Mechanic.Core.Services;

namespace Mechanic.Core.Tests.Services;

public class FileServiceTest
{
    [Fact]
    public async Task FileService_Can_Write_File()
    {
        var fileService = new FileService();
        var path = $"{Path.GetTempPath()}\\file.txt";
        var helloWorld = "Hello World!";
        await fileService.WriteAllText(path, helloWorld);

        var result = await fileService.ReadAllText(path);
        Assert.Equal(helloWorld, result);
    }

    [Fact]
    public async Task FileService_Can_Read_File()
    {
        var fileService = new FileService();
        var path = $"{Path.GetTempPath()}\\file.txt";
        var helloWorld = "Hello World!";
        await fileService.WriteAllText(path, helloWorld);

        var result = await fileService.ReadAllText(path);
        Assert.Equal(helloWorld, result);
    }
}