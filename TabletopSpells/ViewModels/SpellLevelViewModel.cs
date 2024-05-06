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
                OnPropertyChanged(nameof(DetailText));  // Notify that DetailText needs to update
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
                OnPropertyChanged(nameof(DetailText));  // Notify that DetailText needs to update
            }
        }
    }

    public string DisplayText
    {
        get; set;
    }

    // ReadOnly computed property
    public string DetailText => $"Used: {SpellsUsed} / Max: {MaxSpells}";

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
