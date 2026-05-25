using AiKnowledgeCopilot.Application.Background;
using AiKnowledgeCopilot.Application.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiKnowledgeCopilot.Infrastructure.HostedServices;

public class DocumentProcessingHostedService
    : BackgroundService
{
    private readonly IDocumentProcessingQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DocumentProcessingHostedService>
        _logger;

    public DocumentProcessingHostedService(
        IDocumentProcessingQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<DocumentProcessingHostedService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Document processing worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var documentId =
                    await _queue.DequeueAsync(stoppingToken);

                await ProcessDocumentAsync(
                    documentId,
                    stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing document.");
            }
        }
    }

    private async Task ProcessDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var repository =
            scope.ServiceProvider
                .GetRequiredService<IDocumentRepository>();

        var document =
            await repository.GetByIdAsync(
                documentId,
                cancellationToken);

        if (document is null)
        {
            return;
        }

        document.MarkAsProcessing();

        await repository.SaveChangesAsync(
            cancellationToken);

        await Task.Delay(5000, cancellationToken);

        document.MarkAsCompleted();

        await repository.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Document {DocumentId} processed.",
            documentId);
    }
}