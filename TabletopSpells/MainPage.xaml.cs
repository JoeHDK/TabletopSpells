using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.Pages;

namespace TabletopSpells;
public partial class MainPage : ContentPage
{
    ObservableCollection<Character> Characters
    {
        get; set;
    }

    public MainPage()
    {
        InitializeComponent();
        Characters = new ObservableCollection<Character>();
        LoadCharacters();
        CharacterListView.ItemsSource = Characters;
        this.BindingContext = this; // Set the BindingContext
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
            // Get the names of the classes from the enum
            var classOptions = Enum.GetNames(typeof(Class)).OrderBy(c => c).ToArray();

            // Display a list of classes to choose from
            string selectedClass = await DisplayActionSheet("Select Class", "Cancel", null, classOptions);

            if (!string.IsNullOrWhiteSpace(selectedClass) && selectedClass != "Cancel")
            {
                // Parse the selected class
                if (Enum.TryParse(selectedClass, out Class characterClass))
                {
                    SaveCharacter(characterName, characterClass);
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
            await Navigation.PushAsync(new CharacterOverviewPage(selectedCharacter.Name, SharedViewModel.Instance));

            LoadCharacters();
        }

    ((CollectionView)sender).SelectedItem = null;
    }


    private void SaveCharacter(string characterName, Class characterClass)
    {
        var characters = GetExistingCharacters();

        if (characters.All(c => c.Name != characterName))
        {
            var newCharacter = new Character { Name = characterName, CharacterClass = characterClass };
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