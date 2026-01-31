using System.Globalization;

namespace TabletopSpells.Converters
{
    public class NonClassSpellConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = (int)value;
            return level == -1 ? "Non class spells" : $"Level {level}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
