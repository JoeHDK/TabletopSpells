using TabletopSpells.Models.Enums;

namespace TabletopSpells.Models;

public class Character
{
    public Guid? ID { get; set; }
    public required string Name { get; set; }
    private List<Spell> Spells { get; } = new();
    public Class CharacterClass { get; init; }
    public Dictionary<int, int> MaxSpellsPerDay { get; set; } = new();
    public Dictionary<int, int> SpellsUsedToday { get; set; } = new();
    public Game GameType { get; init; }
    public int Level { get; set; }

    // Stores the prepared spells explicitly chosen by the user
    private List<Spell> _manuallyPreparedSpells = new();

    // New property for ability scores and modifiers
    public Dictionary<string, int> AbilityScores { get; set; } = new()
    {
        { "Strength", 10 },
        { "Dexterity", 10 },
        { "Constitution", 10 },
        { "Intelligence", 10 },
        { "Wisdom", 10 },
        { "Charisma", 10 }
    };

    // Derived property to calculate ability modifiers
    private Dictionary<string, int>? AbilityModifiers =>
        AbilityScores?.ToDictionary(
            kvp => kvp.Key,
            kvp => (kvp.Value - 10) / 2
        );

    public void AddSpell(Spell spell)
    {
        if (!Spells.Contains(spell))
        {
            Spells.Add(spell);
        }
    }

    public void RemoveSpell(Spell spell)
    {
        Spells.Remove(spell);
    }

    public bool CastSpell(int spellLevel)
    {
        if (spellLevel is 0 or -1)
            return true;

        if (!SpellsUsedToday.TryGetValue(spellLevel, out var value) || value >= MaxSpellsPerDay[spellLevel])
            return false;

        SpellsUsedToday[spellLevel]++;
        return true;
    }

    /// <summary>
    /// Retrieve the relevant ability modifier for a specific class.
    /// This determines how many spells a character can prepare, among other class-based mechanics.
    /// </summary>
    public int GetRelevantAbilityModifier()
    {
        var relevantAbility = CharacterClass switch
        {
            Class.Wizard => "Intelligence",
            Class.Cleric => "Wisdom",
            Class.Druid => "Wisdom",
            Class.Artificer => "Intelligence",
            Class.Paladin => "Charisma",
            Class.Sorcerer => "Charisma",
            Class.Bard => "Charisma",
            _ => "Strength" // Default to Strength if class not recognized
        };

        return AbilityModifiers != null && AbilityModifiers.TryGetValue(relevantAbility, out var value) ? value : 0;
    }

    /// <summary>
    /// Retrieves or displays the spells currently prepared by a character.
    /// This pulls from the manually prepared spells and enforces the limit based on rules.
    /// </summary>
    public List<Spell> GetPreparedSpells()
    {
        // Enforce the preparation limit dynamically
        var spellsPreparedLimit = Level + GetRelevantAbilityModifier();

        return _manuallyPreparedSpells
            .OrderByDescending(spell => spell.SpellLevel)
            .Take(spellsPreparedLimit)
            .ToList();
    }

    /// <summary>
    /// Toggles a spell's prepared status.
    /// Adds the spell if it is not already prepared, or removes it if it is.
    /// Enforces the preparation limit dynamically.
    /// </summary>
    public bool TogglePreparedSpell(Spell spell)
    {
        var spellsPreparedLimit = Level + GetRelevantAbilityModifier();

        if (_manuallyPreparedSpells.Contains(spell))
        {
            // If already prepared, unprepare it
            _manuallyPreparedSpells.Remove(spell);
            return true;
        }
        else if (_manuallyPreparedSpells.Count < spellsPreparedLimit)
        {
            // If not prepared and under limit, prepare it
            _manuallyPreparedSpells.Add(spell);
            return true;
        }

        // Spell limit exceeded, cannot add new spell
        return false;
    }
}