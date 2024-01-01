using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;

public class SharedViewModel : INotifyPropertyChanged
{
    private static SharedViewModel instance;
    public static SharedViewModel Instance => instance ?? (instance = new SharedViewModel());

    // Dictionary to store spells for each character
    public Dictionary<string, ObservableCollection<Spell>> CharacterSpells { get; private set; } = new Dictionary<string, ObservableCollection<Spell>>();

    public void AddSpell(string characterName, Spell spell)
    {
        if (!CharacterSpells.ContainsKey(characterName))
        {
            CharacterSpells[characterName] = new ObservableCollection<Spell>();
        }

        if (!CharacterSpells[characterName].Any(s => s.Name == spell.Name))
        {
            CharacterSpells[characterName].Add(spell);
            SaveSpellsForCharacter(characterName);
            OnPropertyChanged(nameof(CharacterSpells));
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
            }
        }
        else
        {
            CharacterSpells[characterName] = new ObservableCollection<Spell>();
        }

        OnPropertyChanged(nameof(CharacterSpells));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
