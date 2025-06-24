using Mechanic.Core.Services;

namespace Mechanic.Core.Tests.Services;

public class FileServiceTest
{
    [Fact]
    public void FileService_Can_Write_File()
    {
        var fileService = new FileService();
        var path = $"{Path.GetTempPath()}\\file.txt";
        var helloWorld = "Hello World!";
        fileService.WriteAllText(path, helloWorld);

        var result = fileService.ReadAllText(path);
        Assert.Equal(helloWorld, result);
    }

    [Fact]
    public void FileService_Can_Read_File()
    {
        var fileService = new FileService();
        var path = $"{Path.GetTempPath()}\\file.txt";
        var helloWorld = "Hello World!";
        fileService.WriteAllText(path, helloWorld);
        
        var result = fileService.ReadAllText(path);
        Assert.Equal(helloWorld, result);
    }
}