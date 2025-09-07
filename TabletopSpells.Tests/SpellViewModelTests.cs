using TabletopSpells.ViewModels;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using Xunit;
using FluentAssertions;

namespace TabletopSpells.Tests;

public class SpellViewModelTests
{
    [Fact]
    public void SpellViewModel_Reflects_Spell_Name()
    {
        var spell = new Spell { Name = "Magic Missile" };
        var character = new Character { Name = "Test", GameType = Game.dnd5e, CharacterClass = Class.Wizard };
        var vm = new SpellViewModel(spell, character);
        vm.Spell.Name.Should().Be("Magic Missile");
    }
}
