using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AiKnowledgeCopilot.Infrastructure.AI;

public class OpenAiHttpClient
{
    private static readonly HttpStatusCode[] TransientStatusCodes =
    [
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.TooManyRequests,
        HttpStatusCode.InternalServerError,
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.GatewayTimeout
    ];

    private readonly HttpClient _httpClient;

    private readonly OpenAiOptions _options;

    private readonly ILogger<OpenAiHttpClient> _logger;

    public OpenAiHttpClient(
        HttpClient httpClient,
        OpenAiOptions options,
        ILogger<OpenAiHttpClient> logger)
    {
        _httpClient = httpClient;

        _options = options;

        _logger = logger;
    }

    public async Task<string> PostJsonAsync(
        string endpoint,
        object request,
        CancellationToken cancellationToken)
    {
        ValidateEndpoint(endpoint);

        for (int attempt = 1;
             attempt <= _options.MaxRetryAttempts + 1;
             attempt++)
        {
            try
            {
                using var timeoutTokenSource =
                    CancellationTokenSource
                        .CreateLinkedTokenSource(
                            cancellationToken);

                timeoutTokenSource.CancelAfter(
                    TimeSpan.FromSeconds(
                        _options.RequestTimeoutSeconds));

                using var content =
                    CreateJsonContent(request);

                using var response =
                    await _httpClient.PostAsync(
                        endpoint,
                        content,
                        timeoutTokenSource.Token);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content
                        .ReadAsStringAsync(
                            cancellationToken);
                }

                var responseBody =
                    await response.Content
                        .ReadAsStringAsync(
                            cancellationToken);

                if (!ShouldRetry(response.StatusCode) ||
                    attempt > _options.MaxRetryAttempts)
                {
                    throw CreateHttpException(
                        response.StatusCode,
                        responseBody);
                }

                await DelayBeforeRetryAsync(
                    attempt,
                    response.StatusCode,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested &&
                      attempt <= _options.MaxRetryAttempts)
            {
                await DelayBeforeRetryAsync(
                    attempt,
                    null,
                    cancellationToken);
            }
            catch (HttpRequestException)
                when (attempt <= _options.MaxRetryAttempts)
            {
                await DelayBeforeRetryAsync(
                    attempt,
                    null,
                    cancellationToken);
            }
        }

        throw new InvalidOperationException(
            "OpenAI request retry pipeline ended unexpectedly.");
    }

    private static void ValidateEndpoint(
        string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException(
                "Endpoint is required.",
                nameof(endpoint));
        }
    }

    private static StringContent CreateJsonContent(
        object request)
    {
        var json =
            JsonSerializer.Serialize(request);

        return new StringContent(
            json,
            Encoding.UTF8,
            "application/json");
    }

    private static bool ShouldRetry(
        HttpStatusCode statusCode)
    {
        return TransientStatusCodes.Contains(statusCode);
    }

    private async Task DelayBeforeRetryAsync(
        int attempt,
        HttpStatusCode? statusCode,
        CancellationToken cancellationToken)
    {
        var delay =
            TimeSpan.FromSeconds(
                _options.RetryBaseDelaySeconds *
                Math.Pow(2, attempt - 1));

        _logger.LogWarning(
            "OpenAI request failed with status {StatusCode}. Retrying attempt {Attempt} after {DelaySeconds} seconds.",
            statusCode,
            attempt,
            delay.TotalSeconds);

        await Task.Delay(
            delay,
            cancellationToken);
    }

    private static HttpRequestException CreateHttpException(
        HttpStatusCode statusCode,
        string responseBody)
    {
        var message =
            string.IsNullOrWhiteSpace(responseBody)
                ? $"OpenAI request failed with status code {(int)statusCode}."
                : $"OpenAI request failed with status code {(int)statusCode}: {responseBody}";

        return new HttpRequestException(
            message,
            null,
            statusCode);
    }
}