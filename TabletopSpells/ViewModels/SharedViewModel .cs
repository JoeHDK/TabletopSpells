using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TabletopSpells.Models; // Ensure this namespace contains the Character and Spell classes

public class SharedViewModel : INotifyPropertyChanged
{
    private static SharedViewModel instance;
    public static SharedViewModel Instance => instance ?? (instance = new SharedViewModel());

    private Character currentCharacter;
    public Character CurrentCharacter
    {
        get => currentCharacter;
        set
        {
            if (currentCharacter != value)
            {
                currentCharacter = value;
                OnPropertyChanged(nameof(CurrentCharacter));
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

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
