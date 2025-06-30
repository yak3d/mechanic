using Mechanic.Core.Models;
using Mechanic.Core.Project.Models.Json;
using Shouldly;
using GameName = Mechanic.Core.Models.GameName;

namespace Mechanic.Core.Tests.Models;

public class GameNameTest
{
    [Theory]
    [InlineData(Project.Models.Json.GameName.TES4Oblivion, GameName.Tes4Oblivion)]
    [InlineData(Project.Models.Json.GameName.TES5Skyrim, GameName.Tes5Skyrim)]
    [InlineData(Project.Models.Json.GameName.SkyrimSpecialEdition, GameName.SkyrimSpecialEdition)]
    [InlineData(Project.Models.Json.GameName.Fallout3, GameName.Fallout3)]
    [InlineData(Project.Models.Json.GameName.FalloutNewVegas, GameName.FalloutNewVegas)]
    [InlineData(Project.Models.Json.GameName.Fallout4, GameName.Fallout4)]
    [InlineData(Project.Models.Json.GameName.Starfield, GameName.Starfield)]
    public void FromJsonGameReturnsCorrectGame(Project.Models.Json.GameName input, GameName expected)
    {
        var result = input.FromJsonGame();

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(GameName.Tes4Oblivion, Project.Models.Json.GameName.TES4Oblivion)]
    [InlineData(GameName.Tes5Skyrim, Project.Models.Json.GameName.TES5Skyrim)]
    [InlineData(GameName.SkyrimSpecialEdition, Project.Models.Json.GameName.SkyrimSpecialEdition)]
    [InlineData(GameName.Fallout3, Project.Models.Json.GameName.Fallout3)]
    [InlineData(GameName.FalloutNewVegas, Project.Models.Json.GameName.FalloutNewVegas)]
    [InlineData(GameName.Fallout4, Project.Models.Json.GameName.Fallout4)]
    [InlineData(GameName.Starfield, Project.Models.Json.GameName.Starfield)]
    public void ToJson_ConvertsEachEnum_Properly(GameName input, Project.Models.Json.GameName expected)
    {
        var result = input.ToJson();

        result.ShouldBe(expected);
    }

    [Fact]
    public void ToGameFileType_CastFromInt_ShouldThrowForInvalidValues()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            ((Project.Models.Json.GameName)100).FromJsonGame());

        Should.Throw<ArgumentOutOfRangeException>(() =>
            ((Project.Models.Json.GameName)(-5)).FromJsonGame());

        Should.Throw<ArgumentOutOfRangeException>(() =>
            ((Project.Models.Json.GameName)int.MaxValue).FromJsonGame());
    }

    [Fact]
    public void ToJson_CastFromInt_ShouldThrowForInvalidValues()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => ((GameName)100).ToJson());
    }
}