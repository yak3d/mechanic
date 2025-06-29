using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Mechanic.Core.Models;
using Shouldly;

namespace Mechanic.Core.Tests.Models;

public class GameExtensionsTests
{
    [Theory]
    [InlineData(GameName.Tes4Oblivion)]
    [InlineData(GameName.Tes5Skyrim)]
    [InlineData(GameName.SkyrimSpecialEdition)]
    [InlineData(GameName.Fallout3)]
    [InlineData(GameName.FalloutNewVegas)]
    [InlineData(GameName.Fallout4)]
    [InlineData(GameName.Starfield)]
    public void GetDisplayName_WhenGameHasDisplayAttribute_ShouldReturnDisplayName(GameName gameName)
    {
        var result = gameName.GetDisplayName();

        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();

        var enumString = gameName.ToString();
        var field = gameName.GetType().GetField(enumString);
        var displayAttribute = field?.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute?.Name != null)
        {
            result.ShouldBe(displayAttribute.Name);
        }
        else
        {
            result.ShouldBe(enumString);
        }
    }

    [Theory]
    [InlineData(GameName.Tes4Oblivion, "The Elder Scrolls IV: Oblivion")]
    [InlineData(GameName.Tes5Skyrim, "The Elder Scrolls V: Skyrim")]
    [InlineData(GameName.SkyrimSpecialEdition, "The Elder Scrolls V: Skyrim Special Edition")]
    [InlineData(GameName.Fallout3, "Fallout 3")]
    [InlineData(GameName.FalloutNewVegas, "Fallout: New Vegas")]
    [InlineData(GameName.Fallout4, "Fallout 4")]
    [InlineData(GameName.Starfield, "Starfield")]
    public void GetDisplayName_ShouldReturnExpectedDisplayNames(GameName gameName, string expectedDisplayName)
    {
        var result = gameName.GetDisplayName();

        result.ShouldBe(expectedDisplayName);
    }

    [Fact]
    public void GetDisplayName_WhenCalledMultipleTimes_ShouldReturnConsistentResults()
    {
        var game = GameName.Tes5Skyrim;

        var result1 = game.GetDisplayName();
        var result2 = game.GetDisplayName();

        result1.ShouldBe(result2);
    }

    [Theory]
    [InlineData(GameName.Tes4Oblivion)]
    [InlineData(GameName.Tes5Skyrim)]
    [InlineData(GameName.SkyrimSpecialEdition)]
    [InlineData(GameName.Fallout3)]
    [InlineData(GameName.FalloutNewVegas)]
    [InlineData(GameName.Fallout4)]
    [InlineData(GameName.Starfield)]
    public void GetDisplayName_ShouldNotReturnNullOrEmpty(GameName gameName)
    {
        var result = gameName.GetDisplayName();

        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldNotBeNullOrWhiteSpace();
    }
}
