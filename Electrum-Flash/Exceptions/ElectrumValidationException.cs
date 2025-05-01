using Vulpes.Electrum.Domain.Security;

namespace Vulpes.Electrum.Domain.Exceptions;
public class ElectrumValidationException : Exception
{
    public ElectrumValidationException(ElectrumValidationResult electrumValidationResult) : base(string.Join(", ", electrumValidationResult.Messages))
    { }
}
