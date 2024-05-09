namespace TabletopSpells.Pages;
public partial class SpellLogPage : ContentPage
{
    public SpellLogPage(string? characterName)
    {
        InitializeComponent();
        this.BindingContext = SharedViewModel.Instance;
        SharedViewModel.Instance.LoadLogs(characterName);
    }

}
