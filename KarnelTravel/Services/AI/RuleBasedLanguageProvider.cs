namespace KarnelTravel.Services.AI;

public sealed class RuleBasedLanguageProvider : ILanguageModelProvider
{
    public Task<string> GenerateAsync(string instruction, CancellationToken cancellationToken = default) =>
        Task.FromResult(instruction);
}
