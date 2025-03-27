using CommunityToolkit.Maui.Views;
using TabletopSpells.Models;

namespace TabletopSpells.Pages;

public partial class PrepareSpellsPopup : Popup
{
    private readonly Character _character;
    private readonly List<Spell> _allSpells;
    private readonly List<Spell> _initiallyPrepared;
    private readonly List<CheckBox> _checkboxes = new();

    public List<Spell> SelectedSpells { get; private set; } = new();

    public PrepareSpellsPopup(Character character, List<Spell> allSpells, List<Spell> preparedSpells)
    {
        InitializeComponent();

        _character = character;
        _allSpells = allSpells;
        _initiallyPrepared = preparedSpells;

        LoadSpellsGroupedAndSorted();
    }

    private void LoadSpellsGroupedAndSorted()
    {
        var grouped = _allSpells
            .GroupBy(s =>
                int.TryParse(s.SpellLevel, out var level)
                    ? level
                    : 0) // Fallback to 0 for "Cantrips" or invalid values
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            var groupLabel = new Label
            {
                Text = group.Key == 0 ? "Cantrips" : $"Level {group.Key}",
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 10, 0, 5)
            };
            SpellsList.Children.Add(groupLabel);

            foreach (var spell in group.OrderBy(s => s.Name))
            {
                bool isAlwaysPrepared = _character.GetPreparedSpells().Contains(spell) 
                                        && !_allSpells.Contains(spell); // added manually

                if (isAlwaysPrepared)
                {
                    var label = new Label
                    {
                        Text = $"{spell.Name} (Always Prepared)",
                        FontAttributes = FontAttributes.Italic,
                        TextColor = Colors.Gray,
                        VerticalOptions = LayoutOptions.Center
                    };

                    var row = new HorizontalStackLayout
                    {
                        Children = { label },
                        Spacing = 10
                    };

                    SpellsList.Children.Add(row);
                    continue;
                }

                bool isChecked = _initiallyPrepared.Any(p => p.Name == spell.Name);

                var checkbox = new CheckBox
                {
                    IsChecked = isChecked,
                    BindingContext = spell
                };

                var spellLabel = new Label
                {
                    Text = spell.Name,
                    VerticalOptions = LayoutOptions.Center
                };

                var checkboxRow = new HorizontalStackLayout
                {
                    Children = { checkbox, spellLabel },
                    Spacing = 10
                };

                _checkboxes.Add(checkbox);
                SpellsList.Children.Add(checkboxRow);
            }


        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        int maxSpells = _character.Level + _character.GetRelevantAbilityModifier();

        SelectedSpells = _checkboxes
            .Where(cb => cb.IsChecked)
            .Select(cb => (Spell)cb.BindingContext)
            .ToList();

        if (SelectedSpells.Count > maxSpells)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Too Many Spells",
                $"You can only prepare {maxSpells} spells.",
                "OK"
            );
            return;
        }

        Close(); // Closes the popup and returns selected spells
    }
}
