using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Net.Http.Headers;
using System.Text;
using AiKnowledgeCopilot.API.Middleware;
using AiKnowledgeCopilot.API.Security;
using AiKnowledgeCopilot.Application.AI;
using AiKnowledgeCopilot.Application.Background;
using AiKnowledgeCopilot.Application.Chunking;
using AiKnowledgeCopilot.Application.Documents;
using AiKnowledgeCopilot.Application.Repositories;
using AiKnowledgeCopilot.Application.Search;
using AiKnowledgeCopilot.Application.Security;
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
using AiKnowledgeCopilot.API.Observability;
using AiKnowledgeCopilot.Application.Observability;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

Log.Logger =
    new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override(
            "Microsoft",
            Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override(
            "Microsoft.AspNetCore",
            Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console(
            outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {UserId} {RequestMethod} {RequestPath} {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File(
            path: "Logs/ai-knowledge-copilot-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14,
            outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] CorrelationId={CorrelationId} UserId={UserId} Method={RequestMethod} Path={RequestPath} {Message:lj} {Properties:j}{NewLine}{Exception}")
        .CreateLogger();

try
{
    var builder =
        WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    var openTelemetryOptions =
    builder.Configuration
        .GetSection(OpenTelemetryOptions.SectionName)
        .Get<OpenTelemetryOptions>()
    ?? new OpenTelemetryOptions();

    if (openTelemetryOptions.SamplingRatio < 0 ||
        openTelemetryOptions.SamplingRatio > 1)
    {
        throw new InvalidOperationException(
            "OpenTelemetry:SamplingRatio must be between 0.0 and 1.0.");
    }

    builder.Services.AddSingleton(openTelemetryOptions);

    builder.Services
        .AddOpenTelemetry()
        .ConfigureResource(resource =>
        {
            resource.AddService(
                serviceName: openTelemetryOptions.ServiceName,
                serviceVersion: openTelemetryOptions.ServiceVersion);
        })
        .WithTracing(tracing =>
        {
            tracing
                .SetSampler(
                    new TraceIdRatioBasedSampler(
                        openTelemetryOptions.SamplingRatio))
                .AddSource(
                    AiKnowledgeCopilotTelemetry.ActivitySourceName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();

            if (openTelemetryOptions.ConsoleExporterEnabled)
            {
                tracing.AddConsoleExporter();
            }

            if (openTelemetryOptions.OtlpExporterEnabled)
            {
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint =
                        new Uri(openTelemetryOptions.OtlpEndpoint);

                    options.Protocol =
                        OtlpExportProtocol.Grpc;
                });
            }
        });

    var jwtOptions =
        builder.Configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
        ?? new JwtOptions();

    if (Encoding.UTF8.GetByteCount(jwtOptions.SigningKey) < 32)
    {
        throw new InvalidOperationException(
            "Jwt:SigningKey must be at least 32 bytes.");
    }

    builder.Services.AddSingleton(jwtOptions);

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"));
    });

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddScoped<
    ICorrelationContext,
    HttpCorrelationContext>();

    builder.Services.AddAuthentication(
            JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                jwtOptions.SigningKey)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),

                    NameClaimType =
                        System.Security.Claims.ClaimTypes.NameIdentifier,

                    RoleClaimType =
                        System.Security.Claims.ClaimTypes.Role
                };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(
            AuthorizationPolicies.DocumentUser,
            policy =>
            {
                policy.RequireAuthenticatedUser();

                policy.RequireRole(
                    AuthorizationPolicies.UserRole,
                    AuthorizationPolicies.AdminRole);
            });

        options.AddPolicy(
            AuthorizationPolicies.AdminOnly,
            policy =>
            {
                policy.RequireAuthenticatedUser();

                policy.RequireRole(
                    AuthorizationPolicies.AdminRole);
            });
    });

    builder.Services.AddScoped<
        ICurrentUserContext,
        JwtCurrentUserContext>();

    builder.Services.AddScoped<
        IDevelopmentJwtTokenService,
        DevelopmentJwtTokenService>();

    builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

    builder.Services.AddScoped<IDocumentService, DocumentService>();

    builder.Services.AddScoped<
        IDocumentStatusService,
        DocumentStatusService>();

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
        IQuestionService,
        QuestionService>();

    builder.Services.AddScoped<
        IFileStorageService,
        LocalFileStorageService>();

    builder.Services.AddSingleton(
        builder.Configuration
            .GetSection(DocumentUploadOptions.SectionName)
            .Get<DocumentUploadOptions>()
        ?? new DocumentUploadOptions());

    builder.Services.AddScoped<
        ISupportedDocumentTypesProvider,
        SupportedDocumentTypesProvider>();

    builder.Services.AddScoped<
        IDocumentUploadValidator,
        DocumentUploadValidator>();

    builder.Services.AddScoped<ITextExtractor, TextFileExtractor>();

    builder.Services.AddScoped<ITextExtractor, PdfFileExtractor>();

    builder.Services.AddScoped<ITextExtractor, DocxFileExtractor>();

    builder.Services.AddScoped<
        ITextExtractionService,
        TextExtractionService>();

    var openAiOptions =
        builder.Configuration
            .GetSection(OpenAiOptions.SectionName)
            .Get<OpenAiOptions>()
        ?? new OpenAiOptions();

    builder.Services.AddSingleton(openAiOptions);

    var embeddingCacheOptions =
        builder.Configuration
            .GetSection(EmbeddingCacheOptions.SectionName)
            .Get<EmbeddingCacheOptions>()
        ?? new EmbeddingCacheOptions();

    builder.Services.AddSingleton(embeddingCacheOptions);

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration =
            builder.Configuration.GetConnectionString("Redis");

        options.InstanceName =
            "AiKnowledgeCopilot:";
    });

    builder.Services.AddScoped<
        IEmbeddingCache,
        RedisEmbeddingCache>();

    builder.Services.AddHttpClient<OpenAiHttpClient>((serviceProvider, client) =>
    {
        var options =
            serviceProvider.GetRequiredService<OpenAiOptions>();

        client.BaseAddress =
            new Uri(options.BaseUrl);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                options.ApiKey);
    });

    builder.Services.AddScoped<OpenAiEmbeddingService>();

    builder.Services.AddScoped<
        IEmbeddingService,
        CachedEmbeddingService>();

    builder.Services.AddScoped<
        IGenerativeAiService,
        OpenAiGenerativeAiService>();

    var documentProcessingQueueOptions =
    builder.Configuration
        .GetSection(DocumentProcessingQueueOptions.SectionName)
        .Get<DocumentProcessingQueueOptions>()
    ?? new DocumentProcessingQueueOptions();

    builder.Services.AddSingleton(documentProcessingQueueOptions);

    builder.Services.AddSingleton<
        IDocumentProcessingQueue,
        InMemoryDocumentProcessingQueue>();

    builder.Services.AddHostedService<
        DocumentProcessingHostedService>();

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title = "AI Knowledge Copilot API",
                Version = "v1"
            });

        options.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "Enter a valid JWT bearer token."
            });

        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference =
                            new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                    },
                    Array.Empty<string>()
                }
            });
    });

    var app =
        builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();

        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseMiddleware<RequestCorrelationMiddleware>();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(
        ex,
        "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}