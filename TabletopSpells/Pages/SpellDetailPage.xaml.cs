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
        if (spellIsKnown)
        {
            CastSpellButton.IsEnabled = true;
            CastSpellButton.IsVisible = true;
            CastSpellButton.Clicked -= OnCastSpellClicked; // Clear any existing subscriptions
            CastSpellButton.Clicked += OnCastSpellClicked;
        }
        else
        {
            CastSpellButton.IsEnabled = false;
            CastSpellButton.IsVisible = false;
            CastSpellButton.Clicked -= OnCastSpellClicked;
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

        if (spell.Ritual)
        {
            bool confirmation = await DisplayAlert("Cast as Ritual",
                                                  $"Do you want to cast '{spell.Name}' as a ritual?",
                                                  "Yes",
                                                  "No");

            castAsRitual = confirmation;

            if (castAsRitual)
            {
                await DisplayAlert("Ritual Cast", $"{spell.Name} has been cast as a ritual.", "OK");
                // Log the ritual cast
                SharedViewModel.Instance.LogSpellCast(character, spell.Name, spellLevel, castAsRitual);
                return;
            }
        }

        // Use the CastSpell method from the Character model
        bool success = character.CastSpell(spellLevel);

        if (success)
        {
            // Log the spell cast
            SharedViewModel.Instance.LogSpellCast(character, spell.Name, spellLevel);

            // Update the spells used information in the SharedViewModel (if not cast as ritual)
            if (!castAsRitual)
            {
                SharedViewModel.Instance.SaveSpellsPerDayDetails(character, character.MaxSpellsPerDay, character.SpellsUsedToday);
            }

            await DisplayAlert("Spell Cast", $"{spell.Name} has been cast.", "OK");
        }
        else
        {
            SharedViewModel.Instance.LogFailedSpellCast(character, spell.Name, spellLevel, "No more spells of this level available");
            await DisplayAlert("Failed", "404: Available spell slot not found", "OK");
        }
    }

    private async void OnAddSpellClicked(object? sender, EventArgs e)
    {
        bool confirmation = await DisplayAlert("Add Spell",
                                              $"Do you want to add '{spell.Name}' " +
                                              $"to {character.Name}?",
                                              "Add",
                                              "Cancel");

        if (confirmation)
        {
            var viewModel = SharedViewModel.Instance;
            viewModel.AddSpell(character, spell);
            viewModel.SaveSpellForCharacter(character, spell);
            await DisplayAlert("Spell Added", $"{spell.Name} has been added to {character.Name}.", "OK");
            CheckIfSpellIsKnown(); // Re-check if the spell is known
            UpdateButtons(); // Update buttons after adding the spell
            await Navigation.PopAsync();
        }
    }

    private async void OnRemoveSpellClicked(object? sender, EventArgs e)
    {
        bool confirmation = await DisplayAlert("Remove Spell",
                                              $"Are you sure you want to remove '{spell.Name}' " +
                                              $"from {character.Name}?",
                                              "Remove",
                                              "Cancel");

        if (confirmation)
        {
            // Logic to remove the spell from the character's known spells
            var viewModel = SharedViewModel.Instance;
            if (viewModel.CharacterSpells.ContainsKey(character.ID))
            {
                viewModel.CharacterSpells[character.ID].Remove(spell);
                viewModel.SaveSpellForCharacter(character, spell);
                await DisplayAlert("Spell Removed", $"{spell.Name} has been removed from {character.Name}.", "OK");
                CheckIfSpellIsKnown(); // Re-check if the spell is known
                UpdateButtons(); // Update buttons after removing the spell
                await Navigation.PopAsync();
            }
        }
    }
}
