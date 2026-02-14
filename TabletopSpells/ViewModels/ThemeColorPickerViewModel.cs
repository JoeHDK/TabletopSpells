using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TabletopSpells.ViewModels
{
    public class ThemeColorPickerViewModel : INotifyPropertyChanged
    {
        private int _red;
        private int _green;
        private int _blue;
        private readonly Action<Color> _onApply;

        public int Red
        {
            get => _red;
            set
            {
                if (_red == value) return;
                _red = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PreviewColor));
            }
        }

        public int Green
        {
            get => _green;
            set
            {
                if (_green == value) return;
                _green = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PreviewColor));
            }
        }

        public int Blue
        {
            get => _blue;
            set
            {
                if (_blue == value) return;
                _blue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PreviewColor));
            }
        }

        public Color PreviewColor => Color.FromRgb(Red, Green, Blue);

        public ThemeColorPickerViewModel(Color initialColor, Action<Color> onApply)
        {
            _onApply = onApply;
            Red = (int)(initialColor.Red * 255);
            Green = (int)(initialColor.Green * 255);
            Blue = (int)(initialColor.Blue * 255);
        }

        public void Apply()
        {
            _onApply(PreviewColor);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
