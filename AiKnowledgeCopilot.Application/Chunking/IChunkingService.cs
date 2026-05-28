namespace AiKnowledgeCopilot.Application.Chunking;

public interface IChunkingService
{
    List<string> Chunk(
        string content,
        int chunkSize = 200,
        int overlap = 50);
}