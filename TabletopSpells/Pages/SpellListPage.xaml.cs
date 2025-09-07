using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SpellListPage : ContentPage, INotifyPropertyChanged
{
    private readonly Game gameType;
    private readonly Character? character;
    private int? selectedSpellLevel = null;
    private string currentSearchText = "";

    public ObservableCollection<Spell> Spells { get; set; } = new();
    public ObservableCollection<Spell> FilteredSpells { get; set; } = new();
    public ObservableCollection<SpellViewModel> SpellViewModels { get; set; } = new();

    public bool IsDivineCaster => character?.IsDivineCaster ?? false;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public string PreparedSpellCountText =>
        $"Prepared: {character?.GetPreparedSpells().Count ?? 0} / {character?.Level + character?.GetRelevantAbilityModifier() ?? 0}";

    public SpellListPage(Character character, Game gameType)
    {
        InitializeComponent();
        this.character = character;
        this.gameType = gameType;

        SharedViewModel.Instance.CurrentCharacter = character;
        SharedViewModel.Instance.LoadSpellsForCharacter(character);
        SharedViewModel.Instance.LoadPreparedSpells(character);

        Spells = new ObservableCollection<Spell>(GetAllSpellsFromJson(gameType));
        FilteredSpells = new ObservableCollection<Spell>(Spells);

        if (IsDivineCaster)
        {
            ReloadDivineSpellViewModels();
        }

        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        SharedViewModel.Instance.SpellsChanged = () =>
        {
            if (!IsDivineCaster) return;
            ReloadDivineSpellViewModels();
        };

        ReloadDivineSpellViewModels();
    }

    private void ReloadDivineSpellViewModels()
    {
        if (character == null) return;

        var spells = SharedViewModel.Instance.SpellsForCharacter(character);
        var viewModels = spells
            .Select(s => new SpellViewModel(s, character))
            .OrderByDescending(vm => vm.IsPrepared)
            .ThenBy(vm => vm.Spell.SpellLevel)
            .ThenBy(vm => vm.Spell.Name)
            .ToList();

        SpellViewModels.Clear();
        foreach (var vm in viewModels)
        {
            SpellViewModels.Add(vm);
        }
        
        OnPropertyChanged(nameof(SpellViewModels));
        OnPropertyChanged(nameof(PreparedSpellCountText));
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        currentSearchText = e.NewTextValue.ToLower();
        FilterSpells();
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
            if (selectedSpellLevel == i)
                levels[i] = $"* {levels[i]}";
        }

        var action = await DisplayActionSheet("Filters", null, null, levels.ToArray());
        if (action == "Cancel" || string.IsNullOrEmpty(action)) return;

        var cleanAction = action.Replace("*", "").Trim();
        var match = Regex.Match(cleanAction, @"\d+");
        var selectedLevel = match.Success ? int.Parse(match.Value) : (cleanAction == "Cantrips" ? 0 : -1);

        selectedSpellLevel = (selectedSpellLevel == selectedLevel) ? null : selectedLevel;

        FilterSpells();
        UpdateTitle();
    }

    private void UpdateTitle()
    {
        Title = selectedSpellLevel switch
        {
            null => "Spells",
            0 => "Cantrips",
            1 => "1st level spells",
            2 => "2nd level spells",
            3 => "3rd level spells",
            _ => $"{selectedSpellLevel}th level spells"
        };
    }

    private void FilterSpells()
    {
        if (IsDivineCaster)
        {
            var filtered = SpellViewModels
                .Where(vm =>
                    (string.IsNullOrEmpty(currentSearchText) || vm.Spell.Name.ToLower().Contains(currentSearchText)) &&
                    (!selectedSpellLevel.HasValue || ParseSpellLevel(vm.Spell.SpellLevel, character.CharacterClass.ToString()) == selectedSpellLevel))
                .OrderByDescending(vm => vm.IsPrepared)
                .ThenBy(vm => vm.Spell.SpellLevel)
                .ThenBy(vm => vm.Spell.Name)
                .ToList();

            SpellViewModels.Clear();
            foreach (var vm in filtered)
                SpellViewModels.Add(vm);

            OnPropertyChanged(nameof(SpellViewModels));
            OnPropertyChanged(nameof(PreparedSpellCountText));
        }
        else
        {
            var filteredSpells = Spells
                .Where(spell =>
                    (string.IsNullOrEmpty(currentSearchText) || spell.Name.ToLower().Contains(currentSearchText)) &&
                    (!selectedSpellLevel.HasValue || ParseSpellLevel(spell.SpellLevel, character.CharacterClass.ToString()) == selectedSpellLevel))
                .OrderBy(spell => spell.Name)
                .ToList();

            FilteredSpells.Clear();
            foreach (var spell in filteredSpells)
                FilteredSpells.Add(spell);

            OnPropertyChanged(nameof(FilteredSpells));
        }
    }


    private int ParseSpellLevel(string spellLevel, string characterClass)
    {
        if (string.IsNullOrWhiteSpace(characterClass)) return -1;

        var classLower = characterClass.ToLower();
        var entries = spellLevel.Split(',');

        foreach (var entry in entries)
        {
            var parts = entry.Trim().ToLower().Split(' ');
            if (parts.Length < 2) continue;
            if (!parts[0].Contains(classLower)) continue;

            var match = MyRegex().Match(parts[1]);
            if (match.Success) return int.Parse(match.Value);
        }

        return -1;
    }

    public static List<Spell> GetAllSpellsFromJson(Game gameType)
    {
        try
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            using var stream = gameType switch
            {
                Game.pathfinder1e => assembly.GetManifestResourceStream("TabletopSpells.Spells.Pathfinder1e.json"),
                Game.dnd5e => assembly.GetManifestResourceStream("TabletopSpells.Spells.dnd 5e.json"),
                _ => null
            };

            if (stream == null) return new List<Spell>();

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<List<Spell>>(json, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore,
                Error = (sender, args) =>
                {
                    Debug.WriteLine("JSON Error: " + args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            }) ?? new List<Spell>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception loading spells: {ex}");
            return new List<Spell>();
        }
    }

    private async void OnSpellSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Spell selectedSpell)
        {
            int level = ParseSpellLevel(selectedSpell.SpellLevel, character?.CharacterClass.ToString() ?? "");
            await Navigation.PushAsync(new SpellDetailPage(selectedSpell, character, level, gameType));
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex MyRegex();
}
