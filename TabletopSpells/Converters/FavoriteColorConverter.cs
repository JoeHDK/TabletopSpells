using System.Globalization;
using Microsoft.Maui.Graphics;

namespace TabletopSpells.Converters
{
    public class FavoriteColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isFav)
            {
                return isFav ? Colors.Goldenrod : Colors.Gray;
            }
            return Colors.Gray;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

