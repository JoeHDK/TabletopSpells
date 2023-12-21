using Newtonsoft.Json;
using System.Collections.ObjectModel;

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

    private void LoadCharacters()
    {
        string existingCharactersJson = Preferences.Get("characters", "[]");
        var characters = JsonConvert.DeserializeObject<List<string>>(existingCharactersJson);

        Characters.Clear();
        foreach (var character in characters)
        {
            Characters.Add(character);
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
    private void SaveCharacter(string characterName)
    {
        // Retrieve existing characters
        string existingCharactersJson = Preferences.Get("characters", "[]");
        var characters = JsonConvert.DeserializeObject<List<string>>(existingCharactersJson);


        // Add new character
        if (!characters.Contains(characterName))
        {   
            characters.Add(characterName);
            LoadCharacters();
        }

        // Save updated list
        string updatedCharactersJson = JsonConvert.SerializeObject(characters);
        Preferences.Set("characters", updatedCharactersJson);
    }

}