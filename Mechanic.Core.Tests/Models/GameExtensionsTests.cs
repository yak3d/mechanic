using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Mechanic.Core.Models;
using Shouldly;

namespace Mechanic.Core.Tests.Models;

public class GameExtensionsTests
{
    [Theory]
    [InlineData(Game.Tes4Oblivion)]
    [InlineData(Game.Tes5Skyrim)]
    [InlineData(Game.SkyrimSpecialEdition)]
    [InlineData(Game.Fallout3)]
    [InlineData(Game.FalloutNewVegas)]
    [InlineData(Game.Fallout4)]
    [InlineData(Game.Starfield)]
    public void GetDisplayName_WhenGameHasDisplayAttribute_ShouldReturnDisplayName(Game game)
    {
        var result = game.GetDisplayName();

        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();

        var enumString = game.ToString();
        var field = game.GetType().GetField(enumString);
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
    [InlineData(Game.Tes4Oblivion, "The Elder Scrolls IV: Oblivion")]
    [InlineData(Game.Tes5Skyrim, "The Elder Scrolls V: Skyrim")]
    [InlineData(Game.SkyrimSpecialEdition, "The Elder Scrolls V: Skyrim Special Edition")]
    [InlineData(Game.Fallout3, "Fallout 3")]
    [InlineData(Game.FalloutNewVegas, "Fallout: New Vegas")]
    [InlineData(Game.Fallout4, "Fallout 4")]
    [InlineData(Game.Starfield, "Starfield")]
    public void GetDisplayName_ShouldReturnExpectedDisplayNames(Game game, string expectedDisplayName)
    {
        var result = game.GetDisplayName();

        result.ShouldBe(expectedDisplayName);
    }

    [Fact]
    public void GetDisplayName_WhenCalledMultipleTimes_ShouldReturnConsistentResults()
    {
        var game = Game.Tes5Skyrim;

        var result1 = game.GetDisplayName();
        var result2 = game.GetDisplayName();

        result1.ShouldBe(result2);
    }

    [Theory]
    [InlineData(Game.Tes4Oblivion)]
    [InlineData(Game.Tes5Skyrim)]
    [InlineData(Game.SkyrimSpecialEdition)]
    [InlineData(Game.Fallout3)]
    [InlineData(Game.FalloutNewVegas)]
    [InlineData(Game.Fallout4)]
    [InlineData(Game.Starfield)]
    public void GetDisplayName_ShouldNotReturnNullOrEmpty(Game game)
    {
        var result = game.GetDisplayName();

        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldNotBeNullOrWhiteSpace();
    }
}
