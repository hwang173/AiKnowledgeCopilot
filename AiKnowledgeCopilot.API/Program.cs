using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Infrastructure.Repositories;
using AiKnowledgeCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AiKnowledgeCopilot.Application.Services;
using AiKnowledgeCopilot.Infrastructure.Services;
using AiKnowledgeCopilot.Application.Background;
using AiKnowledgeCopilot.Infrastructure.Background;
using AiKnowledgeCopilot.Infrastructure.HostedServices;
using AiKnowledgeCopilot.Application.Chunking;
using AiKnowledgeCopilot.Infrastructure.Chunking;

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

builder.Services.AddSingleton<
    IDocumentProcessingQueue,
    InMemoryDocumentProcessingQueue>();

builder.Services.AddHostedService<
    DocumentProcessingHostedService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
