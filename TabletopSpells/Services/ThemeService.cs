using System.Text.Json;
using TabletopSpells.Themes;

namespace TabletopSpells.Services
{
    /// <summary>
    /// Service for managing and persisting theme preferences.
    /// </summary>
    public class ThemeService
    {
        private const string ThemePreferenceKey = "app_theme";
        private const string CharacterThemesFileName = "character_themes.json";

        /// <summary>
        /// Loads the saved theme preference and applies it.
        /// </summary>
        public static void LoadThemePreference()
        {
            try
            {
                var themeName = Preferences.Default.Get(ThemePreferenceKey, "Dark");
                var theme = themeName == "Light" ? AppColors.LightTheme : AppColors.DarkTheme;
                AppColors.SetTheme(theme);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading theme preference: {ex.Message}");
                AppColors.SetTheme(AppColors.DarkTheme);
            }
        }

        /// <summary>
        /// Saves the current theme preference.
        /// </summary>
        public static void SaveThemePreference()
        {
            try
            {
                Preferences.Default.Set(ThemePreferenceKey, AppColors.Current.Name);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving theme preference: {ex.Message}");
            }
        }

        /// <summary>
        /// Switches to dark theme.
        /// </summary>
        public static void SetDarkTheme()
        {
            AppColors.SetTheme(AppColors.DarkTheme);
            SaveThemePreference();
        }

        /// <summary>
        /// Switches to light theme.
        /// </summary>
        public static void SetLightTheme()
        {
            AppColors.SetTheme(AppColors.LightTheme);
            SaveThemePreference();
        }

        /// <summary>
        /// Toggles between light and dark themes.
        /// </summary>
        public static void ToggleTheme()
        {
            AppColors.ToggleTheme();
            SaveThemePreference();
        }

        /// <summary>
        /// Loads character-specific themes from storage.
        /// </summary>
        public static async Task LoadCharacterThemesAsync()
        {
            try
            {
                var filePath = Path.Combine(FileSystem.AppDataDirectory, CharacterThemesFileName);
                if (!File.Exists(filePath))
                    return;

                var json = await File.ReadAllTextAsync(filePath);
                var themes = JsonSerializer.Deserialize<Dictionary<string, ThemeData>>(json);
                
                if (themes != null)
                {
                    foreach (var kvp in themes)
                    {
                        if (Guid.TryParse(kvp.Key, out var characterId))
                        {
                            var theme = ThemeDataToColorTheme(kvp.Value);
                            AppColors.SetCharacterTheme(characterId, theme);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading character themes: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves character-specific themes to storage.
        /// </summary>
        public static async Task SaveCharacterThemeAsync(Guid characterId, ColorTheme theme)
        {
            try
            {
                var filePath = Path.Combine(FileSystem.AppDataDirectory, CharacterThemesFileName);
                
                Dictionary<string, ThemeData> themes;
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    themes = JsonSerializer.Deserialize<Dictionary<string, ThemeData>>(json) 
                             ?? new Dictionary<string, ThemeData>();
                }
                else
                {
                    themes = new Dictionary<string, ThemeData>();
                }

                themes[characterId.ToString()] = ColorThemeToThemeData(theme);
                
                var newJson = JsonSerializer.Serialize(themes, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, newJson);
                
                AppColors.SetCharacterTheme(characterId, theme);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving character theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a character-specific theme.
        /// </summary>
        public static async Task ClearCharacterThemeAsync(Guid characterId)
        {
            try
            {
                AppColors.ClearCharacterTheme(characterId);
                
                var filePath = Path.Combine(FileSystem.AppDataDirectory, CharacterThemesFileName);
                if (!File.Exists(filePath))
                    return;

                var json = await File.ReadAllTextAsync(filePath);
                var themes = JsonSerializer.Deserialize<Dictionary<string, ThemeData>>(json);
                
                if (themes != null && themes.Remove(characterId.ToString()))
                {
                    var newJson = JsonSerializer.Serialize(themes, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(filePath, newJson);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing character theme: {ex.Message}");
            }
        }

        private static ThemeData ColorThemeToThemeData(ColorTheme theme)
        {
            return new ThemeData
            {
                Name = theme.Name,
                PageBackground = theme.PageBackground.ToHex(),
                TextPrimary = theme.TextPrimary.ToHex(),
                SpellPrepared = theme.SpellPrepared.ToHex(),
                SpellNative = theme.SpellNative.ToHex(),
                Primary = theme.Primary.ToHex(),
                Success = theme.Success.ToHex(),
                Error = theme.Error.ToHex(),
                ButtonBackground = theme.ButtonBackground.ToHex()
            };
        }

        private static ColorTheme ThemeDataToColorTheme(ThemeData data)
        {
            var baseTheme = data.Name == "Light" ? AppColors.LightTheme : AppColors.DarkTheme;
            var theme = AppColors.CreateCustomTheme(data.Name ?? "Custom", baseTheme);
            
            if (!string.IsNullOrEmpty(data.PageBackground))
                theme.PageBackground = Color.FromArgb(data.PageBackground);
            if (!string.IsNullOrEmpty(data.TextPrimary))
                theme.TextPrimary = Color.FromArgb(data.TextPrimary);
            if (!string.IsNullOrEmpty(data.SpellPrepared))
                theme.SpellPrepared = Color.FromArgb(data.SpellPrepared);
            if (!string.IsNullOrEmpty(data.SpellNative))
                theme.SpellNative = Color.FromArgb(data.SpellNative);
            if (!string.IsNullOrEmpty(data.Primary))
                theme.Primary = Color.FromArgb(data.Primary);
            if (!string.IsNullOrEmpty(data.Success))
                theme.Success = Color.FromArgb(data.Success);
            if (!string.IsNullOrEmpty(data.Error))
                theme.Error = Color.FromArgb(data.Error);
            if (!string.IsNullOrEmpty(data.ButtonBackground))
                theme.ButtonBackground = Color.FromArgb(data.ButtonBackground);
            
            return theme;
        }

        private class ThemeData
        {
            public string? Name { get; set; }
            public string? PageBackground { get; set; }
            public string? TextPrimary { get; set; }
            public string? SpellPrepared { get; set; }
            public string? SpellNative { get; set; }
            public string? Primary { get; set; }
            public string? Success { get; set; }
            public string? Error { get; set; }
            public string? ButtonBackground { get; set; }
        }
    }
}

