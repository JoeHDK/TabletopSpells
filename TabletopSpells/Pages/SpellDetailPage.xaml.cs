namespace TabletopSpells.Pages;
public partial class SpellDetailPage : ContentPage
{
    private readonly Spell spell;
    private readonly string characterName;
    private readonly int spellLevel;
    private bool spellIsKnown;

    public SpellDetailPage(Spell spell, string characterName, int spellLevel)
    {
        InitializeComponent();
        this.spell = spell;
        this.characterName = characterName;
        this.BindingContext = spell;
        this.spellLevel = spellLevel;
        CheckIfSpellIsKnown();
        UpdateButton();
        ShowCastSpellButton();
    }

    private void CheckIfSpellIsKnown()
    {
        var viewModel = SharedViewModel.Instance;
        spellIsKnown = viewModel.CharacterSpells.ContainsKey(characterName) &&
                       viewModel.CharacterSpells[characterName].Any(s => s.Name == spell.Name);
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
            SharedViewModel.Instance.SaveSpellsPerDayDetails(characterName, character.MaxSpellsPerDay, character.SpellsUsedToday);
            SharedViewModel.Instance.LogSpellCast(characterName, spell.Name, spellLevel);
            DisplayAlert("Spell Cast", $"{spell.Name} has been cast.", "OK");
        }
        else
        {
            SharedViewModel.Instance.LogFailedSpellCast(characterName, spell.Name, spellLevel, "No slots available");
            DisplayAlert("Failed", "404: Available spell slot not found", "OK");
        }
    }

    private async void OnAddSpellClicked(object? sender, EventArgs e)
    {
        bool confirmation = await DisplayAlert("Add Spell",
                                              $"Do you want to add '{spell.Name}' " +
                                              $"to {characterName}?",
                                              "Add",
                                              "Cancel");

        if (confirmation)
        {
            var viewModel = SharedViewModel.Instance;
            viewModel.AddSpell(characterName, spell);
            viewModel.SaveSpellsForCharacter(characterName);
            await DisplayAlert("Spell Added", $"{spell.Name} has been added to {characterName}.", "OK");
            await Navigation.PopAsync();
        }
    }

    private async void OnRemoveSpellClicked(object? sender, EventArgs e)
    {
        bool confirmation = await DisplayAlert("Remove Spell",
                                              $"Are you sure you want to remove '{spell.Name}' " +
                                              $"from {characterName}?",
                                              "Remove",
                                              "Cancel");

        if (confirmation)
        {
            // Logic to remove the spell from the character's known spells
            var viewModel = SharedViewModel.Instance;
            if (viewModel.CharacterSpells.ContainsKey(characterName))
            {
                viewModel.CharacterSpells[characterName].Remove(spell);
                viewModel.SaveSpellsForCharacter(characterName);
                await DisplayAlert("Spell Removed", $"{spell.Name} has been removed from {characterName}.", "OK");
                await Navigation.PopAsync();
            }
        }
    }
}
