using System.Globalization;
using Microsoft.Maui.Graphics;

namespace TabletopSpells.Converters
{
    public class FavoriteGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFav)
            {
                return isFav ? "★" : "☆";
            }
            return "☆";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

