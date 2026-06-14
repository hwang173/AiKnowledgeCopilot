using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Background;
using AiKnowledgeCopilot.Application.Chunking;
using AiKnowledgeCopilot.Application.Documents;
using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Application.Search;
using AiKnowledgeCopilot.Application.Services;
using AiKnowledgeCopilot.Application.Storage;
using AiKnowledgeCopilot.Infrastructure.AI;
using AiKnowledgeCopilot.Infrastructure.Background;
using AiKnowledgeCopilot.Infrastructure.Chunking;
using AiKnowledgeCopilot.Infrastructure.Documents;
using AiKnowledgeCopilot.Infrastructure.HostedServices;
using AiKnowledgeCopilot.Infrastructure.Persistence;
using AiKnowledgeCopilot.Infrastructure.Repositories;
using AiKnowledgeCopilot.Infrastructure.Search;
using AiKnowledgeCopilot.Infrastructure.Services;
using AiKnowledgeCopilot.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

builder.Services.AddScoped<IDocumentService, DocumentService>();

builder.Services.AddScoped<
    IChunkingService,
    SimpleChunkingService>();

builder.Services.AddScoped<
    ISemanticSearchService,
    SemanticSearchService>();

builder.Services.AddScoped<
    IChunkRepository,
    ChunkRepository>();

builder.Services.AddScoped<
    IGenerativeAiService,
    OpenAiGenerativeAiService>();

builder.Services.AddScoped<
    IQuestionService,
    QuestionService>();

builder.Services.AddScoped<
    IFileStorageService,
    LocalFileStorageService>();

builder.Services.AddScoped<
    ITextExtractionService,
    TextExtractionService>();

builder.Services.AddHttpClient<
    IEmbeddingService,
    OpenAiEmbeddingService>((serviceProvider, client) =>
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        var apiKey =
            configuration["OpenAI:ApiKey"];

        var baseUrl =
            configuration["OpenAI:BaseUrl"];

        client.BaseAddress =
            new Uri(baseUrl!);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey);
    });

builder.Services.AddHttpClient<
    IGenerativeAiService,
    OpenAiGenerativeAiService>(
(serviceProvider, client) =>
{
    var configuration =
        serviceProvider.GetRequiredService<IConfiguration>();

    var apiKey =
        configuration["OpenAI:ApiKey"];

    var baseUrl =
        configuration["OpenAI:BaseUrl"];

    client.BaseAddress =
        new Uri(baseUrl!);

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
            "Bearer",
            apiKey);
});

builder.Services.AddSingleton<
    IDocumentProcessingQueue,
    InMemoryDocumentProcessingQueue>();

builder.Services.AddHostedService<
    DocumentProcessingHostedService>();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
