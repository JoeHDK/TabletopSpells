using System;
using System.Collections.Generic;
using TabletopSpells.Helpers;
using Xunit;
using System.IO;
using System.Diagnostics;

namespace TabletopSpells.Tests
{
    public class SpellsPerDayPersistenceTests
    {
        [Fact]
        public void SaveAndLoadSpellsPerDay_WritesFileAndReadsBack()
        {
            var characterId = Guid.NewGuid();
            var max = new Dictionary<int,int> { [1] = 3, [2] = 2 };
            var used = new Dictionary<int,int> { [1] = 0, [2] = 0 };

            // Save
            LocalStorageHelper.SaveSpellsPerDayDetails(characterId, max, used);

            var path = LocalStorageHelper.GetSpellsPerDayPath(characterId);
            Debug.WriteLine($"Test: spellsPerDay path = {path}");

            Assert.True(File.Exists(path), "spellsPerDay file should exist after save");

            var fileLength = new FileInfo(path).Length;
            Assert.True(fileLength > 0, "spellsPerDay file should not be empty");

            // Load back
            var (loadedMax, loadedUsed) = LocalStorageHelper.LoadSpellsPerDayDetails(characterId);

            Assert.NotNull(loadedMax);
            Assert.NotNull(loadedUsed);
            Assert.Equal(max.Count, loadedMax.Count);
            Assert.Equal(used.Count, loadedUsed.Count);

            // Cleanup
            try { File.Delete(path); } catch { }
            try { var dir = Path.GetDirectoryName(path); if (Directory.Exists(dir) && Directory.GetFiles(dir).Length==0) Directory.Delete(dir,true); } catch { }
        }
    }
}

