using TabletopSpells.Models.Enums;

public class Spell
{
    public required string Name
    {
        get; set;
    }
    public required string SpellLevel
    {
        get; set;
    } // e.g., "sorcerer/wizard 2, magus 2"
    public School School
    {
        get; set;
    }
    public required string Description
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
    } // Source book or reference
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
