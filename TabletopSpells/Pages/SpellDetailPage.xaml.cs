using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages;
public partial class SpellDetailPage : ContentPage
{
    //private Game gameType;
    private readonly Spell spell;
    private readonly Character character;
    private readonly int spellLevel;
    private bool spellIsKnown;
    private bool castAsRitual;

    public SpellDetailPage(Spell spell, Character character, int spellLevel, Game gameType)
    {
        InitializeComponent();
        this.spell = spell;
        //this.gameType = gameType;
        this.character = character;
        BindingContext = spell;
        this.spellLevel = spellLevel;
        CheckIfSpellIsKnown();
        UpdateButtons();
    }

    private void CheckIfSpellIsKnown()
    {
        var viewModel = SharedViewModel.Instance;
        spellIsKnown = viewModel.CharacterSpells.ContainsKey(character.ID) &&
                       viewModel.CharacterSpells[character.ID].Any(s => s.Name == spell.Name);
    }

    private void UpdateButtons()
    {
        AddOrRemoveButton.Text = spellIsKnown ? "Remove Spell" : "Add Spell";

        // Clear existing click event subscriptions to avoid multiple subscriptions
        AddOrRemoveButton.Clicked -= OnAddSpellClicked;
        AddOrRemoveButton.Clicked -= OnRemoveSpellClicked;

        // Subscribe to the appropriate event
        if (spellIsKnown)
        {
            AddOrRemoveButton.Clicked += OnRemoveSpellClicked;
        }
        else
        {
            AddOrRemoveButton.Clicked += OnAddSpellClicked;
        }

        // Update the visibility and enabled status of the CastSpellButton
        ShowCastSpellButton();
    }

    private void ShowCastSpellButton()
    {
        // Initially hide and disable the cast spell button
        CastSpellButton.IsVisible = false;
        CastSpellButton.IsEnabled = false;
        CastSpellButton.Clicked -= OnCastSpellClicked; // Clear any existing event subscriptions

        // If the spell isn't known to the character, no further action is needed
        if (!spellIsKnown)
        {
            return;
        }

        // Determine if there are any available spell slots for the spell's level or higher
        var hasAvailableSpellSlots = false;
        for (var level = spellLevel; level <= 9; level++) // Assuming spell levels range from 0 to 9
        {
            if (!character.MaxSpellsPerDay.TryGetValue(level, out var maxSpells) ||
                maxSpells <= character.SpellsUsedToday.GetValueOrDefault(level, 0)) continue;
            hasAvailableSpellSlots = true;
            break;
        }

        // Show the cast spell button since the spell is known
        CastSpellButton.IsVisible = true;

        // Enable the button if the spell can be cast as a ritual, has available slots, or is a cantrip (level 0)
        if (spell.Ritual || hasAvailableSpellSlots || spellLevel == 0)
        {
            CastSpellButton.IsEnabled = true;
            CastSpellButton.Clicked += OnCastSpellClicked; // Attach the event handler
        }
        else
        {
            // Keep the button disabled and reduce its opacity to indicate it's unavailable
            CastSpellButton.Opacity = 0.5;
        }
    }


    private async void OnCastSpellClicked(object? sender, EventArgs e)
{
    // Retrieve the character from the SharedViewModel using the character name
    var currentCharacter = SharedViewModel.Instance.CurrentCharacter;

    if (currentCharacter == null)
    {
        await DisplayAlert("Error", "No character selected.", "OK");
        return;
    }

    // Check if the spell can be cast as a ritual and ask the user for confirmation
    if (spell.Ritual)
    {
        var castAsRitualConfirmation = await DisplayAlert("Cast as Ritual",
                                                           $"Do you want to cast '{spell.Name}' as a ritual?",
                                                           "Yes",
                                                           "No");

        if (castAsRitualConfirmation)
        {
            castAsRitual = true;
            await DisplayAlert("Ritual Cast", $"{spell.Name} has been cast as a ritual.", "OK");
            SharedViewModel.Instance.LogSpellCast(currentCharacter, spell.Name, spellLevel, castAsRitual);
            ReloadUI();
            return; // Exit after casting as a ritual
        }
    }

    // Prepare the list of spell slots to display
    var spellSlots = new List<string>();
    foreach (var (level, maxSpells) in currentCharacter.MaxSpellsPerDay)
    {
        if (level < spellLevel) continue;
        
        var usedSpells = currentCharacter.SpellsUsedToday.GetValueOrDefault(level, 0);
        var remainingSpells = maxSpells - usedSpells;

        // Only display levels with defined slots
        if (maxSpells <= 0) continue;
        // Format: "Level {n} (x remaining)"
        var slotDisplay = $"Level {level} ({remainingSpells} remaining)";
        spellSlots.Add(slotDisplay);
    }

    if (spellSlots.Count == 0)
    {
        await DisplayAlert("No Spell Slots", $"No spell slots available for {spell.Name}.", "OK");
        return;
    }

    // Use DisplayActionSheet to show all slots
    var selectedSlot = await DisplayActionSheet($"{spell.Name} (lvl: {spellLevel})", "Cancel", null, spellSlots.ToArray());

    if (selectedSlot == "Cancel" || string.IsNullOrEmpty(selectedSlot))
    {
        return; // User canceled the selection
    }

    // Extract the spell level from the selected string
    var selectedLevel = int.Parse(selectedSlot.Split(' ')[1]);

    // Cast the spell using the selected level
    var success = currentCharacter.CastSpell(selectedLevel);

    if (success)
    {
        if (spell.Name != null)
        {
            SharedViewModel.Instance.LogSpellCast(currentCharacter, spell.Name, selectedLevel, castAsRitual);

            // Update spells used in SharedViewModel
            SharedViewModel.Instance.SaveSpellsPerDayDetails(currentCharacter, currentCharacter.MaxSpellsPerDay,
                currentCharacter.SpellsUsedToday);

            await DisplayAlert("Spell Cast", $"{spell.Name} has been cast at level {selectedLevel}.", "OK");
        }
    }
    else
    {
        if (spell.Name != null)
        {
            SharedViewModel.Instance.LogFailedSpellCast(currentCharacter, spell.Name, selectedLevel,
                "Unable to cast spell.");
            await DisplayAlert("Failed", $"Failed to cast {spell.Name} at level {selectedLevel}.", "OK");
        }
    }

    ReloadUI();
}


    private void ReloadUI()
    {
        CheckIfSpellIsKnown(); // Re-check if the spell is known
        UpdateButtons();       // Update the buttons based on the current state
        OnPropertyChanged(nameof(character.SpellsUsedToday)); // Trigger UI update for spells used today
        OnPropertyChanged(nameof(character.MaxSpellsPerDay)); // Trigger UI update for max spells per day
    }

    private async void OnAddSpellClicked(object? sender, EventArgs e)
    {
        var confirmation = await DisplayAlert("Add Spell",
                                              $"Do you want to add '{spell.Name}' " +
                                              $"to {character.Name}?",
                                              "Cancel",
                                              "Add");

        if (confirmation) return;
        // Logic to add the spell to the character's known spells'
        var viewModel = SharedViewModel.Instance;
        viewModel.AddSpell(character, spell);
        viewModel.SaveSpellForCharacter(character, spell);
        //await DisplayAlert("Spell Added", $"{spell.Name} has been added to {character.Name}.", "OK");
        CheckIfSpellIsKnown(); // Re-check if the spell is known
        UpdateButtons(); // Update buttons after adding the spell
        //await Navigation.PopAsync();
    }

    private async void OnRemoveSpellClicked(object? sender, EventArgs e)
    {
        var confirmation = await DisplayAlert("Remove Spell",
                                              $"Are you sure you want to remove '{spell.Name}' " +
                                              $"from {character.Name}?",
                                              "Cancel",
                                              "Remove");

        if (confirmation) return;
        // Logic to remove the spell from the character's known spells
        var viewModel = SharedViewModel.Instance;
        if (!viewModel.CharacterSpells.ContainsKey(character.ID)) return;
        viewModel.CharacterSpells[character.ID].Remove(spell);
        viewModel.RemoveSpellForCharacter(character, spell);
        //await DisplayAlert("Spell Removed", $"{spell.Name} has been removed from {character.Name}.", "OK");
        CheckIfSpellIsKnown(); // Re-check if the spell is known
        UpdateButtons(); // Update buttons after removing the spell
        //await Navigation.PopAsync();
    }
}
