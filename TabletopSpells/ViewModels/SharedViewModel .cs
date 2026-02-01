using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using TabletopSpells.Helpers;
using TabletopSpells.Models;
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
        [];

    private Dictionary<Guid?, ObservableCollection<Spell>> characterSpells = new();

    public ObservableCollection<Character> Characters { get; private set; }

    #endregion

    #region Properties

    private Character? currentCharacter;

    public Character? CurrentCharacter
    {
        get => currentCharacter;
        set
        {
            if (currentCharacter == value) return;

            currentCharacter = value;
            OnPropertyChanged(nameof(CurrentCharacter));

            if (currentCharacter == null) return;

            LoadSpellsPerDayDetails(currentCharacter);
            MigrateSpellsIfNeeded(currentCharacter);
        }
    }

    public Dictionary<Guid?, ObservableCollection<Spell>> CharacterSpells
    {
        get => characterSpells;
        set
        {
            if (characterSpells == value) return;
            characterSpells = value;
            OnPropertyChanged(nameof(CharacterSpells));
        }
    }

    #endregion

    #region Constructor

    public SharedViewModel()
    {
        Characters = new ObservableCollection<Character>(LoadCharacters());
    }

    #endregion

    #region Character-Related Persistence Methods

    private List<Character> LoadCharacters()
    {
        try
        {
            var fromFile = LocalStorageHelper.LoadCharactersFromFile();
            if (fromFile.Count > 0)
                return fromFile;

            var charactersJson = Preferences.Get(CharactersKey, "[]");
            var fromPref = JsonConvert.DeserializeObject<List<Character>>(charactersJson) ?? new List<Character>();

            if (fromPref.Count != 0) LocalStorageHelper.SaveCharactersToFile(fromPref);
            return fromPref;
        }
        catch (Exception)
        {
            return [];
        }
    }

    public Task SaveCharacterAsync(Character character)
    {
        var characters = LoadCharacters();

        var existingCharacter = characters.FirstOrDefault(c => c.ID == character.ID);
        if (existingCharacter != null)
        {
            existingCharacter.Name = character.Name;
            existingCharacter.Level = character.Level;
            existingCharacter.AbilityScores = character.AbilityScores;
            existingCharacter.Subclass = character.Subclass;
        }
        else
        {
            characters.Add(character);
        }

        LocalStorageHelper.SaveCharactersToFile(characters);

        RefreshInMemoryCharacters(characters);
        OnPropertyChanged(nameof(Characters));
        return Task.CompletedTask;
    }

    private void RefreshInMemoryCharacters(List<Character> characters)
    {
        Characters.Clear();
        foreach (var character in characters)
        {
            Characters.Add(character);
        }
    }

    public Character? GetCharacterById(Guid characterId)
    {
        return LoadCharacters().FirstOrDefault(c => c.ID == characterId);
    }

    public void DeleteCharacter(Character character)
    {
        var characters = LoadCharacters();
        characters.RemoveAll(c => c.ID == character.ID);
        LocalStorageHelper.SaveCharactersToFile(characters);

        if (character.ID != null)
        {
            LocalStorageHelper.DeleteCharacterFolder(character.ID.Value);
        }

        Preferences.Remove($"spellLogs_{character.Name}");
        Characters.Remove(character);
        OnPropertyChanged(nameof(Characters));
    }

    #endregion

    #region Spell Management & Migration

    private void MigrateSpellsIfNeeded(Character character)
    {
        var oldSpellKeysRaw = Preferences.Get($"spellKeys_{character.Name}", string.Empty);
        var oldSpellKeys = oldSpellKeysRaw.Split(',').Where(key => !string.IsNullOrWhiteSpace(key)).ToList();
        if (oldSpellKeys.Count == 0) return;
        {
            var spells = new ObservableCollection<Spell>();

            foreach (var spell in (from key in oldSpellKeys
                         select Preferences.Get(key, string.Empty)
                         into compressedSpellJson
                         where !string.IsNullOrEmpty(compressedSpellJson)
                         select CompressionHelper.DecompressString(compressedSpellJson)
                         into spellJson
                         select JsonConvert.DeserializeObject<Spell>(spellJson)).OfType<Spell>())
            {
                spells.Add(spell);
                SaveSpellForCharacter(character, spell);
            }

            try
            {
                Preferences.Remove($"spellKeys_{character.Name}");
            }
            catch
            {
            }

            foreach (var key in oldSpellKeys)
            {
                Preferences.Remove(key);
            }

            CharacterSpells[character.ID] = spells;
            CharacterSpells[character.ID].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
        }
    }

    public Action? SpellsChanged { get; set; }

    public void AddSpell(Character character, Spell spell)
    {
        character.ID ??= Guid.NewGuid();

        if (!CharacterSpells.TryGetValue(character.ID, out var value))
        {
            value = [];
            CharacterSpells[character.ID] = value;
            CharacterSpells[character.ID].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
        }

        if (value.Any(s => s.Name == spell.Name)) return;
        character.IsDivineCaster = ClassHelper.IsDivineCaster(character.CharacterClass);
        character.AddSpell(spell);
        value.Add(spell);

        if (character.ID != null)
        {
            LocalStorageHelper.SaveSpellToFile(character.ID.Value, spell);
        }
    }

    public void LoadSpellsForCharacter(Character? character)
    {
        if (character == null) return;

        var spells = new ObservableCollection<Spell>();

        if (character.ID != null)
        {
            var list = LocalStorageHelper.LoadSpellFiles(character.ID.Value);
            foreach (var spell in list)
            {
                var className = character.CharacterClass.ToString();
                if (!string.IsNullOrEmpty(spell.SpellLevel) && spell.SpellLevel.ToLower().Contains(className.ToLower()))
                {
                    spell.IsNativeSpell = true;
                    LocalStorageHelper.SaveSpellToFile(character.ID.Value, spell);
                }


                spells.Add(spell);
            }
        }

        CharacterSpells[character.ID] = spells;
        CharacterSpells[character.ID].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
    }

    public void LoadPreparedSpells(Character character)
    {
        if (character.ID == null) return;

        try
        {
            var preparedSpellIds = LocalStorageHelper.LoadPreparedSpellIds(character.ID.Value);
            Debug.WriteLine($"=== LoadPreparedSpells for {character.Name} ===");
            Debug.WriteLine($"Loaded {preparedSpellIds.Count} prepared spell IDs from file");
            foreach (var id in preparedSpellIds)
            {
                Debug.WriteLine($"  - Saved ID: {id}");
            }

            if (!CharacterSpells.TryGetValue(character.ID, out var liveSpells))
            {
                Debug.WriteLine($"ERROR: No CharacterSpells found for {character.Name}");
                return;
            }

            Debug.WriteLine($"CharacterSpells has {liveSpells.Count} spells:");
            foreach (var spell in liveSpells)
            {
                Debug.WriteLine($"  - {spell.Name} (ID: {spell.Id})");
            }

            foreach (var savedId in preparedSpellIds)
            {
                var match = liveSpells.FirstOrDefault(s => s.Id == savedId);
                if (match != null)
                {
                    character.AddSpell(match);
                    character.TogglePreparedSpell(match);
                    Debug.WriteLine($"  ✓ Prepared: {match.Name}");
                }
                else
                {
                    Debug.WriteLine($"  ✗ Spell ID {savedId} not found in CharacterSpells");
                }
            }

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
            foreach (var spell in character.GetPreparedSpells())
            {
                Debug.WriteLine($"  - {spell.Name} (ID: {spell.Id})");
            }
            Debug.WriteLine($"=== LoadPreparedSpells Complete ===");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading prepared spells for {character.Name}: {ex.Message}");
        }
    }

    private void SaveCharacterAlwaysPreparedSpells(Guid? characterId, List<Guid> ids)
    {
        if (characterId == null) return;

        try
        {
            LocalStorageHelper.SaveAlwaysPreparedSpellIds(characterId.Value, ids);
            Debug.WriteLine($"Saving ALWAYS prepared spells: {ids.Count} for {characterId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving always prepared spells: {ex.Message}");
        }
    }

    private void SaveAlwaysPreparedSpells(Character character)
    {
        if (character?.ID == null) return;
        var ids = character.GetPreparedSpells().Where(s => s.IsAlwaysPrepared).Select(s => s.Id).ToList();
        SaveCharacterAlwaysPreparedSpells(character.ID, ids);
    }

    public ObservableCollection<Spell> SpellsForCharacter(Character character)
    {
        if (!CharacterSpells.ContainsKey(character.ID))
        {
            LoadSpellsForCharacter(character);
        }

        // Always reload prepared spells for divine casters to ensure current state
        // This handles switching between characters and coming back
        if (character.IsDivineCaster)
        {
            // Clear prepared spells first to ensure fresh load
            character.ClearManualllyPreparedSpells();
            // Now reload prepared state from disk
            LoadPreparedSpells(character);
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
            var logsJson = Preferences.Get($"spellLogs_{character.Name}", "[]");
            logs = JsonConvert.DeserializeObject<List<SpellCastLog>>(logsJson) ?? new List<SpellCastLog>();
            logs.Add(failedLog);
            Preferences.Set($"spellLogs_{character.Name}", JsonConvert.SerializeObject(logs));
        }

        OnPropertyChanged(nameof(GroupedLogs));
    }

    private int GetCurrentSessionId()
    {
        var sessionId = LocalStorageHelper.LoadSessionId();
        if (sessionId != 0) return sessionId;
        sessionId = GenerateNewSessionId();
        LocalStorageHelper.SaveSessionId(sessionId);
        return sessionId;
    }

    private int GenerateNewSessionId()
    {
        return (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public void LogSpellCast(Character character, string spellName, int spellLevel, bool castAsRitual)
    {
        if (character == null || string.IsNullOrWhiteSpace(spellName))
            throw new ArgumentNullException("Character and spell name cannot be null or empty.");

        var successLog = new SpellCastLog
        {
            CharacterName = character.Name,
            SpellName = spellName,
            SpellLevel = spellLevel,
            CastTime = DateTime.UtcNow,
            Success = true,
            Reason = "Spell cast successfully.",
            SessionId = GetCurrentSessionId()
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

        OnPropertyChanged(nameof(GroupedLogs));
    }

    public void ResetSpellsUsedToday()
    {
        Debug.WriteLine("=== SharedViewModel.ResetSpellsUsedToday START ===");
        foreach (var character in Characters)
        {
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

                LoadSpellsPerDayDetails(character);
                Debug.WriteLine(
                    $"After reload: character '{character.Name}' SpellsUsedToday count={character.SpellsUsedToday.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error verifying saved spells per day for character {character.ID}: {ex.Message}");
            }
        }

        try
        {
            LocalStorageHelper.SaveCharactersToFile(Characters.ToList());
            Debug.WriteLine($"Saved characters list to disk. Count={Characters.Count}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save characters list: {ex.Message}");
        }

        if (CurrentCharacter != null)
        {
            LoadSpellsPerDayDetails(CurrentCharacter);
            OnPropertyChanged(nameof(CurrentCharacter));
        }

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
        if (character.ID == null)
        {
            character.ID = Guid.NewGuid();
            try
            {
                LocalStorageHelper.SaveCharactersToFile(Characters.ToList());
            }
            catch
            {
            }
        }

        var preparedSpells = character.GetPreparedSpells();
        Debug.WriteLine($"=== SavePreparedSpells ===");
        Debug.WriteLine($"Character: {character.Name}");
        Debug.WriteLine($"Prepared count: {preparedSpells.Count}");
        foreach (var spell in preparedSpells)
        {
            Debug.WriteLine($"  - {spell.Name} (ID: {spell.Id})");
        }

        SaveCharacterPreparedSpells(character.ID, preparedSpells);
        SaveAlwaysPreparedSpells(character);
        Debug.WriteLine($"=== SavePreparedSpells Complete ===");
    }

    public void ToggleSpellFavorite(Character character, Spell spell)
    {
        if (!CharacterSpells.ContainsKey(character.ID)) return;

        var characterSpell = CharacterSpells[character.ID].FirstOrDefault(s => s.Id == spell.Id);
        if (characterSpell != null)
        {
            characterSpell.IsFavoriteSpell = !characterSpell.IsFavoriteSpell;

            SaveSpellForCharacter(character, characterSpell);

            OnPropertyChanged(nameof(CharacterSpells));
            SpellsChanged?.Invoke();
        }
    }
}