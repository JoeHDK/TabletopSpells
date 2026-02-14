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
        private const string CustomDarkThemeFileName = "custom_dark_theme.json";
        private const string CustomLightThemeFileName = "custom_light_theme.json";

        /// <summary>
        /// Loads the saved theme preference and applies it.
        /// </summary>
        public static void LoadThemePreference()
        {
            try
            {
                LoadCustomThemes();
                var themeName = Preferences.Default.Get(ThemePreferenceKey, "Dark");
                ApplyThemeByName(themeName);
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

        /// <summary>
        /// Sets a custom dark theme.
        /// </summary>
        public static void SetCustomDarkTheme(ColorTheme theme)
        {
            theme.Name = "CustomDark";
            AppColors.CustomDarkTheme = theme;
            AppColors.SetTheme(theme);
            _ = SaveCustomThemeAsync(CustomDarkThemeFileName, theme);
            SaveThemePreference();
        }

        /// <summary>
        /// Sets a custom light theme.
        /// </summary>
        public static void SetCustomLightTheme(ColorTheme theme)
        {
            theme.Name = "CustomLight";
            AppColors.CustomLightTheme = theme;
            AppColors.SetTheme(theme);
            _ = SaveCustomThemeAsync(CustomLightThemeFileName, theme);
            SaveThemePreference();
        }

        /// <summary>
        /// Switches to a theme by name.
        /// </summary>
        public static void SetThemeByName(string themeName)
        {
            ApplyThemeByName(themeName);
            SaveThemePreference();
        }

        private static void ApplyThemeByName(string themeName)
        {
            switch (themeName)
            {
                case "Light":
                    AppColors.SetTheme(AppColors.LightTheme);
                    break;
                case "CustomDark":
                    AppColors.SetTheme(AppColors.CustomDarkTheme);
                    break;
                case "CustomLight":
                    AppColors.SetTheme(AppColors.CustomLightTheme);
                    break;
                default:
                    AppColors.SetTheme(AppColors.DarkTheme);
                    break;
            }
        }

        private static void LoadCustomThemes()
        {
            AppColors.CustomDarkTheme = LoadCustomTheme(CustomDarkThemeFileName, AppColors.DarkTheme, "CustomDark");
            AppColors.CustomLightTheme = LoadCustomTheme(CustomLightThemeFileName, AppColors.LightTheme, "CustomLight");
        }

        private static ColorTheme LoadCustomTheme(string fileName, ColorTheme baseTheme, string name)
        {
            try
            {
                var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                if (!File.Exists(filePath))
                {
                    return AppColors.CreateCustomTheme(name, baseTheme);
                }

                var json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<ThemeData>(json);
                if (data == null)
                {
                    return AppColors.CreateCustomTheme(name, baseTheme);
                }

                data.Name = name;
                return ThemeDataToColorTheme(data, baseTheme);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading custom theme: {ex.Message}");
                return AppColors.CreateCustomTheme(name, baseTheme);
            }
        }

        private static async Task SaveCustomThemeAsync(string fileName, ColorTheme theme)
        {
            try
            {
                var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                var json = JsonSerializer.Serialize(ColorThemeToThemeData(theme), new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving custom theme: {ex.Message}");
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
                SpellNonNative = theme.SpellNonNative.ToHex(),
                SpellDomain = theme.SpellDomain.ToHex(),
                Primary = theme.Primary.ToHex(),
                Success = theme.Success.ToHex(),
                Error = theme.Error.ToHex(),
                ButtonBackground = theme.ButtonBackground.ToHex()
            };
        }

        private static ColorTheme ThemeDataToColorTheme(ThemeData data)
        {
            var baseTheme = data.Name == "Light" ? AppColors.LightTheme : AppColors.DarkTheme;
            return ThemeDataToColorTheme(data, baseTheme);
        }

        private static ColorTheme ThemeDataToColorTheme(ThemeData data, ColorTheme baseTheme)
        {
            var theme = AppColors.CreateCustomTheme(data.Name ?? "Custom", baseTheme);

            if (!string.IsNullOrEmpty(data.PageBackground))
                theme.PageBackground = Color.FromArgb(data.PageBackground);
            if (!string.IsNullOrEmpty(data.TextPrimary))
                theme.TextPrimary = Color.FromArgb(data.TextPrimary);
            if (!string.IsNullOrEmpty(data.SpellPrepared))
                theme.SpellPrepared = Color.FromArgb(data.SpellPrepared);
            if (!string.IsNullOrEmpty(data.SpellNative))
                theme.SpellNative = Color.FromArgb(data.SpellNative);
            if (!string.IsNullOrEmpty(data.SpellNonNative))
                theme.SpellNonNative = Color.FromArgb(data.SpellNonNative);
            if (!string.IsNullOrEmpty(data.SpellDomain))
                theme.SpellDomain = Color.FromArgb(data.SpellDomain);
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
            public string? SpellNonNative { get; set; }
            public string? SpellDomain { get; set; }
            public string? Primary { get; set; }
            public string? Success { get; set; }
            public string? Error { get; set; }
            public string? ButtonBackground { get; set; }
        }
    }
}

