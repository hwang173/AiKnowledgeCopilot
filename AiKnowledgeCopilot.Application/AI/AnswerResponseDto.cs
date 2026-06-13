namespace AiKnowledgeCopilot.Application.AI;

public class AnswerResponseDto
{
    public string Answer { get; set; }
        = string.Empty;

    public List<string> Sources { get; set; }
        = new();
}