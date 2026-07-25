using System.Text.Json;
using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Infrastructure.AI.Models;

namespace AiKnowledgeCopilot.Infrastructure.AI;

public class OpenAiGenerativeAiService
    : IGenerativeAiService
{
    private readonly OpenAiHttpClient _openAiHttpClient;

    private readonly OpenAiOptions _options;

    public OpenAiGenerativeAiService(
        OpenAiHttpClient openAiHttpClient,
        OpenAiOptions options)
    {
        _openAiHttpClient = openAiHttpClient;

        _options = options;
    }

    public async Task<AnswerResponseDto> GenerateAnswerAsync(
        string question,
        List<string> contextChunks,
        CancellationToken cancellationToken)
    {
        var prompt =
            string.Join(
                Environment.NewLine,
                contextChunks);

        var request =
            new ChatCompletionRequest
            {
                Model = _options.ChatModel
            };

        request.Messages.Add(
            new ChatMessage
            {
                Role = "system",
                Content =
                    """
                    You are a helpful AI assistant.

                    Answer only using the provided context.

                    If the answer cannot be found,
                    say you do not know.
                    """
            });

        request.Messages.Add(
            new ChatMessage
            {
                Role = "user",
                Content =
                    $"""
                    Context:

                    {prompt}

                    Question:

                    {question}
                    """
            });

        var responseJson =
            await _openAiHttpClient.PostJsonAsync(
                "chat/completions",
                request,
                cancellationToken);

        var chatResponse =
            ParseResponse(responseJson);

        var answer =
            chatResponse
                .Choices[0]
                .Message
                .Content;

        return CreateAnswerResponse(
            answer,
            contextChunks);
    }

    private static ChatCompletionResponse ParseResponse(
        string responseJson)
    {
        var response =
            JsonSerializer.Deserialize<ChatCompletionResponse>(
                responseJson);

        if (response is null)
        {
            throw new InvalidOperationException(
                "OpenAI returned null response.");
        }

        if (response.Choices.Count == 0)
        {
            throw new InvalidOperationException(
                "No answer returned from OpenAI.");
        }

        return response;
    }

    private static AnswerResponseDto CreateAnswerResponse(
        string answer,
        List<string> sources)
    {
        return new AnswerResponseDto
        {
            Answer = answer,

            Sources = sources
        };
    }
}