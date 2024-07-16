using TabletopSpells.Models.Enums;

namespace TabletopSpells;
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        this.BindingContext = SharedViewModel.Instance;
    }
    private async void OnPathfinder1eSelected(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CharacterSelectPage(Game.pathfinder1e));
    }

    private async void OnDnd5eSelected(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CharacterSelectPage(Game.dnd5e));
    }
}