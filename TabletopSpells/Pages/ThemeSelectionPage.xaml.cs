using TabletopSpells.Services;
using TabletopSpells.Themes;
using TabletopSpells.ViewModels;

namespace TabletopSpells.Pages
{
    public partial class ThemeSelectionPage : ContentPage
    {
        private readonly ThemeSelectionViewModel _viewModel;

        public ThemeSelectionPage()
        {
            InitializeComponent();
            _viewModel = new ThemeSelectionViewModel();
            BindingContext = _viewModel;
        }

        private async void OnCustomSwatchTapped(object sender, TappedEventArgs e)
        {
            if (sender is not BindableObject bindable || bindable.BindingContext is not ThemeOptionViewModel theme)
                return;

            if (!theme.IsCustom)
                return;

            if (e.Parameter is not string colorKey)
                return;

            var initialColor = GetColor(theme.Theme, colorKey);
            var picker = new ThemeColorPickerViewModel(initialColor, color => ApplyCustomColor(theme, colorKey, color));
            await Navigation.PushAsync(new ThemeColorPickerPage(picker));
        }

        private void OnSelectThemeClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not ThemeOptionViewModel theme)
                return;

            _viewModel.ApplyTheme(theme);
            _viewModel.RefreshThemes();
            DisplayAlert("Theme Changed", $"{theme.Title} has been applied", "OK");
        }

        private static Color GetColor(ColorTheme theme, string colorKey)
        {
            return colorKey switch
            {
                "Primary" => theme.Primary,
                "SpellPrepared" => theme.SpellPrepared,
                "SpellDomain" => theme.SpellDomain,
                "SpellNonNative" => theme.SpellNonNative,
                _ => theme.Primary
            };
        }

        private static void ApplyCustomColor(ThemeOptionViewModel theme, string colorKey, Color color)
        {
            var updatedTheme = AppColors.CreateCustomTheme(theme.Theme.Name, theme.Theme);

            switch (colorKey)
            {
                case "Primary":
                    updatedTheme.Primary = color;
                    break;
                case "SpellPrepared":
                    updatedTheme.SpellPrepared = color;
                    break;
                case "SpellDomain":
                    updatedTheme.SpellDomain = color;
                    break;
                case "SpellNonNative":
                    updatedTheme.SpellNonNative = color;
                    break;
            }

            if (theme.Theme.Name == "CustomDark")
            {
                ThemeService.SetCustomDarkTheme(updatedTheme);
            }
            else
            {
                ThemeService.SetCustomLightTheme(updatedTheme);
            }

            theme.UpdateTheme(updatedTheme);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.RefreshThemes();
        }
    }
}
