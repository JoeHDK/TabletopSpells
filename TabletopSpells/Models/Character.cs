using TabletopSpells.Models.Enums;

namespace TabletopSpells.Models;

public class Character
{
    public Guid? ID
    {
        get; set;
    }

    public required string Name
    {
        get; set;
    }

    public List<Spell> Spells
    {
        get; set;
    }

    public Class CharacterClass
    {
        get; set;
    }

    public Dictionary<int, int> MaxSpellsPerDay
    {
        get; set;
    }

    public Dictionary<int, int> SpellsUsedToday
    {
        get; set;
    }

    public Game GameType
    {
        get; set;
    }

    public Character()
    {
        Spells = new List<Spell>();
        MaxSpellsPerDay = new Dictionary<int, int>();
        SpellsUsedToday = new Dictionary<int, int>();
    }

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
        if (spellLevel == 0 || spellLevel == -1)  
            return true;
        else if (!SpellsUsedToday.ContainsKey(spellLevel) || SpellsUsedToday[spellLevel] >= MaxSpellsPerDay[spellLevel])
            return false;
        
        SpellsUsedToday[spellLevel]++;
        return true;
    }

}
