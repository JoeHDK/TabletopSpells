using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using TabletopSpells.Models;

namespace TabletopSpells.Pages;
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SpellsPerDayPage : ContentPage
{
    private ObservableCollection<SpellLevelViewModel> spellLevels;

    public SpellsPerDayPage()
    {
        InitializeComponent();
        BindingContext = SharedViewModel.Instance;
        spellLevels = new ObservableCollection<SpellLevelViewModel>();
        LoadSpellLevels();
        lvSpellsPerDay.ItemsSource = spellLevels;
    }

    private void LoadSpellLevels()
    {
        Character character = SharedViewModel.Instance.CurrentCharacter;
        if (character == null)
        {
            DisplayAlert("Error", "No character loaded.", "OK");
            return;
        }

        spellLevels.Clear();

        // Ensure all spell levels are initialized (0 to 9 as example)
        for (int level = 1; level <= 9; level++)
        {
            // Get or set default max spells and spells used
            int maxSpells = character.MaxSpellsPerDay.TryGetValue(level, out int max) ? max : 0;
            int spellsUsed = character.SpellsUsedToday.TryGetValue(level, out int used) ? used : 0;

            spellLevels.Add(new SpellLevelViewModel
            {
                Level = level,
                MaxSpells = maxSpells,
                SpellsUsed = spellsUsed,
                DisplayText = $"Level {level} Spells"
                // DetailText is computed automatically, no need to set it here
            });

            // Check and update defaults if necessary
            if (!character.MaxSpellsPerDay.ContainsKey(level))
            {
                character.MaxSpellsPerDay[level] = 0;  // Ensure defaults are set if missing
            }
            if (!character.SpellsUsedToday.ContainsKey(level))
            {
                character.SpellsUsedToday[level] = 0;  // Ensure defaults are set if missing
            }
        }

        // Save any changes if defaults were added
        SharedViewModel.Instance.SaveSpellsPerDayDetails(character, character.MaxSpellsPerDay, character.SpellsUsedToday);
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
                OnPropertyChanged(nameof(SpellLevelViewModel.DetailText)); // Ensure the UI updates
            }
        }
    }
    
    public async void OnResetSpellsPerDayClicked(object sender, EventArgs e)
    {
        SharedViewModel sharedViewModel = SharedViewModel.Instance;
        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Confirm Reset",
            "Reset all spells used today to zero?",
            "Yes", "No");
        
        if (confirm)
        {
            sharedViewModel.ResetSpellsUsedToday();
            await Navigation.PopAsync();
        }
    }
}