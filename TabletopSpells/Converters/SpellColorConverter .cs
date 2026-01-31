using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TabletopSpells.Converters
{
    public class SpellColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if the spell is prepared to provide visual cue
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
