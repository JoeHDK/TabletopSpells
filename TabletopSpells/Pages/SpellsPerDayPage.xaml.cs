using System.Collections.ObjectModel;
using TabletopSpells.Models;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages;
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SpellsPerDayPage : ContentPage
{
    public ObservableCollection<SpellLevelViewModel> SpellLevels { get; private set; } = new ObservableCollection<SpellLevelViewModel>();

    public SpellsPerDayPage()
    {
        InitializeComponent();
        // Bind the page to itself so templates resolve to SpellLevelViewModel
        BindingContext = this;
        LoadSpellLevels();
        lvSpellsPerDay.ItemsSource = SpellLevels;
    }

    private void LoadSpellLevels()
    {
        Character? character = SharedViewModel.Instance.CurrentCharacter;
        if (character == null)
        {
            // Use Dispatcher to ensure UI thread invocation
            Application.Current?.MainPage?.DisplayAlert("Error", "No character loaded.", "OK");
            return;
        }

        SpellLevels.Clear();

        // Ensure all spell levels are initialized (0 to 9 as example)
        for (int level = 1; level <= 9; level++)
        {
            int maxSpells = character.MaxSpellsPerDay.TryGetValue(level, out int max) ? max : 0;
            int spellsUsed = character.SpellsUsedToday.TryGetValue(level, out int used) ? used : 0;

            SpellLevels.Add(new SpellLevelViewModel
            {
                Level = level,
                MaxSpells = maxSpells,
                SpellsUsed = spellsUsed,
                DisplayText = $"Level {level} Spells"
            });

            if (!character.MaxSpellsPerDay.ContainsKey(level))
            {
                character.MaxSpellsPerDay[level] = 0;  // Ensure defaults are set if missing
            }
            if (!character.SpellsUsedToday.ContainsKey(level))
            {
                character.SpellsUsedToday[level] = 0;  // Ensure defaults are set if missing
            }
        }

        SharedViewModel.Instance.SaveSpellsPerDayDetails(character, character.MaxSpellsPerDay, character.SpellsUsedToday);
        OnPropertyChanged(nameof(SpellLevels));
    }

    private async void OnSpellLevelSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is SpellLevelViewModel selectedLevel)
        {
            // Deselect the item
            ((ListView)sender).SelectedItem = null;

            // Display prompt to edit max spells with the current max spells as the placeholder
            string result = await DisplayPromptAsync("Max Spells",
                                                     $"Enter a new max spells value for level {selectedLevel.Level} (current: {selectedLevel.MaxSpells})",
                                                     accept: "Save",
                                                     cancel: "Cancel",
                                                     initialValue: "", // Start with an empty input
                                                     placeholder: selectedLevel.MaxSpells.ToString()); // Show current max as a placeholder

            if (int.TryParse(result, out int newMax) && newMax != selectedLevel.MaxSpells)
            {
                // Update the model if the new value is different
                selectedLevel.MaxSpells = newMax;

                // Update the actual character model or ViewModel here as needed
                Character character = SharedViewModel.Instance.CurrentCharacter;
                if (character != null)
                {
                    character.MaxSpellsPerDay[selectedLevel.Level] = newMax;
                    SharedViewModel.Instance.SaveSpellsPerDayDetails(character,
                                                                    character.MaxSpellsPerDay,
                                                                    character.SpellsUsedToday);
                }

                // Refresh the UI
                OnPropertyChanged(nameof(SpellLevels)); // Ensure the UI updates
            }
        }
    }
    
    public void OnResetSpellsPerDayClicked(object sender, EventArgs e)
    {
        var sharedViewModel = SharedViewModel.Instance;
        sharedViewModel.ResetSpellsUsedToday();
        LoadSpellLevels();
    }
}