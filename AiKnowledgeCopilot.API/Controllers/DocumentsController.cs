using AiKnowledgeCopilot.API.Models;
using AiKnowledgeCopilot.Application.Documents;
using AiKnowledgeCopilot.Application.Services;
using AiKnowledgeCopilot.Application.Storage;
using Microsoft.AspNetCore.Mvc;

namespace AiKnowledgeCopilot.API.Controllers;

[ApiController]
[Route("documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService
        _documentService;

    private readonly IFileStorageService
        _fileStorageService;

    public DocumentsController(
        IDocumentService documentService,
        IFileStorageService fileStorageService)
    {
        _documentService =
            documentService;

        _fileStorageService =
            fileStorageService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(
        [FromForm] UploadDocumentForm form,
        CancellationToken cancellationToken)
    {
        var filePath =
            await _fileStorageService
                .SaveAsync(
                    new FileUploadRequest
                    {
                        FileName = form.File.FileName,
                        Content = form.File.OpenReadStream()
                    },
                    cancellationToken);

        var command =
            new UploadDocumentCommand
            {
                FileName =
                    form.File.FileName,

                FilePath =
                    filePath,

                UploadedByUserId =
                    form.UploadedByUserId
            };

        var documentId =
            await _documentService
                .UploadAsync(
                    command,
                    cancellationToken);

        return Ok(
            new
            {
                DocumentId = documentId
            });
    }
}