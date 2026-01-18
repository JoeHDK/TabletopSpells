using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using TabletopSpells.Helpers;
using TabletopSpells.Models;
using TabletopSpells.Pages;
using TabletopSpells.Repositories;

namespace TabletopSpells.ViewModels;

public class SharedViewModel : INotifyPropertyChanged
{
    #region Singleton Pattern

    private static SharedViewModel? _instance;
    public static SharedViewModel Instance => _instance ??= new SharedViewModel();

    #endregion

    #region Fields & Constants

    private const string CharactersKey = "characters";

    public ObservableCollection<Grouping<int, SpellCastLog>> GroupedLogs { get; set; } =
        new ObservableCollection<Grouping<int, SpellCastLog>>();

    private Dictionary<Guid?, ObservableCollection<Spell>> characterSpells = new();

    // List of all characters
    public ObservableCollection<Character> Characters { get; private set; }

    #endregion

    #region Properties

    private Character? currentCharacter;

    public Character? CurrentCharacter
    {
        get => currentCharacter;
        set
        {
            if (currentCharacter != value)
            {
                currentCharacter = value;
                OnPropertyChanged(nameof(CurrentCharacter));
                if (currentCharacter != null)
                {
                    // Load spell slots directly into the character
                    LoadSpellsPerDayDetails(currentCharacter);
                    
                    // Migrate spells from name-based keys to ID-based keys
                    MigrateSpellsIfNeeded(currentCharacter);
                    
                    // Note: Do NOT load prepared spells here!
                    // LoadPreparedSpells must happen AFTER spells are loaded/auto-filled,
                    // which happens in SpellsForCharacter() method
                }
            }
        }
    }

    public Dictionary<Guid?, ObservableCollection<Spell>> CharacterSpells
    {
        get => characterSpells;
        set
        {
            if (characterSpells != value)
            {
                characterSpells = value;
                OnPropertyChanged(nameof(CharacterSpells));
            }
        }
    }

    #endregion

    #region Constructor

    public SharedViewModel()
    {
        // Load all characters into the ObservableCollection at startup
        Characters = new ObservableCollection<Character>(LoadCharacters());
    }

    #endregion

    #region Character-Related Persistence Methods

    /// <summary>
    /// Retrieves all characters from persistent storage.
    /// </summary>
    private List<Character> LoadCharacters()
    {
        // Prefer file-based characters storage; fall back to Preferences for migration
        try
        {
            var fromFile = LocalStorageHelper.LoadCharactersFromFile();
            if (fromFile != null && fromFile.Count > 0)
                return fromFile;

            // Fallback to older Preferences-based storage if file storage is empty
            string charactersJson = Preferences.Get(CharactersKey, "[]");
            var fromPref = JsonConvert.DeserializeObject<List<Character>>(charactersJson) ?? new List<Character>();

            // Persist migrated characters to file for future runs
            if (fromPref.Any()) LocalStorageHelper.SaveCharactersToFile(fromPref);
            return fromPref;
        }
        catch (Exception)
        {
            return new List<Character>();
        }
    }

    /// <summary>
    /// Saves a character to persistent storage and updates the in-memory collection.
    /// </summary>
    public async Task SaveCharacterAsync(Character character)
    {
        if (character == null) return;

        var characters = LoadCharacters();

        // Update existing character or add a new one
        var existingCharacter = characters.FirstOrDefault(c => c.ID == character.ID);
        if (existingCharacter != null)
        {
            existingCharacter.Name = character.Name;
            existingCharacter.Level = character.Level;
            existingCharacter.AbilityScores = character.AbilityScores;
        }
        else
        {
            characters.Add(character);
        }

        // Save updated characters to file storage
        LocalStorageHelper.SaveCharactersToFile(characters);

        // Update the in-memory collection
        RefreshInMemoryCharacters(characters);
        OnPropertyChanged(nameof(Characters)); // Notify UI of changes
    }

    /// <summary>
    /// Refreshes the in-memory Characters ObservableCollection.
    /// </summary>
    private void RefreshInMemoryCharacters(List<Character> characters)
    {
        Characters.Clear();
        foreach (var character in characters)
        {
            Characters.Add(character);
        }
    }

    /// <summary>
    /// Retrieves a specific character by its ID.
    /// </summary>
    public Character? GetCharacterById(Guid characterId)
    {
        return LoadCharacters().FirstOrDefault(c => c.ID == characterId);
    }

    /// <summary>
    /// Deletes a character from the list.
    /// </summary>
    public void DeleteCharacter(Character character)
    {
        if (character == null) return;

        // Load all characters from storage
        var characters = LoadCharacters();

        // Remove the character from the list
        characters.RemoveAll(c => c.ID == character.ID);

        // Save the updated character list back to file storage
        LocalStorageHelper.SaveCharactersToFile(characters);

        // Cleanup associated persistent data for the deleted character
        // Remove stored spell files for this character
        if (character.ID != null)
        {
            LocalStorageHelper.DeleteCharacterFolder(character.ID.Value);
        }

        // Remove stored logs for the character (legacy key)
        // Also remove any legacy preferences keys if present (best-effort)
        try
        {
            Preferences.Remove($"spellLogs_{character.Name}");
        }
        catch
        {
        }

        // Remove the character from the in-memory collection
        Characters.Remove(character);

        // Notify observers
        OnPropertyChanged(nameof(Characters));
    }

    #endregion

    #region Spell Management & Migration

    private void MigrateSpellsIfNeeded(Character character)
    {
        // Migrate legacy preference-based spell storage (if present)
        try
        {
            var oldSpellKeysRaw = Preferences.Get($"spellKeys_{character.Name}", string.Empty);
            var oldSpellKeys = oldSpellKeysRaw.Split(',').Where(key => !string.IsNullOrWhiteSpace(key)).ToList();
            if (oldSpellKeys.Any())
            {
                var spells = new ObservableCollection<Spell>();

                foreach (var key in oldSpellKeys)
                {
                    var compressedSpellJson = Preferences.Get(key, string.Empty);
                    if (!string.IsNullOrEmpty(compressedSpellJson))
                    {
                        var spellJson = CompressionHelper.DecompressString(compressedSpellJson);
                        var spell = JsonConvert.DeserializeObject<Spell>(spellJson);
                        if (spell != null)
                        {
                            spells.Add(spell);
                            SaveSpellForCharacter(character, spell); // Save with new file-based storage
                        }
                    }
                }

                // Remove old preference keys after migration
                try
                {
                    Preferences.Remove($"spellKeys_{character.Name}");
                }
                catch
                {
                }

                foreach (var key in oldSpellKeys)
                {
                    try
                    {
                        Preferences.Remove(key);
                    }
                    catch
                    {
                    }
                }

                CharacterSpells[character.ID] = spells;
                CharacterSpells[character.ID].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
            }
        }
        catch
        {
            // ignore migration errors
        }
    }

    public Action? SpellsChanged { get; set; }

    public void AddSpell(Character character, Spell spell)
    {
        if (character.ID == null)
        {
            character.ID = Guid.NewGuid();
        }

        if (!CharacterSpells.ContainsKey(character.ID))
        {
            CharacterSpells[character.ID] = new ObservableCollection<Spell>();
            CharacterSpells[character.ID].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
        }

        if (CharacterSpells[character.ID].Any(s => s.Name == spell.Name)) return;

        // Add to character's own known spells (sets IsAlwaysPrepared for divine casters)
        // Defensive: ensure character.IsDivineCaster reflects the class
        character.IsDivineCaster = ClassHelper.IsDivineCaster(character.CharacterClass);

        character.AddSpell(spell);

        // Do NOT automatically mark spells as always-prepared here. Domain (always-prepared)
        // status is explicit and should be set via the UI (Prepare -> Mark as Domain) or via
        // a dedicated API that calls SaveAlwaysPreparedSpells.

        // Also keep the in-memory CharacterSpells collection in sync
        CharacterSpells[character.ID].Add(spell);

        // Persist the spell to file storage
        if (character.ID != null)
        {
            LocalStorageHelper.SaveSpellToFile(character.ID.Value, spell);
        }
    }

    /// <summary>
    /// Loads spells for the given character from persistent storage.
    /// </summary>
    public void LoadSpellsForCharacter(Character character)
    {
        if (character == null) return;

        // Load spells from file-based storage for the character
        var spells = new ObservableCollection<Spell>();

        if (character.ID != null)
        {
            try
            {
                var list = LocalStorageHelper.LoadSpellFiles(character.ID.Value);
                foreach (var sp in list)
                {
                    // Determine if the loaded spell is native to this character's class and mark it
                    try
                    {
                        var className = character.CharacterClass.ToString();
                        if (!string.IsNullOrEmpty(sp.SpellLevel) && sp.SpellLevel.ToLower().Contains(className.ToLower()))
                        {
                            sp.IsNativeSpell = true;
                            // Persist the updated flag so future loads reflect native state
                            LocalStorageHelper.SaveSpellToFile(character.ID.Value, sp);
                        }
                    }
                    catch { }

                    spells.Add(sp);
                }
            }
            catch
            {
                // ignore file read errors
            }
        }

        CharacterSpells[character.ID] = spells;
        CharacterSpells[character.ID].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
    }

    public void LoadPreparedSpells(Character character)
    {
        if (character == null || character.ID == null) return;

        try
        {
            var preparedSpellIds = LocalStorageHelper.LoadPreparedSpellIds(character.ID.Value);
            Debug.WriteLine($"=== LoadPreparedSpells for {character.Name} ===");
            Debug.WriteLine($"Loaded {preparedSpellIds.Count} prepared spell IDs from file");

            if (!CharacterSpells.TryGetValue(character.ID, out var liveSpells))
            {
                Debug.WriteLine($"ERROR: No CharacterSpells found for {character.Name}");
                return;
            }

            Debug.WriteLine($"CharacterSpells has {liveSpells.Count} spells");

            foreach (var savedId in preparedSpellIds)
            {
                var match = liveSpells.FirstOrDefault(s => s.Id == savedId);
                if (match != null)
                {
                    // Ensure spell is in known spells first
                    character.AddSpell(match);
                    // Then toggle it as prepared
                    character.TogglePreparedSpell(match);
                    Debug.WriteLine($"  ✓ Prepared: {match.Name}");
                }
                else
                {
                    Debug.WriteLine($"  ✗ Spell ID {savedId} not found in CharacterSpells");
                }
            }

            // Load always-prepared (domain) spells and mark them on the model
            var alwaysIds = LocalStorageHelper.LoadAlwaysPreparedSpellIds(character.ID.Value);
            Debug.WriteLine($"Loaded {alwaysIds.Count} always-prepared spell IDs");
            
            foreach (var aid in alwaysIds)
            {
                var m = liveSpells.FirstOrDefault(s => s.Id == aid);
                if (m != null)
                {
                    m.IsAlwaysPrepared = true;
                    character.AddSpell(m);
                    if (!character.AlwaysPreparedSpells.Contains(m.Name ?? string.Empty))
                        character.AlwaysPreparedSpells.Add(m.Name ?? string.Empty);
                    Debug.WriteLine($"  ✓ Always Prepared: {m.Name}");
                }
                else
                {
                    Debug.WriteLine($"  ✗ Always-prepared spell ID {aid} not found");
                }
            }

            var totalPrepared = character.GetPreparedSpells().Count;
            Debug.WriteLine($"Total prepared spells after load: {totalPrepared}");
            Debug.WriteLine($"=== LoadPreparedSpells Complete ===");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading prepared spells for {character.Name}: {ex.Message}");
        }
    }

    public void SaveCharacterAlwaysPreparedSpells(Guid? characterId, List<Guid> ids)
    {
        if (characterId == null) return;

        try
        {
            if (characterId.HasValue)
            {
                LocalStorageHelper.SaveAlwaysPreparedSpellIds(characterId.Value, ids);
            }
            Debug.WriteLine($"Saving ALWAYS prepared spells: {ids.Count} for {characterId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving always prepared spells: {ex.Message}");
        }
    }

    public void SaveAlwaysPreparedSpells(Character character)
    {
        if (character?.ID == null) return;
        var ids = character.GetPreparedSpells().Where(s => s.IsAlwaysPrepared).Select(s => s.Id).ToList();
        SaveCharacterAlwaysPreparedSpells(character.ID, ids);
    }

    /// <summary>
    /// Retrieves all spells for the given character from the in-memory dictionary.
    /// If not already loaded, initializes them from persistent storage.
    /// </summary>
    public ObservableCollection<Spell> SpellsForCharacter(Character character)
    {
        if (!CharacterSpells.ContainsKey(character.ID))
        {
            LoadSpellsForCharacter(character);

            // Auto-fill divine casters with all class spells
            if (character.IsDivineCaster && (!CharacterSpells[character.ID]?.Any() ?? true))
            {
                var allSpells = SpellRepository.GetAllSpellsFromJson(character.GameType); // reuse your loader
                foreach (var spell in allSpells)
                {
                    AddSpell(character, spell);
                }
            }

            // After ensuring CharacterSpells contains the character's spells (loaded or auto-filled),
            // load prepared spell IDs and apply them to the in-memory spell collection so prepared state persists.
            try
            {
                LoadPreparedSpells(character);
            }
            catch { /* ignore any issues applying prepared flags */ }
        }

        return CharacterSpells[character.ID];
    }

    public List<Spell> LoadAllClassSpells(Character character)
    {
        var allSpells = SpellRepository.GetAllSpellsFromJson(character.GameType); // reuse your loader
        var className = character.CharacterClass.ToString().ToLower();

        return allSpells
            .Where(spell =>
                !string.IsNullOrEmpty(spell.SpellLevel) &&
                spell.SpellLevel.ToLower().Contains(className))
            .ToList();
    }

    public void SaveSpellForCharacter(Character character, Spell spell)
    {
        // Persist spell to per-character file storage
        if (character?.ID != null)
        {
            LocalStorageHelper.SaveSpellToFile(character.ID.Value, spell);
        }
    }

    public void RemoveSpellForCharacter(Character character, Spell? spell)
    {
        if (character?.ID == null || spell == null) return;

        if (CharacterSpells.TryGetValue(character.ID, out var spells))
        {
            var existing = spells.FirstOrDefault(s => s.Id == spell.Id) ??
                           spells.FirstOrDefault(s => s.Name == spell.Name);
            if (existing != null)
                spells.Remove(existing);
        }

        character.RemoveSpell(spell);

        if (character.IsDivineCaster)
        {
            var prepared = character.GetPreparedSpells();
            if (prepared.Any(s => s.Id == spell.Id))
                character.TogglePreparedSpell(spell);

            SavePreparedSpells(character);
        }

        // Remove persisted spell file for this character
        if (character.ID != null)
        {
            LocalStorageHelper.DeleteSpellFile(character.ID.Value, spell);
        }

        OnPropertyChanged(nameof(CharacterSpells));
    }


    public static string GenerateSpellKey(Character character, Spell spell)
    {
        if (character?.ID == null || spell == null)
            throw new ArgumentNullException("Character or Spell cannot be null.");

        return $"{character.ID}_{spell.Name}_{spell.SpellLevel}";
    }

    #endregion

    #region Spell Slots & Logs

    public void SaveSpellsPerDayDetails(Character character, Dictionary<int, int> maxSpellsPerDay,
        Dictionary<int, int> spellsUsedToday)
    {
        try
        {
            if (character?.ID != null)
                LocalStorageHelper.SaveSpellsPerDayDetails(character.ID.Value, maxSpellsPerDay, spellsUsedToday);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving spell data: {ex.Message}");
        }
    }

    private static void LoadSpellsPerDayDetails(Character character)
    {
        if (character.ID != null)
        {
            var (max, used) = LocalStorageHelper.LoadSpellsPerDayDetails(character.ID.Value);
            character.MaxSpellsPerDay = max;
            character.SpellsUsedToday = used;
            return;
        }

        character.MaxSpellsPerDay = new Dictionary<int, int>();
        character.SpellsUsedToday = new Dictionary<int, int>();
    }

    public void LoadLogs(Character character)
    {
        var logs = new List<SpellCastLog>();
        if (character.ID != null)
        {
            logs = LocalStorageHelper.LoadSpellLogs<SpellCastLog>(character.ID.Value);
        }

        GroupedLogs.Clear();

        var groupedData = logs
            .GroupBy(log => log.SessionId)
            .OrderByDescending(group => group.Key)
            .Select(group => new Grouping<int, SpellCastLog>(group.Key, group.OrderByDescending(log => log.CastTime)))
            .ToList();

        foreach (var newGroup in groupedData.Select(group => new Grouping<int, SpellCastLog>(group.Key, group)))
        {
            GroupedLogs.Add(newGroup);
        }

        OnPropertyChanged(nameof(GroupedLogs));
    }

    public void LogFailedSpellCast(Character character, string spellName, int spellLevel, string reason)
    {
        if (character == null || string.IsNullOrWhiteSpace(spellName) || string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException("Character, spell name, and reason cannot be null or empty.");

        var failedLog = new SpellCastLog
        {
            CharacterName = character.Name,
            SpellName = spellName,
            SpellLevel = spellLevel,
            CastTime = DateTime.UtcNow,
            Success = false,
            Reason = reason,
            SessionId = GetCurrentSessionId()
        };

        List<SpellCastLog> logs;
        if (character.ID != null)
        {
            logs = LocalStorageHelper.LoadSpellLogs<SpellCastLog>(character.ID.Value);
            logs.Add(failedLog);
            LocalStorageHelper.SaveSpellLogs(character.ID.Value, logs.Cast<object>().ToList());
        }
        else
        {
            // Fallback to preferences for legacy records
            var logsJson = Preferences.Get($"spellLogs_{character.Name}", "[]");
            logs = JsonConvert.DeserializeObject<List<SpellCastLog>>(logsJson) ?? new List<SpellCastLog>();
            logs.Add(failedLog);
            Preferences.Set($"spellLogs_{character.Name}", JsonConvert.SerializeObject(logs));
        }

        OnPropertyChanged(nameof(GroupedLogs));
    }

    private int GetCurrentSessionId()
    {
        // Retrieve the current session ID; defaults to 0 if not present
        var sessionId = LocalStorageHelper.LoadSessionId();
        if (sessionId != 0) return sessionId;
        sessionId = GenerateNewSessionId();
        LocalStorageHelper.SaveSessionId(sessionId);
        return sessionId;
    }

    /// <summary>
    /// Generates a new, unique session ID.
    /// Can be based on a timestamp or increment logic to ensure uniqueness.
    /// </summary>
    /// <returns>A unique session ID as an integer.</returns>
    private int GenerateNewSessionId()
    {
        // Example: Use a timestamp as the session ID for uniqueness
        return (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Logs a successful spell cast for the given character.
    /// This includes the spell information, level, and a success indicator.
    /// </summary>
    /// <param name="character">The character casting the spell.</param>
    /// <param name="spellName">The name of the spell being cast.</param>
    /// <param name="spellLevel">The spell level being cast.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void LogSpellCast(Character character, string spellName, int spellLevel, bool castAsRitual)
    {
        if (character == null || string.IsNullOrWhiteSpace(spellName))
            throw new ArgumentNullException("Character and spell name cannot be null or empty.");

        // Create a log entry for the successful spell cast
        var successLog = new SpellCastLog
        {
            CharacterName = character.Name,
            SpellName = spellName,
            SpellLevel = spellLevel,
            CastTime = DateTime.UtcNow,
            Success = true,
            Reason = "Spell cast successfully.",
            SessionId = GetCurrentSessionId() // Ensure session tracking
        };

        if (character.ID != null)
        {
            var logs = LocalStorageHelper.LoadSpellLogs<SpellCastLog>(character.ID.Value);
            logs.Add(successLog);
            LocalStorageHelper.SaveSpellLogs(character.ID.Value, logs.Cast<object>().ToList());
        }
        else
        {
            var logsJson = Preferences.Get($"spellLogs_{character.Name}", "[]");
            var logs = JsonConvert.DeserializeObject<List<SpellCastLog>>(logsJson) ?? new List<SpellCastLog>();
            logs.Add(successLog);
            Preferences.Set($"spellLogs_{character.Name}", JsonConvert.SerializeObject(logs));
        }

        // Notify observers about changes (if needed)
        OnPropertyChanged(nameof(GroupedLogs));
    }

    /// <summary>
    /// Resets the record of spells used by all characters for the current day.
    /// Typically called at the start of a new in-game day or after a long rest.
    /// </summary>
    public void ResetSpellsUsedToday()
    {
        Debug.WriteLine("=== SharedViewModel.ResetSpellsUsedToday START ===");
        // Iterate through all active characters and reset their spells used
        foreach (var character in Characters)
        {
            // Ensure the character has a persistent ID so we can save per-character files
            if (character.ID == null)
            {
                character.ID = Guid.NewGuid();
                Debug.WriteLine($"Assigned new ID {character.ID} to character '{character.Name}'");
            }

            Debug.WriteLine($"Clearing SpellsUsedToday for character '{character.Name}' ({character.ID})");
            character.SpellsUsedToday.Clear();
            SaveSpellsPerDayDetails(character, character.MaxSpellsPerDay, character.SpellsUsedToday);

            try
            {
                var path = TabletopSpells.Helpers.LocalStorageHelper.GetSpellsPerDayPath(character.ID.Value);
                var exists = System.IO.File.Exists(path);
                var len = exists ? new System.IO.FileInfo(path).Length : 0;
                Debug.WriteLine($"Saved spellsPerDay file exists={exists} path={path} length={len}");

                // Reload the saved details immediately to validate persistence and update the in-memory model
                LoadSpellsPerDayDetails(character);
                Debug.WriteLine($"After reload: character '{character.Name}' SpellsUsedToday count={character.SpellsUsedToday.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error verifying saved spells per day for character {character.ID}: {ex.Message}");
            }
        }

        // Persist characters (so any newly assigned IDs are saved) and notify observers
        try
        {
            LocalStorageHelper.SaveCharactersToFile(Characters.ToList());
            Debug.WriteLine($"Saved characters list to disk. Count={Characters.Count}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save characters list: {ex.Message}");
        }

        // If the current character is set, reload its spells-per-day to ensure UI is up-to-date
        if (CurrentCharacter != null)
        {
            LoadSpellsPerDayDetails(CurrentCharacter);
            OnPropertyChanged(nameof(CurrentCharacter));
        }

        // Notify observers if any UI or bindings are tracking spells used
        OnPropertyChanged(nameof(Characters));
        Debug.WriteLine("=== SharedViewModel.ResetSpellsUsedToday END ===");
    }
    #endregion

    #region INotifyPropertyChanged Implementation

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    public void SaveCharacterPreparedSpells(Guid? characterId, List<Spell> preparedSpells)
    {
        if (characterId == null) return;

        try
        {
            var ids = preparedSpells.Select(s => s.Id).ToList();
            if (characterId.HasValue)
            {
                LocalStorageHelper.SavePreparedSpellIds(characterId.Value, ids);
            }

            Debug.WriteLine($"Saving prepared spells: {ids.Count} for {characterId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving prepared spells: {ex.Message}");
        }
    }

    public void SavePreparedSpells(Character character)
    {
        if (character == null) return;
        // Ensure the character has an ID so prepared IDs can be persisted per-character
        if (character.ID == null)
        {
            character.ID = Guid.NewGuid();
            try
            {
                // Persist updated characters list so the new ID is durable
                LocalStorageHelper.SaveCharactersToFile(Characters.ToList());
            }
            catch { }
        }

        SaveCharacterPreparedSpells(character.ID, character.GetPreparedSpells());
        SaveAlwaysPreparedSpells(character); // also persist always-prepared list
    }

    /// <summary>
    /// Toggles the favorite status of a spell for a character.
    /// </summary>
    public void ToggleSpellFavorite(Character character, Spell spell)
    {
        if (!CharacterSpells.ContainsKey(character.ID)) return;

        var characterSpell = CharacterSpells[character.ID].FirstOrDefault(s => s.Id == spell.Id);
        if (characterSpell != null)
        {
            characterSpell.IsFavoriteSpell = !characterSpell.IsFavoriteSpell;

            // Persist change to file
            SaveSpellForCharacter(character, characterSpell);

            // Notify UI
            OnPropertyChanged(nameof(CharacterSpells));
            SpellsChanged?.Invoke();
        }
    }
}
