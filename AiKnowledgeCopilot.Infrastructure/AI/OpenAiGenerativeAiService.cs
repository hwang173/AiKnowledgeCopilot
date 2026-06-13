using System.Text;
using System.Text.Json;
using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Infrastructure.AI.Models;

namespace AiKnowledgeCopilot.Infrastructure.AI;

public class OpenAiGenerativeAiService
    : IGenerativeAiService
{
    private readonly HttpClient _httpClient;

    public OpenAiGenerativeAiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
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
            Model = "gpt-4o-mini"
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

        var response =
            await SendRequestAsync(
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseJson =
            await response.Content
                .ReadAsStringAsync(
                    cancellationToken);

        var chatResponse =
            ParseResponse(
                responseJson);

        var answer =
            chatResponse
                .Choices[0]
                .Message
                .Content;

        return CreateAnswerResponse(
            answer,
            contextChunks);
    }

    private async Task<HttpResponseMessage>
    SendRequestAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var json =
            JsonSerializer.Serialize(
                request);

        var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        return await _httpClient.PostAsync(
            "chat/completions",
            content,
            cancellationToken);
    }

    private static ChatCompletionResponse
    ParseResponse(
        string responseJson)
    {
        var response =
            JsonSerializer.Deserialize<
                ChatCompletionResponse>(
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

    private static AnswerResponseDto
    CreateAnswerResponse(
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