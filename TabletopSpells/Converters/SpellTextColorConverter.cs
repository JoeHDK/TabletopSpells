using System.Globalization;

namespace TabletopSpells.Converters
{
    /// <summary>
    /// Multi-value converter that determines spell text color based on both IsNativeSpell and IsPrepared.
    /// </summary>
    public class SpellTextColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
            {
                System.Diagnostics.Debug.WriteLine($"SpellTextColorConverter: Not enough values ({values.Length})");
                return Colors.Gray;
            }

            // First value: IsNativeSpell (bool)
            // Second value: IsPrepared (bool)
            
            bool isNativeSpell = values[0] is bool native && native;
            bool isPrepared = values[1] is bool prepared && prepared;

            System.Diagnostics.Debug.WriteLine($"SpellTextColorConverter: IsNativeSpell={isNativeSpell}, IsPrepared={isPrepared}");

            // Priority: If prepared, show as LightGreen
            if (isPrepared)
            {
                System.Diagnostics.Debug.WriteLine($"  -> Returning LightGreen");
                return Colors.LightGreen;
            }

            // If not prepared but is native spell, show as White/LightGray
            if (isNativeSpell)
            {
                System.Diagnostics.Debug.WriteLine($"  -> Returning White");
                return Colors.White;
            }

            // If not native spell and not prepared, show as Gray (dimmed)
            System.Diagnostics.Debug.WriteLine($"  -> Returning Gray");
            return Colors.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

