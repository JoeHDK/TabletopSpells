using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using TabletopSpells.Models;
using TabletopSpells.Pages;

namespace TabletopSpells;
public partial class MainPage : ContentPage
{
    ObservableCollection<string> Characters
    {
        get; set;
    }

    public MainPage()
    {
        InitializeComponent();
        Characters = new ObservableCollection<string>();
        LoadCharacters();
        CharacterListView.ItemsSource = Characters;
        this.BindingContext = this; // Set the BindingContext
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadCharacters();
    }

    private List<Character> allCharacters = new List<Character>();

    private void LoadCharacters()
    {
        ObservableCollection<string> Characters = new ObservableCollection<string>();

        Preferences.Set("characters", "[]"); // Temporarily clear characters for testing

        string existingCharactersJson = Preferences.Get("characters", "[]");
        allCharacters = JsonConvert.DeserializeObject<List<Character>>(existingCharactersJson) ?? new List<Character>();

        Characters.Clear();
        foreach (var character in allCharacters)
        {
            Characters.Add(character.Name); // Populate with names for the UI
        }
    }


    private async void OnCreateNewCharacterClicked(object sender, EventArgs e)
    {
        string characterName = await DisplayPromptAsync("New Character", "Enter character name:");

        if (!string.IsNullOrWhiteSpace(characterName))
        {
            SaveCharacter(characterName);
        }
    }

    private async void OnCharacterSelected(object sender, SelectionChangedEventArgs e)
    {
        var selectedCharacterName = e.CurrentSelection.FirstOrDefault() as string;
        var selectedCharacter = allCharacters.FirstOrDefault(c => c.Name == selectedCharacterName);

        if (selectedCharacter != null)
        {
            await Navigation.PushAsync(new CharacterDetailPage(selectedCharacter));
        }

        ((CollectionView)sender).SelectedItem = null;
    }

    private void SaveCharacter(string characterName)
    {
        string existingCharactersJson = Preferences.Get("characters", "[]");
        var characters = JsonConvert.DeserializeObject<List<Character>>(existingCharactersJson) ?? new List<Character>();

        if (characters.All(c => c.Name != characterName))
        {
            var newCharacter = new Character { Name = characterName, ID = characters.Count + 1 };
            characters.Add(newCharacter);

            string updatedCharactersJson = JsonConvert.SerializeObject(characters);
            Preferences.Set("characters", updatedCharactersJson);

            Characters.Add(characterName); // Update ObservableCollection
        }
    }

}