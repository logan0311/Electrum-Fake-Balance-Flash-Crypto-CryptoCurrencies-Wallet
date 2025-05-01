namespace Vulpes.Electrum.Domain.Security;
public record ElectrumValidationResult
{
    private readonly bool isValid;
    private readonly List<string> messages;
    public IEnumerable<string> Messages => messages;

    public static implicit operator bool(ElectrumValidationResult validationResult) => validationResult.isValid;

    private ElectrumValidationResult()
    {
        isValid = false;
        messages = [];
    }

    private ElectrumValidationResult(bool isValid, string message)
    {
        this.isValid = isValid;
        messages = [message];
    }

    private ElectrumValidationResult(bool isValid, List<string> messages)
    {
        this.isValid = isValid;
        this.messages = messages;
    }

    public static ElectrumValidationResult Verify(Func<bool> validation, string message)
    {
        var isValid = validation();
        var messageResult = message;
        if (isValid)
        {
            messageResult = string.Empty;
        }

        return new ElectrumValidationResult(isValid, messageResult);
    }

    public static ElectrumValidationResult Verify(Func<bool> validation, string message, ElectrumValidationResult innerValidationResult)
    {
        var currentResult = Verify(validation, message);

        // If this was valid, then we can just return the inner result and be done with it.
        if (currentResult)
        {
            return innerValidationResult;
        }

        // If this was not valid, then we have to merge the two results.
        return MergeResults(currentResult, innerValidationResult);
    }

    public static ElectrumValidationResult Verify() => Verify(() => true, string.Empty);

    private static ElectrumValidationResult MergeResults(ElectrumValidationResult result1, ElectrumValidationResult result2)
    {
        var combinedMessages = new List<string>();
        if (!result1.isValid)
        {
            combinedMessages.AddRange(result1.messages);
        }
        if (!result2.isValid)
        {
            combinedMessages.AddRange(result2.messages);
        }

        return new ElectrumValidationResult(result1.isValid && result2.isValid, combinedMessages);
    }
};