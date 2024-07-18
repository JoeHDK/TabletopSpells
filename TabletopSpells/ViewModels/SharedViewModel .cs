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
    
    private Dictionary<string, ObservableCollection<Spell>> characterSpells = new Dictionary<string, ObservableCollection<Spell>>();


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
                }
            }
        }
    }

    public Dictionary<string, ObservableCollection<Spell>> CharacterSpells
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

    public void AddSpell(Character character, Spell spell)
    {
        if (!CharacterSpells.ContainsKey(character.Name))
        {
            CharacterSpells[character.Name] = new ObservableCollection<Spell>();
            CharacterSpells[character.Name].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
        }

        if (!CharacterSpells[character.Name].Any(s => s.Name == spell.Name))
        {
            CharacterSpells[character.Name].Add(spell);
            SaveSpellsForCharacter(character);
        }
    }

    public void SaveSpellsForCharacter(Character character)
    {
        if (CharacterSpells.ContainsKey(character.Name))
        {
            var spellsJson = JsonConvert.SerializeObject(CharacterSpells[character.Name]);
            Preferences.Set($"spells_{character.Name}", spellsJson);
        }
    }

    public void LoadSpellsForCharacter(Character character)
    {
        var spellsJson = Preferences.Get($"spells_{character.Name}", string.Empty);
        if (!string.IsNullOrEmpty(spellsJson))
        {
            var spells = JsonConvert.DeserializeObject<ObservableCollection<Spell>>(spellsJson);
            if (spells != null)
            {
                CharacterSpells[character.Name] = spells;
                CharacterSpells[character.Name].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
            }
        }
        else
        {
            CharacterSpells[character.Name] = new ObservableCollection<Spell>();
            CharacterSpells[character.Name].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
        }
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


    public void LogSpellCast(Character character, string spellName, int spellLevel)
    {
        var sessionId = Preferences.Get($"currentSession_{character.ID}", 0);
        var logEntry = new SpellCastLog
        {
            CastTime = DateTime.Now,
            SpellName = spellName,
            SpellLevel = spellLevel,
            SessionId = sessionId
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
        var logs = JsonConvert.DeserializeObject<List<SpellCastLog>>(logsJson);

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
            Debug.WriteLine($"Log Time: {log.CastTime}, Name: {log.SpellName}");
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
