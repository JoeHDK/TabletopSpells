using Newtonsoft.Json;
using TabletopSpells.Models.Enums;

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
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<Spell>>(json) ?? [];
        }
        catch (Exception ex)
        {
            // Handle or log the exception as needed
            Console.WriteLine($"Error loading spells from JSON: {ex.Message}");
            return [];
        }
    }

    public IEnumerable<Spell> GetSpellsByLevel(int level)
    {
        return spells.Where(spell => spell.SpellLevel != null && 
                                     spell.SpellLevel
                                         .Contains(
                                             $"level {level}", 
                                             StringComparison.OrdinalIgnoreCase)).ToList();
    }


    public IEnumerable<Spell> GetSpellsByName(string name)
    {
        return spells.Where(spell => spell.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public static List<Spell> GetAllSpellsFromJson(Game gameType)
    {
        string resourceName = gameType switch
        {
            Game.dnd5e => "TabletopSpells.Spells.dnd 5e.json",
            Game.pathfinder1e => "TabletopSpells.Spells.Pathfinder1e.json",
            _ => throw new ArgumentException($"Unknown game type: {gameType}")
        };
        
        System.Diagnostics.Debug.WriteLine($"=== SpellRepository.GetAllSpellsFromJson START ===");
        System.Diagnostics.Debug.WriteLine($"GameType: {gameType}");
        System.Diagnostics.Debug.WriteLine($"ResourceName: {resourceName}");
        
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Resource not found: {resourceName}");
                    var availableResources = assembly.GetManifestResourceNames();
                    System.Diagnostics.Debug.WriteLine($"Available resources ({availableResources.Length} total):");
                    foreach (var res in availableResources.Where(r => r.Contains("Spell")))
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {res}");
                    }
                    return new List<Spell>();
                }
                
                System.Diagnostics.Debug.WriteLine($"✓ Resource found, size: {stream.Length} bytes");
                
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    System.Diagnostics.Debug.WriteLine($"✓ JSON loaded, length: {json.Length} characters");
                    
                    // Use Newtonsoft.Json to properly deserialize JsonProperty attributes
                    var result = JsonConvert.DeserializeObject<List<Spell>>(json) ?? new List<Spell>();
                    System.Diagnostics.Debug.WriteLine($"✓ Deserialization complete");
                    System.Diagnostics.Debug.WriteLine($"✓ Successfully loaded {result.Count} spells from {gameType}");
                    
                    // Log first few spells
                    if (result.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"First 3 spells:");
                        for (int i = 0; i < Math.Min(3, result.Count); i++)
                        {
                            var spell = result[i];
                            System.Diagnostics.Debug.WriteLine($"  [{i}] Name='{spell.Name}' Level='{spell.SpellLevel}'");
                        }
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"=== SpellRepository.GetAllSpellsFromJson END ===");
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error loading spells: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"=== SpellRepository.GetAllSpellsFromJson END (ERROR) ===");
            return new List<Spell>();
        }
    }
}
