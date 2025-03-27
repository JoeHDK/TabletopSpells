using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages;

public partial class CharacterOverviewPage : ContentPage
{
    public Character character;
    public Game gameType;
    public ObservableCollection<string> Spells { get; private set; }

    private SharedViewModel viewModel;

    public CharacterOverviewPage(Character character, SharedViewModel viewModel, Game gameType)
    {
        InitializeComponent();
        this.character = character;
        this.Title = $"{character.Name}'s home";
        this.viewModel = viewModel; // Use the passed viewModel

        this.BindingContext = this.viewModel;
        this.viewModel.LoadSpellsForCharacter(character);

        this.gameType = gameType;
    }

    [Obsolete]
    private void OnCharacterSelected(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CharacterDetailPage(character, SharedViewModel.Instance, gameType));
    }

    private void OnSpellPerDaySelected(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SpellsPerDayPage());
    }

    private void OnSpellLogSelected(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SpellLogPage(character));
    }
    
    private void OnStatsPageButtonClicked(object sender, EventArgs e)
    {
        // Navigate to StatsPage, passing the character and viewModel instances
        Navigation.PushAsync(new StatsPage(character, viewModel));
    }

    [Obsolete]
    private async void OnDeleteCharacterClicked(object sender, EventArgs e)
    {
        bool deleteConfirmed = await DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete {character.Name}?",
            "No",
            "Yes"
        );

        if (!deleteConfirmed)
        {
            await DeleteCharacter(character);
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); });
        }
    }

    private async Task DeleteCharacter(Character character)
    {
        try
        {
            string existingCharactersJson = Preferences.Get("characters", "[]");
            var characters = JsonConvert.DeserializeObject<List<Character>>(existingCharactersJson);

            var characterToRemove = characters.FirstOrDefault(c => c.ID == character.ID);
            if (characterToRemove != null)
            {
                characters.Remove(characterToRemove);
                string updatedCharactersJson = JsonConvert.SerializeObject(characters);
                Preferences.Set("characters", updatedCharactersJson);

                viewModel.CharacterSpells.Remove(character.ID);
                Preferences.Remove($"spells_{character.ID}");

                viewModel.OnPropertyChanged(nameof(viewModel.CharacterSpells));
                await DisplayAlert("Success", $"{character.Name} has been removed.", "OK");
            }
            else
            {
                await DisplayAlert("Error", $"Character '{character.Name}' not found.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in DeleteCharacter: {ex.Message}");
            await DisplayAlert("Error", "An error occurred while deleting the character.", "OK");
        }
    }

    private async void OnPrepareSpellsClicked(object sender, EventArgs e)
{
    // Load character's spells into memory (Spells available for selection)
    viewModel.LoadPreparedSpells(character);

    // All spells the character can see
    var allSpellNames = viewModel.SpellsForCharacter(character)
        .Select(spell => spell.Name) // Extract names
        .ToList();

    // Fetch currently prepared spells for the dialog
    var preparedSpellNames = character.GetPreparedSpells()
        .Select(spell => spell.Name) // Extract names of prepared spells
        .ToList();

    // Show dialog to let the user select prepared spells
    var selectedSpellNames = await DisplayMultipleChoiceDialog("Prepare Spells", allSpellNames, preparedSpellNames);

    // If user closed the dialog or made no changes, stop further processing
    if (selectedSpellNames == null) return;

    // Update the character's prepared spells based on user selection
    foreach (var spell in viewModel.SpellsForCharacter(character))
    {
        if (selectedSpellNames.Contains(spell.Name))
        {
            // Attempt to prepare the spell if it isn't already prepared
            if (!character.GetPreparedSpells().Contains(spell))
            {
                var success = character.TogglePreparedSpell(spell);
                if (!success)
                {
                    // Inform the user if the spell preparation limit is reached
                    await DisplayAlert(
                        "Spell Limit Reached",
                        $"You cannot prepare more than {character.Level + character.GetRelevantAbilityModifier()} spells.",
                        "OK"
                    );
                }
            }
        }
        else
        {
            // If the spell was prepared but is no longer selected, unprepare it
            if (character.GetPreparedSpells().Contains(spell))
            {
                character.TogglePreparedSpell(spell);
            }
        }
    }

    // Save changes to the prepared spells list
    SavePreparedSpells(character);

    // Notify the user changes have been saved
    await DisplayAlert("Prepared Spells Updated", "Your prepared spells have been successfully updated!", "OK");
}

    /// <summary>
    /// Display a multi-choice dialog to let the user prepare spells.
    /// </summary>
    private async Task<List<string>> DisplayMultipleChoiceDialog(string title, List<string> allSpells,
        List<string> preparedSpells)
    {
        var selectedSpells = new List<string>();

        // This example shows a simple selection dialog (you can replace this with a custom modal)
        foreach (var spell in allSpells)
        {
            var isPrepared = preparedSpells.Contains(spell);
            var prepare = await DisplayAlert("Prepare Spell", $"Do you want to prepare {spell}?", "Yes", "No");

            if (prepare)
            {
                selectedSpells.Add(spell);
            }

            // Ensure the maximum number of prepared spells is respected
            if (selectedSpells.Count >= character.Level + character.GetRelevantAbilityModifier())
            {
                await DisplayAlert("Limit Reached", "You've reached the maximum number of prepared spells.", "OK");
                break;
            }
        }

        return selectedSpells;
    }

    /// <summary>
    /// Save the prepared spells back to Preferences or your backend.
    /// </summary>
    private void SavePreparedSpells(Character character)
    {
        // Save prepared spells using SharedViewModel's persistence method
        SharedViewModel.Instance.SaveCharacterPreparedSpells(character.ID, character.GetPreparedSpells());
    }

}