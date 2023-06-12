namespace PrototypeGPT.Application.Providers;

public interface IPrompterGptProvider
{
    Task<string> PromptGptAsync(string prompt, CancellationToken cancellationToken = default);
}
