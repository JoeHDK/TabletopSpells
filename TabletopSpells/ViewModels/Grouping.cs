using System.Collections.ObjectModel;

public class Grouping<TKey, TElement> : ObservableCollection<TElement>
{
    public TKey Key
    {
        get; private set;
    }

    public Grouping(TKey key, IEnumerable<TElement> items)
    {
        Key = key;
        foreach (var item in items)
            this.Items.Add(item);
    }
}
