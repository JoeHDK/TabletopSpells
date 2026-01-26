﻿using TabletopSpells.ViewModels;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using Xunit;
using FluentAssertions;

namespace TabletopSpells.Tests;

public class SharedViewModelTests
{
    [Fact]
    public void AddSpell_ForDivineCaster_AddsToKnownSpells()
    {
        var vm = new SharedViewModel();
        var character = new Character { Name = "Divine", GameType = Game.dnd5e, CharacterClass = Class.Cleric };
        var spell = new Spell { Name = "Cure Wounds" };
        vm.AddSpell(character, spell);
        
        // Adding a spell doesn't automatically prepare it
        vm.SpellsForCharacter(character).Should().Contain(spell);
        character.GetPreparedSpells().Should().NotContain(spell);
    }

    [Fact]
    public void PrepareSpell_ForDivineCaster_MarkSpellAsPrepared()
    {
        var vm = new SharedViewModel();
        var character = new Character { Name = "Divine", GameType = Game.dnd5e, CharacterClass = Class.Cleric };
        var spell = new Spell { Name = "Cure Wounds" };
        vm.AddSpell(character, spell);
        
        // Now explicitly prepare it
        character.TogglePreparedSpell(spell);
        character.GetPreparedSpells().Should().Contain(spell);
    }
}
