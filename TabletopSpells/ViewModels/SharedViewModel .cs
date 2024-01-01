using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;

public class SharedViewModel : INotifyPropertyChanged
{
    private static SharedViewModel instance;
    public static SharedViewModel Instance => instance ?? (instance = new SharedViewModel());

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
        // Check if the character already has a spell collection
        if (!CharacterSpells.ContainsKey(characterName))
        {
            // Create a new ObservableCollection for the character
            CharacterSpells[characterName] = new ObservableCollection<Spell>();

            // Subscribe to CollectionChanged for the new collection
            // This subscription is done only once per character
            CharacterSpells[characterName].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
        }

        // Check if the spell is not already in the character's collection
        if (!CharacterSpells[characterName].Any(s => s.Name == spell.Name))
        {
            // Add the new spell to the character's collection
            CharacterSpells[characterName].Add(spell);

            // Save the updated collection
            SaveSpellsForCharacter(characterName);

            // Note: No need to call OnPropertyChanged here, as the CollectionChanged event will handle it
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
                // Subscribe to CollectionChanged for the loaded collection
                CharacterSpells[characterName].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
            }
        }
        else
        {
            CharacterSpells[characterName] = new ObservableCollection<Spell>();
            // Subscribe to CollectionChanged for the new collection
            CharacterSpells[characterName].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
        }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
