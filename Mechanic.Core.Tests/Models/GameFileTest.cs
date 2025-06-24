using Mechanic.Core.Models;
using Mechanic.Core.Project.Models.Json;
using Shouldly;

namespace Mechanic.Core.Tests.Models;

public class GameFileTest
{
    [Fact]
    public void FromJson_WithValidGameFiles_ShouldCreateGameFileCorrectly()
    {
        var testGuid = Guid.NewGuid();
        var gameFiles = new GameFiles
        {
            Id = testGuid.ToString(),
            Path = "game/materials/metal.mat",
            FileType = GameFilesFileType.MAT
        };

        var result = GameFile.FromJson(gameFiles);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(testGuid);
        result.Path.ShouldBe("game/materials/metal.mat");
        result.GameFileType.ShouldBe(GameFileType.Material);
    }
   
    [Fact]
    public void FromJson_WithMATFileType_ShouldMapToMAT()
    {
        var gameFiles = new GameFiles
        {
            Id = Guid.NewGuid().ToString(),
            Path = "materials/wood.mat",
            FileType = GameFilesFileType.MAT
        };

        var result = GameFile.FromJson(gameFiles);

        result.GameFileType.ShouldBe(GameFileType.Material);
    }

    [Fact]
    public void FromJson_WithDDSFileType_ShouldMapToDDS()
    {
        var gameFiles = new GameFiles
        {
            Id = Guid.NewGuid().ToString(),
            Path = "textures/diffuse.dds",
            FileType = GameFilesFileType.DDS
        };

        var result = GameFile.FromJson(gameFiles);

        result.GameFileType.ShouldBe(GameFileType.DirectDrawSurface);
    }

    [Fact]
    public void FromJson_WithWEMFileType_ShouldMapToWEM()
    {
        var gameFiles = new GameFiles
        {
            Id = Guid.NewGuid().ToString(),
            Path = "audio/music.wem",
            FileType = GameFilesFileType.WEM
        };

        var result = GameFile.FromJson(gameFiles);

        result.GameFileType.ShouldBe(GameFileType.WwiseEncodedMedia);
    }

    [Fact]
    public void FromJson_WithPEXFileType_ShouldMapToPEX()
    {
        var gameFiles = new GameFiles
        {
            Id = Guid.NewGuid().ToString(),
            Path = "scripts/quest.pex",
            FileType = GameFilesFileType.PEX
        };

        var result = GameFile.FromJson(gameFiles);

        result.GameFileType.ShouldBe(GameFileType.PapyrusExecutable);
    }
    
    [Fact]
    public void FromJson_WithValidUuidV4Format_ShouldParseCorrectly()
    {
        var validUuid = "550e8400-e29b-41d4-a716-446655440000";
        var gameFiles = new GameFiles
        {
            Id = validUuid,
            Path = "test/file.mat",
            FileType = GameFilesFileType.MAT
        };

        var result = GameFile.FromJson(gameFiles);

        result.Id.ShouldBe(Guid.Parse(validUuid));
    }

    [Fact]
    public void FromJson_WithInvalidGuidFormat_ShouldThrowFormatException()
    {
        var gameFiles = new GameFiles
        {
            Id = "invalid-guid-format",
            Path = "test/file.mat",
            FileType = GameFilesFileType.MAT
        };

        Should.Throw<FormatException>(() => GameFile.FromJson(gameFiles));
    }

    [Fact]
    public void FromJson_WithEmptyGuidString_ShouldThrowFormatException()
    {
        var gameFiles = new GameFiles
        {
            Id = "",
            Path = "test/file.mat",
            FileType = GameFilesFileType.MAT
        };

        Should.Throw<FormatException>(() => GameFile.FromJson(gameFiles));
    }

    [Fact]
    public void FromJson_WithNullGuidString_ShouldThrowArgumentNullException()
    {
        var gameFiles = new GameFiles
        {
            Id = null,
            Path = "test/file.mat",
            FileType = GameFilesFileType.MAT
        };

        Should.Throw<ArgumentNullException>(() => GameFile.FromJson(gameFiles));
    }
    [Fact]
    public void ToJson_ShouldReturnCorrectJsonObject()
    {
        var gameFile = new GameFile
        {
            Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
            Path = "/test/path/file.dds",
            GameFileType = GameFileType.DirectDrawSurface
        };

        var result = gameFile.ToJson();

        result.Id.ShouldBe("12345678-1234-1234-1234-123456789012");
        result.Path.ShouldBe("/test/path/file.dds");
        result.FileType.ShouldBe(gameFile.GameFileType.ToJson());
    }

    [Theory]
    [InlineData(GameFileType.Material, "/materials/wood.nif")]
    [InlineData(GameFileType.DirectDrawSurface, "/textures/armor.dds")]
    [InlineData(GameFileType.WwiseEncodedMedia, "/sounds/music.wem")]
    [InlineData(GameFileType.PapyrusExecutable, "/scripts/quest.pex")]
    public void ToJson_WithDifferentFileTypes_ShouldPreserveAllProperties(GameFileType fileType, string path)
    {
        var id = Guid.NewGuid();
        var gameFile = new GameFile
        {
            Id = id,
            Path = path,
            GameFileType = fileType
        };

        var result = gameFile.ToJson();

        result.Id.ShouldBe(id.ToString());
        result.Path.ShouldBe(path);
        result.FileType.ShouldBe(fileType.ToJson());
    }

    [Fact]
    public void ToJson_WithEmptyGuid_ShouldReturnEmptyGuidString()
    {
        var gameFile = new GameFile
        {
            Id = Guid.Empty,
            Path = "/test/path",
            GameFileType = GameFileType.Material
        };

        var result = gameFile.ToJson();

        result.Id.ShouldBe("00000000-0000-0000-0000-000000000000");
    }
}
