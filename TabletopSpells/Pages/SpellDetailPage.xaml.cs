namespace TabletopSpells.Pages;
public partial class SpellDetailPage : ContentPage
{
    public SpellDetailPage(Spell spell)
    {
        InitializeComponent();
        this.BindingContext = spell;
    }
}
