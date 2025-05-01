using Vulpes.Electrum.Domain.Exceptions;

namespace Vulpes.Electrum.Domain.Security;
public record AccessResult
{
    public static AccessResult Empty { get; } = new();

    public bool AccessGranted { get; init; } = false;
    public string Message { get; init; } = string.Empty;

    public void ThrowIfAccessDenied()
    {
        if (!AccessGranted)
        {
            throw new AccessDeniedException(this);
        }
    }

    public static implicit operator bool(AccessResult accessResult) => accessResult.AccessGranted;

    public static AccessResult Success() => new() { AccessGranted = true };
    public static AccessResult Fail(string message) => new() { AccessGranted = false, Message = message };
}