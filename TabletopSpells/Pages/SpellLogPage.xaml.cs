using TabletopSpells.Models;

namespace TabletopSpells.Pages;
public partial class SpellLogPage : ContentPage
{
    private Character character;
    public SpellLogPage(Character character)
    {
        InitializeComponent();
        this.BindingContext = SharedViewModel.Instance;
        SharedViewModel.Instance.LoadLogs(character);
    }
    
}
