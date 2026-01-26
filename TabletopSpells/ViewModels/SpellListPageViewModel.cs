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
                // Ensure per-character spells are loaded so we can merge favorites/always-prepared flags
                var savedForChar = SharedViewModel.Instance.SpellsForCharacter(Character);
                foreach (var saved in savedForChar ?? new ObservableCollection<Spell>())
                {
                    // no-op: ensures the collection is initialized
                }
                System.Diagnostics.Debug.WriteLine($"Loaded {allClassSpells.Count} class spells");
                // Merge saved per-character flags into the repository spells so the UI represents favorites correctly
                foreach (var sp in allClassSpells)
                {
                    var match = savedForChar?.FirstOrDefault(s => s.Id == sp.Id || s.Name == sp.Name);
                    if (match != null)
                    {
                        sp.IsFavoriteSpell = match.IsFavoriteSpell;
                        sp.IsAlwaysPrepared = match.IsAlwaysPrepared;
                    }
                }

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
                // Merge per-character saved flags (favorites) into repository spells so the UI shows per-character favorites
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
            }
            
            FilterSpells();
            
            System.Diagnostics.Debug.WriteLine($"After FilterSpells:");
            System.Diagnostics.Debug.WriteLine($"  FilteredSpells.Count: {FilteredSpells.Count}");
            System.Diagnostics.Debug.WriteLine($"  FilteredSpellViewModels.Count: {FilteredSpellViewModels.Count}");
            System.Diagnostics.Debug.WriteLine($"=== SpellListPageViewModel Constructor END ===");
        }

        public void ReloadDivineSpellViewModels()
        {
            if (IsDivineCaster)
            {
                var allClassSpells = SharedViewModel.Instance.LoadAllClassSpells(Character);
                // Merge per-character saved flags
                var savedForChar = SharedViewModel.Instance.SpellsForCharacter(Character);
                foreach (var sp in allClassSpells)
                {
                    var match = savedForChar?.FirstOrDefault(s => s.Id == sp.Id || s.Name == sp.Name);
                    if (match != null)
                    {
                        sp.IsFavoriteSpell = match.IsFavoriteSpell;
                        sp.IsAlwaysPrepared = match.IsAlwaysPrepared;
                    }
                }
                SpellViewModels = new ObservableCollection<SpellViewModel>(
                    allClassSpells.Select(spell => new SpellViewModel(spell, Character))
                );
                FilteredSpellViewModels = new ObservableCollection<SpellViewModel>(SpellViewModels);
            }
            else
            {
                // Load all available spells for non-divine casters
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
                
                // Favorites first in alphabetical order, then the rest by level then alphabetical
                var favorites = filtered.Where(vm => vm.Spell.IsFavoriteSpell).OrderBy(vm => vm.Spell.Name);
                var rest = filtered.Where(vm => !vm.Spell.IsFavoriteSpell)
                    .OrderBy(vm => ParseSpellLevel(vm.Spell.SpellLevel, Character.CharacterClass.ToString()))
                    .ThenBy(vm => vm.Spell.Name);
                
                var final = favorites.Concat(rest);
                FilteredSpellViewModels = new ObservableCollection<SpellViewModel>(final);
                return;
            }

            // For non-divine casters, filter the loaded spells
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

            // Favorites first in alphabetical order, then the rest by level then alphabetical
            var favs = spells.Where(s => s.IsFavoriteSpell).OrderBy(s => s.Name);
            var others = spells.Where(s => !s.IsFavoriteSpell)
                .OrderBy(s => ParseSpellLevel(s.SpellLevel, Character.CharacterClass.ToString()))
                .ThenBy(s => s.Name);
            
            var ordered = favs.Concat(others);
            
            FilteredSpellViewModels = new ObservableCollection<SpellViewModel>(
                ordered.Select(s => SpellViewModels.FirstOrDefault(vm => vm.Spell.Id == s.Id) ?? new SpellViewModel(s, Character))
            );
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
