namespace Vulpes.Electrum.Domain.Querying;
public abstract class QueryHandler<TQuery, TResponse>
    where TQuery : Query
{
    protected abstract Task<TResponse> InternalRequestAsync(TQuery query);

    /// <summary>
    /// Requests data from the provided query. Task is virtual to allow customization.
    /// </summary>
    public virtual async Task<TResponse> RequestAsync(TQuery query) => await InternalRequestAsync(query);
}
