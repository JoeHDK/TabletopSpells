using TabletopSpells.Models.Enums;

namespace TabletopSpells.Helpers
{
    /// <summary>
    /// Helper class for managing subclass operations and mappings.
    /// Supports all official D&D 5e subclasses from PHB, Xanathar's, Tasha's, and other supplements.
    /// </summary>
    public static class SubclassHelper
    {
        /// <summary>
        /// Gets the parent class for a given subclass.
        /// </summary>
        public static Class GetParentClass(Subclass subclass)
        {
            return subclass switch
            {
                // Barbarian
                Subclass.BarbarianPathOfTheBerserker or Subclass.BarbarianPathOfTheTotemWarrior or 
                Subclass.BarbarianPathOfTheWildMagic or Subclass.BarbarianPathOfTheZealot or 
                Subclass.BarbarianPathOfTheAncestralGuardian or Subclass.BarbarianPathOfTheRune or 
                Subclass.BarbarianPathOfTheBeast or Subclass.BarbarianPathOfTheWildHunt => Class.Barbarian,

                // Bard
                Subclass.BardCollegeOfLore or Subclass.BardCollegeOfPerformance or 
                Subclass.BardCollegeOfGlamour or Subclass.BardCollegeOfWhispers or 
                Subclass.BardCollegeOfSwords or Subclass.BardCollegeOfEloquence or 
                Subclass.BardCollegeOfSpirits or Subclass.BardCollegeOfPuppets => Class.Bard,

                // Cleric
                Subclass.ClericAirDomain or Subclass.ClericAnimalDomain or Subclass.ClericArcanaDomain or 
                Subclass.ClericDeathDomain or Subclass.ClericForgeDomain or Subclass.ClericGraveDomain or 
                Subclass.ClericKnowledgeDomain or Subclass.ClericLifeDomain or Subclass.ClericLightDomain or 
                Subclass.ClericNatureDomain or Subclass.ClericTempestDomain or Subclass.ClericTrickeryDomain or 
                Subclass.ClericWarDomain or Subclass.ClericTwilightDomain or Subclass.ClericLoveDomain or 
                Subclass.ClericOrderDomain => Class.Cleric,

                // Druid
                Subclass.DruidCircleOfTheLand or Subclass.DruidCircleOfTheMoon or 
                Subclass.DruidCircleOfSpores or Subclass.DruidCircleOfDreams or 
                Subclass.DruidCircleOfTheShepherd or Subclass.DruidCircleOfWildFire or 
                Subclass.DruidCircleOfStar or Subclass.DruidCircleOfTheTaiga => Class.Druid,

                // Fighter
                Subclass.FighterChampion or Subclass.FighterBattleMaster or Subclass.FighterEldritchKnight or 
                Subclass.FighterArcaneArcher or Subclass.FighterCavalier or Subclass.FighterCensus or 
                Subclass.FighterRune or Subclass.FighterPsiWarrior or Subclass.FighterBeast or 
                Subclass.FighterRoninSoul => Class.Fighter,

                // Monk
                Subclass.MonkWayOfTheOpenHand or Subclass.MonkWayOfTheLongDeath or 
                Subclass.MonkWayOfTheFourElements or Subclass.MonkWayOfShadow or 
                Subclass.MonkWayOfTheSunSoul or Subclass.MonkWayOfMercy or Subclass.MonkWayOfTheKensei or 
                Subclass.MonkWayOfTranscendence or Subclass.MonkWayOfTheAstralSelf or 
                Subclass.MonkWayOfTheCobalt or Subclass.MonkWayOfTheQuivering => Class.Monk,

                // Paladin
                Subclass.PaladinOathOfDevotion or Subclass.PaladinOathOfTheAncients or 
                Subclass.PaladinOathOfVengeance or Subclass.PaladinOathOfConquest or 
                Subclass.PaladinOathOfRedemption or Subclass.PaladinOathOfTheeWatchers or 
                Subclass.PaladinOathOfTheCrown or Subclass.PaladinOathOfCompulsion or 
                Subclass.PaladinOathOfDespair => Class.Paladin,

                // Ranger
                Subclass.RangerHunter or Subclass.RangerBeastMaster or Subclass.RangerGloomStalker or 
                Subclass.RangerMonk or Subclass.RangerLandStride or Subclass.RangerFeyWanderer or 
                Subclass.RangerSwiftBlade or Subclass.RangerSoulknife => Class.Ranger,

                // Rogue
                Subclass.RogueAssassin or Subclass.RogueThief or Subclass.RogueTrickster or 
                Subclass.RogueArcaneConundrum or Subclass.RogueSoulknife or Subclass.RogueShadowdancer or 
                Subclass.RogueInquisitive or Subclass.RogueSwashbuckler or Subclass.RogueAcrobat => Class.Rogue,

                // Sorcerer
                Subclass.SorcererDraconicBloodline or Subclass.SorcererWildMagic or Subclass.SorcererStormSorcery or 
                Subclass.SorcererShadowMagic or Subclass.SorcererDivineSource or Subclass.SorcererSilverforest or 
                Subclass.SorcererRuneScarred or Subclass.SorcererSessionOfTheClockworkSoul or 
                Subclass.SorcererAbberantMind or Subclass.SorcererSpiritual or Subclass.SorcererPhoenix or 
                Subclass.SorcererTortle => Class.Sorcerer,

                // Warlock
                Subclass.WarlockArchfey or Subclass.WarlockFiend or Subclass.WarlockGreatOldOne or 
                Subclass.WarlockCelestial or Subclass.WarlockHexblade or Subclass.WarlockUndying or 
                Subclass.WarlockGenies or Subclass.WarlockRaven or Subclass.WarlockSeeker or 
                Subclass.WarlockChaos or Subclass.WarlockLuck => Class.Warlock,

                // Wizard
                Subclass.WizardAbjuration or Subclass.WizardConjuration or Subclass.WizardDivination or 
                Subclass.WizardEnchantment or Subclass.WizardEvocation or Subclass.WizardIllusion or 
                Subclass.WizardNecromancy or Subclass.WizardTransmutation or Subclass.WizardChronoturgy or 
                Subclass.WizardGravityMastery or Subclass.WizardWar or Subclass.WizardBladesingers or 
                Subclass.WizardAlchemist or Subclass.WizardTheurgy => Class.Wizard,

                // Artificer
                Subclass.ArtificerAlchemist or Subclass.ArtificerArtillerist or Subclass.ArtificerBattlesmith or 
                Subclass.ArtificerRune or Subclass.ArtificerRitual or Subclass.ArtificerFiend or 
                Subclass.ArtificerClock => Class.Artificer,

                _ => Class.Barbarian
            };
        }

        /// <summary>
        /// Gets a display-friendly name for a subclass.
        /// </summary>
        public static string GetDisplayName(Subclass subclass)
        {
            return subclass switch
            {
                // Barbarian
                Subclass.BarbarianPathOfTheBerserker => "Path of the Berserker",
                Subclass.BarbarianPathOfTheTotemWarrior => "Path of the Totem Warrior",
                Subclass.BarbarianPathOfTheWildMagic => "Path of Wild Magic",
                Subclass.BarbarianPathOfTheZealot => "Path of the Zealot",
                Subclass.BarbarianPathOfTheAncestralGuardian => "Path of the Ancestral Guardian",
                Subclass.BarbarianPathOfTheRune => "Path of the Rune",
                Subclass.BarbarianPathOfTheBeast => "Path of the Beast",
                Subclass.BarbarianPathOfTheWildHunt => "Path of the Wild Hunt",

                // Bard
                Subclass.BardCollegeOfLore => "College of Lore",
                Subclass.BardCollegeOfPerformance => "College of Performance",
                Subclass.BardCollegeOfGlamour => "College of Glamour",
                Subclass.BardCollegeOfWhispers => "College of Whispers",
                Subclass.BardCollegeOfSwords => "College of Swords",
                Subclass.BardCollegeOfEloquence => "College of Eloquence",
                Subclass.BardCollegeOfSpirits => "College of Spirits",
                Subclass.BardCollegeOfPuppets => "College of Puppets",

                // Cleric
                Subclass.ClericAirDomain => "Air Domain",
                Subclass.ClericAnimalDomain => "Animal Domain",
                Subclass.ClericArcanaDomain => "Arcana Domain",
                Subclass.ClericDeathDomain => "Death Domain",
                Subclass.ClericForgeDomain => "Forge Domain",
                Subclass.ClericGraveDomain => "Grave Domain",
                Subclass.ClericKnowledgeDomain => "Knowledge Domain",
                Subclass.ClericLifeDomain => "Life Domain",
                Subclass.ClericLightDomain => "Light Domain",
                Subclass.ClericNatureDomain => "Nature Domain",
                Subclass.ClericTempestDomain => "Tempest Domain",
                Subclass.ClericTrickeryDomain => "Trickery Domain",
                Subclass.ClericWarDomain => "War Domain",
                Subclass.ClericTwilightDomain => "Twilight Domain",
                Subclass.ClericLoveDomain => "Love Domain",
                Subclass.ClericOrderDomain => "Order Domain",

                // Druid
                Subclass.DruidCircleOfTheLand => "Circle of the Land",
                Subclass.DruidCircleOfTheMoon => "Circle of the Moon",
                Subclass.DruidCircleOfSpores => "Circle of Spores",
                Subclass.DruidCircleOfDreams => "Circle of Dreams",
                Subclass.DruidCircleOfTheShepherd => "Circle of the Shepherd",
                Subclass.DruidCircleOfWildFire => "Circle of Wildfire",
                Subclass.DruidCircleOfStar => "Circle of Stars",
                Subclass.DruidCircleOfTheTaiga => "Circle of the Taiga",

                // Fighter
                Subclass.FighterChampion => "Champion",
                Subclass.FighterBattleMaster => "Battle Master",
                Subclass.FighterEldritchKnight => "Eldritch Knight",
                Subclass.FighterArcaneArcher => "Arcane Archer",
                Subclass.FighterCavalier => "Cavalier",
                Subclass.FighterCensus => "Censor",
                Subclass.FighterRune => "Rune Knight",
                Subclass.FighterPsiWarrior => "Psi Warrior",
                Subclass.FighterBeast => "Beast Master",
                Subclass.FighterRoninSoul => "Ronin Soul",

                // Monk
                Subclass.MonkWayOfTheOpenHand => "Way of the Open Hand",
                Subclass.MonkWayOfTheLongDeath => "Way of the Long Death",
                Subclass.MonkWayOfTheFourElements => "Way of the Four Elements",
                Subclass.MonkWayOfShadow => "Way of Shadow",
                Subclass.MonkWayOfTheSunSoul => "Way of the Sun Soul",
                Subclass.MonkWayOfMercy => "Way of Mercy",
                Subclass.MonkWayOfTheKensei => "Way of the Kensei",
                Subclass.MonkWayOfTranscendence => "Way of Transcendence",
                Subclass.MonkWayOfTheAstralSelf => "Way of the Astral Self",
                Subclass.MonkWayOfTheCobalt => "Way of the Cobalt Soul",
                Subclass.MonkWayOfTheQuivering => "Way of the Quivering Palm",

                // Paladin
                Subclass.PaladinOathOfDevotion => "Oath of Devotion",
                Subclass.PaladinOathOfTheAncients => "Oath of the Ancients",
                Subclass.PaladinOathOfVengeance => "Oath of Vengeance",
                Subclass.PaladinOathOfConquest => "Oath of Conquest",
                Subclass.PaladinOathOfRedemption => "Oath of Redemption",
                Subclass.PaladinOathOfTheeWatchers => "Oath of the Watchers",
                Subclass.PaladinOathOfTheCrown => "Oath of the Crown",
                Subclass.PaladinOathOfCompulsion => "Oath of Compulsion",
                Subclass.PaladinOathOfDespair => "Oath of Despair",

                // Ranger
                Subclass.RangerHunter => "Hunter",
                Subclass.RangerBeastMaster => "Beast Master",
                Subclass.RangerGloomStalker => "Gloom Stalker",
                Subclass.RangerMonk => "Monk",
                Subclass.RangerLandStride => "Land Stride",
                Subclass.RangerFeyWanderer => "Fey Wanderer",
                Subclass.RangerSwiftBlade => "Swift Blade",
                Subclass.RangerSoulknife => "Soulknife",

                // Rogue
                Subclass.RogueAssassin => "Assassin",
                Subclass.RogueThief => "Thief",
                Subclass.RogueTrickster => "Trickster",
                Subclass.RogueArcaneConundrum => "Arcane Conundrum",
                Subclass.RogueSoulknife => "Soulknife",
                Subclass.RogueShadowdancer => "Shadowdancer",
                Subclass.RogueInquisitive => "Inquisitive",
                Subclass.RogueSwashbuckler => "Swashbuckler",
                Subclass.RogueAcrobat => "Acrobat",

                // Sorcerer
                Subclass.SorcererDraconicBloodline => "Draconic Bloodline",
                Subclass.SorcererWildMagic => "Wild Magic",
                Subclass.SorcererStormSorcery => "Storm Sorcery",
                Subclass.SorcererShadowMagic => "Shadow Magic",
                Subclass.SorcererDivineSource => "Divine Soul",
                Subclass.SorcererSilverforest => "Silverforest",
                Subclass.SorcererRuneScarred => "Rune Scarred",
                Subclass.SorcererSessionOfTheClockworkSoul => "Clockwork Soul",
                Subclass.SorcererAbberantMind => "Aberrant Mind",
                Subclass.SorcererSpiritual => "Spiritual",
                Subclass.SorcererPhoenix => "Phoenix Sorcery",
                Subclass.SorcererTortle => "Tortle",

                // Warlock
                Subclass.WarlockArchfey => "Archfey",
                Subclass.WarlockFiend => "Fiend",
                Subclass.WarlockGreatOldOne => "Great Old One",
                Subclass.WarlockCelestial => "Celestial",
                Subclass.WarlockHexblade => "Hexblade",
                Subclass.WarlockUndying => "Undying",
                Subclass.WarlockGenies => "Genies",
                Subclass.WarlockRaven => "Raven",
                Subclass.WarlockSeeker => "Seeker",
                Subclass.WarlockChaos => "Chaos",
                Subclass.WarlockLuck => "Luck",

                // Wizard
                Subclass.WizardAbjuration => "Abjuration",
                Subclass.WizardConjuration => "Conjuration",
                Subclass.WizardDivination => "Divination",
                Subclass.WizardEnchantment => "Enchantment",
                Subclass.WizardEvocation => "Evocation",
                Subclass.WizardIllusion => "Illusion",
                Subclass.WizardNecromancy => "Necromancy",
                Subclass.WizardTransmutation => "Transmutation",
                Subclass.WizardChronoturgy => "Chronurgy Magic",
                Subclass.WizardGravityMastery => "Gravity Mastery",
                Subclass.WizardWar => "War Magic",
                Subclass.WizardBladesingers => "Bladesinger",
                Subclass.WizardAlchemist => "Alchemist",
                Subclass.WizardTheurgy => "Theurgy",

                // Artificer
                Subclass.ArtificerAlchemist => "Alchemist",
                Subclass.ArtificerArtillerist => "Artillerist",
                Subclass.ArtificerBattlesmith => "Battlesmith",
                Subclass.ArtificerRune => "Rune",
                Subclass.ArtificerRitual => "Ritual",
                Subclass.ArtificerFiend => "Fiend",
                Subclass.ArtificerClock => "Clockwork",

                Subclass.None => "None",
                _ => subclass.ToString()
            };
        }

        /// <summary>
        /// Gets all available subclasses for a given parent class.
        /// </summary>
        public static List<Subclass> GetSubclassesForClass(Class parentClass)
        {
            return parentClass switch
            {
                Class.Barbarian => new List<Subclass>
                {
                    Subclass.BarbarianPathOfTheBerserker, Subclass.BarbarianPathOfTheTotemWarrior,
                    Subclass.BarbarianPathOfTheWildMagic, Subclass.BarbarianPathOfTheZealot,
                    Subclass.BarbarianPathOfTheAncestralGuardian, Subclass.BarbarianPathOfTheRune,
                    Subclass.BarbarianPathOfTheBeast, Subclass.BarbarianPathOfTheWildHunt
                },

                Class.Bard => new List<Subclass>
                {
                    Subclass.BardCollegeOfLore, Subclass.BardCollegeOfPerformance,
                    Subclass.BardCollegeOfGlamour, Subclass.BardCollegeOfWhispers,
                    Subclass.BardCollegeOfSwords, Subclass.BardCollegeOfEloquence,
                    Subclass.BardCollegeOfSpirits, Subclass.BardCollegeOfPuppets
                },

                Class.Cleric => new List<Subclass>
                {
                    Subclass.ClericAirDomain, Subclass.ClericAnimalDomain, Subclass.ClericArcanaDomain,
                    Subclass.ClericDeathDomain, Subclass.ClericForgeDomain, Subclass.ClericGraveDomain,
                    Subclass.ClericKnowledgeDomain, Subclass.ClericLifeDomain, Subclass.ClericLightDomain,
                    Subclass.ClericNatureDomain, Subclass.ClericTempestDomain, Subclass.ClericTrickeryDomain,
                    Subclass.ClericWarDomain, Subclass.ClericTwilightDomain, Subclass.ClericLoveDomain,
                    Subclass.ClericOrderDomain
                },

                Class.Druid => new List<Subclass>
                {
                    Subclass.DruidCircleOfTheLand, Subclass.DruidCircleOfTheMoon,
                    Subclass.DruidCircleOfSpores, Subclass.DruidCircleOfDreams,
                    Subclass.DruidCircleOfTheShepherd, Subclass.DruidCircleOfWildFire,
                    Subclass.DruidCircleOfStar, Subclass.DruidCircleOfTheTaiga
                },

                Class.Fighter => new List<Subclass>
                {
                    Subclass.FighterChampion, Subclass.FighterBattleMaster, Subclass.FighterEldritchKnight,
                    Subclass.FighterArcaneArcher, Subclass.FighterCavalier, Subclass.FighterCensus,
                    Subclass.FighterRune, Subclass.FighterPsiWarrior, Subclass.FighterBeast,
                    Subclass.FighterRoninSoul
                },

                Class.Monk => new List<Subclass>
                {
                    Subclass.MonkWayOfTheOpenHand, Subclass.MonkWayOfTheLongDeath,
                    Subclass.MonkWayOfTheFourElements, Subclass.MonkWayOfShadow,
                    Subclass.MonkWayOfTheSunSoul, Subclass.MonkWayOfMercy, Subclass.MonkWayOfTheKensei,
                    Subclass.MonkWayOfTranscendence, Subclass.MonkWayOfTheAstralSelf,
                    Subclass.MonkWayOfTheCobalt, Subclass.MonkWayOfTheQuivering
                },

                Class.Paladin => new List<Subclass>
                {
                    Subclass.PaladinOathOfDevotion, Subclass.PaladinOathOfTheAncients,
                    Subclass.PaladinOathOfVengeance, Subclass.PaladinOathOfConquest,
                    Subclass.PaladinOathOfRedemption, Subclass.PaladinOathOfTheeWatchers,
                    Subclass.PaladinOathOfTheCrown, Subclass.PaladinOathOfCompulsion,
                    Subclass.PaladinOathOfDespair
                },

                Class.Ranger => new List<Subclass>
                {
                    Subclass.RangerHunter, Subclass.RangerBeastMaster, Subclass.RangerGloomStalker,
                    Subclass.RangerMonk, Subclass.RangerLandStride, Subclass.RangerFeyWanderer,
                    Subclass.RangerSwiftBlade, Subclass.RangerSoulknife
                },

                Class.Rogue => new List<Subclass>
                {
                    Subclass.RogueAssassin, Subclass.RogueThief, Subclass.RogueTrickster,
                    Subclass.RogueArcaneConundrum, Subclass.RogueSoulknife, Subclass.RogueShadowdancer,
                    Subclass.RogueInquisitive, Subclass.RogueSwashbuckler, Subclass.RogueAcrobat
                },

                Class.Sorcerer => new List<Subclass>
                {
                    Subclass.SorcererDraconicBloodline, Subclass.SorcererWildMagic, Subclass.SorcererStormSorcery,
                    Subclass.SorcererShadowMagic, Subclass.SorcererDivineSource, Subclass.SorcererSilverforest,
                    Subclass.SorcererRuneScarred, Subclass.SorcererSessionOfTheClockworkSoul,
                    Subclass.SorcererAbberantMind, Subclass.SorcererSpiritual, Subclass.SorcererPhoenix,
                    Subclass.SorcererTortle
                },

                Class.Warlock => new List<Subclass>
                {
                    Subclass.WarlockArchfey, Subclass.WarlockFiend, Subclass.WarlockGreatOldOne,
                    Subclass.WarlockCelestial, Subclass.WarlockHexblade, Subclass.WarlockUndying,
                    Subclass.WarlockGenies, Subclass.WarlockRaven, Subclass.WarlockSeeker,
                    Subclass.WarlockChaos, Subclass.WarlockLuck
                },

                Class.Wizard => new List<Subclass>
                {
                    Subclass.WizardAbjuration, Subclass.WizardConjuration, Subclass.WizardDivination,
                    Subclass.WizardEnchantment, Subclass.WizardEvocation, Subclass.WizardIllusion,
                    Subclass.WizardNecromancy, Subclass.WizardTransmutation, Subclass.WizardChronoturgy,
                    Subclass.WizardGravityMastery, Subclass.WizardWar, Subclass.WizardBladesingers,
                    Subclass.WizardAlchemist, Subclass.WizardTheurgy
                },

                Class.Artificer => new List<Subclass>
                {
                    Subclass.ArtificerAlchemist, Subclass.ArtificerArtillerist, Subclass.ArtificerBattlesmith,
                    Subclass.ArtificerRune, Subclass.ArtificerRitual, Subclass.ArtificerFiend,
                    Subclass.ArtificerClock
                },

                _ => new List<Subclass> { Subclass.None }
            };
        }
    }
}

