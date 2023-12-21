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

    private static List<string>? GetExistingCharacters()
    {
        string existingCharactersJson = Preferences.Get("characters", "[]");
        var characters = JsonConvert.DeserializeObject<List<string>>(existingCharactersJson);
        return characters;
    }

    private void LoadCharacters()
    {
        List<string>? characters = GetExistingCharacters();

        if (characters.Count != Characters.Count)
        {
            Characters.Clear();
            foreach (string character in characters)
            {
                Characters.Add(character);
            }
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
        List<string>? characters = GetExistingCharacters();

        // Check if the character already exists
        if (!characters.Contains(characterName))
        {
            characters.Add(characterName);

            // Save updated list
            string updatedCharactersJson = JsonConvert.SerializeObject(characters);
            Preferences.Set("characters", updatedCharactersJson);

            // Update ObservableCollection directly
            Characters.Add(characterName);
        }
    }


    private async void OnManageCharactersClicked(object sender, EventArgs e)
    {
        string result = await DisplayActionSheet("Remove Character", "Cancel", null, Characters.ToArray());

        if (!string.IsNullOrWhiteSpace(result) && result != "Cancel")
        {
            bool delete = await DisplayAlert("Confirm Delete", $"Delete '{result}'?", "Yes", "No");
            if (delete)
            {
                DeleteCharacter(result);
            }
        }
    }

    private void DeleteCharacter(string characterName)
    {
        // Retrieve the current list of characters
        string existingCharactersJson = Preferences.Get("characters", "[]");
        var characters = JsonConvert.DeserializeObject<List<string>>(existingCharactersJson);

        // Remove the selected character
        characters.Remove(characterName);

        // Save the updated list
        string updatedCharactersJson = JsonConvert.SerializeObject(characters);
        Preferences.Set("characters", updatedCharactersJson);

        // Update the ObservableCollection
        Characters.Remove(characterName);
    }
}