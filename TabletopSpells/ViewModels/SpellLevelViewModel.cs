using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TabletopSpells.ViewModels
{
    public class SpellLevelViewModel : INotifyPropertyChanged
    {
        private int _maxSpells;
        private int _spellsUsed;

        public int Level
        {
            get; set;
        }

        public int MaxSpells
        {
            get => _maxSpells;
            set
            {
                if (_maxSpells != value)
                {
                    _maxSpells = value;
                    OnPropertyChanged(nameof(MaxSpells));
                    OnPropertyChanged(nameof(DetailText));
                    OnPropertyChanged(nameof(ProgressValue));
                }
            }
        }

        public int SpellsUsed
        {
            get => _spellsUsed;
            set
            {
                if (_spellsUsed != value)
                {
                    _spellsUsed = value;
                    OnPropertyChanged(nameof(SpellsUsed));
                    OnPropertyChanged(nameof(DetailText));
                    OnPropertyChanged(nameof(ProgressValue));
                }
            }
        }

        public float ProgressValue => MaxSpells > 0 ? (float)SpellsUsed / MaxSpells : 0;

        public string DisplayText { get; set; } = string.Empty;

        public string DetailText => $"{SpellsUsed} / {MaxSpells}";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }
    }
}
