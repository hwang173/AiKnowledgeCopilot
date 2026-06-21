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

    private readonly IDocumentUploadValidator
        _uploadValidator;

    public DocumentsController(
        IDocumentService documentService,
        IFileStorageService fileStorageService,
        IDocumentUploadValidator uploadValidator)
    {
        _documentService =
            documentService;

        _fileStorageService =
            fileStorageService;

        _uploadValidator =
            uploadValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(
        [FromForm] UploadDocumentForm form,
        CancellationToken cancellationToken)
    {
        var validationResult =
            _uploadValidator.Validate(
                new DocumentUploadValidationRequest
                {
                    FileName = form.File?.FileName,
                    FileSizeInBytes = form.File?.Length ?? 0,
                    UploadedByUserId = form.UploadedByUserId
                });

        if (!validationResult.IsValid)
        {
            return CreateValidationProblem(
                validationResult);
        }

        var filePath =
            await _fileStorageService
                .SaveAsync(
                    new FileUploadRequest
                    {
                        FileName =
                            validationResult.SanitizedFileName!,

                        Content =
                            form.File!.OpenReadStream()
                    },
                    cancellationToken);

        var command =
            new UploadDocumentCommand
            {
                FileName =
                    validationResult.SanitizedFileName!,

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

    private BadRequestObjectResult CreateValidationProblem(
        DocumentUploadValidationResult validationResult)
    {
        var problemDetails =
            new ProblemDetails
            {
                Title = "Document upload validation failed.",
                Detail = validationResult.ErrorMessage,
                Status = StatusCodes.Status400BadRequest
            };

        problemDetails.Extensions["errorCode"] =
            validationResult.ErrorCode;

        return BadRequest(problemDetails);
    }
}