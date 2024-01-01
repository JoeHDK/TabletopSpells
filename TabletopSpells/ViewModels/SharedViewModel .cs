using System.Collections.ObjectModel;
using System.ComponentModel;

public class SharedViewModel : INotifyPropertyChanged
{
    private static SharedViewModel instance;
    public static SharedViewModel Instance => instance ?? (instance = new SharedViewModel());

    public ObservableCollection<Spell> Spells { get; set; } = new ObservableCollection<Spell>();

    public void AddSpell(Spell spell)
    {
        if (!Spells.Any(s => s.Name == spell.Name))
        {
            Spells.Add(spell);
            OnPropertyChanged(nameof(Spells));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
