using Mechanic.Core.Models;
using Mechanic.Core.Services;
using Mechanic.Core.Tests.Utils;
using Shouldly;
using Xunit.Abstractions;

namespace Mechanic.Core.Tests.Services;

public class JsonFileProjectSerializationServiceTest(ITestOutputHelper testOutputHelper) : LoggingTest(testOutputHelper)
{
    [Fact]
    public void Can_Deserialize_Json_File()
    {
        var logger = CreateLogger<JsonFileProjectSerializationService>();
        var service = new JsonFileProjectSerializationService(logger, new FileService());
        var project = new MechanicProject
        {
            Id = "com.example.TestProject"
        };
        var path = $"{Path.GetTempPath()}file.json";
        
        service.SerializeProject(project, path);
        
        var result = service.DeserializeProject(path);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(project.Id);
    }
    
    [Fact]
    public void Can_Serialize_Project_To_JsonFile()
    {
        var logger = CreateLogger<JsonFileProjectSerializationService>();
        var service = new JsonFileProjectSerializationService(logger, new FileService());
        var project = new MechanicProject
        {
            Id = "com.example.TestProject"
        };
        var path = $"{Path.GetTempPath()}file.json";
        
        service.SerializeProject(project, path);
        
        var result = File.ReadAllText(path);
        Assert.True(!string.IsNullOrEmpty(result));
    }
}