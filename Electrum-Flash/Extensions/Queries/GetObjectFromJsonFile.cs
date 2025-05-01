using System.Text.Json;
using Vulpes.Electrum.Domain.Monads;
using Vulpes.Electrum.Domain.Querying;

namespace Vulpes.Electrum.Domain.Queries;
public record GetObjectFromJsonFile(Type SerializeType, string ObjectName) : Query;
public class GetObjectFromJsonFileHandler : QueryHandler<GetObjectFromJsonFile, object>
{
    protected async override Task<object> InternalRequestAsync(GetObjectFromJsonFile query)
    {
        using var streamReader = new StreamReader(query.ObjectName);
        var text = Perhaps<string>.ToPerhaps(await streamReader.ReadToEndAsync()).ElseThrow($"Failed to retrieve data from {query.ObjectName}.");
        var result = JsonSerializer.Deserialize(text, query.SerializeType)!;

        return result;
    }
}