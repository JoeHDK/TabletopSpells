using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TabletopSpells.Repositories;
public class SpellRepository
{
    private List<Spell> spells;

    public SpellRepository(string jsonFilePath)
    {
        spells = LoadSpellsFromJson(jsonFilePath);
    }

    private List<Spell> LoadSpellsFromJson(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Spell>>(json);
        }
        catch (Exception ex)
        {
            // Handle or log the exception as needed
            Console.WriteLine($"Error loading spells from JSON: {ex.Message}");
            return new List<Spell>();
        }
    }

    public IEnumerable<Spell> GetSpellsByLevel(int level)
    {
        // Assuming SpellLevel is a string like "sorcerer/wizard 2, magus 2"
        // Adjust the logic if the format is different
        return spells.Where(spell => spell.SpellLevel.Contains($"level {level}", StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public IEnumerable<Spell> GetSpellsByName(string name)
    {
        return spells.Where(spell => spell.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    // Other methods as needed...
}
