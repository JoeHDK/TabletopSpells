namespace TabletopSpells.ViewModels;
public class SpellLevelViewModel
{
    public int Level
    {
        get; set;
    }
    public int MaxSpells
    {
        get; set;
    }
    public int SpellsUsed
    {
        get; set;
    }
    public string? DisplayText
    {
        get; set;
    }
    public string? DetailText
    {
        get; set;
    }
}
