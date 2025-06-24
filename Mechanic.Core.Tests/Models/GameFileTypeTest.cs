using Mechanic.Core.Models;
using Mechanic.Core.Project.Models.Json;
using Shouldly;

namespace Mechanic.Core.Tests.Models;

public class GameFileTypeTest
{
    [Theory]
    [InlineData(GameFilesFileType.MAT, GameFileType.Mat)]
    [InlineData(GameFilesFileType.DDS, GameFileType.Dds)]
    [InlineData(GameFilesFileType.WEM, GameFileType.Wem)]
    [InlineData(GameFilesFileType.PEX, GameFileType.Pex)]
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
}