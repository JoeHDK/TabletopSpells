﻿using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using TabletopSpells.Models;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PrepareSpellsPage : ContentPage
    {
        private readonly PrepareSpellsViewModel _viewModel;
        private readonly List<Spell> _originalSpells;
        private readonly Character _character;

        /// <summary>
        /// Initialize the prepare spells modal.
        /// </summary>
        /// <param name="spells">The character's spell collection to filter added spells from</param>
        /// <param name="getMaxPrepared">Function that returns the max prepared spell count for this character</param>
        /// <param name="character">The character whose prepared spells are being modified</param>
        public PrepareSpellsPage(List<Spell> spells, Func<int> getMaxPrepared, Character character)
        {
            InitializeComponent();

            _originalSpells = spells ?? throw new ArgumentNullException(nameof(spells));
            _character = character ?? throw new ArgumentNullException(nameof(character));

            _viewModel = new PrepareSpellsViewModel(spells, getMaxPrepared);
            _viewModel.RequestClose += OnRequestClose;
            BindingContext = _viewModel;
        }

        private async void OnRequestClose(object? sender, PrepareSpellsViewModel.CloseEventArgs e)
        {
            if (e.ApplyChanges)
            {
                // Apply the IsPrepared state back to the original spell objects
                // AND add them to the character's known spells
                foreach (var item in _viewModel.AddedSpells)
                {
                    var original = FindOriginalSpell(item.Id);
                    if (original != null)
                    {
                        original.IsPrepared = item.IsPrepared;
                        
                        // If preparing, ensure spell is in known spells and toggle it as prepared
                        if (item.IsPrepared)
                        {
                            _character.AddSpell(original);
                            _character.TogglePreparedSpell(original);
                        }
                        else
                        {
                            // If unpreparing, toggle it off
                            _character.TogglePreparedSpell(original);
                        }
                    }
                }

                // Also save the spell objects to persistent storage for each prepared spell
                // This ensures the spell data is available when loading prepared spells later
                foreach (var spell in _character.GetPreparedSpells())
                {
                    SharedViewModel.Instance.SaveSpellForCharacter(_character, spell);
                }

                // Persist the prepared spells to storage
                SharedViewModel.Instance.SavePreparedSpells(_character);
                
                // Reload the character's prepared spells from storage so GetPreparedSpells() returns correct results
                // Clear the old list first
                _character.ClearManualllyPreparedSpells();
                // Then reload from storage
                SharedViewModel.Instance.LoadPreparedSpells(_character);
            }

            await Navigation.PopModalAsync();
        }

        private Spell? FindOriginalSpell(Guid id)
        {
            foreach (var spell in _originalSpells)
            {
                if (spell.Id == id)
                    return spell;
            }
            return null;
        }
    }
}

