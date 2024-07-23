﻿namespace TabletopSpells.Models
{
    public class SpellCastLog
    {
        public DateTime CastTime
        {
            get; set;
        }
        public string? SpellName
        {
            get; set;
        }
        public int SpellLevel
        {
            get; set;
        }
        public int SessionId
        {
            get; set;
        }
        public string? FailedReason
        {
            get; set;
        }
        public bool CastAsRitual
        {
            get; set;
        } 
    }
}
