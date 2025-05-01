namespace Vulpes.Electrum.Domain.Extensions;
public static class EnumerableExtensions
{
    public static TObject GetRandom<TObject>(this IEnumerable<TObject> enumerable)
    {
        var randomIndex = new Random().Next(0, enumerable.Count() - 1);
        return enumerable.ElementAt(randomIndex);
    }
}
