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
    private readonly Mock<IXmlSerializer> mockXmlSerializer;
    private readonly Mock<IFileService> mockFileService;
    private readonly PyroService pyroService;

    public PyroServiceTest()
    {
        Mock<ILogger<PyroService>> mockLogger = new();
        this.mockXmlSerializer = new Mock<IXmlSerializer>();
        this.mockFileService = new Mock<IFileService>();
        this.pyroService = new PyroService(mockLogger.Object, this.mockXmlSerializer.Object, this.mockFileService.Object);
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
        this.mockXmlSerializer.Setup(serializer => serializer.SerializeAsync(mockProject)).Returns(Task.FromResult(expectedXml));

        var result = await this.pyroService.CreatePyroProjectAsync(gameName, projectName, namespaceName, gamePath);

        result.IsRight.ShouldBeTrue();
        result.IfRight(filePath => filePath.ShouldBe(expectedFilePath));
        this.mockFileService.Verify(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
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

        var result = await this.pyroService.CreatePyroProjectAsync(gameName, projectName, projectNamespace, gamePath);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(err => err.ShouldBeOfType<GameUnsupportedInPyro>());

        this.mockFileService.Verify(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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

        this.mockXmlSerializer.Setup(s => s.SerializeAsync(It.IsAny<PapyrusProject>()))
            .ThrowsAsync(new InvalidOperationException("XML serialization failed"));

        var result = await this.pyroService.CreatePyroProjectAsync(gameName, projectName, namespaceName, gamePath);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ShouldBeOfType<UnableToSerializeProjectToXml>());

        this.mockFileService.Verify(fs => fs.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public Task CreatePyroProjectAsync_WhenFactoryCreationFails_ShouldReturnFactoryError()
    {
        const string projectName = "TestProject";
        const string namespaceName = "TestNamespace";
        const string gamePath = @"C:\Games\Skyrim";

        var mockFactory = new Mock<IPyroProjectFactory>();
        mockFactory.Setup(f => f.CreateProjectAsync(projectName, namespaceName, gamePath))
            .ThrowsAsync(new InvalidOperationException("Failed to create Factory"));
        return Task.CompletedTask;
    }
}
