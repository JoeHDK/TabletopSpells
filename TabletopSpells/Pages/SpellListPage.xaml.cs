using System.Text.RegularExpressions;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SpellListPage
{
    private readonly SpellListPageViewModel viewModel;

    public SpellListPage(Character character, Game gameType)
    {
        InitializeComponent();
        viewModel = new SpellListPageViewModel(character, character.IsDivineCaster, gameType);
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.ReloadDivineSpellViewModels();
    }

    private async void OnMenuClicked(object sender, EventArgs e)
    {
        var levels = new List<string>
        {
            "Cantrips", "1st level", "2nd level", "3rd level",
            "4th level", "5th level", "6th level", "7th level",
            "8th level", "9th level"
        };

        for (var i = 0; i < levels.Count; i++)
        {
            if (viewModel.SelectedSpellLevel == i)
                levels[i] = $"* {levels[i]}";
        }

        var action = await DisplayActionSheet("Filters", null, null, levels.ToArray());
        if (action == "Cancel" || string.IsNullOrEmpty(action)) return;

        var cleanAction = action.Replace("*", "").Trim();
        var match = Regex.Match(cleanAction, @"\d+");
        var selectedLevel = match.Success ? int.Parse(match.Value) : (cleanAction == "Cantrips" ? 0 : -1);

        viewModel.SelectedSpellLevel = (viewModel.SelectedSpellLevel == selectedLevel) ? null : selectedLevel;

        viewModel.FilterSpells();
        UpdateTitle();
    }

    private void UpdateTitle()
    {
        Title = viewModel.SelectedSpellLevel switch
        {
            null => "Spells",
            0 => "Cantrips",
            1 => "1st level spells",
            2 => "2nd level spells",
            3 => "3rd level spells",
            _ => $"{viewModel.SelectedSpellLevel}th level spells"
        };
    }

    private async void OnSpellSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is SpellViewModel selectedViewModel)
        {
            var selectedSpell = selectedViewModel.Spell;
            var level = viewModel.ParseSpellLevel(selectedSpell.SpellLevel, viewModel.Character?.CharacterClass.ToString() ?? "");
            
            await Navigation.PushAsync(new SpellDetailPage(selectedSpell, viewModel.Character, level, viewModel.GameType));
            
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        viewModel.SearchText = e.NewTextValue;
        viewModel.FilterSpells();
    }
}
