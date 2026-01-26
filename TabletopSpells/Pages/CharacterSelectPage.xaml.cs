﻿using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TabletopSpells.Helpers;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.Pages;
using TabletopSpells.ViewModels;

namespace TabletopSpells;

public partial class CharacterSelectPage : ContentPage
{
    public Game gameType;
    public Character character;
    
    public ObservableCollection<Character> Characters { get; set; } = new();

    public CharacterSelectPage(Game gameType)
    {
        InitializeComponent();
        this.gameType = gameType;

        LoadCharacters();
        CharacterListView.ItemsSource = Characters;
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadCharacters();
    }

    private void LoadCharacters()
    {
        Characters.Clear();
        var characters = GetExistingCharacters();

        bool charactersUpdated = false;

        foreach (var character in characters)
        {
            if (character.ID == null || character.ID == Guid.Empty)
            {
                character.ID = Guid.NewGuid();
                charactersUpdated = true;
            }

            // Ensure IsDivineCaster is set (covers edge cases where it might be missing from storage)
            character.IsDivineCaster = ClassHelper.IsDivineCaster(character.CharacterClass);

            if (character.GameType == gameType)
                Characters.Add(character);
        }

        if (charactersUpdated)
        {
            SaveCharacters(characters);
        }
    }

    private void SaveCharacters(List<Character> characters)
    {
        LocalStorageHelper.SaveCharactersToFile(characters);
    }

    private List<Character> GetExistingCharacters()
    {
        try
        {
            return LocalStorageHelper.LoadCharactersFromFile();
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"JSON Error: {ex.Message}");
            return new();
        }
    }

    private void SaveCharacter(Character character)
    {
        var characters = GetExistingCharacters();

        if (characters.All(c => c.Name != character.Name))
        {
            characters.Add(character);
            SaveCharacters(characters);
            Characters.Add(character);
        }
    }

    private async void OnCreateNewCharacterClicked(object sender, EventArgs e)
    {
        string? characterName = await DisplayPromptAsync("New Character", "Enter character name:");

        if (string.IsNullOrWhiteSpace(characterName))
            return;

        var classOptions = ClassHelper.GetClassesByGame(gameType)
                                      .Select(c => c.ToString())
                                      .OrderBy(c => c)
                                      .ToArray();

        if (classOptions.Length == 0)
        {
            await DisplayAlert("Error", "No classes available for the selected game.", "OK");
            return;
        }

        string selectedClass = await DisplayActionSheet("Select Class", "Cancel", null, classOptions);

        if (string.IsNullOrWhiteSpace(selectedClass) || selectedClass == "Cancel")
            return;

        if (Enum.TryParse(selectedClass, out Class characterClass))
        {
            var newCharacter = new Character
            {
                Name = characterName,
                CharacterClass = characterClass,
                GameType = gameType,
                ID = Guid.NewGuid(),
                IsDivineCaster = ClassHelper.IsDivineCaster(characterClass)
            };

            SaveCharacter(newCharacter);
        }
    }

    [Obsolete]
    private async void OnCharacterSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Character selectedCharacter)
        {
            SharedViewModel.Instance.CurrentCharacter = selectedCharacter;
            // Don't call LoadPreparedSpells here - it will be called automatically by SpellsForCharacter
            // when CharacterOverviewPage calls LoadSpellsForCharacter
            await Navigation.PushAsync(new CharacterOverviewPage(selectedCharacter, SharedViewModel.Instance, gameType));
            LoadCharacters();
        }

        ((CollectionView)sender).SelectedItem = null;
    }
}
