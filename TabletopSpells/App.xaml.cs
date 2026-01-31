using TabletopSpells.Services;

namespace TabletopSpells
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Initialize theme preferences
            ThemeService.LoadThemePreference();
            _ = ThemeService.LoadCharacterThemesAsync();

            MainPage = new AppShell();
        }
    }
}
