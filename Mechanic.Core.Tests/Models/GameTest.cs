using Mechanic.Core.Models;
using Mechanic.Core.Project.Models.Json;
using Shouldly;

namespace Mechanic.Core.Tests.Models;

public class GameTest
{
    [Theory]
    [InlineData(MechanicProjectGame.TES4Oblivion, Game.Tes4Oblivion)]
    [InlineData(MechanicProjectGame.TES5Skyrim, Game.Tes5Skyrim)]
    [InlineData(MechanicProjectGame.SkyrimSpecialEdition, Game.SkyrimSpecialEdition)]
    [InlineData(MechanicProjectGame.Fallout3, Game.Fallout3)]
    [InlineData(MechanicProjectGame.FalloutNewVegas, Game.FalloutNewVegas)]
    [InlineData(MechanicProjectGame.Fallout4, Game.Fallout4)]
    [InlineData(MechanicProjectGame.Starfield, Game.Starfield)]
    public void ToGame_ValidMechanicProjectGame_ReturnsCorrectGame(MechanicProjectGame input, Game expected)
    {
        var result = input.FromJsonGame();

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(Game.Tes4Oblivion, MechanicProjectGame.TES4Oblivion)]
    [InlineData(Game.Tes5Skyrim, MechanicProjectGame.TES5Skyrim)]
    [InlineData(Game.SkyrimSpecialEdition, MechanicProjectGame.SkyrimSpecialEdition)]
    [InlineData(Game.Fallout3, MechanicProjectGame.Fallout3)]
    [InlineData(Game.FalloutNewVegas, MechanicProjectGame.FalloutNewVegas)]
    [InlineData(Game.Fallout4, MechanicProjectGame.Fallout4)]
    [InlineData(Game.Starfield, MechanicProjectGame.Starfield)]
    public void ToJson_ConvertsEachEnum_Properly(Game input, MechanicProjectGame expected)
    {
        var result = input.ToJson();

        result.ShouldBe(expected);
    }

    [Fact]
    public void ToGameFileType_CastFromInt_ShouldThrowForInvalidValues()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            ((MechanicProjectGame)100).FromJsonGame());

        Should.Throw<ArgumentOutOfRangeException>(() =>
            ((MechanicProjectGame)(-5)).FromJsonGame());

        Should.Throw<ArgumentOutOfRangeException>(() =>
            ((MechanicProjectGame)int.MaxValue).FromJsonGame());
    }

    [Fact]
    public void ToJson_CastFromInt_ShouldThrowForInvalidValues()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => ((Game)100).ToJson());
    }
}