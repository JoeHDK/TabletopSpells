using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using Xunit;
using FluentAssertions;

namespace TabletopSpells.Tests;

public class CharacterTests
{
    [Fact]
    public void Setting_Cleric_Class_Sets_IsDivineCaster_True()
    {
        var character = new Character { Name = "Test", GameType = Game.dnd5e, CharacterClass = Class.Cleric };
        character.IsDivineCaster.Should().BeTrue();
    }

    [Fact]
    public void Setting_Wizard_Class_Sets_IsDivineCaster_False()
    {
        var character = new Character { Name = "Test", GameType = Game.dnd5e, CharacterClass = Class.Wizard };
        character.IsDivineCaster.Should().BeFalse();
    }
}
