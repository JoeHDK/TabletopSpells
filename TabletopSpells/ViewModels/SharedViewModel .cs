using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using TabletopSpells.Models;

public class SharedViewModel : INotifyPropertyChanged
{
    private static SharedViewModel? instance;
    public static SharedViewModel Instance => instance ?? (instance = new SharedViewModel());
    public ObservableCollection<Grouping<int, SpellCastLog>> GroupedLogs { get; set; } = new ObservableCollection<Grouping<int, SpellCastLog>>();

    private Dictionary<Guid?, ObservableCollection<Spell>> characterSpells = new Dictionary<Guid?, ObservableCollection<Spell>>();

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

        if (!CharacterSpells[character.ID].Any(s => s.Name == spell.Name))
        {
            CharacterSpells[character.ID].Add(spell);
            SaveSpellForCharacter(character, spell);
        }
    }

    public void SaveSpellForCharacter(Character character, Spell spell)
    {
        var spellJson = JsonConvert.SerializeObject(spell);
        var compressedSpellJson = CompressionHelper.CompressString(spellJson);
        Preferences.Set($"spell_{character.ID}_{spell.Name}", compressedSpellJson);

        // Update the list of spell keys for this character
        var spellKeys = Preferences.Get($"spellKeys_{character.ID}", string.Empty).Split(',').ToList();
        if (!spellKeys.Contains($"spell_{character.ID}_{spell.Name}"))
        {
            spellKeys.Add($"spell_{character.ID}_{spell.Name}");
            Preferences.Set($"spellKeys_{character.ID}", string.Join(",", spellKeys));
        }
    }

    public void RemoveSpellForCharacter(Character character, Spell spell)
    {
        Preferences.Remove($"spell_{character.ID}_{spell.Name}");

        // Update the list of spell keys for this character
        var spellKeys = Preferences.Get($"spellKeys_{character.ID}", string.Empty).Split(',').ToList();
        spellKeys.Remove($"spell_{character.ID}_{spell.Name}");
        Preferences.Set($"spellKeys_{character.ID}", string.Join(",", spellKeys));
    }

    public void LoadSpellsForCharacter(Character character)
    {
        var spellKeys = Preferences.Get($"spellKeys_{character.ID}", string.Empty).Split(',').Where(key => !string.IsNullOrWhiteSpace(key)).ToList();
        var spells = new ObservableCollection<Spell>();

        foreach (var key in spellKeys)
        {
            var compressedSpellJson = Preferences.Get(key, string.Empty);
            if (!string.IsNullOrEmpty(compressedSpellJson))
            {
                var spellJson = CompressionHelper.DecompressString(compressedSpellJson);
                var spell = JsonConvert.DeserializeObject<Spell>(spellJson);
                if (spell != null)
                {
                    spells.Add(spell);
                }
            }
        }

        CharacterSpells[character.ID] = spells;
        CharacterSpells[character.ID].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
    }

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
            // Optionally, provide feedback to the user that saving failed
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

    public void ResetSpellsUsedToday()
    {
        if (CurrentCharacter == null) return;

        // Increment session ID on reset
        var sessionId = Preferences.Get($"currentSession_{CurrentCharacter.ID}", 0);
        Preferences.Set($"currentSession_{CurrentCharacter.ID}", sessionId + 1);

        foreach (var key in CurrentCharacter.SpellsUsedToday.Keys.ToList())
        {
            CurrentCharacter.SpellsUsedToday[key] = 0;
        }

        SaveSpellsPerDayDetails(CurrentCharacter, CurrentCharacter.MaxSpellsPerDay, CurrentCharacter.SpellsUsedToday);
        OnPropertyChanged("CurrentCharacter"); // Make sure this triggers UI updates

        // Optionally notify that session has changed
        OnPropertyChanged("SessionId");
    }

    public void LogSpellCast(Character character, string spellName, int spellLevel, bool castAsRitual = false)
    {
        var sessionId = Preferences.Get($"currentSession_{character.ID}", 0);
        var logEntry = new SpellCastLog
        {
            CastTime = DateTime.Now,
            SpellName = spellName,
            SpellLevel = spellLevel,
            SessionId = sessionId,
            CastAsRitual = castAsRitual // Include castAsRitual in the log entry
        };

        UpdateLogs(character.Name, logEntry);
    }

    public void LogFailedSpellCast(Character character, string spellName, int spellLevel, string reason)
    {
        var sessionId = Preferences.Get($"currentSession_{character.ID}", 0);
        var logEntry = new SpellCastLog
        {
            CastTime = DateTime.Now,
            SpellName = spellName,
            SpellLevel = spellLevel,
            SessionId = sessionId,
            FailedReason = reason
        };

        UpdateLogs(character.Name, logEntry);
    }

    private void UpdateLogs(string characterName, SpellCastLog logEntry)
    {
        var logsJson = Preferences.Get($"spellLogs_{characterName}", "[]");
        var logs = JsonConvert.DeserializeObject<List<SpellCastLog>>(logsJson) ?? new List<SpellCastLog>();
        logs.Add(logEntry);
        Preferences.Set($"spellLogs_{characterName}", JsonConvert.SerializeObject(logs));
    }

    public void LoadLogs(Character character)
    {
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

        foreach (var log in logs)
        {
            Debug.WriteLine($"Log Time: {log.CastTime}, Name: {log.SpellName}, Ritual: {log.CastAsRitual}");
        }

        foreach (var group in groupedData)
        {
            var newGroup = new Grouping<int, SpellCastLog>(group.Key, group);
            GroupedLogs.Add(newGroup);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
