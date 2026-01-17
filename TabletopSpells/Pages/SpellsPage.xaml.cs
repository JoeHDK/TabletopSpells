using System.Collections.ObjectModel;
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

    public ObservableCollection<Grouping<int, Spell>> GroupedSpells { get; set; } = new();

    public SpellsPage(Character character, SharedViewModel viewModel, Game gameType)
    {
        InitializeComponent();

        this.character = character;
        this.gameType = gameType;

        Title = $"{character.Name}'s spells";

        SharedViewModel.Instance.LoadSpellsForCharacter(character);

        var castable = GetCastableSpells(character, gameType);
        GroupAndDisplaySpells(castable);
    }

    private void GroupAndDisplaySpells(List<Spell> spells)
    {
        var grouped = spells
            .GroupBy(spell => ParseSpellLevel(spell.SpellLevel, character.CharacterClass.ToString()))
            .OrderBy(g => g.Key)
            .Select(g => new Grouping<int, Spell>(g.Key, g.OrderBy(s => s.Name)));

        GroupedSpells = new ObservableCollection<Grouping<int, Spell>>(grouped);
        SpellListView.ItemsSource = GroupedSpells;
    }

    private List<Spell> GetCastableSpells(Character character, Game game)
    {
        var allSpellsFromJson = SpellRepository.GetAllSpellsFromJson(game);
        var castable = new List<Spell>();

        // Always prepared
        var alwaysPrepared = character.AlwaysPreparedSpells
            .SelectMany(name =>
                allSpellsFromJson.Where(spell =>
                    spell.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        castable.AddRange(alwaysPrepared);

        // Prepared spells
        if (character.IsDivineCaster)
        {
            castable.AddRange(character.GetPreparedSpells());
        }

        // Manually added
        var manuallyAdded = SharedViewModel.Instance.SpellsForCharacter(character);
        castable.AddRange(manuallyAdded);

        // Deduplicate
        return castable
            .GroupBy(spell => (spell.Name?.ToLowerInvariant(), spell.SpellLevel))
            .Select(grouping => grouping.First())
            .OrderBy(spell => spell.SpellLevel)
            .ThenBy(spell => spell.Name)
            .ToList();
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
        if (selectionChangedEventArgs.CurrentSelection.FirstOrDefault() is Spell { SpellLevel: not null } selectedSpell)
        {
            var spellLevel = ParseSpellLevel(selectedSpell.SpellLevel, character.CharacterClass.ToString());
            await Navigation.PushAsync(new SpellDetailPage(selectedSpell, character, spellLevel, gameType));
        }

        ((CollectionView)sender).SelectedItem = null;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex MyRegex();
}
