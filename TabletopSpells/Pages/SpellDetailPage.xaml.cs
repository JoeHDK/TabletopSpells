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
    
    public SpellDetailPage(Spell spell, Character character, int spellLevel, Game gameType)
    {
        InitializeComponent();
        this.spell = spell;
        this.gameType = gameType;
        this.character = character;
        this.BindingContext = spell;
        this.spellLevel = spellLevel;
        CheckIfSpellIsKnown();
        UpdateButton();
        ShowCastSpellButton();
    }

    private void CheckIfSpellIsKnown()
    {
        var viewModel = SharedViewModel.Instance;
        spellIsKnown = viewModel.CharacterSpells.ContainsKey(character.Name) &&
                       viewModel.CharacterSpells[character.Name].Any(s => s.Name == spell.Name);
    }

    private void ShowCastSpellButton()
    {
        CastSpellButton.Clicked -= OnCastSpellClicked;
        if (spellIsKnown)
        {
            CastSpellButton.IsEnabled = true;
            CastSpellButton.Text = "Cast";
            CastSpellButton.Clicked += OnCastSpellClicked;
        }
        else
        {
            CastSpellButton.IsEnabled = false;
            CastSpellButton.Text = "";
            CastSpellButton.Clicked -= OnCastSpellClicked;
        }
    }
    
    private void UpdateButton()
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
    }

    private void OnCastSpellClicked(object? sender, EventArgs e)
    {
        // Retrieve the character from the SharedViewModel using the character name
        var character = SharedViewModel.Instance.CurrentCharacter;

        if (character == null)
        {
            DisplayAlert("Error", "No character selected.", "OK");
            return;
        }

        // Use the CastSpell method from the Character model
        bool success = character.CastSpell(spellLevel);

        if (success)
        {
            // Update the spells used information in the SharedViewModel
            SharedViewModel.Instance.SaveSpellsPerDayDetails(character, character.MaxSpellsPerDay, character.SpellsUsedToday);
            SharedViewModel.Instance.LogSpellCast(character, spell.Name, spellLevel);
            DisplayAlert("Spell Cast", $"{spell.Name} has been cast.", "OK");
        }
        else
        {
            SharedViewModel.Instance.LogFailedSpellCast(character, spell.Name, spellLevel, "No more spells of this level available");
            DisplayAlert("Failed", "404: Available spell slot not found", "OK");
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
            viewModel.SaveSpellsForCharacter(character);
            await DisplayAlert("Spell Added", $"{spell.Name} has been added to {character.Name}.", "OK");
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
            if (viewModel.CharacterSpells.ContainsKey(character.Name))
            {
                viewModel.CharacterSpells[character.Name].Remove(spell);
                viewModel.SaveSpellsForCharacter(character);
                await DisplayAlert("Spell Removed", $"{spell.Name} has been removed from {character.Name}.", "OK");
                await Navigation.PopAsync();
            }
        }
    }
}
