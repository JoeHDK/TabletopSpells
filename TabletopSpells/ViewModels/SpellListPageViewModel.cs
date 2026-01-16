using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.Repositories;
using TabletopSpells.ViewModels;

namespace TabletopSpells.ViewModels
{
    public class SpellListPageViewModel : INotifyPropertyChanged
    {
        public bool IsDivineCaster { get; set; }
        public ObservableCollection<SpellViewModel> SpellViewModels { get; set; }
        public Character Character { get; set; }

        public string PreparedSpellCountText
    {
        get
        {
            var prepared = SpellViewModels.Count(s => s.IsPrepared);
            var limit = Character.Level + Character.GetRelevantAbilityModifier();
            return $"Prepared: {prepared}/{limit}";
        }
    }

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
            System.Diagnostics.Debug.WriteLine($"=== SpellListPageViewModel Constructor START ===");
            System.Diagnostics.Debug.WriteLine($"Character: {character?.Name ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"IsDivineCaster: {isDivineCaster}");
            System.Diagnostics.Debug.WriteLine($"GameType: {gameType}");
            
            Character = character;
            IsDivineCaster = isDivineCaster;
            GameType = gameType ?? Game.dnd5e; // Default to dnd5e if not provided
            
            System.Diagnostics.Debug.WriteLine($"GameType after default: {GameType}");
            
            if (IsDivineCaster)
            {
                System.Diagnostics.Debug.WriteLine($"Loading spells for DIVINE CASTER");
                // Use the full class spell list for divine casters
                var allClassSpells = SharedViewModel.Instance.LoadAllClassSpells(Character);
                System.Diagnostics.Debug.WriteLine($"Loaded {allClassSpells.Count} class spells");
                SpellViewModels = new ObservableCollection<SpellViewModel>(
                    allClassSpells.Select(spell => new SpellViewModel(spell, Character))
                );
                FilteredSpellViewModels = new ObservableCollection<SpellViewModel>(SpellViewModels);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Loading spells for NON-DIVINE CASTER");
                // For non-divine casters, load ALL available spells from repository to allow adding new ones
                var allSpells = SpellRepository.GetAllSpellsFromJson(GameType);
                System.Diagnostics.Debug.WriteLine($"Loaded {allSpells.Count} total spells");
                SpellViewModels = new ObservableCollection<SpellViewModel>(
                    allSpells.Select(spell => new SpellViewModel(spell, Character))
                );
                FilteredSpells = new ObservableCollection<Spell>(allSpells);
                System.Diagnostics.Debug.WriteLine($"Created {SpellViewModels.Count} SpellViewModels");
                System.Diagnostics.Debug.WriteLine($"Created {FilteredSpells.Count} FilteredSpells");
            }
            
            foreach (var svm in SpellViewModels)
                svm.PropertyChanged += SpellViewModel_PropertyChanged;
            FilterSpells(); // Ensure FilteredSpells/FilteredSpellViewModels is populated on startup
            
            System.Diagnostics.Debug.WriteLine($"After FilterSpells:");
            System.Diagnostics.Debug.WriteLine($"  FilteredSpells.Count: {FilteredSpells.Count}");
            System.Diagnostics.Debug.WriteLine($"  FilteredSpellViewModels.Count: {FilteredSpellViewModels.Count}");
            System.Diagnostics.Debug.WriteLine($"=== SpellListPageViewModel Constructor END ===");
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
                // Load all available spells for non-divine casters
                var allSpells = SpellRepository.GetAllSpellsFromJson(GameType);
                SpellViewModels = new ObservableCollection<SpellViewModel>(
                    allSpells.Select(spell => new SpellViewModel(spell, Character))
                );
                FilteredSpells = new ObservableCollection<Spell>(allSpells);
            }
            
            foreach (var svm in SpellViewModels)
                svm.PropertyChanged -= SpellViewModel_PropertyChanged; // Remove old handlers
            foreach (var svm in SpellViewModels)
                svm.PropertyChanged += SpellViewModel_PropertyChanged; // Add new handlers
            
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

            // For non-divine casters, filter the loaded spells (not Character.KnownSpells which is empty)
            var spells = SpellViewModels.Select(svm => svm.Spell).AsEnumerable();

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

        /// <summary>
        /// Checks if the character can prepare another spell or if they're at the limit.
        /// </summary>
        public bool CanPrepareMoReSpells()
        {
            int limit = Character.Level + Character.GetRelevantAbilityModifier();
            return SpellViewModels.Count(s => s.IsPrepared) < limit;
        }

        /// <summary>
        /// Gets all currently prepared spells.
        /// </summary>
        public List<Spell> GetPreparedSpells()
        {
            return SpellViewModels
                .Where(s => s.IsPrepared)
                .Select(s => s.Spell)
                .ToList();
        }

        /// <summary>
        /// Replaces an old prepared spell with a new one.
        /// </summary>
        public void ReplacePrepareddSpell(Spell oldSpell, Spell newSpell)
        {
            // Find and unprepare the old spell
            var oldViewModel = SpellViewModels.FirstOrDefault(s => s.Spell.Id == oldSpell.Id);
            if (oldViewModel != null)
            {
                oldViewModel.IsPrepared = false;
            }

            // Prepare the new spell
            var newViewModel = SpellViewModels.FirstOrDefault(s => s.Spell.Id == newSpell.Id);
            if (newViewModel != null)
            {
                newViewModel.IsPrepared = true;
            }

            OnPropertyChanged(nameof(PreparedSpellCountText));
        }
    }
}
