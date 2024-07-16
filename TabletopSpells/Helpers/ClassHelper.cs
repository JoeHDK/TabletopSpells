using TabletopSpells.Models.Enums;

namespace TabletopSpells.Helpers
{
    public static class ClassHelper
    {
        private static readonly Dictionary<Class, List<Game>> ClassGameMapping = new Dictionary<Class, List<Game>>
        {
            // Classes for Pathfinder 1st edition
            { Class.barbarian, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.bard, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.cleric, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.druid, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.fighter, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.monk, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.paladin, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.ranger, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.rogue, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.sorcerer, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.wizard, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.warlock, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.inquisitor, new List<Game> { Game.pathfinder1e} },
            { Class.summoner, new List<Game> { Game.pathfinder1e} },
            { Class.witch, new List<Game> { Game.pathfinder1e} },
            { Class.alchemist, new List<Game> { Game.pathfinder1e} },
            { Class.magus, new List<Game> { Game.pathfinder1e} },
            { Class.oracle, new List<Game> { Game.pathfinder1e} },
            { Class.shaman, new List<Game> { Game.pathfinder1e} },
            { Class.spiritualist, new List<Game> { Game.pathfinder1e} },
            { Class.occultist, new List<Game> { Game.pathfinder1e} },
            { Class.psychic, new List<Game> { Game.pathfinder1e} },
            { Class.mesmerist, new List<Game> { Game.pathfinder1e} },

        };

        public static IEnumerable<Class> GetClassesByGame(Game game)
        {
            return ClassGameMapping.Where(kvp => kvp.Value.Contains(game)).Select(kvp => kvp.Key);
        }
    }
}