using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using TabletopSpells.Services;
using TabletopSpells.Themes;

namespace TabletopSpells.ViewModels
{
    public class ThemeSelectionViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ThemeOptionViewModel> Themes { get; }
        private ThemeOptionViewModel? _selectedTheme;

        public ThemeOptionViewModel? SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme == value) return;
                _selectedTheme = value;
                OnPropertyChanged();
            }
        }

        public ThemeSelectionViewModel()
        {
            Themes = new ObservableCollection<ThemeOptionViewModel>
            {
                new ThemeOptionViewModel("Dark", "Default dark theme", AppColors.DarkTheme, false),
                new ThemeOptionViewModel("Light", "Default light theme", AppColors.LightTheme, false),
                new ThemeOptionViewModel("CustomDark", "Custom dark theme", AppColors.CustomDarkTheme, true),
                new ThemeOptionViewModel("CustomLight", "Custom light theme", AppColors.CustomLightTheme, true)
            };

            SelectedTheme = Themes.FirstOrDefault(t => t.Theme.Name == AppColors.Current.Name) ?? Themes.FirstOrDefault();
        }

        public void RefreshThemes()
        {
            var dark = Themes.FirstOrDefault(t => t.Theme.Name == "Dark");
            if (dark != null) dark.UpdateTheme(AppColors.DarkTheme);

            var light = Themes.FirstOrDefault(t => t.Theme.Name == "Light");
            if (light != null) light.UpdateTheme(AppColors.LightTheme);

            var customDark = Themes.FirstOrDefault(t => t.Theme.Name == "CustomDark");
            if (customDark != null) customDark.UpdateTheme(AppColors.CustomDarkTheme);

            var customLight = Themes.FirstOrDefault(t => t.Theme.Name == "CustomLight");
            if (customLight != null) customLight.UpdateTheme(AppColors.CustomLightTheme);

            SelectedTheme = Themes.FirstOrDefault(t => t.Theme.Name == AppColors.Current.Name) ?? SelectedTheme;
        }

        public void ApplyTheme(ThemeOptionViewModel theme)
        {
            switch (theme.Theme.Name)
            {
                case "Light":
                    ThemeService.SetThemeByName("Light");
                    break;
                case "CustomDark":
                    ThemeService.SetThemeByName("CustomDark");
                    break;
                case "CustomLight":
                    ThemeService.SetThemeByName("CustomLight");
                    break;
                default:
                    ThemeService.SetThemeByName("Dark");
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ThemeOptionViewModel : INotifyPropertyChanged
    {
        public string Title { get; }
        public string Description { get; }
        public bool IsCustom { get; }
        private ColorTheme _theme;

        public ColorTheme Theme
        {
            get => _theme;
            private set
            {
                _theme = value;
                OnPropertyChanged(nameof(Theme));
                OnPropertyChanged(nameof(PageBackground));
                OnPropertyChanged(nameof(Primary));
                OnPropertyChanged(nameof(SpellPrepared));
                OnPropertyChanged(nameof(SpellDomain));
                OnPropertyChanged(nameof(SpellNonNative));
                OnPropertyChanged(nameof(TextPrimary));
            }
        }

        public Color PageBackground => Theme.PageBackground;
        public Color Primary => Theme.Primary;
        public Color SpellPrepared => Theme.SpellPrepared;
        public Color SpellDomain => Theme.SpellDomain;
        public Color SpellNonNative => Theme.SpellNonNative;
        public Color TextPrimary => Theme.TextPrimary;

        public ThemeOptionViewModel(string title, string description, ColorTheme theme, bool isCustom)
        {
            Title = title == "CustomDark" ? "Custom Dark Theme" :
                title == "CustomLight" ? "Custom Light Theme" :
                title + " Theme";
            Description = description;
            IsCustom = isCustom;
            _theme = theme;
        }

        public void UpdateTheme(ColorTheme theme)
        {
            Theme = theme;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

