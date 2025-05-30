using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using TabletopSpells.Models;

namespace TabletopSpells.ViewModels;

public class SharedViewModel : INotifyPropertyChanged
{
    #region Singleton Pattern
    private static SharedViewModel? instance;
    public static SharedViewModel Instance => instance ??= new SharedViewModel();
    #endregion

    #region Fields & Constants
    private const string CharactersKey = "characters";
    public ObservableCollection<Grouping<int, SpellCastLog>> GroupedLogs { get; set; } = new ObservableCollection<Grouping<int, SpellCastLog>>();

    private Dictionary<Guid?, ObservableCollection<Spell>> characterSpells = new Dictionary<Guid?, ObservableCollection<Spell>>();

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
                    LoadSpellsPerDayDetails(currentCharacter);  // Load spell details directly into the character
                    MigrateSpellsIfNeeded(currentCharacter);    // Migrate spells from name-based keys to ID-based keys
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
        // Retrieve characters from preferences or return an empty list
        string charactersJson = Preferences.Get(CharactersKey, "[]");
        return JsonConvert.DeserializeObject<List<Character>>(charactersJson) ?? new List<Character>();
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

        // Save updated characters to Preferences
        string updatedCharactersJson = JsonConvert.SerializeObject(characters);
        Preferences.Set(CharactersKey, updatedCharactersJson);

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

        // Save the updated character list back to storage
        string updatedCharactersJson = JsonConvert.SerializeObject(characters);
        Preferences.Set(CharactersKey, updatedCharactersJson);

        // Cleanup associated persistent data for the deleted character
        var spellKeysKey = $"spellKeys_{character.ID}";
        Preferences.Remove(spellKeysKey); // Remove the list of spell keys

        // Remove each spell stored for this character
        var spellKeys = Preferences.Get(spellKeysKey, string.Empty)
            .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var spellKey in spellKeys)
        {
            Preferences.Remove(spellKey); // Remove the individual spell
        }

        // Remove stored logs for the character
        var logsKey = $"spellLogs_{character.Name}";
        Preferences.Remove(logsKey);

        // Remove the character from the in-memory collection
        Characters.Remove(character);
    
        // Notify observers
        OnPropertyChanged(nameof(Characters));
    }
    #endregion

    #region Spell Management & Migration
    private void MigrateSpellsIfNeeded(Character character)
    {
        var oldSpellKeys = Preferences.Get($"spellKeys_{character.Name}", string.Empty).Split(',').Where(key => !string.IsNullOrWhiteSpace(key)).ToList();
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
                        SaveSpellForCharacter(character, spell);  // Save with new ID-based key
                    }
                }
            }

            // Remove old spell keys
            Preferences.Remove($"spellKeys_{character.Name}");
            foreach (var key in oldSpellKeys)
            {
                Preferences.Remove(key);
            }

            CharacterSpells[character.ID] = spells;
            CharacterSpells[character.ID].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
        }
    }

    public void AddSpell(Character character, Spell spell)
    {
        if (!CharacterSpells.ContainsKey(character.ID))
        {
            CharacterSpells[character.ID] = new ObservableCollection<Spell>();
            CharacterSpells[character.ID].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
        }

        if (CharacterSpells[character.ID].Any(s => s.Name == spell.Name)) return;

        CharacterSpells[character.ID].Add(spell);
        SaveSpellForCharacter(character, spell);

        // If this is a divine caster, treat newly added spells as always prepared
        if (character.IsDivineCaster && !character.GetPreparedSpells().Contains(spell))
        {
            character.TogglePreparedSpell(spell);
        }
    }


    public void SaveSpellForCharacter(Character character, Spell spell)
    {
        var spellJson = JsonConvert.SerializeObject(spell);
        var compressedSpellJson = CompressionHelper.CompressString(spellJson);
        Preferences.Set($"spell_{character.ID}_{spell.Name}", compressedSpellJson);

        // Update the list of spell keys for this character
        var spellKeys = Preferences.Get($"spellKeys_{character.ID}", string.Empty)
                                    .Split(['|'], StringSplitOptions.RemoveEmptyEntries)
                                    .ToList();

        var spellKey = $"spell_{character.ID}_{spell.Name}";

        if (!spellKeys.Contains(spellKey))
        {
            spellKeys.Add(spellKey);
            Preferences.Set($"spellKeys_{character.ID}", string.Join("|", spellKeys));
        }
    }
    
    /// <summary>
    /// Loads spells for the given character from persistent storage.
    /// </summary>
    public void LoadSpellsForCharacter(Character character)
    {
        if (character == null) return;

        // Retrieve the stored spell keys for the character
        var spellKeysRaw = Preferences.Get($"spellKeys_{character.ID}", string.Empty);

        // Determine the delimiter used (comma for old format, pipe for new format)
        char delimiter = spellKeysRaw.Contains('|') ? '|' : ',';

        // Split the keys using the detected delimiter
        var spellKeys = spellKeysRaw.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .ToList();

        // Initialize the ObservableCollection to hold the spells
        var spells = new ObservableCollection<Spell>();

        foreach (var spell in (from key in spellKeys select Preferences.Get(key, string.Empty) into compressedSpellJson where !string.IsNullOrEmpty(compressedSpellJson) select CompressionHelper.DecompressString(compressedSpellJson) into spellJson select JsonConvert.DeserializeObject<Spell>(spellJson)).OfType<Spell>())
        {
            spells.Add(spell);
        }

        // If the delimiter was a comma (old format), migrate to the new format
        if (delimiter == ',')
        {
            Preferences.Set($"spellKeys_{character.ID}", string.Join("|", spellKeys));
        }

        // Store the spells in the CharacterSpells dictionary and handle changes
        CharacterSpells[character.ID] = spells;
        CharacterSpells[character.ID].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
    }
    
    public void LoadPreparedSpells(Character character)
    {
        if (character == null || character.ID == null) return;

        try
        {
            // Retrieve the stored prepared spells JSON for the character
            var preparedSpellsJson = Preferences.Get($"preparedSpells_{character.ID}", "[]");

            // Deserialize into a list of spells
            var preparedSpells = JsonConvert.DeserializeObject<List<Spell>>(preparedSpellsJson) ?? new List<Spell>();

            // Ensure prepared spells are updated in the character
            foreach (var spell in preparedSpells)
            {
                character.TogglePreparedSpell(spell); // Dynamically prepare the spells
            }

            Debug.WriteLine($"Prepared spells loaded successfully for {character.Name}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading prepared spells for {character.Name}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Toggles a spell's prepared state for the current character and persists the change.
    /// Returns true if the toggle was successful (added or removed), false if blocked (e.g., limit reached).
    /// </summary>
    public bool TogglePreparedSpellAndSave(Character character, Spell spell)
    {
        if (character == null || spell == null) return false;
    
        var success = character.TogglePreparedSpell(spell);
    
        if (success)
            SavePreparedSpells(character);
    
        return success;
    }

    
    /// <summary>
    /// Retrieves all spells for the given character from the in-memory dictionary.
    /// If not already loaded, initializes them from persistent storage.
    /// </summary>
    public ObservableCollection<Spell> SpellsForCharacter(Character character)
    {
        // Check if spells for the character already exist in memory
        if (!CharacterSpells.ContainsKey(character.ID))
        {
            // Load spells for the character if not already cached
            LoadSpellsForCharacter(character);
        }

        // Return the in-memory list of spells for this character
        return CharacterSpells[character.ID];
    }
    
    /// <summary>
    /// Removes the specified spell for the given character.
    /// Updates both the in-memory data and persistent storage.
    /// </summary>
    public void RemoveSpellForCharacter(Character character, Spell spell)
    {
        // Ensure the character exists in the dictionary
        if (!CharacterSpells.TryGetValue(character.ID, out var spells)) return;
        // Remove the spell from the in-memory collection
        if (!spells.Contains(spell)) return;
        spells.Remove(spell);

        // Update the persistent storage
        var spellKeys = Preferences.Get($"spellKeys_{character.ID}", string.Empty)
            .Split(['|'], StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        var spellKey = GenerateSpellKey(character, spell);
        if (spellKeys.Remove(spellKey))
        {
            // Update the spell keys in preferences
            Preferences.Set($"spellKeys_{character.ID}", string.Join("|", spellKeys));
            Preferences.Remove(spellKey); // Remove the stored JSON of the removed spell
        }

        // Notify any observers about the changes
        OnPropertyChanged(nameof(CharacterSpells));
    }
    
    /// <summary>
    /// Generates a unique key for associating a specific spell with a character.
    /// The key combines the character's ID, spell name, and spell level.
    /// </summary>
    /// <param name="character">The character associated with the spell.</param>
    /// <param name="spell">The spell to generate the key for.</param>
    /// <returns>A unique key for the character and spell combination.</returns>
    public static string GenerateSpellKey(Character character, Spell spell)
    {
        if (character?.ID == null || spell == null)
            throw new ArgumentNullException("Character or Spell cannot be null.");

        return $"{character.ID}_{spell.Name}_{spell.SpellLevel}";
    }
    #endregion

    #region Spell Slots & Logs
    public void SaveSpellsPerDayDetails(Character character, Dictionary<int, int> maxSpellsPerDay, Dictionary<int, int> spellsUsedToday)
    {
        try
        {
            var maxSpellsJson = JsonConvert.SerializeObject(maxSpellsPerDay);
            Preferences.Set($"maxSpells_{character.ID}", maxSpellsJson);

            var usedSpellsJson = JsonConvert.SerializeObject(spellsUsedToday);
            Preferences.Set($"usedSpells_{character.ID}", usedSpellsJson);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving spell data: {ex.Message}");
        }
    }

    public void LoadSpellsPerDayDetails(Character character)
    {
        if (character == null) return;

        var maxSpellsJson = Preferences.Get($"maxSpells_{character.ID}", "{}");
        var usedSpellsJson = Preferences.Get($"usedSpells_{character.ID}", "{}");

        character.MaxSpellsPerDay = JsonConvert.DeserializeObject<Dictionary<int, int>>(maxSpellsJson) ?? new Dictionary<int, int>();
        character.SpellsUsedToday = JsonConvert.DeserializeObject<Dictionary<int, int>>(usedSpellsJson) ?? new Dictionary<int, int>();
    }
    
    /// <summary>
    /// Loads the spell cast logs for the given character.
    /// Groups logs by session ID to allow for easy navigation or display.
    /// </summary>
    public void LoadLogs(Character character)
    {
        if (character == null) return;

        // Retrieve the stored logs for the character
        var logsJson = Preferences.Get($"spellLogs_{character.Name}", "[]");
        var logs = JsonConvert.DeserializeObject<List<SpellCastLog>>(logsJson) ?? new List<SpellCastLog>();

        // Clear existing logs to avoid duplication
        GroupedLogs.Clear();

        // Group logs by session ID or another relevant property
        var groupedData = logs
            .GroupBy(log => log.SessionId)
            .OrderByDescending(group => group.Key)
            .Select(group => new Grouping<int, SpellCastLog>(group.Key, group.OrderByDescending(log => log.CastTime)))
            .ToList();

        foreach (var group in groupedData)
        {
            var newGroup = new Grouping<int, SpellCastLog>(group.Key, group);
            GroupedLogs.Add(newGroup);
        }

        OnPropertyChanged(nameof(GroupedLogs));
    }

    /// <summary>
    /// Logs a failed attempt to cast a spell for the given character.
    /// This includes the spell information, level, and failure reason.
    /// </summary>
    /// <param name="character">The character attempting to cast the spell.</param>
    /// <param name="spellName">The name of the spell being cast.</param>
    /// <param name="spellLevel">The spell level being cast.</param>
    /// <param name="reason">The reason the spell cast failed.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void LogFailedSpellCast(Character character, string spellName, int spellLevel, string reason)
    {
        if (character == null || string.IsNullOrWhiteSpace(spellName) || string.IsNullOrWhiteSpace(reason))
            throw new ArgumentNullException("Character, spell name, and reason cannot be null or empty.");

        // Create a log entry for the failed spell cast
        var failedLog = new SpellCastLog
        {
            CharacterName = character.Name,
            SpellName = spellName,
            SpellLevel = spellLevel,
            CastTime = DateTime.UtcNow,
            Success = false,
            Reason = reason,
            SessionId = GetCurrentSessionId() // Ensure session tracking
        };

        // Retrieve existing logs for the character from persistent storage
        var logsJson = Preferences.Get($"spellLogs_{character.Name}", "[]");
        var logs = JsonConvert.DeserializeObject<List<SpellCastLog>>(logsJson) ?? new List<SpellCastLog>();

        // Add the new log entry
        logs.Add(failedLog);

        // Save the updated logs back to persistent storage
        Preferences.Set($"spellLogs_{character.Name}", JsonConvert.SerializeObject(logs));

        // Notify about changes (if needed)
        OnPropertyChanged(nameof(GroupedLogs));
    }
    
    /// <summary>
    /// Retrieves the current session ID from persistent storage.
    /// If no session ID exists, a new session ID is generated and saved.
    /// </summary>
    /// <returns>The current session ID as an integer.</returns>
    private int GetCurrentSessionId()
    {
        // Retrieve the current session ID; defaults to 0 if not present
        var sessionId = Preferences.Get("currentSessionId", 0);

        // If no valid session ID exists, generate a new session ID
        if (sessionId != 0) return sessionId;
        sessionId = GenerateNewSessionId();
        Preferences.Set("currentSessionId", sessionId);

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
    public void LogSpellCast(Character character, string spellName, int spellLevel, bool castAsRitual )
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

        // Retrieve existing logs for the character from persistent storage
        var logsJson = Preferences.Get($"spellLogs_{character.Name}", "[]");
        var logs = JsonConvert.DeserializeObject<List<SpellCastLog>>(logsJson) ?? new List<SpellCastLog>();

        // Add the new log entry
        logs.Add(successLog);

        // Save the updated logs back to persistent storage
        Preferences.Set($"spellLogs_{character.Name}", JsonConvert.SerializeObject(logs));

        // Notify observers about changes (if needed)
        OnPropertyChanged(nameof(GroupedLogs));
    }
    
    /// <summary>
    /// Resets the record of spells used by all characters for the current day.
    /// Typically called at the start of a new in-game day or after a long rest.
    /// </summary>
    public void ResetSpellsUsedToday()
    {
        // Iterate through all active characters and reset their spells used
        foreach (var character in Characters)
        {
            character.SpellsUsedToday.Clear();
            SaveSpellsPerDayDetails(character, character.MaxSpellsPerDay, character.SpellsUsedToday);
        }

        // Notify observers if any UI or bindings are tracking spells used
        OnPropertyChanged(nameof(Characters));
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
            // Serialize the prepared spells list
            var preparedSpellsJson = JsonConvert.SerializeObject(preparedSpells);

            // Persist the prepared spells using the character's ID as the key
            Preferences.Set($"preparedSpells_{characterId}", preparedSpellsJson);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving prepared spells: {ex.Message}");
        }
    }
    
    public void SavePreparedSpells(Character character)
    {
        if (character == null || character.ID == null) return;

        SaveCharacterPreparedSpells(character.ID, character.GetPreparedSpells());
    }


}