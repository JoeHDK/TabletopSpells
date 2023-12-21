using TabletopSpells.Models.Enums;

public class Spell
{
    public string Name
    {
        get; set;
    }
    public string SpellLevel
    {
        get; set;
    } // e.g., "sorcerer/wizard 6, magus 6"
    public School School
    {
        get; set;
    }
    public string Description
    {
        get; set;
    }
    public string Duration
    {
        get; set;
    }
    public string Components
    {
        get; set;
    }
    public string SavingThrow
    {
        get; set;
    }
    public string Range
    {
        get; set;
    }
    public string Source
    {
        get; set;
    }
    public string Targets
    {
        get; set;
    }
    public string CastingTime
    {
        get; set;
    }

    // Additional fields can be added as needed
}
