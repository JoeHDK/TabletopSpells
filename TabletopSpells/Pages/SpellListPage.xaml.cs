using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;

namespace TabletopSpells.Pages;
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SpellListPage : ContentPage
{
    private Game gameType;
    private Character? character;
    private int? selectedSpellLevel = null;
    private string currentSearchText = "";
    public SpellListPage(Character character, Game gameType)
    {
        InitializeComponent();
        this.gameType = gameType;
        Spells = new ObservableCollection<Spell>(GetAllSpellsFromJson(gameType));
        FilteredSpells = new ObservableCollection<Spell>(Spells);
        BindingContext = this;
        this.character = character;
    }

    public ObservableCollection<Spell> Spells
    {
        get; set;
    }
    public ObservableCollection<Spell> FilteredSpells
    {
        get; set;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        currentSearchText = e.NewTextValue.ToLower();
        FilterSpells(); // Apply all filters whenever search text changes
    }

    [Obsolete]
    private async void OnMenuClicked(object sender, EventArgs e)
    {
        // List of level options including Cantrips
        var levels = new List<string>
        {
            "Cantrips",
            "1st level",
            "2nd level",
            "3rd level",
            "4th level",
            "5th level",
            "6th level",
            "7th level",
            "8th level",
            "9th level"
        };

        // Highlight the active filter with an asterisk
        for (int i = 0; i < levels.Count; i++)
        {
            int levelNumber = i; // Cantrips are level 0
            if (selectedSpellLevel.HasValue && selectedSpellLevel.Value == levelNumber)
            {
                levels[i] = $"* {levels[i]}"; // Append an asterisk to highlight the active filter
            }
        }

        // Show the action sheet with the dynamically generated options
        string action = await DisplayActionSheet("Filters", null, null, levels.ToArray());

        // Early exit if 'Cancel' is selected or no action is returned
        if (action == "Cancel" || string.IsNullOrEmpty(action))
        {
            return;
        }

        // Strip any asterisk and extra spaces from the action to clean it up for parsing
        string cleanAction = action.Replace("*", "").Trim();
        int selectedLevel = 0; // Default to Cantrips if no number found
        var match = Regex.Match(cleanAction, @"\d+"); // Find the first number
        if (match.Success)
        {
            selectedLevel = int.Parse(match.Value);
        }
        else if (cleanAction == "Cantrips")
        {
            selectedLevel = 0; // Cantrips are considered level 0
        }

        if (selectedSpellLevel.HasValue && selectedSpellLevel.Value == selectedLevel)
        {
            selectedSpellLevel = null; // Toggle off if the same level is selected again
        }
        else
        {
            selectedSpellLevel = selectedLevel; // Set the selected level
        }

        FilterSpells(); // Re-apply filters based on the updated selected level
        UpdateTitle();  // Update the title to reflect the current filter status
    }

    private void UpdateTitle()
    {
        switch (selectedSpellLevel)
        {

            case null:
                Title = "Spells";
                break;
            case 0:
                Title = "Cantrips";
                break;
            case 1:
                Title = "1st level spells";
                break;
            case 2:
                Title = "2nd level spells";
                break;
            case 3:
                Title = "3rd level spells";
                break;
            default:
                Title = $"{selectedSpellLevel}th level spells";
                break;
        }
    }

    private void FilterSpells()
    {
        var filtered = Spells.Where(spell =>
            (string.IsNullOrEmpty(currentSearchText) || spell.Name.ToLower().Contains(currentSearchText)) &&
            (!selectedSpellLevel.HasValue || ParseSpellLevel(spell.SpellLevel, character.CharacterClass.ToString() ?? "") == selectedSpellLevel.Value)
        ).OrderBy(spell => spell.Name).ToList(); // Sort here and convert to list once

        FilteredSpells.Clear();
        foreach (var spell in filtered)
        {
            FilteredSpells.Add(spell);
        }
    }

    private int ParseSpellLevel(string spellLevel, string characterClass)
    {
        if (string.IsNullOrWhiteSpace(characterClass))
        {
            Debug.WriteLine("Character class is not specified.");
            return -1; // Indicate no specific class level found
        }

        string classLowerCase = characterClass.Trim().ToLower();
        string[] entries = spellLevel.Split(',');

        foreach (var entry in entries)
        {
            var trimmedEntry = entry.Trim().ToLower();
            // Split the entry into parts to isolate class names and level number
            string[] parts = trimmedEntry.Split(' ');
            if (parts.Length < 2)
            {
                Debug.WriteLine($"Invalid spell level format in entry: '{entry}'");
                continue;
            }

            // Checking for the presence of the class name in the combined class names section
            string combinedClasses = parts[0];
            if (combinedClasses.Contains(classLowerCase))
            {
                // Extract the level number which is supposed to be the last part after a space
                string levelPart = parts[1];
                var match = Regex.Match(levelPart, @"\d+");
                if (match.Success)
                {
                    return int.Parse(match.Value);
                }
                else
                {
                    Debug.WriteLine($"No numeric level found for class {characterClass} in part '{entry}'.");
                }
            }
        }

        Debug.WriteLine($"Class {characterClass} not found in spellLevel '{spellLevel}'.");
        return -1; // Return an invalid level if not found
    }
    

    private List<Spell> GetAllSpellsFromJson(Game gameType)
    {
        try
        {
            Stream? stream = null;
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
            switch (gameType)
            {
                case Game.pathfinder1e:
                    stream = assembly.GetManifestResourceStream("TabletopSpells.Spells.Pathfinder1e.json");
                    break;
                case Game.dnd5e:
                    stream = assembly.GetManifestResourceStream("TabletopSpells.Spells.dnd 5e.json");
                    break;
                default:
                    stream = null;
                    break;
            }

            if (stream == null)
            {
                Debug.WriteLine("Spell data stream not found.");
                return new List<Spell>();
            }

            using var reader = new StreamReader(stream);
            var jsonContent = reader.ReadToEnd();

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore,
                Error = (sender, args) =>
                {
                    Debug.WriteLine("JSON Error: " + args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            };

            var spells = JsonConvert.DeserializeObject<List<Spell>>(jsonContent, settings);

            if (spells == null)
            {
                Debug.WriteLine("Failed to deserialize spells.");
                return new List<Spell>();
            }

            return spells;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception occurred while loading spells: {ex}");
            return new List<Spell>();
        }
    }

    private async void OnSpellSelected(object sender, SelectionChangedEventArgs e)
    {
        var selectedSpell = e.CurrentSelection.FirstOrDefault() as Spell;
        if (selectedSpell != null)
        {
            int spellLevel = ParseSpellLevel(selectedSpell.SpellLevel, character.CharacterClass.ToString() ?? "");
            
            await Navigation.PushAsync(new SpellDetailPage(selectedSpell, character, spellLevel, gameType));
            
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}