using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
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

    // Display spell details in a popup
    private async Task DisplaySpellDetails(Spell spell)
    {
        // Logic to display the spell details
        await DisplayAlert("Spell Details", $"{spell.Name}\n" +
                                            $"{spell.Duration}\n" +
                                            $"{spell.School}\n" +
                                            $"{spell.SavingThrow}\n" +
                                            $"{spell.CastingTime}\n" +
                                            $"{spell.Components}\n" +
                                            $"{spell.Description}\n",
                                            $"{spell.Range}\n" + "OK");
    }

    private List<Spell> GetAllSpellsFromJson()
    {
        try
        {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainPage)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("TabletopSpells.Resources.Spells.Pathfinder1e.json");

            if (stream == null) throw new InvalidOperationException("Could not load the spell data.");

            using var reader = new StreamReader(stream);
            var jsonContent = reader.ReadToEnd();
            var spells = JsonConvert.DeserializeObject<List<Spell>>(jsonContent);
            return spells ?? new List<Spell>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading spells from JSON: {ex.Message}");
            return new List<Spell>(); // Return an empty list on error
        }
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

    private async void OnCharacterSelected(object sender, SelectionChangedEventArgs e)
    {
        // Get the selected character
        var selectedCharacter = e.CurrentSelection.FirstOrDefault() as string;
        if (selectedCharacter != null)
        {
            // Navigate to the CharacterDetailPage with the selected character's name
            await Navigation.PushAsync(new CharacterDetailPage(selectedCharacter));
        }

        // Optionally clear selection
        ((CollectionView)sender).SelectedItem = null;
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
}