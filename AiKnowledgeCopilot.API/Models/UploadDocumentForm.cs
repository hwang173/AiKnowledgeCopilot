using Microsoft.AspNetCore.Http;

namespace AiKnowledgeCopilot.API.Models;

public class UploadDocumentForm
{
    public IFormFile File { get; set; }
        = default!;

    public string UploadedByUserId { get; set; }
        = string.Empty;
}