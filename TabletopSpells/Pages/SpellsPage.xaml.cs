using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.Repositories;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages;

public partial class SpellsPage : ContentPage
{
    private readonly Game gameType;
    private readonly Character character;

    public ObservableCollection<Grouping<int, SpellViewModel>> GroupedSpells { get; set; } = [];

    public SpellsPage(Character character, SharedViewModel viewModel, Game gameType)
    {
        InitializeComponent();

        this.character = character;
        this.gameType = gameType;

        Title = $"{character.Name}'s spells";

        // Use SpellsForCharacter which handles auto-filling and loading prepared spells
        // Store the result to use the persisted spells
        var characterSpells = viewModel.SpellsForCharacter(character);

        var castable = GetCastableSpells(character, gameType, characterSpells);
        GroupAndDisplaySpells(castable);
    }

    private void GroupAndDisplaySpells(List<Spell> spells)
    {
        var grouped = spells
            .GroupBy(spell => ParseSpellLevel(spell.SpellLevel, character.CharacterClass.ToString()))
            .OrderBy(g => g.Key)
            .Select(g => new Grouping<int, SpellViewModel>(
                g.Key, 
                g.OrderBy(s => s.Name).Select(s => new SpellViewModel(s, character))));

        GroupedSpells = new ObservableCollection<Grouping<int, SpellViewModel>>(grouped);
        SpellListView.ItemsSource = GroupedSpells;
    }

    private List<Spell> GetCastableSpells(Character character, Game game, ObservableCollection<Spell> characterSpells)
    {
        var castable = new List<Spell>();

        // Always prepared
        var alwaysPrepared = character.AlwaysPreparedSpells
            .SelectMany(name =>
                characterSpells.Where(spell =>
                    spell.Name != null && spell.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        castable.AddRange(alwaysPrepared);

        // Prepared spells
        if (character.IsDivineCaster)
        {
            castable.AddRange(character.GetPreparedSpells());
        }

        // Manually added spells (all spells in characterSpells are manually added or always prepared)
        castable.AddRange(characterSpells);

        // Deduplicate by spell name and level
        return castable
            .GroupBy(spell => (spell.Name?.ToLowerInvariant(), spell.SpellLevel))
            .Select(grouping => grouping.First())
            .OrderBy(spell => spell.SpellLevel)
            .ThenBy(spell => spell.Name)
            .ToList();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("=== SpellsPage.OnAppearing - Refreshing spells ===");
        
        // Reload spells from disk to ensure persistence across page navigations
        var characterSpells = SharedViewModel.Instance.SpellsForCharacter(character);
        var castable = GetCastableSpells(character, gameType, characterSpells);
        GroupAndDisplaySpells(castable);
        
        // Show Prepare Spells button only for divine casters
        PrepareSpellsToolbarItem.IsEnabled = character.IsDivineCaster;
        if (!character.IsDivineCaster)
        {
            ToolbarItems.Remove(PrepareSpellsToolbarItem);
        }
    }

    private int ParseSpellLevel(string spellLevel, string characterClass)
    {
        var parts = spellLevel.Split(',');
        var minLevel = int.MaxValue;

        foreach (var part in parts)
        {
            var match = MyRegex().Match(part);
            if (!match.Success || !int.TryParse(match.Value, out var level)) continue;
            if (part.Trim()
                .Contains(characterClass, StringComparison.CurrentCultureIgnoreCase))
                return level;

            minLevel = Math.Min(minLevel, level);
        }

        return minLevel == int.MaxValue ? -1 : minLevel;
    }

    [Obsolete]
    private async void OnSearchSpellsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SpellListPage(character, gameType));
    }

    private async void OnSpellSelected(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        if (selectionChangedEventArgs.CurrentSelection.FirstOrDefault() is SpellViewModel { Spell.SpellLevel: not null } selectedSpellViewModel)
        {
            var selectedSpell = selectedSpellViewModel.Spell;
            var spellLevel = ParseSpellLevel(selectedSpell.SpellLevel, character.CharacterClass.ToString());
            await Navigation.PushAsync(new SpellDetailPage(selectedSpell, character, spellLevel, gameType));
        }

        ((CollectionView)sender).SelectedItem = null;
    }

    private async void OnPrepareSpellsClicked(object sender, EventArgs e)
    {
        if (!character.IsDivineCaster)
            return;

        // Get the character's known spells and convert to List
        var knownSpells = SharedViewModel.Instance.SpellsForCharacter(character).ToList();
        
        // Set IsPrepared state on each spell based on Character.GetPreparedSpells()
        var preparedSpellIds = character.GetPreparedSpells().Select(s => s.Id).ToHashSet();
        foreach (var spell in knownSpells)
        {
            spell.IsPrepared = preparedSpellIds.Contains(spell.Id);
        }
        
        // Function to compute max prepared spells
        int GetMaxPrepared() => character.Level + character.GetRelevantAbilityModifier();

        // Open the prepare spells modal
        await Navigation.PushModalAsync(new PrepareSpellsPage(knownSpells, GetMaxPrepared, character));
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex MyRegex();
}
