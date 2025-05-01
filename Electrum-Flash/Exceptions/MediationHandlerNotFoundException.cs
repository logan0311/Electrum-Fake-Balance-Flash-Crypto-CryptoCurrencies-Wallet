namespace Vulpes.Electrum.Domain.Exceptions;
public class MediationHandlerNotFoundException : Exception
{
    public MediationHandlerNotFoundException(Type type) : base($"Mediation handler of type {type} was not found. It may be unregistered.")
    { }
}
