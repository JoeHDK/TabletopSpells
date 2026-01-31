using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using TabletopSpells.Helpers;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages;

public partial class CharacterOverviewPage : ContentPage
{
    public Character character;
    public Game gameType;
    public ObservableCollection<string> Spells { get; private set; }

    private SharedViewModel viewModel;

    public CharacterOverviewPage(Character character, SharedViewModel viewModel, Game gameType)
    {
        InitializeComponent();
        this.character = character;
        Title = $"{character.Name}'s home";
        this.viewModel = viewModel;
        BindingContext = this.viewModel;
        this.viewModel.SpellsForCharacter(character);
        this.gameType = gameType;
    }

    [Obsolete]
    private void OnSpellsSelected(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SpellsPage(character, SharedViewModel.Instance, gameType));
    }

    private void OnSpellPerDaySelected(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SpellsPerDayPage());
    }

    private void OnSpellLogSelected(object sender, EventArgs e)
    {
        Navigation.PushAsync(new SpellLogPage(character));
    }
    
    private void OnStatsPageButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new StatsPage(character, viewModel));
    }

    private void OnOptionsClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CharacterOptionsPage(character, viewModel));
    }
}