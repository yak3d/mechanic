using Mechanic.Core.Models;
using Mechanic.Core.Project.Models.Json;
using Shouldly;

namespace Mechanic.Core.Tests.Models;

public class SourceFileTypeTest
{
        [Theory]
        [InlineData(SourceFilesFileType.FBX, SourceFileType.Fbx)]
        [InlineData(SourceFilesFileType.BLEND, SourceFileType.Blend)]
        [InlineData(SourceFilesFileType.TIFF, SourceFileType.Tiff)]
        [InlineData(SourceFilesFileType.WAV, SourceFileType.Wav)]
        [InlineData(SourceFilesFileType.PSC, SourceFileType.Psc)]
        [InlineData(SourceFilesFileType.OTHER, SourceFileType.Other)]
        public void FromJsonSourceFileType_AllValidValues_ShouldReturnExpectedMapping(
            SourceFilesFileType input, 
            SourceFileType expected)
        {
            var result = input.ToSourceFileType();
            
            result.ShouldBe(expected);
        }
        
        [Fact]
        public void FromJsonSourceFileType_AllDefinedEnumValues_ShouldNotThrowException()
        {
            var definedValues = Enum.GetValues<SourceFilesFileType>();
            
            foreach (var value in definedValues)
            {
                var result = Should.NotThrow(() => value.ToSourceFileType());
                result.ShouldBeOfType<SourceFileType>();
            }
        }

        [Fact]
        public void FromJsonSourceFileType_CastFromInt_ShouldThrowForInvalidValues()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => 
                ((SourceFilesFileType)100).ToSourceFileType());
            
            Should.Throw<ArgumentOutOfRangeException>(() => 
                ((SourceFilesFileType)(-5)).ToSourceFileType());
            
            Should.Throw<ArgumentOutOfRangeException>(() => 
                ((SourceFilesFileType)int.MaxValue).ToSourceFileType());
        }

        [Fact]
        public void FromJsonSourceFileType_DefaultEnumValue_ShouldHandleProperly()
        {
            var defaultValue = default(SourceFilesFileType);
            
            if (Enum.IsDefined(typeof(SourceFilesFileType), defaultValue))
            {
                Should.NotThrow(() => defaultValue.ToSourceFileType());
            }
            else
            {
                Should.Throw<ArgumentOutOfRangeException>(() => 
                    defaultValue.ToSourceFileType());
            }
        }
}