﻿using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using TabletopSpells.Helpers;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages;

public partial class CharacterOverviewPage : ContentPage
{
    public Character character;
    public Game gameType;
    public ObservableCollection<string> Spells { get; private set; }

    private SharedViewModel viewModel;

    public CharacterOverviewPage(Character character, SharedViewModel viewModel, Game gameType)
    {
        InitializeComponent();
        this.character = character;
        this.Title = $"{character.Name}'s home";
        this.viewModel = viewModel; // Use the passed viewModel

        this.BindingContext = this.viewModel;
        // Use SpellsForCharacter which calls LoadSpellsForCharacter and LoadPreparedSpells in correct order
        this.viewModel.SpellsForCharacter(character);

        this.gameType = gameType;
    }

    [Obsolete]
    private void OnSpellsSelected(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SpellsPage(character, SharedViewModel.Instance, gameType));
    }

    private void OnSpellPerDaySelected(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SpellsPerDayPage());
    }

    private void OnSpellLogSelected(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SpellLogPage(character));
    }
    
    private void OnStatsPageButtonClicked(object sender, EventArgs e)
    {
        // Navigate to StatsPage, passing the character and viewModel instances
        Navigation.PushAsync(new StatsPage(character, viewModel));
    }

    [Obsolete]
    private async void OnDeleteCharacterClicked(object sender, EventArgs e)
    {
        bool deleteConfirmed = await DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete {character.Name}?",
            "No",
            "Yes"
        );

        if (!deleteConfirmed)
        {
            await DeleteCharacter(character);
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); });
        }
    }

    private async Task DeleteCharacter(Character character)
    {
        try
        {
            var characters = LocalStorageHelper.LoadCharactersFromFile();
            var characterToRemove = characters.FirstOrDefault(c => c.ID == character.ID);
            if (characterToRemove != null)
            {
                characters.Remove(characterToRemove);
                LocalStorageHelper.SaveCharactersToFile(characters);

                viewModel.CharacterSpells.Remove(character.ID);
                LocalStorageHelper.DeleteCharacterFolder(character.ID.Value);

                viewModel.OnPropertyChanged(nameof(viewModel.CharacterSpells));
                await DisplayAlert("Success", $"{character.Name} has been removed.", "OK");
            }
            else
            {
                await DisplayAlert("Error", $"Character '{character.Name}' not found.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in DeleteCharacter: {ex.Message}");
            await DisplayAlert("Error", "An error occurred while deleting the character.", "OK");
        }
    }
}