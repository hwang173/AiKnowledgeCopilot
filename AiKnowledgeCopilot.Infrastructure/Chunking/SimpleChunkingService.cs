using AiKnowledgeCopilot.Application.Chunking;
using System.Text.RegularExpressions;

namespace AiKnowledgeCopilot.Infrastructure.Chunking;

public class SimpleChunkingService
    : IChunkingService
{
    public List<string> Chunk(
        string content,
        int chunkSize = 200,
        int overlap = 50)
    {
        content = Regex.Replace(content, @"\s+", " ");
        var chunks = new List<string>();

        if (string.IsNullOrWhiteSpace(content))
        {
            return chunks;
        }

        var start = 0;

        while (start < content.Length)
        {
            var length =
                Math.Min(chunkSize,
                    content.Length - start);

            var chunk =
                content.Substring(start, length);

            chunks.Add(chunk);

            start += chunkSize - overlap;
        }

        return chunks;
    }
}