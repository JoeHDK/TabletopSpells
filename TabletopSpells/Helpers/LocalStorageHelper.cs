using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TabletopSpells.Models;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace TabletopSpells.Helpers
{
    public static class LocalStorageHelper
    {
        private static string AppData
        {
            get
            {
                try
                {
                    return FileSystem.AppDataDirectory;
                }
                catch (NotImplementedException)
                {
                    // Running in unit test / headless environment - use temp folder
                    var dir = Path.Combine(Path.GetTempPath(), "TabletopSpells_AppData");
                    Directory.CreateDirectory(dir);
                    return dir;
                }
            }
        }

        public static string GetCharacterDir(Guid characterId)
        {
            return Path.Combine(AppData, "characters", characterId.ToString());
        }

        public static string GetCharacterSpellsDir(Guid characterId)
        {
            return Path.Combine(GetCharacterDir(characterId), "spells");
        }

        public static string GetPreparedSpellsPath(Guid characterId)
        {
            return Path.Combine(GetCharacterDir(characterId), "preparedSpells.json");
        }

        public static string GetAlwaysPreparedSpellsPath(Guid characterId)
        {
            return Path.Combine(GetCharacterDir(characterId), "alwaysPreparedSpells.json");
        }

        public static void EnsureCharacterDirs(Guid characterId)
        {
            var dir = GetCharacterSpellsDir(characterId);
            Directory.CreateDirectory(dir);
        }

        public static void SaveSpellToFile(Guid characterId, Spell spell)
        {
            EnsureCharacterDirs(characterId);
            var dir = GetCharacterSpellsDir(characterId);
            var path = Path.Combine(dir, spell.Id + ".json");
            var json = JsonConvert.SerializeObject(spell);
            File.WriteAllText(path, json);
        }

        public static void DeleteSpellFile(Guid characterId, Spell spell)
        {
            try
            {
                var dir = GetCharacterSpellsDir(characterId);
                var path = Path.Combine(dir, spell.Id + ".json");
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // ignore for now
            }
        }

        public static List<Spell> LoadSpellFiles(Guid characterId)
        {
            var outList = new List<Spell>();
            try
            {
                var dir = GetCharacterSpellsDir(characterId);
                if (!Directory.Exists(dir)) return outList;
                var files = Directory.GetFiles(dir, "*.json");
                foreach (var f in files)
                {
                    try
                    {
                        var json = File.ReadAllText(f);
                        var spell = JsonConvert.DeserializeObject<Spell>(json);
                        if (spell != null) outList.Add(spell);
                    }
                    catch
                    {
                        // skip corrupted file
                    }
                }
            }
            catch
            {
                // ignore
            }

            return outList;
        }

        public static void SavePreparedSpellIds(Guid characterId, List<Guid> ids)
        {
            try
            {
                EnsureCharacterDirs(characterId);
                var path = GetPreparedSpellsPath(characterId);
                var json = JsonConvert.SerializeObject(ids);
                File.WriteAllText(path, json);
            }
            catch
            {
                // ignore
            }
        }

        public static void SaveAlwaysPreparedSpellIds(Guid characterId, List<Guid> ids)
        {
            try
            {
                EnsureCharacterDirs(characterId);
                var path = GetAlwaysPreparedSpellsPath(characterId);
                var json = JsonConvert.SerializeObject(ids);
                File.WriteAllText(path, json);
            }
            catch
            {
                // ignore
            }
        }

        public static List<Guid> LoadPreparedSpellIds(Guid characterId)
        {
            try
            {
                var path = GetPreparedSpellsPath(characterId);
                if (!File.Exists(path)) return new List<Guid>();
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<List<Guid>>(json) ?? new List<Guid>();
            }
            catch
            {
                return new List<Guid>();
            }
        }

        public static List<Guid> LoadAlwaysPreparedSpellIds(Guid characterId)
        {
            try
            {
                var path = GetAlwaysPreparedSpellsPath(characterId);
                if (!File.Exists(path)) return new List<Guid>();
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<List<Guid>>(json) ?? new List<Guid>();
            }
            catch
            {
                return new List<Guid>();
            }
        }

        public static void DeleteCharacterFolder(Guid characterId)
        {
            try
            {
                var dir = GetCharacterDir(characterId);
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
            catch
            {
                // ignore
            }
        }

        public static bool CharacterHasSpellFiles(Guid characterId)
        {
            var dir = GetCharacterSpellsDir(characterId);
            if (!Directory.Exists(dir)) return false;
            return Directory.GetFiles(dir, "*.json").Length > 0;
        }

        public static string GetCharactersPath()
        {
            return Path.Combine(AppData, "characters.json");
        }

        public static List<Character> LoadCharactersFromFile()
        {
            try
            {
                var path = GetCharactersPath();
                if (!File.Exists(path)) return new List<Character>();
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<List<Character>>(json) ?? new List<Character>();
            }
            catch
            {
                return new List<Character>();
            }
        }

        public static void SaveCharactersToFile(List<Character> characters)
        {
            try
            {
                var path = GetCharactersPath();
                var json = JsonConvert.SerializeObject(characters);
                File.WriteAllText(path, json);
            }
            catch
            {
                // ignore
            }
        }

        public static string GetSpellLogsPath(Guid characterId)
        {
            return Path.Combine(GetCharacterDir(characterId), "spellLogs.json");
        }

        public static void SaveSpellLogs(Guid characterId, List<object> logs)
        {
            try
            {
                EnsureCharacterDirs(characterId);
                var path = GetSpellLogsPath(characterId);
                var json = JsonConvert.SerializeObject(logs);
                File.WriteAllText(path, json);
            }
            catch
            {
                // ignore
            }
        }

        public static List<T> LoadSpellLogs<T>(Guid characterId)
        {
            try
            {
                var path = GetSpellLogsPath(characterId);
                if (!File.Exists(path)) return [];
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<List<T>>(json) ?? [];
            }
            catch
            {
                return [];
            }
        }

        public static string GetSpellsPerDayPath(Guid characterId)
        {
            return Path.Combine(GetCharacterDir(characterId), "spellsPerDay.json");
        }

        public static void SaveSpellsPerDayDetails(Guid characterId, Dictionary<int,int> maxSpellsPerDay, Dictionary<int,int> spellsUsedToday)
        {
            try
            {
                EnsureCharacterDirs(characterId);
                var path = GetSpellsPerDayPath(characterId);
                var payload = new { Max = maxSpellsPerDay, Used = spellsUsedToday };
                var json = JsonConvert.SerializeObject(payload);
                File.WriteAllText(path, json);
                Debug.WriteLine($"Saved spells per day for {characterId} to {path}. Payload length: {json?.Length ?? 0}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save spells per day for {characterId}: {ex.Message}");
                // ignore silently otherwise
            }
        }

        public static (Dictionary<int,int> Max, Dictionary<int,int> Used) LoadSpellsPerDayDetails(Guid characterId)
        {
            try
            {
                var path = GetSpellsPerDayPath(characterId);
                if (!File.Exists(path)) return (new Dictionary<int,int>(), new Dictionary<int,int>());
                var json = File.ReadAllText(path);
                var jObj = Newtonsoft.Json.Linq.JObject.Parse(json);
                var maxToken = jObj["Max"];
                var usedToken = jObj["Used"];
                var max = maxToken != null ? maxToken.ToObject<Dictionary<int,int>>() ?? new Dictionary<int,int>() : new Dictionary<int,int>();
                var used = usedToken != null ? usedToken.ToObject<Dictionary<int,int>>() ?? new Dictionary<int,int>() : new Dictionary<int,int>();
                return (max, used);
            }
            catch
            {
                return (new Dictionary<int,int>(), new Dictionary<int,int>());
            }
        }

        private static string GetSessionPath() => Path.Combine(AppData, "currentSessionId.json");

        public static int LoadSessionId()
        {
            try
            {
                var path = GetSessionPath();
                if (!File.Exists(path)) return 0;
                var txt = File.ReadAllText(path);
                if (int.TryParse(txt, out var id)) return id;
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public static void SaveSessionId(int id)
        {
            try
            {
                File.WriteAllText(GetSessionPath(), id.ToString());
            }
            catch
            {
                // ignore
            }
        }
    }
}
