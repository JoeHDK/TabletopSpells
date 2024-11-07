using TabletopSpells.Models;
using TabletopSpells.Models.Enums;

namespace TabletopSpells.Pages;
public partial class SpellDetailPage : ContentPage
{
    private Game gameType;
    private readonly Spell spell;
    private Character character;
    private readonly int spellLevel;
    private bool spellIsKnown;
    private bool castAsRitual;

    public SpellDetailPage(Spell spell, Character character, int spellLevel, Game gameType)
    {
        InitializeComponent();
        this.spell = spell;
        this.gameType = gameType;
        this.character = character;
        this.BindingContext = spell;
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

        // Determine if there are any available spell slots for the current spell level
        bool hasAvailableSpellSlots = character.MaxSpellsPerDay.TryGetValue(spellLevel, out int maxSpells) &&
                                      maxSpells > character.SpellsUsedToday.GetValueOrDefault(spellLevel, 0);

        // Show the cast spell button since the spell is known
        CastSpellButton.IsVisible = true;

        if (spell.Ritual || hasAvailableSpellSlots || spellLevel == 0)
        {
            // Enable the button and attach the event handler if slots are available
            CastSpellButton.IsEnabled = true;
            CastSpellButton.Clicked += OnCastSpellClicked;
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
        var character = SharedViewModel.Instance.CurrentCharacter;

        if (character == null)
        {
            await DisplayAlert("Error", "No character selected.", "OK");
            return;
        }

        // Check if the spell can be cast as a ritual and ask the user for confirmation
        if (spell.Ritual)
        {
            bool castAsRitualConfirmation = await DisplayAlert("Cast as Ritual",
                                                               $"Do you want to cast '{spell.Name}' as a ritual?",
                                                               "Yes",
                                                               "No");

            if (castAsRitualConfirmation)
            {
                castAsRitual = true;
                await DisplayAlert("Ritual Cast", $"{spell.Name} has been cast as a ritual.", "OK");
                SharedViewModel.Instance.LogSpellCast(character, spell.Name, spellLevel, castAsRitual);
                ReloadUI();
                return; // Exit after casting as a ritual
            }
        }

        // If not cast as a ritual, check if there are available spell slots
        bool hasAvailableSpellSlots = character.MaxSpellsPerDay.TryGetValue(spellLevel, out int maxSpells) &&
                                      maxSpells > character.SpellsUsedToday.GetValueOrDefault(spellLevel, 0) || spellLevel == 0;

        //if (spellLevel == 0)
        //    hasAvailableSpellSlots = true;
        
        if (!hasAvailableSpellSlots)
        {
            // Log the failed spell cast due to no available slots
            SharedViewModel.Instance.LogFailedSpellCast(character, spell.Name, spellLevel, "No more spells of this level available");
            await DisplayAlert("Failed", "Available spell slot not found", "OK");
            return;
        }

        // Cast the spell normally (not as a ritual) since we have available slots
        bool success = character.CastSpell(spellLevel);

        if (success)
        {
            SharedViewModel.Instance.LogSpellCast(character, spell.Name, spellLevel);

            // Update the spells used information in the SharedViewModel
            if(spellLevel != 0)
                SharedViewModel.Instance.SaveSpellsPerDayDetails(character, character.MaxSpellsPerDay, character.SpellsUsedToday);

            await DisplayAlert("Spell Cast", $"{spell.Name} has been cast.", "OK");
        }
        else
        {
            SharedViewModel.Instance.LogFailedSpellCast(character, spell.Name, spellLevel, "No more spells of this level available");
            await DisplayAlert("Failed", "Available spell slot not found", "OK");
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
        bool confirmation = await DisplayAlert("Add Spell",
                                              $"Do you want to add '{spell.Name}' " +
                                              $"to {character.Name}?",
                                              "Cancel",
                                              "Add");

        if (!confirmation)
        {
            var viewModel = SharedViewModel.Instance;
            viewModel.AddSpell(character, spell);
            viewModel.SaveSpellForCharacter(character, spell);
            //await DisplayAlert("Spell Added", $"{spell.Name} has been added to {character.Name}.", "OK");
            CheckIfSpellIsKnown(); // Re-check if the spell is known
            UpdateButtons(); // Update buttons after adding the spell
            //await Navigation.PopAsync();
        }
    }

    private async void OnRemoveSpellClicked(object? sender, EventArgs e)
    {
        bool confirmation = await DisplayAlert("Remove Spell",
                                              $"Are you sure you want to remove '{spell.Name}' " +
                                              $"from {character.Name}?",
                                              "Cancel",
                                              "Remove");

        if (!confirmation)
        {
            // Logic to remove the spell from the character's known spells
            var viewModel = SharedViewModel.Instance;
            if (viewModel.CharacterSpells.ContainsKey(character.ID))
            {
                viewModel.CharacterSpells[character.ID].Remove(spell);
                viewModel.RemoveSpellForCharacter(character, spell);
                //await DisplayAlert("Spell Removed", $"{spell.Name} has been removed from {character.Name}.", "OK");
                CheckIfSpellIsKnown(); // Re-check if the spell is known
                UpdateButtons(); // Update buttons after removing the spell
                //await Navigation.PopAsync();
            }
        }
    }
}
