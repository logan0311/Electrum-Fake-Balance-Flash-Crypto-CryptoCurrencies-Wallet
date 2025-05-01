using Vulpes.Electrum.Domain.Commanding;
using Vulpes.Electrum.Domain.Exceptions;
using Vulpes.Electrum.Domain.Querying;

namespace Vulpes.Electrum.Domain.Mediation;
public class Mediator : IMediator
{
    private readonly Dictionary<string /*command type*/, object /*handler*/> commandHandlers;
    private readonly Dictionary<string /*query type*/ , object /*handler*/> queryHandlers;

    public Mediator()
    {
        commandHandlers = [];
        queryHandlers = [];
    }

    public Mediator Register<TCommand>(CommandHandler<TCommand> commandHandler)
        where TCommand : Command
    {
        commandHandlers.Add(typeof(TCommand).Name, commandHandler);
        return this;
    }

    public Mediator Register<TQuery, TResponse>(QueryHandler<TQuery, TResponse> queryHandler)
        where TQuery : Query
    {
        queryHandlers.Add(typeof(TQuery).Name, queryHandler);
        return this;
    }

    public async Task<TResponse> RequestResponseAsync<TQuery, TResponse>(TQuery query)
        where TQuery : Query
    {
        var handler = ExtractQueryHandler<TQuery, TResponse>();
        return await handler.RequestAsync(query);
    }

    public async Task<bool> EvaluateAccessAsync<TCommand>(TCommand command)
        where TCommand : Command
    {
        var handler = ExtractCommandHandler<TCommand>()!;
        return await handler.ValidateAccessAsync(command);
    }

    public async Task ExecuteCommandAsync<TCommand>(TCommand command)
        where TCommand : Command
    {
        var handler = ExtractCommandHandler<TCommand>();
        await handler.ExecuteAsync(command);
    }

    private CommandHandler<TCommand> ExtractCommandHandler<TCommand>()
        where TCommand : Command
    {
        try
        {
            return commandHandlers[typeof(TCommand).Name] as CommandHandler<TCommand> ?? throw new MediationHandlerNotFoundException(typeof(TCommand));
        }
        catch
        {
            throw new MediationHandlerNotFoundException(typeof(TCommand));
        }
    }

    private QueryHandler<TQuery, TResponse> ExtractQueryHandler<TQuery, TResponse>()
        where TQuery : Query
    {
        try
        {
            return queryHandlers[typeof(TQuery).Name] as QueryHandler<TQuery, TResponse> ?? throw new MediationHandlerNotFoundException(typeof(TQuery));
        }
        catch
        {
            throw new MediationHandlerNotFoundException(typeof(TQuery));
        }
    }
}
