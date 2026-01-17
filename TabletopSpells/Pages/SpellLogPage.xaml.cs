using TabletopSpells.Models;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages;
public partial class SpellLogPage : ContentPage
{
    public SpellLogPage(Character character)
    {
        InitializeComponent();
        BindingContext = SharedViewModel.Instance;
        SharedViewModel.Instance.LoadLogs(character);
    }
    
}
