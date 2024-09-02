using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TabletopSpells.Converters
{
    public class SpellColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Assume value is a boolean indicating whether the spell is native to the character's class
            if (value is bool isNativeSpell)
            {
                return isNativeSpell ? Colors.White : Colors.DarkGray;
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
