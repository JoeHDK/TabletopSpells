using System.Globalization;

namespace TabletopSpells.Converters
{
    public class NonClassSpellConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int level = (int)value;
            if (level == -1)
                return "Non class spells";
            else
                return $"Level {level}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
