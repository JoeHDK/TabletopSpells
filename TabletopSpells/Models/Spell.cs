using Newtonsoft.Json;
using TabletopSpells.Models.Enums;

public class Spell
{
    [JsonProperty("name")]
    public string Name
    {
        get; set;
    }
    [JsonProperty("spell_level")]
    public string? SpellLevel
    {
        get; set;
    } // e.g., "sorcerer/wizard 6, magus 6"
    public School? School
    {
        get; set;
    }
    [JsonProperty("description")]
    public string? Description
    {
        get; set;
    }
    [JsonProperty("duration")]
    public string? Duration
    {
        get; set;
    }
    [JsonProperty("components")]
    public string? Components
    {
        get; set;
    }
    [JsonProperty("saving_throw")]
    public string? SavingThrow
    {
        get; set;
    }
    [JsonProperty("range")]
    public string? Range
    {
        get; set;
    }
    [JsonProperty("source")]
    public string? Source
    {
        get; set;
    }
    [JsonProperty("targets")]
    public string? Targets
    {
        get; set;
    }
    [JsonProperty("casting_time")]
    public string? CastingTime
    {
        get; set;
    }
    [JsonProperty("ritual")]
    public bool Ritual
    {
        get; set;
    }

    public bool IsNativeSpell
    {
        get; set;
    }

    // Additional fields can be added as needed
}
