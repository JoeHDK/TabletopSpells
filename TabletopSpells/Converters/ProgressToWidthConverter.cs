using System.Globalization;
using Microsoft.Maui.Controls;

namespace TabletopSpells.Converters
{
    /// <summary>
    /// Converts a progress value (0.0 to 1.0) and parent width to the actual width for a progress bar.
    /// Used for cross-platform progress bar rendering that works consistently on Android and Windows.
    /// </summary>
    public class ProgressToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return 0;

            // values[0] = ProgressValue (double between 0.0 and 1.0)
            // values[1] = Parent Width (double)
            
            if (values[0] is double progress && values[1] is double parentWidth)
            {
                // Ensure progress is between 0 and 1
                progress = Math.Max(0, Math.Min(1, progress));
                
                // Calculate the width based on progress
                var width = parentWidth * progress;
                
                return Math.Max(0, width); // Ensure non-negative
            }

            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

