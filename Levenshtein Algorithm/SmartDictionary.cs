using Levenshtein_Algorithm.Extensions;

namespace Levenshtein_Algorithm.SmartDictionary;

public class SmartDictionary<T>
{
    private List<T> _allItems;
    private readonly Func<T, string> _keyAccessor;

    public SmartDictionary(Func<T, string> keyAccessor, IEnumerable<T> allItems)
    {
        ArgumentNullException.ThrowIfNull(allItems, nameof(allItems));
        ArgumentNullException.ThrowIfNull(keyAccessor, nameof(keyAccessor));

        _allItems = allItems.ToList();
        _keyAccessor = keyAccessor;
    }

    protected Rater[] Rate(string searchString)
    {
        Rater[] result = new Rater[_allItems.Count];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = RateItem(searchString, new Rater { Item = _allItems[i] });
        }

        return result;
    }

    protected Rater RateItem(string search, Rater rater)
    {
        var toSearch = search.ToLower();
        var destination = _keyAccessor(rater.Item!).ToLower();
        bool firstMatch = true;

        for (var j = 0; j < toSearch.Length; j++)
        {
            if (string.IsNullOrEmpty(destination))
            {
                return rater;
            }

            var currChar = toSearch[j];
            var index = destination.IndexOf(currChar);
            if (index == -1)
            {
                continue;
            }

            rater.FoundChars++;

            if (firstMatch)
            {
                rater.Penalty += index;
                firstMatch = false;
            }
            else
            {
                rater.Penalty += index * 1000;
            }

            destination =
                index + 1 < destination.Length ? destination[(index + 1)..] : string.Empty;
        }

        return rater;
    }

    public void Replace(IEnumerable<T> newItems)
    {
        ArgumentNullException.ThrowIfNull(newItems, nameof(newItems));

        _allItems = newItems.ToList();
    }

    public void Add(T newItem)
    {
        ArgumentNullException.ThrowIfNull(newItem, nameof(newItem));

        _allItems.Add(newItem);
    }

    public IEnumerable<T> Search(string search, int maxItems)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(search, nameof(search));
        ArgumentExceptionExtensions.ThrowIfInvalidMaxItems(maxItems, nameof(maxItems));

        var ratedApproximations = Rate(search);
        Array.Sort(
            ratedApproximations,
            new Comparison<Rater>(
                (x, y) =>
                {
                    if (x.FoundChars > y.FoundChars)
                    {
                        return -1;
                    }
                    if (x.FoundChars < y.FoundChars)
                    {
                        return 1;
                    }

                    return x.Penalty.CompareTo(y.Penalty);
                }
            )
        );

        return ratedApproximations.Take(maxItems).Select(m => m.Item);
    }

    protected class Rater
    {
        public T? Item;
        public double Penalty = 0;
        public int FoundChars = 0;
    }
}
