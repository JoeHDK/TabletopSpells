using TabletopSpells.Models.Enums;

namespace TabletopSpells.Models;

public class Character
{
    public int ID
    {
        get; set;
    }

    public Dictionary<int, (int Current, int Maximum)> SpellsPerDay
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

    public Character() => Spells = new List<Spell>();

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
}
