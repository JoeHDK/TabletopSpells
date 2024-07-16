using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TabletopSpells.Helpers;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.Pages;

namespace TabletopSpells;
public partial class CharacterSelectPage : ContentPage
{
    public Game gameType;
    public Character character;
    ObservableCollection<Character> Characters
    {
        get; set;
    }
    
    public CharacterSelectPage(Game gameType)
    {
        InitializeComponent();
        Characters = new ObservableCollection<Character>();
        LoadCharacters();
        CharacterListView.ItemsSource = Characters;
        this.BindingContext = this; // Set the BindingContext
        this.gameType = gameType;

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadCharacters();  // Reload characters each time the page appears
    }

    private void LoadCharacters()
    {
        Characters.Clear();  // Clear existing items
        var characters = GetExistingCharacters();

        bool charactersUpdated = false;

        foreach (var character in characters)
        {
            // Assign an ID if it is null
            if (character.ID == null || character.ID == Guid.Empty)
            {
                character.ID = Guid.NewGuid();
                charactersUpdated = true;
            }

            if (character.GameType == gameType)
                Characters.Add(character);
        }

        // Save updated characters back to preferences if any character was updated
        if (charactersUpdated)
        {
            SaveCharacters(characters);
        }
    }

    private void SaveCharacters(List<Character> characters)
    {
        string updatedCharactersJson = JsonConvert.SerializeObject(characters);
        Preferences.Set("characters", updatedCharactersJson);
    }

    private async void OnCreateNewCharacterClicked(object sender, EventArgs e)
    {
        string? characterName = await DisplayPromptAsync("New Character", "Enter character name:");

        if (!string.IsNullOrWhiteSpace(characterName))
        {
            // Get the names of the classes from the enum
            var classOptions = ClassHelper.GetClassesByGame(gameType)
                                          .Select(c => c.ToString())
                                          .OrderBy(c => c)
                                          .ToArray();

            if (classOptions == null || classOptions.Length == 0)
            {
                await DisplayAlert("Error", "No classes available for the selected game.", "OK");
                return;
            }

            // Display a list of classes to choose from
            string selectedClass = await DisplayActionSheet("Select Class", "Cancel", null, classOptions);

            if (!string.IsNullOrWhiteSpace(selectedClass) && selectedClass != "Cancel")
            {
                // Parse the selected class
                if (Enum.TryParse(selectedClass, out Class characterClass))
                {
                    // Create a new character with the entered name and selected class
                    var newCharacter = new Character
                    {
                        Name = characterName,
                        CharacterClass = characterClass,
                        GameType = gameType,
                        ID = Guid.NewGuid() // Assign a new unique ID
                    };

                    SaveCharacter(newCharacter);
                }
            }
        }
    }


    [Obsolete]
    private async void OnCharacterSelected(object sender, SelectionChangedEventArgs e)
    {
        var selectedCharacter = e.CurrentSelection.FirstOrDefault() as Character;
        if (selectedCharacter != null)
        {
            SharedViewModel.Instance.CurrentCharacter = selectedCharacter;
            await Navigation.PushAsync(new CharacterOverviewPage(selectedCharacter, SharedViewModel.Instance, gameType));

            LoadCharacters();
        }

    ((CollectionView)sender).SelectedItem = null;
    }

    private void SaveCharacter(Character character)
    {
        var characters = GetExistingCharacters();

        if (characters.All(c => c.Name != character.Name))
        {
            
            var newCharacter = new Character { Name = character.Name, CharacterClass = character.CharacterClass, GameType = gameType, ID = new Guid() };
            characters.Add(newCharacter);

            string updatedCharactersJson = JsonConvert.SerializeObject(characters);
            Preferences.Set("characters", updatedCharactersJson);

            Characters.Add(newCharacter); // Add the whole character object to the ObservableCollection
        }
    }

    private List<Character> GetExistingCharacters()
    {
        try
        {
            string existingCharactersJson = Preferences.Get("characters", "[]");
            return JsonConvert.DeserializeObject<List<Character>>(existingCharactersJson) ?? new List<Character>();
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"JSON Error: {ex.Message}");
            return new List<Character>();
        }
    }

}