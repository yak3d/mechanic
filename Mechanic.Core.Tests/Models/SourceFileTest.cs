using Mechanic.Core.Models;
using Shouldly;

namespace Mechanic.Core.Tests.Models;

public class SourceFileTest
{
    [Fact]
    public void ToJson_ShouldReturnCorrectJsonObject()
    {
        var guid1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var guid2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var sourceFile = new SourceFile
        {
            Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
            Path = "/source/models/armor.fbx",
            FileType = SourceFileType.Fbx,
            GameFileLinks = new List<Guid> { guid1, guid2 }
        };

        var result = sourceFile.ToJson();

        result.Id.ShouldBe("12345678-1234-1234-1234-123456789012");
        result.Path.ShouldBe("/source/models/armor.fbx");
        result.FileType.ShouldBe(sourceFile.FileType.ToJson());
        result.DestinationPaths.ShouldBe(new List<string>
        {
            "11111111-1111-1111-1111-111111111111",
            "22222222-2222-2222-2222-222222222222"
        });
    }

    [Theory]
    [InlineData(SourceFileType.Fbx, "/models/weapon.fbx")]
    [InlineData(SourceFileType.Blend, "/models/house.blend")]
    [InlineData(SourceFileType.Tiff, "/textures/diffuse.tiff")]
    [InlineData(SourceFileType.Wav, "/audio/footstep.wav")]
    [InlineData(SourceFileType.Psc, "/scripts/dialogue.psc")]
    [InlineData(SourceFileType.Other, "/data/config.xml")]
    public void ToJson_WithDifferentFileTypes_ShouldPreserveAllProperties(SourceFileType fileType, string path)
    {
        var id = Guid.NewGuid();
        var destinationGuid1 = Guid.NewGuid();
        var destinationGuid2 = Guid.NewGuid();
        var destinationPaths = new List<Guid> { destinationGuid1, destinationGuid2 };
        var sourceFile = new SourceFile
        {
            Id = id,
            Path = path,
            FileType = fileType,
            GameFileLinks = destinationPaths
        };

        var result = sourceFile.ToJson();

        result.Id.ShouldBe(id.ToString());
        result.Path.ShouldBe(path);
        result.FileType.ShouldBe(fileType.ToJson());
        result.DestinationPaths.ShouldBe(new List<string> { destinationGuid1.ToString(), destinationGuid2.ToString() });
    }

    [Fact]
    public void ToJson_WithEmptyDestinationPaths_ShouldReturnEmptyList()
    {
        var sourceFile = new SourceFile
        {
            Id = Guid.NewGuid(),
            Path = "/test/path",
            FileType = SourceFileType.Fbx,
            GameFileLinks = new List<Guid>()
        };

        var result = sourceFile.ToJson();

        result.DestinationPaths.ShouldNotBeNull();
        result.DestinationPaths.ShouldBeEmpty();
    }

    [Fact]
    public void ToJson_WithSingleDestinationPath_ShouldReturnListWithOneItem()
    {
        var destinationGuid = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var sourceFile = new SourceFile
        {
            Id = Guid.NewGuid(),
            Path = "/test/path",
            FileType = SourceFileType.Wav,
            GameFileLinks = [destinationGuid]
        };

        var result = sourceFile.ToJson();

        result.DestinationPaths.Count.ShouldBe(1);
        result.DestinationPaths.ToList()[0].ShouldBe("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    }

    [Fact]
    public void ToJson_WithMultipleDestinationPaths_ShouldPreserveOrder()
    {
        var guid1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var guid2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var guid3 = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var destinationPaths = new List<Guid> { guid1, guid2, guid3 };
        var sourceFile = new SourceFile
        {
            Id = Guid.NewGuid(),
            Path = "/test/path",
            FileType = SourceFileType.Tiff,
            GameFileLinks = destinationPaths
        };

        var result = sourceFile.ToJson();

        result.DestinationPaths.ShouldBe(new List<string>
        {
            "11111111-1111-1111-1111-111111111111",
            "22222222-2222-2222-2222-222222222222",
            "33333333-3333-3333-3333-333333333333"
        });
    }

    [Fact]
    public void ToJson_WithEmptyGuid_ShouldReturnEmptyGuidString()
    {
        var sourceFile = new SourceFile
        {
            Id = Guid.Empty,
            Path = "/test/path",
            FileType = SourceFileType.Other,
            GameFileLinks = new List<Guid>()
        };

        var result = sourceFile.ToJson();

        result.Id.ShouldBe("00000000-0000-0000-0000-000000000000");
    }

    [Fact]
    public void ToJson_WithEmptyGuidsInDestinationPaths_ShouldConvertToEmptyGuidStrings()
    {
        var sourceFile = new SourceFile
        {
            Id = Guid.NewGuid(),
            Path = "/test/path",
            FileType = SourceFileType.Psc,
            GameFileLinks = new List<Guid> { Guid.Empty, Guid.Empty }
        };

        var result = sourceFile.ToJson();

        result.DestinationPaths.ShouldBe(new List<string>
        {
            "00000000-0000-0000-0000-000000000000",
            "00000000-0000-0000-0000-000000000000"
        });
    }
}