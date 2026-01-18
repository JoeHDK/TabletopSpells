# Integration Guide: Subclass + Domain Spells

This guide explains how to use the new Subclass system with the existing `DomainSpells.json` file.

## Current State

You now have:
1. **Subclass Enum** - Defines all available subclasses
2. **SubclassHelper** - Maps subclasses to display names and parent classes  
3. **Character.Subclass** - Property to store selected subclass
4. **StatsPage UI** - Picker to select subclass for a character
5. **DomainSpells.json** - Complete spell list organized by class/domain/oath/circle

## Example Flows

### Flow 1: Divine Caster Selects Domain (Cleric)

**Current State:**
1. Character created as Cleric
2. User goes to StatsPage
3. SubclassPicker shows available Cleric domains: Air, Animal, Arcana, Death, Forge, Grave, Knowledge, Life, Light, Nature, Tempest, Trickery, War
4. User selects "Air" domain
5. `character.Subclass = Subclass.ClericAir`

**To Add Domain Spells Automatically:**
```csharp
// In CharacterSelectPage or character creation flow:
if (character.IsDivineCaster && character.Subclass != Subclass.None)
{
    var domainSpells = DomainSpellManager.GetDomainSpells(character);
    foreach (var spell in domainSpells)
    {
        spell.IsAlwaysPrepared = true;
        character.AddSpell(spell);
    }
}
```

### Flow 2: Divine Caster Selects Oath (Paladin)

**Current State:**
1. Character created as Paladin
2. User goes to StatsPage
3. SubclassPicker shows available Paladin oaths: Devotion, Ancients, Vengeance, Conquest, Redemption
4. User selects "Devotion" oath
5. `character.Subclass = Subclass.PaladinDevoti`

**To Add Oath Spells Automatically:**
```csharp
// Gets spells from DomainSpells.json["domainSpells"]["Paladin"]["oaths"]["Devotion"]
var oathSpells = DomainSpellManager.GetOathSpells(character);
foreach (var spell in oathSpells)
{
    spell.IsAlwaysPrepared = true;
    character.AddSpell(spell);
}
```

## How to Implement Domain Spell Loading

### Option 1: Simple Approach (Load on Character Load)

Create `Services/DomainSpellManager.cs`:

```csharp
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using Newtonsoft.Json;
using System.Reflection;

namespace TabletopSpells.Services
{
    public class DomainSpellManager
    {
        private static Dictionary<string, dynamic> _domainSpellsCache;

        public static List<string> GetDomainSpellNames(Character character)
        {
            if (_domainSpellsCache == null)
            {
                _domainSpellsCache = LoadDomainSpellsFromJson();
            }

            var className = character.CharacterClass.ToString();
            var subclassName = SubclassHelper.GetDisplayName(character.Subclass);

            // Navigate JSON based on class type
            try
            {
                if (_domainSpellsCache.ContainsKey("domainSpells"))
                {
                    var classNode = _domainSpellsCache["domainSpells"][className];
                    
                    // Cleric/Druid use "domains"/"circles", Paladin uses "oaths"
                    var categoryNode = classNode.ContainsKey("domains") ? classNode["domains"] :
                                      classNode.ContainsKey("circles") ? classNode["circles"] :
                                      classNode.ContainsKey("oaths") ? classNode["oaths"] :
                                      classNode;

                    var spellNode = categoryNode[subclassName];
                    var spellNames = new List<string>();

                    for (int level = 1; level <= 9; level++)
                    {
                        var key = $"spellLevel{level}";
                        if (spellNode.ContainsKey(key))
                        {
                            foreach (var spell in spellNode[key])
                            {
                                spellNames.Add((string)spell);
                            }
                        }
                    }

                    return spellNames;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading domain spells: {ex.Message}");
            }

            return new List<string>();
        }

        private static Dictionary<string, dynamic> LoadDomainSpellsFromJson()
        {
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                using var stream = assembly.GetManifestResourceStream("TabletopSpells.Resources.DomainSpells.json");
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading DomainSpells.json: {ex.Message}");
                return new Dictionary<string, dynamic>();
            }
        }
    }
}
```

### Option 2: Call in SharedViewModel

In `SharedViewModel.LoadSpellsForCharacter()`:

```csharp
public void LoadSpellsForCharacter(Character character)
{
    if (character == null) return;

    var spells = new ObservableCollection<Spell>();

    if (character.ID != null)
    {
        try
        {
            var list = LocalStorageHelper.LoadSpellFiles(character.ID.Value);
            foreach (var sp in list)
            {
                // ... existing code ...
                spells.Add(sp);
            }
        }
        catch { }
    }

    // NEW: Load domain spells for divine casters
    if (character.IsDivineCaster && character.Subclass != Subclass.None)
    {
        var domainSpellNames = DomainSpellManager.GetDomainSpellNames(character);
        var allSpells = SpellRepository.GetAllSpellsFromJson(character.GameType);
        
        foreach (var domainSpellName in domainSpellNames)
        {
            var spell = allSpells.FirstOrDefault(s => 
                s.Name != null && s.Name.Equals(domainSpellName, StringComparison.OrdinalIgnoreCase));
            
            if (spell != null && !spells.Any(s => s.Id == spell.Id))
            {
                spell.IsAlwaysPrepared = true;
                spells.Add(spell);
            }
        }
    }

    CharacterSpells[character.ID] = spells;
    CharacterSpells[character.ID].CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterSpells));
}
```

## Testing Checklist

- [ ] Create a Cleric character
- [ ] Go to StatsPage
- [ ] Verify SubclassPicker shows Cleric domains
- [ ] Select "Life" domain
- [ ] Verify `character.Subclass == Subclass.ClericLife`
- [ ] Save character
- [ ] Reload character
- [ ] Verify Subclass is still "Life"
- [ ] (Optional) Verify domain spells auto-loaded as always-prepared

## JSON Structure Reference

```json
{
  "domainSpells": {
    "Cleric": {
      "domains": {
        "Life": {
          "spellLevel1": ["Bless", "Cure Wounds"],
          "spellLevel2": ["Lesser Restoration", "Spiritual Weapon"],
          ...
        }
      }
    },
    "Druid": {
      "circles": {
        "Land": { ... }
      }
    },
    "Paladin": {
      "oaths": {
        "Devotion": { ... }
      }
    }
  }
}
```

## Files to Create/Modify

1. **New**: `Services/DomainSpellManager.cs` - Loads domain spells from JSON
2. **Modify**: `ViewModels/SharedViewModel.cs` - Call DomainSpellManager in LoadSpellsForCharacter()
3. **Optional**: Create UI for domain selection during character creation

