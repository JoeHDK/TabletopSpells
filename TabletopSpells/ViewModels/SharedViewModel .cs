using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using TabletopSpells.Models; // Ensure this namespace contains the Character and Spell classes

public class SharedViewModel : INotifyPropertyChanged
{
    private static SharedViewModel? instance;
    public static SharedViewModel Instance => instance ?? (instance = new SharedViewModel());

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

    private Dictionary<string, ObservableCollection<Spell>> characterSpells = new Dictionary<string, ObservableCollection<Spell>>();
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

    public void AddSpell(string characterName, Spell spell)
    {
        if (!CharacterSpells.ContainsKey(characterName))
        {
            CharacterSpells[characterName] = new ObservableCollection<Spell>();
            CharacterSpells[characterName].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
        }

        if (!CharacterSpells[characterName].Any(s => s.Name == spell.Name))
        {
            CharacterSpells[characterName].Add(spell);
            SaveSpellsForCharacter(characterName);
        }
    }

    public void SaveSpellsForCharacter(string characterName)
    {
        if (CharacterSpells.ContainsKey(characterName))
        {
            var spellsJson = JsonConvert.SerializeObject(CharacterSpells[characterName]);
            Preferences.Set($"spells_{characterName}", spellsJson);
        }
    }

    public void LoadSpellsForCharacter(string characterName)
    {
        var spellsJson = Preferences.Get($"spells_{characterName}", string.Empty);
        if (!string.IsNullOrEmpty(spellsJson))
        {
            var spells = JsonConvert.DeserializeObject<ObservableCollection<Spell>>(spellsJson);
            if (spells != null)
            {
                CharacterSpells[characterName] = spells;
                CharacterSpells[characterName].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
            }
        }
        else
        {
            CharacterSpells[characterName] = new ObservableCollection<Spell>();
            CharacterSpells[characterName].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
        }
    }

    public void SaveSpellsPerDayDetails(string characterName, Dictionary<int, int> maxSpellsPerDay, Dictionary<int, int> spellsUsedToday)
    {
        try
        {
            var maxSpellsJson = JsonConvert.SerializeObject(maxSpellsPerDay);
            Preferences.Set($"maxSpells_{characterName}", maxSpellsJson);

            var usedSpellsJson = JsonConvert.SerializeObject(spellsUsedToday);
            Preferences.Set($"usedSpells_{characterName}", usedSpellsJson);
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

        var maxSpellsJson = Preferences.Get($"maxSpells_{character.Name}", "{}");
        var usedSpellsJson = Preferences.Get($"usedSpells_{character.Name}", "{}");

        character.MaxSpellsPerDay = JsonConvert.DeserializeObject<Dictionary<int, int>>(maxSpellsJson) ?? new Dictionary<int, int>();
        character.SpellsUsedToday = JsonConvert.DeserializeObject<Dictionary<int, int>>(usedSpellsJson) ?? new Dictionary<int, int>();
    }

    public void ResetSpellsUsedToday()
    {
        if (CurrentCharacter == null) return;

        foreach (var key in CurrentCharacter.SpellsUsedToday.Keys.ToList())
        {
            CurrentCharacter.SpellsUsedToday[key] = 0;
        }

        SaveSpellsPerDayDetails(CurrentCharacter.Name, CurrentCharacter.MaxSpellsPerDay, CurrentCharacter.SpellsUsedToday);
        OnPropertyChanged("CurrentCharacter"); // Make sure this triggers UI updates
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
