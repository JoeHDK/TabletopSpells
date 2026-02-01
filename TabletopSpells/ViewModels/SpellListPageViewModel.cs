using System.Collections.ObjectModel;
using System.ComponentModel;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.Repositories;

namespace TabletopSpells.ViewModels
{
    public class SpellListPageViewModel : INotifyPropertyChanged
    {
        public bool IsDivineCaster { get; set; }
        public ObservableCollection<SpellViewModel> SpellViewModels { get; set; }
        public Character Character { get; set; }

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

        private ObservableCollection<Spell> _filteredSpells = [];

        public ObservableCollection<Spell> FilteredSpells
        {
            get => _filteredSpells;
            set
            {
                if (_filteredSpells == value) return;
                _filteredSpells = value;
                OnPropertyChanged(nameof(FilteredSpells));
            }
        }

        private ObservableCollection<SpellViewModel> _filteredSpellViewModels = [];

        public ObservableCollection<SpellViewModel> FilteredSpellViewModels
        {
            get => _filteredSpellViewModels;
            set
            {
                if (_filteredSpellViewModels == value) return;
                _filteredSpellViewModels = value;
                OnPropertyChanged(nameof(FilteredSpellViewModels));
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
            GameType = gameType ?? Game.dnd5e;

            System.Diagnostics.Debug.WriteLine($"GameType after default: {GameType}");

            // Load ALL spells for both divine and non-divine casters
            // This allows users to browse and search all spells when no filter is applied
            System.Diagnostics.Debug.WriteLine($"Loading ALL spells from JSON");
            var allSpells = SpellRepository.GetAllSpellsFromJson(GameType);
            var savedForChar = SharedViewModel.Instance.SpellsForCharacter(Character);
            
            foreach (var sp in allSpells)
            {
                var match = savedForChar?.FirstOrDefault(s => s.Id == sp.Id || s.Name == sp.Name);
                if (match != null)
                {
                    sp.IsFavoriteSpell = match.IsFavoriteSpell;
                    sp.IsAlwaysPrepared = match.IsAlwaysPrepared;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Loaded {allSpells.Count} total spells");
            SpellViewModels = new ObservableCollection<SpellViewModel>(
                allSpells.Select(spell => new SpellViewModel(spell, Character))
            );
            FilteredSpells = new ObservableCollection<Spell>(allSpells);
            System.Diagnostics.Debug.WriteLine($"Created {SpellViewModels.Count} SpellViewModels");
            System.Diagnostics.Debug.WriteLine($"Created {FilteredSpells.Count} FilteredSpells");

            FilterSpells();

            System.Diagnostics.Debug.WriteLine($"After FilterSpells:");
            System.Diagnostics.Debug.WriteLine($"  FilteredSpells.Count: {FilteredSpells.Count}");
            System.Diagnostics.Debug.WriteLine($"  FilteredSpellViewModels.Count: {FilteredSpellViewModels.Count}");
            System.Diagnostics.Debug.WriteLine($"=== SpellListPageViewModel Constructor END ===");
        }

        public void ReloadDivineSpellViewModels()
        {
            // Load ALL spells for both types of casters
            var allSpells = SpellRepository.GetAllSpellsFromJson(GameType);
            var savedForChar = SharedViewModel.Instance.SpellsForCharacter(Character);
            
            foreach (var sp in allSpells)
            {
                var match = savedForChar?.FirstOrDefault(s => s.Id == sp.Id || s.Name == sp.Name);
                if (match != null)
                {
                    sp.IsFavoriteSpell = match.IsFavoriteSpell;
                    sp.IsAlwaysPrepared = match.IsAlwaysPrepared;
                }
            }

            SpellViewModels = new ObservableCollection<SpellViewModel>(
                allSpells.Select(spell => new SpellViewModel(spell, Character))
            );
            FilteredSpells = new ObservableCollection<Spell>(allSpells);

            OnPropertyChanged(nameof(SpellViewModels));
            FilterSpells();
        }

        public void FilterSpells()
        {
            var characterClassName = Character.CharacterClass.ToString().ToLowerInvariant();
            var spells = SpellViewModels.Select(spellViewModel => spellViewModel.Spell).AsEnumerable();

            // Filter by spell level if selected (and restrict to class spells when level filter is active)
            if (SelectedSpellLevel.HasValue)
            {
                spells = spells.Where(spell =>
                    ParseSpellLevel(spell.SpellLevel, characterClassName) == SelectedSpellLevel.Value &&
                    IsSpellAvailableForClass(spell, characterClassName));
            }
            // When no level filter, show ALL spells (no class restriction)

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lower = SearchText.ToLowerInvariant();
                spells = spells.Where(s =>
                    (!string.IsNullOrEmpty(s.Name) && s.Name.ToLowerInvariant().Contains(lower)) ||
                    (!string.IsNullOrEmpty(s.SpellLevel) && s.SpellLevel.ToLowerInvariant().Contains(lower)));
            }

            // Order by spell level, then alphabetically
            var ordered = spells
                .OrderBy(s => ParseSpellLevel(s.SpellLevel, characterClassName))
                .ThenBy(s => s.Name);

            FilteredSpellViewModels = new ObservableCollection<SpellViewModel>(
                ordered.Select(s =>
                    SpellViewModels.FirstOrDefault(vm => vm.Spell.Id == s.Id) ?? new SpellViewModel(s, Character))
            );
        }

        /// <summary>
        /// Checks if a spell is available for the character's class.
        /// </summary>
        private bool IsSpellAvailableForClass(Spell spell, string characterClass)
        {
            if (string.IsNullOrEmpty(spell.SpellLevel))
                return false;

            return spell.SpellLevel.ToLowerInvariant().Contains(characterClass);
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
        }
    }
}