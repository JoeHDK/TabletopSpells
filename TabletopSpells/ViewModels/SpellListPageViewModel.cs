using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.ViewModels;

namespace TabletopSpells.ViewModels
{
    public class SpellListPageViewModel : INotifyPropertyChanged
    {
        public bool IsDivineCaster { get; set; }
        public ObservableCollection<SpellViewModel> SpellViewModels { get; set; }
        public Character Character { get; set; }

        public string PreparedSpellCountText => $"Prepared: {SpellViewModels.Count(s => s.IsPrepared)}";

        public Game GameType { get; set; }
        private int? _selectedSpellLevel;
        public int? SelectedSpellLevel
        {
            get => _selectedSpellLevel;
            set
            {
                if (_selectedSpellLevel != value)
                {
                    _selectedSpellLevel = value;
                    OnPropertyChanged(nameof(SelectedSpellLevel));
                    FilterSpells();
                }
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    FilterSpells();
                }
            }
        }

        private ObservableCollection<Spell> _filteredSpells = new ObservableCollection<Spell>();
        public ObservableCollection<Spell> FilteredSpells
        {
            get => _filteredSpells;
            set
            {
                if (_filteredSpells != value)
                {
                    _filteredSpells = value;
                    OnPropertyChanged(nameof(FilteredSpells));
                }
            }
        }

        private ObservableCollection<SpellViewModel> _filteredSpellViewModels = new ObservableCollection<SpellViewModel>();
        public ObservableCollection<SpellViewModel> FilteredSpellViewModels
        {
            get => _filteredSpellViewModels;
            set
            {
                if (_filteredSpellViewModels != value)
                {
                    _filteredSpellViewModels = value;
                    OnPropertyChanged(nameof(FilteredSpellViewModels));
                }
            }
        }

        public SpellListPageViewModel(Character character, bool isDivineCaster, Game? gameType = null)
        {
            Character = character;
            IsDivineCaster = isDivineCaster;
            GameType = gameType ?? Game.dnd5e; // Default to dnd5e if not provided
            if (IsDivineCaster)
            {
                // Use the full class spell list for divine casters
                var allClassSpells = SharedViewModel.Instance.LoadAllClassSpells(Character);
                SpellViewModels = new ObservableCollection<SpellViewModel>(
                    allClassSpells.Select(spell => new SpellViewModel(spell, Character))
                );
                FilteredSpellViewModels = new ObservableCollection<SpellViewModel>(SpellViewModels);
            }
            else
            {
                SpellViewModels = new ObservableCollection<SpellViewModel>(
                    Character.KnownSpells.Select(spell => new SpellViewModel(spell, Character))
                );
            }
            foreach (var svm in SpellViewModels)
                svm.PropertyChanged += SpellViewModel_PropertyChanged;
            FilterSpells(); // Ensure FilteredSpells/FilteredSpellViewModels is populated on startup
        }

        private void SpellViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SpellViewModel.IsPrepared))
                OnPropertyChanged(nameof(PreparedSpellCountText));
        }

        public void ReloadDivineSpellViewModels()
        {
            if (IsDivineCaster)
            {
                var allClassSpells = SharedViewModel.Instance.LoadAllClassSpells(Character);
                SpellViewModels = new ObservableCollection<SpellViewModel>(
                    allClassSpells.Select(spell => new SpellViewModel(spell, Character))
                );
                FilteredSpellViewModels = new ObservableCollection<SpellViewModel>(SpellViewModels);
            }
            else
            {
                SpellViewModels = new ObservableCollection<SpellViewModel>(
                    Character.KnownSpells.Select(spell => new SpellViewModel(spell, Character))
                );
            }
            OnPropertyChanged(nameof(SpellViewModels));
            FilterSpells();
        }

        public void FilterSpells()
        {
            if (IsDivineCaster)
            {
                var filtered = SpellViewModels.AsEnumerable();
                if (SelectedSpellLevel.HasValue)
                {
                    filtered = filtered.Where(vm =>
                        ParseSpellLevel(vm.Spell.SpellLevel, Character.CharacterClass.ToString()) == SelectedSpellLevel.Value);
                }
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var lower = SearchText.ToLowerInvariant();
                    filtered = filtered.Where(vm =>
                        (!string.IsNullOrEmpty(vm.Spell.Name) && vm.Spell.Name.ToLowerInvariant().Contains(lower)) ||
                        (!string.IsNullOrEmpty(vm.Spell.SpellLevel) && vm.Spell.SpellLevel.ToLowerInvariant().Contains(lower)));
                }
                FilteredSpellViewModels = new ObservableCollection<SpellViewModel>(filtered);
                return;
            }

            var spells = Character.KnownSpells.AsEnumerable();

            if (SelectedSpellLevel.HasValue)
            {
                spells = spells.Where(s =>
                    ParseSpellLevel(s.SpellLevel, Character.CharacterClass.ToString()) == SelectedSpellLevel.Value);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lower = SearchText.ToLowerInvariant();
                spells = spells.Where(s =>
                    (!string.IsNullOrEmpty(s.Name) && s.Name.ToLowerInvariant().Contains(lower)) ||
                    (!string.IsNullOrEmpty(s.SpellLevel) && s.SpellLevel.ToLowerInvariant().Contains(lower)));
            }

            FilteredSpells = new ObservableCollection<Spell>(spells);
        }

        public int ParseSpellLevel(string spellLevel, string characterClass)
        {
            // Stub: Add parsing logic
            if (string.IsNullOrEmpty(spellLevel)) return -1;
            if (spellLevel.ToLower().Contains("cantrip")) return 0;
            var match = System.Text.RegularExpressions.Regex.Match(spellLevel, "\\d+");
            return match.Success ? int.Parse(match.Value) : -1;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
