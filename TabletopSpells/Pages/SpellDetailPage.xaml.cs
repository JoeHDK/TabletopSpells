namespace TabletopSpells.Pages;
public partial class SpellDetailPage : ContentPage
{
    private readonly Spell spell;
    private readonly string characterName;

    public SpellDetailPage(Spell spell, string characterName)
    {
        InitializeComponent();
        this.spell = spell;
        this.characterName = characterName;
        this.BindingContext = spell;
    }

    private async void OnRemoveSpellClicked(object sender, EventArgs e)
    {
        var viewModel = SharedViewModel.Instance;
        if (viewModel.CharacterSpells.ContainsKey(characterName))
        {
            viewModel.CharacterSpells[characterName].Remove(spell);
            viewModel.SaveSpellsForCharacter(characterName);
        }

        // Optionally display a confirmation message or navigate back
        await DisplayAlert("Removed", $"{spell.Name} has been removed from {characterName}.", "OK");
        await Navigation.PopAsync();
    }

}
