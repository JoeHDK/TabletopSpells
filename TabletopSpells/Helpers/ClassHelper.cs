using TabletopSpells.Models.Enums;

namespace TabletopSpells.Helpers
{
    public static class ClassHelper
    {
        private static readonly Dictionary<Class, List<Game>> ClassGameMapping = new Dictionary<Class, List<Game>>
        {
            // Classes for Pathfinder 1st edition
            { Class.Barbarian, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.Bard, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.Cleric, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.Druid, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.Fighter, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.Monk, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.Paladin, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.Ranger, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.Rogue, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.Sorcerer, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.Wizard, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.Warlock, new List<Game> { Game.pathfinder1e, Game.dnd5e } },
            { Class.Inquisitor, new List<Game> { Game.pathfinder1e} },
            { Class.Summoner, new List<Game> { Game.pathfinder1e} },
            { Class.Witch, new List<Game> { Game.pathfinder1e} },
            { Class.Alchemist, new List<Game> { Game.pathfinder1e} },
            { Class.Magus, new List<Game> { Game.pathfinder1e} },
            { Class.Oracle, new List<Game> { Game.pathfinder1e} },
            { Class.Shaman, new List<Game> { Game.pathfinder1e} },
            { Class.Spiritualist, new List<Game> { Game.pathfinder1e} },
            { Class.Occultist, new List<Game> { Game.pathfinder1e} },
            { Class.Psychic, new List<Game> { Game.pathfinder1e} },
            { Class.Mesmerist, new List<Game> { Game.pathfinder1e} },

        };

        public static IEnumerable<Class> GetClassesByGame(Game game)
        {
            return ClassGameMapping.Where(kvp => kvp.Value.Contains(game)).Select(kvp => kvp.Key);
        }
    }
}