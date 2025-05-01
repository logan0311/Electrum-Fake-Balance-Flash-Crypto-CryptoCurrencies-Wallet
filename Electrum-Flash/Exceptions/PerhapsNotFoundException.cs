using Vulpes.Electrum.Domain.Monads;

namespace Vulpes.Electrum.Domain.Exceptions;
public class PerhapsNotFoundException : Exception
{
    public Type RequestedType { get; init; }

    public PerhapsNotFoundException(Type requestedType)
        : base($"The {nameof(Perhaps<Type>)} of type {requestedType.Name} is empty.")
    {
        RequestedType = requestedType;
    }
    public PerhapsNotFoundException(string message, Type requestedType)
        : base(message)
    {
        RequestedType = requestedType;
    }
}
