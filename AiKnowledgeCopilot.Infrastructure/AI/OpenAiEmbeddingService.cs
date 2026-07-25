using System.Text.Json;
using AiKnowledgeCopilot.Application.AI;

namespace AiKnowledgeCopilot.Infrastructure.AI;

public class OpenAiEmbeddingService
    : IEmbeddingService
{
    private readonly OpenAiHttpClient _openAiHttpClient;

    private readonly OpenAiOptions _options;

    public OpenAiEmbeddingService(
        OpenAiHttpClient openAiHttpClient,
        OpenAiOptions options)
    {
        _openAiHttpClient = openAiHttpClient;

        _options = options;
    }

    public async Task<string> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken)
    {
        var request = new
        {
            input = text,
            model = _options.EmbeddingModel
        };

        var responseJson =
            await _openAiHttpClient.PostJsonAsync(
                "embeddings",
                request,
                cancellationToken);

        using var document =
            JsonDocument.Parse(responseJson);

        var embedding =
            document.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding");

        return embedding.ToString();
    }
}