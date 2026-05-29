using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AiKnowledgeCopilot.Application.AI;

namespace AiKnowledgeCopilot.Infrastructure.AI;

public class OpenAiEmbeddingService
    : IEmbeddingService
{
    private readonly HttpClient _httpClient;

    public OpenAiEmbeddingService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken)
    {
        var request = new
        {
            input = text,
            model = "text-embedding-3-small"
        };

        var json =
            JsonSerializer.Serialize(request);

        var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        var response =
            await _httpClient.PostAsync(
                "embeddings",
                content,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseJson =
            await response.Content.ReadAsStringAsync(
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