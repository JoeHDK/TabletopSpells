using TabletopSpells.Themes;

namespace TabletopSpells.Helpers
{
    /// <summary>
    /// XAML Markup Extension to access theme colors.
    /// Usage: {helpers:ThemeColor PageBackground}
    /// </summary>
    [ContentProperty(nameof(ColorName))]
    public class ThemeColorExtension : IMarkupExtension
    {
        public string ColorName { get; set; } = string.Empty;

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(ColorName))
                return AppColors.Current.TextPrimary;

            var property = typeof(ColorTheme).GetProperty(ColorName);
            if (property != null && property.PropertyType == typeof(Color))
            {
                return property.GetValue(AppColors.Current) ?? AppColors.Current.TextPrimary;
            }

            return AppColors.Current.TextPrimary;
        }
    }
}

