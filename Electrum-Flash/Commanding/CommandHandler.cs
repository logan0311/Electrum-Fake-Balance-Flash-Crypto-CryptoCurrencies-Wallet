using Vulpes.Electrum.Domain.Security;

namespace Vulpes.Electrum.Domain.Commanding;

public abstract class CommandHandler<TCommand>
    where TCommand : Command
{
    protected abstract Task<AccessResult> InternalValidateAccessAsync(TCommand command);
    protected abstract Task InternalExecuteAsync(TCommand command);

    /// <summary>
    /// Validates the access to the command. Can be overridden to provide custom access validation.
    /// </summary>
    /// <param name="command">The command being accessed.</param>
    public virtual async Task<AccessResult> ValidateAccessAsync(TCommand command) => await InternalValidateAccessAsync(command);

    /// <summary>
    /// Executes the command.
    /// </summary>
    public virtual async Task ExecuteAsync(TCommand command)
    {
        (await ValidateAccessAsync(command)).ThrowIfAccessDenied();
        await InternalExecuteAsync(command);
    }
}