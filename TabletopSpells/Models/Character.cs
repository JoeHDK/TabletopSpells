using TabletopSpells.Helpers;
using TabletopSpells.Models.Enums;

namespace TabletopSpells.Models;

public class Character
{
    public Guid? ID { get; set; }
    public required string Name { get; set; }

    private Class _characterClass;
    public Class CharacterClass
    {
        get => _characterClass;
        set
        {
            _characterClass = value;
            IsDivineCaster = ClassHelper.IsDivineCaster(value);
        }
    }

    public Subclass Subclass { get; set; } = Subclass.None;

    public Game GameType { get; init; }
    public int Level { get; set; }

    public bool IsDivineCaster { get; set; }

    public Dictionary<int, int> MaxSpellsPerDay { get; set; } = new();
    public Dictionary<int, int> SpellsUsedToday { get; set; } = new();
    public List<string> AlwaysPreparedSpells { get; init; } = new();

    private readonly List<Spell> _knownSpells = new List<Spell>();
    public IEnumerable<Spell> KnownSpells => _knownSpells;

    private readonly List<Spell> _manuallyPreparedSpells = new List<Spell>();

    public Dictionary<string, int> AbilityScores { get; set; } = new()
    {
        ["Strength"] = 10,
        ["Dexterity"] = 10,
        ["Constitution"] = 10,
        ["Intelligence"] = 10,
        ["Wisdom"] = 10,
        ["Charisma"] = 10
    };

    private Dictionary<string, int> AbilityModifiers =>
        AbilityScores.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value - 10) / 2);

    public void AddSpell(Spell spell)
    {
        if (!_knownSpells.Contains(spell))
            _knownSpells.Add(spell);

    }

    public void RemoveSpell(Spell spell) => _knownSpells.Remove(spell);
    
    public bool CastSpell(int spellLevel)
    {
        if (spellLevel is 0 or -1) return true;

        if (!SpellsUsedToday.TryGetValue(spellLevel, out var used) ||
            !MaxSpellsPerDay.TryGetValue(spellLevel, out var max) ||
            used >= max)
            return false;

        SpellsUsedToday[spellLevel]++;
        return true;
    }
    
    public int GetRelevantAbilityModifier()
    {
        string relevant = CharacterClass switch
        {
            Class.Wizard => "Intelligence",
            Class.Artificer => "Intelligence",
            Class.Cleric or Class.Druid => "Wisdom",
            Class.Paladin or Class.Sorcerer or Class.Bard => "Charisma",
            _ => "Strength"
        };

        return AbilityModifiers.TryGetValue(relevant, out var mod) ? mod : 0;
    }
    public List<Spell> GetPreparedSpells()
    {
        int limit = Level + GetRelevantAbilityModifier();

        var manual = _manuallyPreparedSpells
            .OrderByDescending(spell => spell.SpellLevel)
            .Take(limit)
            .ToList();

        var always = _knownSpells.Where(s => s.IsAlwaysPrepared).ToList();

        var combined = always.Concat(manual).GroupBy(s => s.Id).Select(g => g.First()).ToList();
        return combined;
    }

    public bool TogglePreparedSpell(Spell spell)
    {
        var limit = Level + GetRelevantAbilityModifier();
        var existing = _manuallyPreparedSpells.FirstOrDefault(s => s.Id == spell.Id);

        if (existing != null)
        {
            _manuallyPreparedSpells.Remove(existing);
            return true;
        }

        if (_manuallyPreparedSpells.Count >= limit) return false;
        _manuallyPreparedSpells.Add(spell);
        return true;
    }

    public void ClearManualllyPreparedSpells()
    {
        _manuallyPreparedSpells.Clear();
    }

}
