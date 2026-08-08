namespace AiKnowledgeCopilot.Application.Observability;

public interface ICorrelationContext
{
    string CorrelationId { get; }
}