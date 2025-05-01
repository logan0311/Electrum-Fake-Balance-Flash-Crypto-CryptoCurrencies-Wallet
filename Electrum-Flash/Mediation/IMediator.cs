using Vulpes.Electrum.Domain.Commanding;
using Vulpes.Electrum.Domain.Querying;

namespace Vulpes.Electrum.Domain.Mediation;
public interface IMediator
{
    Task<TResponse> RequestResponseAsync<TQuery, TResponse>(TQuery query)
        where TQuery : Query;

    Task ExecuteCommandAsync<TCommand>(TCommand command)
        where TCommand : Command;
    Task<bool> EvaluateAccessAsync<TCommand>(TCommand command)
        where TCommand : Command;
}
