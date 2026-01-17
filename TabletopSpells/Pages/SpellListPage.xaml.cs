using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SpellListPage : ContentPage
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
            int level = viewModel.ParseSpellLevel(selectedSpell.SpellLevel, viewModel.Character?.CharacterClass.ToString() ?? "");
            
            // All casters (divine and non-divine) go to spell detail page
            // The detail page handles the Prepare/Add logic based on caster type
            await Navigation.PushAsync(new SpellDetailPage(selectedSpell, viewModel.Character, level, viewModel.GameType));
            
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private async Task HandleDivineCasterSpellSelection(Spell selectedSpell)
    {
        var confirmation = await DisplayAlert("Prepare Spell",
            $"Do you want to add '{selectedSpell.Name}' to your prepared spells?",
            "Yes",
            "No");

        if (!confirmation)
            return;

        // Check if we're at the prepared spell limit
        if (!viewModel.CanPrepareMoReSpells())
        {
            // Show replacement dialog
            var preparedSpells = viewModel.GetPreparedSpells();
            var replacementDialog = new PrepareSpellReplacementDialog(preparedSpells, selectedSpell.Name);
            await Navigation.PushAsync(replacementDialog);
            
            // Wait for user selection (using a simple polling approach)
            await Task.Delay(1000);
            var selectedReplacement = replacementDialog.GetSelectedReplacement();
            
            if (selectedReplacement != null)
            {
                viewModel.ReplacePrepareddSpell(selectedReplacement, selectedSpell);
                SharedViewModel.Instance.SavePreparedSpells(viewModel.Character);
                await DisplayAlert("Success", $"'{selectedReplacement.Name}' has been replaced with '{selectedSpell.Name}'.", "OK");
            }
            
            await Navigation.PopAsync();
        }
        else
        {
            // Prepare the spell directly
            var spellViewModel = viewModel.SpellViewModels.FirstOrDefault(s => s.Spell.Id == selectedSpell.Id);
            if (spellViewModel != null)
            {
                spellViewModel.IsPrepared = true;
                SharedViewModel.Instance.SavePreparedSpells(viewModel.Character);
                await DisplayAlert("Success", $"'{selectedSpell.Name}' has been added to your prepared spells.", "OK");
            }
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        // Forward the search text to the ViewModel for filtering
        viewModel.SearchText = e.NewTextValue;
        viewModel.FilterSpells();
    }

    private async void OnFavoriteClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is ImageButton btn && btn.CommandParameter is Spell spell)
            {
                var shared = SharedViewModel.Instance;
                // Toggle favorite in shared storage; ensure character is present
                var character = shared.CurrentCharacter ?? shared.Characters.FirstOrDefault();
                if (character == null) return;

                // Ensure the spell is present in character spells collection
                if (!shared.CharacterSpells.ContainsKey(character.ID))
                    shared.CharacterSpells[character.ID] = new ObservableCollection<Spell>();

                var existing = shared.CharacterSpells[character.ID].FirstOrDefault(s => s.Id == spell.Id);
                if (existing == null)
                {
                    // If spell not present, add it (but do not prepare)
                    shared.AddSpell(character, spell);
                    shared.SaveSpellForCharacter(character, spell);
                    existing = shared.CharacterSpells[character.ID].FirstOrDefault(s => s.Id == spell.Id);
                }

                if (existing != null)
                {
                    shared.ToggleSpellFavorite(character, existing);
                }

                // Visual feedback: pulse animation on the button
                try
                {
                    await btn.ScaleTo(1.3, 120, Easing.CubicOut);
                    await btn.ScaleTo(1.0, 120, Easing.CubicIn);
                }
                catch
                {
                    // ignore animation errors on unsupported platforms
                }

                // Refresh page view model's list and UI
                shared.SpellsChanged?.Invoke();
                this.viewModel.ReloadDivineSpellViewModels();
                this.viewModel.FilterSpells();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Unable to toggle favorite: {ex.Message}", "OK");
        }
    }
}
