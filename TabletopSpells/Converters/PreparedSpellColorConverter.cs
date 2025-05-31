using System.Globalization;

namespace TabletopSpells.Converters
{
    public class PreparedSpellColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPrepared)
            {
                return isPrepared ? Colors.LightGreen : Colors.White;
            }

            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}