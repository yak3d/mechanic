using Mechanic.Core.Contracts;
using Mechanic.Core.Models;
using Mechanic.Core.Models.Pyro;
using Mechanic.Core.Services;
using Mechanic.Core.Services.Errors;
using Microsoft.Extensions.Logging;
using Moq;
using Pyro.Models.Generated;
using Shouldly;

namespace Mechanic.Core.Tests.Services;

public class PyroServiceTest
{
    private readonly Mock<ILogger<PyroService>> _mockLogger;
    private readonly Mock<IXmlSerializer> _mockXmlSerializer;
    private readonly Mock<IFileService> _mockFileService;
    private readonly PyroService _pyroService;

    public PyroServiceTest()
    {
        _mockLogger = new Mock<ILogger<PyroService>>();
        _mockXmlSerializer = new Mock<IXmlSerializer>();
        _mockFileService = new Mock<IFileService>();
        _pyroService = new PyroService(_mockLogger.Object, _mockXmlSerializer.Object, _mockFileService.Object);
    }

    [Theory]
    [InlineData(GameName.Tes5Skyrim)]
    [InlineData(GameName.SkyrimSpecialEdition)]
    [InlineData(GameName.Fallout4)]
    [InlineData(GameName.Starfield)]
    public async Task CreatePyroProjectAsync_WithSupportedGame_ShouldCreateProjectSuccessfully(GameName gameName)
    {
        var projectName = "TestProject";
        var namespaceName = "TestNamespace";
        var gamePath = @"C:\Games\TestGame";
        var expectedXml = "<Project>Test XML Content</Project>";
        var expectedFilePath = Path.Combine(gamePath, "Data", "Scripts", $"{projectName}.ppj");

        var mockProject = new PapyrusProject();
        var mockFactory = new Mock<IPyroProjectFactory>();

        mockFactory.Setup(factory => factory.CreateProjectAsync(projectName, namespaceName, gamePath))
            .ReturnsAsync(mockProject);
        _mockXmlSerializer.Setup(serializer => serializer.SerializeAsync(mockProject)).Returns(Task.FromResult(expectedXml));
        
        var result = await _pyroService.CreatePyroProjectAsync(gameName, projectName, namespaceName, gamePath);
        
        result.IsRight.ShouldBeTrue();
        result.IfRight(filePath => filePath.ShouldBe(expectedFilePath));
        _mockFileService.Verify(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData(GameName.Tes4Oblivion)]
    [InlineData(GameName.Fallout3)]
    [InlineData(GameName.FalloutNewVegas)]
    public async Task CreatePyroProjectAsync_WithUnsupportedGame_ShouldReturnError(GameName gameName)
    {
        var projectName = "MyOldProject";
        var projectNamespace = "YeOldeNamespace";
        var gamePath = @"C:\Games\TestGame";
        
        var result = await _pyroService.CreatePyroProjectAsync(gameName, projectName, projectNamespace, gamePath);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(err => err.ShouldBeOfType<GameUnsupportedInPyro>());
        
        _mockFileService.Verify(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreatePyroProjectAsync_WhenSerializationFails_ShouldReturnXmlError()
    {
        var gameName = GameName.Tes5Skyrim;
        var projectName = "TestProject";
        var namespaceName = "TestNamespace";
        var gamePath = @"C:\Games\Skyrim";

        var mockProject = new PapyrusProject();
        var mockFactory = new Mock<IPyroProjectFactory>();
        
        mockFactory.Setup(f => f.CreateProjectAsync(projectName, namespaceName, gamePath))
            .ReturnsAsync(mockProject);
        
        _mockXmlSerializer.Setup(s => s.SerializeAsync(It.IsAny<PapyrusProject>()))
            .ThrowsAsync(new InvalidOperationException("XML serialization failed"));
        
        var result = await _pyroService.CreatePyroProjectAsync(gameName, projectName, namespaceName, gamePath);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ShouldBeOfType<UnableToSerializeProjectToXml>());
        
        _mockFileService.Verify(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreatePyroProjectAsync_WhenFactoryCreationFails_ShouldReturnFactoryError()
    {
        var gameName = GameName.Tes5Skyrim;
        var projectName = "TestProject";
        var namespaceName = "TestNamespace";
        var gamePath = @"C:\Games\Skyrim";
        
        var mockFactory = new Mock<IPyroProjectFactory>();
        mockFactory.Setup(f => f.CreateProjectAsync(projectName, namespaceName, gamePath))
            .ThrowsAsync(new InvalidOperationException("Failed to create Factory"));
        
    }
}