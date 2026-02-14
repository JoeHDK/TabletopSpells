using Microsoft.Maui.Graphics;

namespace TabletopSpells.Themes
{
    /// <summary>
    /// Centralized color management for the application.
    /// Supports dark mode, light mode, and character-specific custom themes.
    /// </summary>
    public static class AppColors
    {
        // Current active theme
        private static ColorTheme _currentTheme = DarkTheme;
        private static readonly Dictionary<Guid, ColorTheme> _characterThemes = new();
        private static ColorTheme _customDarkTheme = CreateCustomTheme("CustomDark", DarkTheme);
        private static ColorTheme _customLightTheme = CreateCustomTheme("CustomLight", LightTheme);

        /// <summary>
        /// Gets the current active color theme.
        /// </summary>
        public static ColorTheme Current => _currentTheme;

        public static ColorTheme CustomDarkTheme
        {
            get => _customDarkTheme;
            set => _customDarkTheme = value;
        }

        public static ColorTheme CustomLightTheme
        {
            get => _customLightTheme;
            set => _customLightTheme = value;
        }

        /// <summary>
        /// Predefined Dark Theme (default)
        /// </summary>
        public static ColorTheme DarkTheme => new ColorTheme
        {
            Name = "Dark",
            
            // Background colors
            PageBackground = Color.FromArgb("#000000"),
            CardBackground = Color.FromArgb("#1A1A1A"),
            SurfaceBackground = Color.FromArgb("#2A2A2A"),
            
            // Primary colors
            Primary = Color.FromArgb("#512BD4"),
            PrimaryDark = Color.FromArgb("#3A1E9E"),
            PrimaryLight = Color.FromArgb("#7045E8"),
            
            // Text colors
            TextPrimary = Color.FromArgb("#FFFFFF"),
            TextSecondary = Color.FromArgb("#B0B0B0"),
            TextTertiary = Color.FromArgb("#808080"),
            TextDisabled = Color.FromArgb("#606060"),
            
            // Spell-specific colors
            SpellPrepared = Color.FromArgb("#90EE90"), // LightGreen
            SpellNative = Color.FromArgb("#FFFFFF"),   // White
            SpellNonNative = Color.FromArgb("#808080"), // Gray
            SpellUnavailable = Color.FromArgb("#606060"), // DarkGray
            SpellDomain = Color.FromArgb("#87CEEB"),   // SkyBlue - for domain/always-prepared spells
            
            // Status colors
            Success = Color.FromArgb("#00FF00"),      // Green
            Warning = Color.FromArgb("#FFA500"),      // Orange
            Error = Color.FromArgb("#FF0000"),        // Red
            Info = Color.FromArgb("#0000FF"),         // Blue
            
            // UI element colors
            Divider = Color.FromArgb("#808080"),
            Border = Color.FromArgb("#404040"),
            Shadow = Color.FromArgb("#000000"),
            Overlay = Color.FromArgb("#00000099"),
            
            // Button colors
            ButtonBackground = Color.FromArgb("#008000"), // Green
            ButtonText = Color.FromArgb("#FFFFFF"),
            ButtonDisabled = Color.FromArgb("#404040"),
            
            // Progress/Stats colors
            HealthBar = Color.FromArgb("#FF0000"),    // Red
            ManaBar = Color.FromArgb("#0000FF"),      // Blue
            StaminaBar = Color.FromArgb("#00FF00"),   // Green
            ExperienceBar = Color.FromArgb("#800080"), // Purple
            
            // Checkbox/Toggle colors
            CheckboxActive = Color.FromArgb("#800080"), // Purple
            CheckboxInactive = Color.FromArgb("#404040"),
            
            // Transparent
            Transparent = Colors.Transparent
        };

        /// <summary>
        /// Predefined Light Theme
        /// </summary>
        public static ColorTheme LightTheme => new ColorTheme
        {
            Name = "Light",
            
            // Background colors
            PageBackground = Color.FromArgb("#FFFFFF"),
            CardBackground = Color.FromArgb("#F5F5F5"),
            SurfaceBackground = Color.FromArgb("#EEEEEE"),
            
            // Primary colors
            Primary = Color.FromArgb("#512BD4"),
            PrimaryDark = Color.FromArgb("#3A1E9E"),
            PrimaryLight = Color.FromArgb("#7045E8"),
            
            // Text colors
            TextPrimary = Color.FromArgb("#000000"),
            TextSecondary = Color.FromArgb("#404040"),
            TextTertiary = Color.FromArgb("#606060"),
            TextDisabled = Color.FromArgb("#A0A0A0"),
            
            // Spell-specific colors
            SpellPrepared = Color.FromArgb("#228B22"), // ForestGreen (darker for light bg)
            SpellNative = Color.FromArgb("#000000"),   // Black
            SpellNonNative = Color.FromArgb("#606060"), // DarkGray
            SpellUnavailable = Color.FromArgb("#A0A0A0"), // LightGray
            SpellDomain = Color.FromArgb("#4682B4"),   // SteelBlue - for domain/always-prepared spells
            
            // Status colors
            Success = Color.FromArgb("#008000"),      // Green
            Warning = Color.FromArgb("#FF8C00"),      // DarkOrange
            Error = Color.FromArgb("#DC143C"),        // Crimson
            Info = Color.FromArgb("#1E90FF"),         // DodgerBlue
            
            // UI element colors
            Divider = Color.FromArgb("#D0D0D0"),
            Border = Color.FromArgb("#C0C0C0"),
            Shadow = Color.FromArgb("#00000040"),
            Overlay = Color.FromArgb("#FFFFFF99"),
            
            // Button colors
            ButtonBackground = Color.FromArgb("#008000"), // Green
            ButtonText = Color.FromArgb("#FFFFFF"),
            ButtonDisabled = Color.FromArgb("#D0D0D0"),
            
            // Progress/Stats colors
            HealthBar = Color.FromArgb("#DC143C"),    // Crimson
            ManaBar = Color.FromArgb("#1E90FF"),      // DodgerBlue
            StaminaBar = Color.FromArgb("#32CD32"),   // LimeGreen
            ExperienceBar = Color.FromArgb("#9370DB"), // MediumPurple
            
            // Checkbox/Toggle colors
            CheckboxActive = Color.FromArgb("#800080"), // Purple
            CheckboxInactive = Color.FromArgb("#D0D0D0"),
            
            // Transparent
            Transparent = Colors.Transparent
        };

        /// <summary>
        /// Sets the application theme.
        /// </summary>
        public static void SetTheme(ColorTheme theme)
        {
            _currentTheme = theme;
            OnThemeChanged?.Invoke();
        }

        /// <summary>
        /// Sets a custom theme for a specific character.
        /// </summary>
        public static void SetCharacterTheme(Guid characterId, ColorTheme theme)
        {
            _characterThemes[characterId] = theme;
        }

        /// <summary>
        /// Gets a character-specific theme, or returns the current app theme if none is set.
        /// </summary>
        public static ColorTheme GetCharacterTheme(Guid characterId)
        {
            return _characterThemes.TryGetValue(characterId, out var theme) ? theme : Current;
        }

        /// <summary>
        /// Removes a character-specific theme.
        /// </summary>
        public static void ClearCharacterTheme(Guid characterId)
        {
            _characterThemes.Remove(characterId);
        }

        /// <summary>
        /// Switches between dark and light themes.
        /// </summary>
        public static void ToggleTheme()
        {
            SetTheme(_currentTheme.Name == "Dark" ? LightTheme : DarkTheme);
        }

        /// <summary>
        /// Event raised when the theme changes.
        /// </summary>
        public static event Action? OnThemeChanged;

        /// <summary>
        /// Creates a custom theme with the specified name and colors.
        /// </summary>
        public static ColorTheme CreateCustomTheme(string name, ColorTheme? baseTheme = null)
        {
            var theme = baseTheme ?? DarkTheme;
            return new ColorTheme
            {
                Name = name,
                PageBackground = theme.PageBackground,
                CardBackground = theme.CardBackground,
                SurfaceBackground = theme.SurfaceBackground,
                Primary = theme.Primary,
                PrimaryDark = theme.PrimaryDark,
                PrimaryLight = theme.PrimaryLight,
                TextPrimary = theme.TextPrimary,
                TextSecondary = theme.TextSecondary,
                TextTertiary = theme.TextTertiary,
                TextDisabled = theme.TextDisabled,
                SpellPrepared = theme.SpellPrepared,
                SpellNative = theme.SpellNative,
                SpellNonNative = theme.SpellNonNative,
                SpellUnavailable = theme.SpellUnavailable,
                SpellDomain = theme.SpellDomain,
                Success = theme.Success,
                Warning = theme.Warning,
                Error = theme.Error,
                Info = theme.Info,
                Divider = theme.Divider,
                Border = theme.Border,
                Shadow = theme.Shadow,
                Overlay = theme.Overlay,
                ButtonBackground = theme.ButtonBackground,
                ButtonText = theme.ButtonText,
                ButtonDisabled = theme.ButtonDisabled,
                HealthBar = theme.HealthBar,
                ManaBar = theme.ManaBar,
                StaminaBar = theme.StaminaBar,
                ExperienceBar = theme.ExperienceBar,
                CheckboxActive = theme.CheckboxActive,
                CheckboxInactive = theme.CheckboxInactive,
                Transparent = theme.Transparent
            };
        }
    }

    /// <summary>
    /// Represents a complete color theme for the application.
    /// </summary>
    public class ColorTheme
    {
        public string Name { get; set; } = "Default";
        
        // Background colors
        public Color PageBackground { get; set; } = Colors.Black;
        public Color CardBackground { get; set; } = Colors.DarkGray;
        public Color SurfaceBackground { get; set; } = Colors.Gray;
        
        // Primary colors
        public Color Primary { get; set; } = Colors.Purple;
        public Color PrimaryDark { get; set; } = Colors.DarkMagenta;
        public Color PrimaryLight { get; set; } = Colors.Violet;
        
        // Text colors
        public Color TextPrimary { get; set; } = Colors.White;
        public Color TextSecondary { get; set; } = Colors.LightGray;
        public Color TextTertiary { get; set; } = Colors.Gray;
        public Color TextDisabled { get; set; } = Colors.DarkGray;
        
        // Spell-specific colors
        public Color SpellPrepared { get; set; } = Colors.LightGreen;
        public Color SpellNative { get; set; } = Colors.White;
        public Color SpellNonNative { get; set; } = Colors.Gray;
        public Color SpellUnavailable { get; set; } = Colors.DarkGray;
        public Color SpellDomain { get; set; } = Colors.LightBlue; // For domain/always-prepared spells
        
        // Status colors
        public Color Success { get; set; } = Colors.Green;
        public Color Warning { get; set; } = Colors.Orange;
        public Color Error { get; set; } = Colors.Red;
        public Color Info { get; set; } = Colors.Blue;
        
        // UI element colors
        public Color Divider { get; set; } = Colors.Gray;
        public Color Border { get; set; } = Colors.DarkGray;
        public Color Shadow { get; set; } = Colors.Black;
        public Color Overlay { get; set; } = Color.FromArgb("#00000099");
        
        // Button colors
        public Color ButtonBackground { get; set; } = Colors.Green;
        public Color ButtonText { get; set; } = Colors.White;
        public Color ButtonDisabled { get; set; } = Colors.DarkGray;
        
        // Progress/Stats colors
        public Color HealthBar { get; set; } = Colors.Red;
        public Color ManaBar { get; set; } = Colors.Blue;
        public Color StaminaBar { get; set; } = Colors.Green;
        public Color ExperienceBar { get; set; } = Colors.Purple;
        
        // Checkbox/Toggle colors
        public Color CheckboxActive { get; set; } = Colors.Purple;
        public Color CheckboxInactive { get; set; } = Colors.DarkGray;
        
        // Transparent
        public Color Transparent { get; set; } = Colors.Transparent;
    }
}

