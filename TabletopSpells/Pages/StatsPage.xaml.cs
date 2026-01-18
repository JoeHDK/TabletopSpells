using TabletopSpells.ViewModels;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.Helpers;

namespace TabletopSpells.Pages;

public partial class StatsPage : ContentPage
{
    private readonly SharedViewModel sharedViewModel;
    private Character character;
    private List<Subclass> availableSubclasses = new();

    public StatsPage(Character character, SharedViewModel viewModel)
    {
        InitializeComponent();
        this.sharedViewModel = viewModel;
        this.character = character;

        LevelEntry.Text = character.Level.ToString();
        StrengthEntry.Text = character.AbilityScores["Strength"].ToString();
        DexterityEntry.Text = character.AbilityScores["Dexterity"].ToString();
        ConstitutionEntry.Text = character.AbilityScores["Constitution"].ToString();
        IntelligenceEntry.Text = character.AbilityScores["Intelligence"].ToString();
        WisdomEntry.Text = character.AbilityScores["Wisdom"].ToString();
        CharismaEntry.Text = character.AbilityScores["Charisma"].ToString();

        PopulateSubclassPicker();

        UpdateModifiers();
    }

    /// <summary>
    /// Populates the subclass picker with options for the character's class.
    /// </summary>
    private void PopulateSubclassPicker()
    {
        availableSubclasses = SubclassHelper.GetSubclassesForClass(character.CharacterClass);
        var displayNames = availableSubclasses.Select(s => SubclassHelper.GetDisplayName(s)).ToList();

        SubclassPicker.ItemsSource = displayNames;

        // Set the current selection
        var currentIndex = availableSubclasses.IndexOf(character.Subclass);
        if (currentIndex >= 0)
        {
            SubclassPicker.SelectedIndex = currentIndex;
        }

        // Handle selection changes
        SubclassPicker.SelectedIndexChanged += (s, e) =>
        {
            if (SubclassPicker.SelectedIndex >= 0 && SubclassPicker.SelectedIndex < availableSubclasses.Count)
            {
                character.Subclass = availableSubclasses[SubclassPicker.SelectedIndex];
            }
        };
    }

    /// <summary>
    /// Updates the modifier labels based on the ability score values entered.
    /// </summary>
    private void UpdateModifiers()
    {
        StrengthModifierLabel.Text = $"Modifier: {GetModifier(StrengthEntry.Text)}";
        DexterityModifierLabel.Text = $"Modifier: {GetModifier(DexterityEntry.Text)}";
        ConstitutionModifierLabel.Text = $"Modifier: {GetModifier(ConstitutionEntry.Text)}";
        IntelligenceModifierLabel.Text = $"Modifier: {GetModifier(IntelligenceEntry.Text)}";
        WisdomModifierLabel.Text = $"Modifier: {GetModifier(WisdomEntry.Text)}";
        CharismaModifierLabel.Text = $"Modifier: {GetModifier(CharismaEntry.Text)}";
    }

    /// <summary>
    /// Calculates the modifier for a given ability score.
    /// </summary>
    private int GetModifier(string scoreText)
    {
        if (int.TryParse(scoreText, out int score))
            return (score - 10) / 2;
        return 0;
    }

    /// <summary>
    /// Saves the updated stats back to the character through the SharedViewModel.
    /// </summary>
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Update character's stats
            character.Level = int.Parse(LevelEntry.Text);
            character.AbilityScores["Strength"] = int.Parse(StrengthEntry.Text);
            character.AbilityScores["Dexterity"] = int.Parse(DexterityEntry.Text);
            character.AbilityScores["Constitution"] = int.Parse(ConstitutionEntry.Text);
            character.AbilityScores["Intelligence"] = int.Parse(IntelligenceEntry.Text);
            character.AbilityScores["Wisdom"] = int.Parse(WisdomEntry.Text);
            character.AbilityScores["Charisma"] = int.Parse(CharismaEntry.Text);

            // Save changes using the shared view model
            await sharedViewModel.SaveCharacterAsync(character);

            await DisplayAlert("Success", "Character stats have been updated and saved successfully!", "OK");
            await Navigation.PopAsync();
        }
        catch
        {
            await DisplayAlert("Error", "Please ensure all stats are valid numbers.", "OK");
        }
    }

    /// <summary>
    /// Event handler to update the modifier labels in real-time when fields change.
    /// </summary>
    private void OnStatChanged(object sender, TextChangedEventArgs e)
    {
        UpdateModifiers();
    }
}