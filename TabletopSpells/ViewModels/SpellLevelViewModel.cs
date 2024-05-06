using System.ComponentModel;
using System.Runtime.CompilerServices;

public class SpellLevelViewModel : INotifyPropertyChanged
{
    private int maxSpells;
    private int spellsUsed;

    public int Level
    {
        get; set;
    }

    public int MaxSpells
    {
        get => maxSpells;
        set
        {
            if (maxSpells != value)
            {
                maxSpells = value;
                OnPropertyChanged(nameof(MaxSpells));
                OnPropertyChanged(nameof(DetailText));
                OnPropertyChanged(nameof(ProgressValue)); // Update progress bar
            }
        }
    }

    public int SpellsUsed
    {
        get => spellsUsed;
        set
        {
            if (spellsUsed != value)
            {
                spellsUsed = value;
                OnPropertyChanged(nameof(SpellsUsed));
                OnPropertyChanged(nameof(DetailText));
                OnPropertyChanged(nameof(ProgressValue)); // Update progress bar
            }
        }
    }




    public float ProgressValue => MaxSpells > 0 ? (float)SpellsUsed / MaxSpells : 0;

    public string DisplayText
    {
        get; set;
    }

    public string DetailText => $"{SpellsUsed} / {MaxSpells}";

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
