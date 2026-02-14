using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace TabletopSpells.ViewModels
{
    public class ThemeColorPickerViewModel : INotifyPropertyChanged
    {
        private Color _selectedColor;
        private readonly Action<Color> _onApply;

        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor == value) return;
                _selectedColor = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PreviewColor));
            }
        }

        public Color PreviewColor => SelectedColor;

        public ThemeColorPickerViewModel(Color initialColor, Action<Color> onApply)
        {
            _onApply = onApply;
            _selectedColor = initialColor;
        }

        public void Apply()
        {
            _onApply(SelectedColor);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
