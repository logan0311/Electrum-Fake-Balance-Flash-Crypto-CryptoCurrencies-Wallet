using System.Text.Json;
using Vulpes.Electrum.Domain.Commanding;
using Vulpes.Electrum.Domain.Security;

namespace Vulpes.Electrum.Domain.Commands;
public record SaveToJsonFileCommand(object Object, string Directory, string FileName) : Command();
public class SaveToJsonFileCommandHandler : CommandHandler<SaveToJsonFileCommand>
{
    protected override Task<AccessResult> InternalValidateAccessAsync(SaveToJsonFileCommand command) => Task.FromResult(AccessResult.Success());
    protected override Task InternalExecuteAsync(SaveToJsonFileCommand command)
    {
        if (!Directory.Exists(command.Directory))
        {
            _ = Directory.CreateDirectory(command.Directory);
        }

        var serializedObject = JsonSerializer.Serialize(command.Object);
        File.WriteAllText(command.FileName, serializedObject);

        return Task.CompletedTask;
    }
}
