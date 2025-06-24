using Mechanic.Core.Models;
using Mechanic.Core.Project.Models.Json;
using Shouldly;

namespace Mechanic.Core.Tests.Models;

public class GameFileTypeTest
{
    [Theory]
    [InlineData(GameFilesFileType.MAT, GameFileType.Material)]
    [InlineData(GameFilesFileType.DDS, GameFileType.DirectDrawSurface)]
    [InlineData(GameFilesFileType.WEM, GameFileType.WwiseEncodedMedia)]
    [InlineData(GameFilesFileType.PEX, GameFileType.PapyrusExecutable)]
    public void ToGameFileType_ReturnsExpectedValue(GameFilesFileType input, GameFileType expected)
    {
        var result = input.ToGameFileType();
        result.ShouldBe(expected);
    }

    [Fact]
    public void ToGameFileType_CastFromInt_ShouldThrowForInvalidValues()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => 
            ((GameFilesFileType)100).ToGameFileType());
            
        Should.Throw<ArgumentOutOfRangeException>(() => 
            ((GameFilesFileType)(-5)).ToGameFileType());
            
        Should.Throw<ArgumentOutOfRangeException>(() => 
            ((GameFilesFileType)int.MaxValue).ToGameFileType());
    }

    [Fact]
    public void ToGameFileType_DefaultEnumValue_ShouldHandleProperly()
    {
            var defaultValue = default(GameFilesFileType);
            
            if (Enum.IsDefined(typeof(GameFilesFileType), defaultValue))
            {
                Should.NotThrow(() => defaultValue.ToGameFileType());
            }
            else
            {
                Should.Throw<ArgumentOutOfRangeException>(() => 
                    defaultValue.ToGameFileType());
            }
    }
    
    [Theory]
    [InlineData(GameFileType.Material, GameFilesFileType.MAT)]
    [InlineData(GameFileType.DirectDrawSurface, GameFilesFileType.DDS)]
    [InlineData(GameFileType.WwiseEncodedMedia, GameFilesFileType.WEM)]
    [InlineData(GameFileType.PapyrusExecutable, GameFilesFileType.PEX)]
    public void ToJson_ReturnsExpectedValue(GameFileType expected, GameFilesFileType input)
    {
        var result = input.ToGameFileType();
        result.ShouldBe(expected);
    }
    
    [Fact]
    public void ToJson_CastFromInt_ShouldThrowForInvalidValues()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => 
            ((GameFileType)100).ToJson());
            
        Should.Throw<ArgumentOutOfRangeException>(() => 
            ((GameFileType)(-5)).ToJson());
            
        Should.Throw<ArgumentOutOfRangeException>(() => 
            ((GameFileType)int.MaxValue).ToJson());
    }
}