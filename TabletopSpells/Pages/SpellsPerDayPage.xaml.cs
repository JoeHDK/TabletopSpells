using System.Collections.ObjectModel;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages;
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SpellsPerDayPage : ContentPage
{
    private ObservableCollection<SpellLevelViewModel> spellLevels;

    public SpellsPerDayPage()
    {
        InitializeComponent();
        spellLevels = new ObservableCollection<SpellLevelViewModel>();
        LoadSpellLevels();
        lvSpellsPerDay.ItemsSource = spellLevels;
    }

    private void LoadSpellLevels()
    {
        // Assume we have 10 spell levels (0-9)
        for (int i = 0; i <= 9; i++)
        {
            // Retrieve values from somewhere, here just as an example
            int maxSpells = 3;  // Placeholder
            int spellsUsed = 1;  // Placeholder
            // TODO - Get data from viewmodel

            spellLevels.Add(new SpellLevelViewModel
            {
                Level = i,
                MaxSpells = maxSpells,
                SpellsUsed = spellsUsed,
                DisplayText = $"Level {i} Spells",
                DetailText = $"Used: {spellsUsed} / Max: {maxSpells}"
            });
        }
    }

    private async void OnSpellLevelSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is SpellLevelViewModel selectedLevel)
        {
            // Reset selection
            lvSpellsPerDay.SelectedItem = null;

            // Prompt for new max spells
            string result = await DisplayPromptAsync("Max Spells", $"Set max spells for level {selectedLevel.Level}", initialValue: selectedLevel.MaxSpells.ToString());
            if (int.TryParse(result, out int newMax))
            {
                selectedLevel.MaxSpells = newMax;
                selectedLevel.DetailText = $"Used: {selectedLevel.SpellsUsed} / Max: {selectedLevel.MaxSpells}";
                // Update the actual character model or ViewModel here as needed
            }
        }
    }
}