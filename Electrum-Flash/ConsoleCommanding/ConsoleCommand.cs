namespace Vulpes.Electrum.Domain.ConsoleCommanding;
public record ConsoleCommand
{
    private const string ArgumentPrefix = "-";

    public string Name { get; init; } = string.Empty;
    public Dictionary<string, string> Arguments { get; init; } = [];

    /// <summary>
    /// Empty constructor privatized to prevent instantiation without arguments.
    /// </summary>
    private ConsoleCommand() { }

    public static ConsoleCommand FromInput(string input)
    {
        var parts = input.Split(' ');
        var arguments = new Dictionary<string, string>();
        var currentKey = string.Empty;
        var currentValue = new List<string>();
        var commandName = string.Empty;
        var commandNameCaptured = false;

        foreach (var part in parts)
        {
            if (part.StartsWith(ArgumentPrefix))
            {
                if (!commandNameCaptured)
                {
                    commandName = string.Join(" ", parts.TakeWhile(p => !p.StartsWith(ArgumentPrefix)));
                    commandNameCaptured = true;
                }

                if (currentKey != string.Empty)
                {
                    arguments[currentKey] = string.Join(" ", currentValue);
                }
                currentKey = part.Replace(ArgumentPrefix, string.Empty);
                currentValue.Clear();
            }
            else if (currentKey != string.Empty)
            {
                currentValue.Add(part);
            }
        }

        if (currentKey != string.Empty)
        {
            arguments[currentKey] = string.Join(" ", currentValue);
        }

        return new ConsoleCommand() with
        {
            Name = commandName,
            Arguments = arguments
        };
    }
}
