using Vulpes.Electrum.Domain.Monads;

namespace Vulpes.Electrum.Domain.Extensions;
public static class PerhapsExtensions
{
    public static Perhaps<TItem> FirstOrPerhaps<TItem>(this IEnumerable<TItem> items)
    {
        using (var enumerator = items.GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                return Perhaps<TItem>.ToPerhaps(enumerator.Current);
            }
        }

        return Perhaps<TItem>.Empty;
    }

    public static Perhaps<TItem> FirstOrPerhaps<TItem>(this IEnumerable<TItem> items, Func<TItem, bool> predicate)
    {
        using (var enumerator = items.Where(predicate).GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                return Perhaps<TItem>.ToPerhaps(enumerator.Current);
            }
        }

        return Perhaps<TItem>.Empty;
    }

    public static Perhaps<TItem> LastOrPerhaps<TItem>(this IEnumerable<TItem> items)
    {
        var choice = Perhaps<TItem>.Empty;

        foreach (var item in items)
        {
            choice = Perhaps<TItem>.ToPerhaps(item);
        }

        return choice;
    }

    public static Perhaps<TItem> LastOrPerhaps<TItem>(this IEnumerable<TItem> items, Func<TItem, bool> predicate)
    {
        var choice = Perhaps<TItem>.Empty;

        foreach (var item in items.Where(predicate))
        {
            choice = Perhaps<TItem>.ToPerhaps(item);
        }

        return choice;
    }
}