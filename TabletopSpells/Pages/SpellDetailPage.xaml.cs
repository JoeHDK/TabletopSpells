
namespace TabletopSpells.Pages;
public partial class SpellDetailPage : ContentPage
{
    private readonly Spell spell;
    private readonly string characterName;
    private bool spellIsKnown;

    public SpellDetailPage(Spell spell, string characterName)
    {
        InitializeComponent();
        this.spell = spell;
        this.characterName = characterName;
        this.BindingContext = spell;
        CheckIfSpellIsKnown();
        UpdateButton();
    }

    private void CheckIfSpellIsKnown()
    {
        var viewModel = SharedViewModel.Instance;
        spellIsKnown = viewModel.CharacterSpells.ContainsKey(characterName) &&
                       viewModel.CharacterSpells[characterName].Any(s => s.Name == spell.Name);
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

    private async void OnAddSpellClicked(object sender, EventArgs e)
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

    private async void OnRemoveSpellClicked(object sender, EventArgs e)
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
