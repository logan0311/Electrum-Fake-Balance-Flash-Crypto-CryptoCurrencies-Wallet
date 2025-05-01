using Vulpes.Electrum.Domain.Exceptions;

namespace Vulpes.Electrum.Domain.Monads;
public class Perhaps<TItem>
{
    public static Perhaps<TItem> Empty => new(default!, true);
    public static Perhaps<TItem> ToPerhaps(TItem item)
    {
        // If the type is a string and it is an empty string, return an empty perhaps.
        if (typeof(TItem) == typeof(string) && string.IsNullOrWhiteSpace(item as string))
        {
            return Empty;
        }

        return new Perhaps<TItem>(item);
    }

    private readonly TItem item;

    public bool IsEmpty { get; init; } = true;

    private Perhaps(TItem item, bool isEmpty)
    {
        this.item = item;
        IsEmpty = isEmpty;
    }

    public Perhaps(TItem item)
    {
        this.item = item;
        IsEmpty = item != null;
    }

    public TItem Get() => item;
    public TItem Else(TItem contingency) => item ?? contingency;
    public TItem ElseThrow(Exception exception) => item ?? throw exception;
    public TItem ElseThrow(string message) => ElseThrow(new PerhapsNotFoundException(message, typeof(TItem)));
    public TItem ElseThrow() => ElseThrow(new PerhapsNotFoundException(typeof(TItem)));
}
