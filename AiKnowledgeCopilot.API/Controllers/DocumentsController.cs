using AiKnowledgeCopilot.Application.Documents;
using AiKnowledgeCopilot.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiKnowledgeCopilot.API.Controllers;

[ApiController]
[Route("documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(
        IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(
        UploadDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var documentId =
            await _documentService.UploadAsync(
                request,
                cancellationToken);

        return Ok(new
        {
            DocumentId = documentId
        });
    }
}