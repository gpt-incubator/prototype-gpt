using PrototypeGPT.Application.Providers;
using PrototypeGPT.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;

namespace PrototypeGPT.Infrastructure;

public class PrompterGptProvider : IPrompterGptProvider
{
    private readonly OpenAIService openAiService;

    public PrompterGptProvider(IOptions<GptOptions> options)
    {
        openAiService = new OpenAIService(new OpenAiOptions()
        {
            ApiKey = options.Value.ApiKey
        });
    }

    public async Task<string> PromptGptAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var completionResult = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = new List<ChatMessage>
            {
                ChatMessage.FromUser(prompt)
            },
            Model = OpenAI.GPT3.ObjectModels.Models.ChatGpt3_5Turbo
        }, cancellationToken: cancellationToken);

        if (completionResult.Successful)
        {
            return completionResult.Choices.First().Message.Content;
        }

        return string.Empty;
    }
}
