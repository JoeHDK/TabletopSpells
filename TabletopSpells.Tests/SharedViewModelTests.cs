using TabletopSpells.ViewModels;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using Xunit;
using FluentAssertions;

namespace TabletopSpells.Tests;

public class SharedViewModelTests
{
    [Fact]
    public void AddSpell_ForDivineCaster_MarksSpellAsPrepared()
    {
        var vm = new SharedViewModel();
        var character = new Character { Name = "Divine", GameType = Game.dnd5e, CharacterClass = Class.Cleric };
        var spell = new Spell { Name = "Cure Wounds" };
        vm.AddSpell(character, spell);
        character.GetPreparedSpells().Should().Contain(spell);
    }
}
