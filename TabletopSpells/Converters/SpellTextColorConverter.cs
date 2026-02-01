using System.Globalization;
using TabletopSpells.Themes;

namespace TabletopSpells.Converters
{
    /// <summary>
    /// Multi-value converter that determines spell text color based on IsNativeSpell, IsPrepared, and IsAlwaysPrepared.
    /// </summary>
    public class SpellTextColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
            {
                System.Diagnostics.Debug.WriteLine($"SpellTextColorConverter: Not enough values ({values.Length})");
                return AppColors.Current.SpellUnavailable;
            }

            // First value: IsNativeSpell (bool)
            // Second value: IsPrepared (bool)
            // Third value (optional): IsAlwaysPrepared (bool) - domain spells
            
            bool isNativeSpell = values[0] is bool native && native;
            bool isPrepared = values[1] is bool prepared && prepared;
            bool isAlwaysPrepared = values.Length > 2 && values[2] is bool always && always;

            System.Diagnostics.Debug.WriteLine($"SpellTextColorConverter: IsNativeSpell={isNativeSpell}, IsPrepared={isPrepared}, IsAlwaysPrepared={isAlwaysPrepared}");

            // Priority 1: Domain spells (always prepared) - show in blue
            if (isAlwaysPrepared)
            {
                System.Diagnostics.Debug.WriteLine($"  -> Returning SpellDomain color (blue)");
                return AppColors.Current.SpellDomain;
            }

            // Priority 2: If prepared, show as prepared color (green)
            if (isPrepared)
            {
                System.Diagnostics.Debug.WriteLine($"  -> Returning SpellPrepared color");
                return AppColors.Current.SpellPrepared;
            }

            // Priority 3: If not prepared but is native spell, show as native color
            if (isNativeSpell)
            {
                System.Diagnostics.Debug.WriteLine($"  -> Returning SpellNative color");
                return AppColors.Current.SpellNative;
            }

            // If not native spell and not prepared, show as non-native (dimmed)
            System.Diagnostics.Debug.WriteLine($"  -> Returning SpellNonNative color");
            return AppColors.Current.SpellNonNative;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

