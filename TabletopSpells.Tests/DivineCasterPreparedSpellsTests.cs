using TabletopSpells.ViewModels;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TabletopSpells.Tests;

public class DivineCasterPreparedSpellsTests
{
    private readonly SharedViewModel _vm;
    private readonly Character _cleric;

    public DivineCasterPreparedSpellsTests()
    {
        _vm = new SharedViewModel();
        _cleric = new Character
        {
            ID = Guid.NewGuid(),
            Name = "Shadowheart",
            GameType = Game.dnd5e,
            CharacterClass = Class.Cleric,
            Level = 5
        };
    }

    [Fact]
    public void Workflow_AddSpells_ThenPrepare_ThenVerifyPersistence()
    {
        // Step 1: Add three spells (they should NOT be castable yet)
        var spellA = new Spell { Id = Guid.NewGuid(), Name = "Guidance" };
        var spellB = new Spell { Id = Guid.NewGuid(), Name = "Healing Word" };
        var spellC = new Spell { Id = Guid.NewGuid(), Name = "Cure Wounds" };

        _vm.AddSpell(_cleric, spellA);
        _vm.AddSpell(_cleric, spellB);
        _vm.AddSpell(_cleric, spellC);

        // Verify all three are in known spells but none are prepared
        var knownSpells = _vm.SpellsForCharacter(_cleric);
        knownSpells.Should().HaveCount(3);
        _cleric.GetPreparedSpells().Should().BeEmpty("no spells have been prepared yet");

        // Step 2: Prepare spell A
        _cleric.TogglePreparedSpell(spellA);
        _vm.SavePreparedSpells(_cleric);

        // Verify A is prepared, B and C are not
        _cleric.GetPreparedSpells().Should().ContainSingle()
            .Which.Name.Should().Be("Guidance");

        // Step 3: Simulate app restart by reloading
        var newCleric = new Character
        {
            ID = _cleric.ID,
            Name = "Shadowheart",
            GameType = Game.dnd5e,
            CharacterClass = Class.Cleric,
            Level = 5
        };
        
        // Manually add known spells
        newCleric.AddSpell(spellA);
        newCleric.AddSpell(spellB);
        newCleric.AddSpell(spellC);

        // Save them to CharacterSpells dict
        var spellsList = new ObservableCollection<Spell>() { spellA, spellB, spellC };
        if (_cleric.ID != null)
        {
            var mockCharSpells = new Dictionary<Guid?, ObservableCollection<Spell>>
            {
                { newCleric.ID, spellsList }
            };
            typeof(SharedViewModel).GetProperty("CharacterSpells")
                ?.SetValue(_vm, mockCharSpells);
        }

        // Load prepared spells (this should restore A as prepared)
        _vm.LoadPreparedSpells(newCleric);

        // Verify A is still prepared after reload
        newCleric.GetPreparedSpells().Should().ContainSingle()
            .Which.Name.Should().Be("Guidance");
    }

    [Fact]
    public void PreparedSpell_CanBeCast()
    {
        var spell = new Spell { Id = Guid.NewGuid(), Name = "Cure Wounds" };
        _vm.AddSpell(_cleric, spell);
        _cleric.TogglePreparedSpell(spell);
        _vm.SavePreparedSpells(_cleric);

        // A prepared spell should be in GetPreparedSpells()
        _cleric.GetPreparedSpells().Should().Contain(spell);
    }

    [Fact]
    public void UnpreparedSpell_CannotBeCast()
    {
        var spell = new Spell { Id = Guid.NewGuid(), Name = "Cure Wounds" };
        _vm.AddSpell(_cleric, spell);

        // Without preparing, it should not be castable
        _cleric.GetPreparedSpells().Should().NotContain(spell);
    }

    [Fact]
    public void UnpreparingSpell_RemovesFromCastableList()
    {
        var spell = new Spell { Id = Guid.NewGuid(), Name = "Cure Wounds" };
        _vm.AddSpell(_cleric, spell);
        
        // Prepare it
        _cleric.TogglePreparedSpell(spell);
        _cleric.GetPreparedSpells().Should().Contain(spell);

        // Unprepare it
        _cleric.TogglePreparedSpell(spell);
        _cleric.GetPreparedSpells().Should().NotContain(spell);
    }
}

