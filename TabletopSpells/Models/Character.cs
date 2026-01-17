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

    public Game GameType { get; init; }
    public int Level { get; set; }

    /// <summary>
    /// Whether the character prepares spells from the divine list (Cleric, Druid, etc.)
    /// Determined automatically when CharacterClass is set.
    /// </summary>
    public bool IsDivineCaster { get; set; }

    public Dictionary<int, int> MaxSpellsPerDay { get; set; } = new();
    public Dictionary<int, int> SpellsUsedToday { get; set; } = new();
    public List<string> AlwaysPreparedSpells { get; init; } = new();

    /// <summary>
    /// The character's full spellbook / known spells.
    /// </summary>
    private readonly List<Spell> _knownSpells = new List<Spell>();
    public IEnumerable<Spell> KnownSpells => _knownSpells;

    /// <summary>
    /// The manually prepared spells the player has selected.
    /// </summary>
    private readonly List<Spell> _manuallyPreparedSpells = new List<Spell>();

    /// <summary>
    /// Basic D&D/PF ability scores.
    /// </summary>
    public Dictionary<string, int> AbilityScores { get; set; } = new()
    {
        ["Strength"] = 10,
        ["Dexterity"] = 10,
        ["Constitution"] = 10,
        ["Intelligence"] = 10,
        ["Wisdom"] = 10,
        ["Charisma"] = 10
    };

    /// <summary>
    /// Calculates modifiers from ability scores.
    /// </summary>
    private Dictionary<string, int> AbilityModifiers =>
        AbilityScores.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value - 10) / 2);

    /// <summary>
    /// Adds a spell to the known list (not prepared).
    /// </summary>
    public void AddSpell(Spell spell)
    {
        if (!_knownSpells.Contains(spell))
            _knownSpells.Add(spell);

        // Divine casters automatically have their spells marked as always prepared
        if (IsDivineCaster && !spell.IsAlwaysPrepared)
            spell.IsAlwaysPrepared = true;
    }

    /// <summary>
    /// Removes a spell from the known list.
    /// </summary>
    public void RemoveSpell(Spell spell) => _knownSpells.Remove(spell);

    /// <summary>
    /// Attempts to cast a spell of a given level. Returns true if a spell slot was available.
    /// </summary>
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

    /// <summary>
    /// Gets the modifier used for spellcasting and preparation for this class.
    /// </summary>
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

    /// <summary>
    /// Returns the list of prepared spells within limit (Level + modifier).
    /// Includes always-prepared spells that don't count against the limit.
    /// </summary>
    public List<Spell> GetPreparedSpells()
    {
        int limit = Level + GetRelevantAbilityModifier();

        var manual = _manuallyPreparedSpells
            .OrderByDescending(spell => spell.SpellLevel)
            .Take(limit)
            .ToList();

        // Always prepared spells (IsAlwaysPrepared) do not count toward the limit
        var always = _knownSpells.Where(s => s.IsAlwaysPrepared).ToList();

        // Merge unique by Id - include always first then manual (manual may include duplicates)
        var combined = always.Concat(manual).GroupBy(s => s.Id).Select(g => g.First()).ToList();
        return combined;
    }

    /// <summary>
    /// Toggles a spell as prepared/unprepared within current limit.
    /// This only affects manually prepared spells; always-prepared spells are managed separately.
    /// </summary>
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

}
