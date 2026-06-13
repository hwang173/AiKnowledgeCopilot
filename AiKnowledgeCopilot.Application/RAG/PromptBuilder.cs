namespace AiKnowledgeCopilot.Application.RAG;

public static class PromptBuilder
{
    public static string BuildAnswerPrompt(
        string question,
        List<string> contextChunks)
    {
        var context =
            string.Join(
                Environment.NewLine +
                Environment.NewLine,
                contextChunks);

        return
$"""
You are an AI knowledge assistant.

Answer the user's question using only the provided context.

If the answer cannot be found in the context,
say that the information is not available.

Context:

{context}

Question:

{question}
""";
    }
}