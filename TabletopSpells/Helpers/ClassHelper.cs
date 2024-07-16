using TabletopSpells.Models.Enums;

namespace TabletopSpells.Helpers
{
    public static class ClassHelper
    {
        private static readonly Dictionary<Class, string> ClassGameMapping = new Dictionary<Class, string>
        {
            { Class.barbarian, "Pathfinder 1e" },
            { Class.bard, "Pathfinder 1e" },
            { Class.cleric, "Pathfinder 1e" },
            { Class.druid, "Pathfinder 1e" },
            { Class.fighter, "Pathfinder 1e" },
            { Class.monk, "Pathfinder 1e" },
            { Class.paladin, "Pathfinder 1e" },
            { Class.ranger, "Pathfinder 1e" },
            { Class.rogue, "Pathfinder 1e" },
            { Class.sorcerer, "Pathfinder 1e" },
            { Class.wizard, "Pathfinder 1e" },
            { Class.inquisitor, "Pathfinder 1e" },
            { Class.summoner, "Pathfinder 1e" },
            { Class.witch, "Pathfinder 1e" },
            { Class.alchemist, "Pathfinder 1e" },
            { Class.magus, "Pathfinder 1e" },
            { Class.oracle, "Pathfinder 1e" },
            { Class.shaman, "Pathfinder 1e" },
            { Class.spiritualist, "Pathfinder 1e" },
            { Class.occultist, "Pathfinder 1e" },
            { Class.psychic, "Pathfinder 1e" },
            { Class.mesmerist, "Pathfinder 1e" },
            
            { Class.barbarian, "dnd 5e" },
            { Class.bard, "dnd 5e" },
            { Class.cleric, "dnd 5e" },
            { Class.druid, "dnd 5e" },
            { Class.fighter, "dnd 5e" },
            { Class.monk, "dnd 5e" },
            { Class.paladin, "dnd 5e" },
            { Class.ranger, "dnd 5e" },
            { Class.rogue, "dnd 5e" },
            { Class.sorcerer, "dnd 5e" },
            { Class.wizard, "dnd 5e" },
            { Class.warlock, "dnd 5e" },
        };

        public static IEnumerable<Class> GetClassesByGame(string game)
        {
            return ClassGameMapping.Where(kvp => kvp.Value == game).Select(kvp => kvp.Key);
        }
    }
}