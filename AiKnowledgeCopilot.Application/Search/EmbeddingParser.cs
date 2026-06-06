using System.Text.Json;

namespace AiKnowledgeCopilot.Application.Search;

public static class EmbeddingParser
{
    public static float[] Parse(
        string embeddingJson)
    {
        var vector =
            JsonSerializer.Deserialize<float[]>(
                embeddingJson);

        if (vector == null)
        {
            throw new InvalidOperationException(
                "Failed to parse embedding.");
        }

        return vector;
    }
}