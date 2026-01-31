using TabletopSpells.Services;
using TabletopSpells.Themes;

namespace TabletopSpells.Pages
{
    public partial class ThemeSelectionPage : ContentPage
    {
        public ThemeSelectionPage()
        {
            InitializeComponent();
            UpdateCurrentThemeIndicator();
        }

        private void OnDarkThemeClicked(object sender, EventArgs e)
        {
            ThemeService.SetDarkTheme();
            UpdateCurrentThemeIndicator();
            DisplayAlert("Theme Changed", "Dark theme has been applied", "OK");
        }

        private void OnLightThemeClicked(object sender, EventArgs e)
        {
            ThemeService.SetLightTheme();
            UpdateCurrentThemeIndicator();
            DisplayAlert("Theme Changed", "Light theme has been applied", "OK");
        }

        private void UpdateCurrentThemeIndicator()
        {
            var currentTheme = AppColors.Current.Name;
            DarkThemeButton.BackgroundColor = currentTheme == "Dark" 
                ? AppColors.Current.Primary 
                : AppColors.Current.ButtonDisabled;
            LightThemeButton.BackgroundColor = currentTheme == "Light" 
                ? AppColors.Current.Primary 
                : AppColors.Current.ButtonDisabled;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateCurrentThemeIndicator();
        }
    }
}

