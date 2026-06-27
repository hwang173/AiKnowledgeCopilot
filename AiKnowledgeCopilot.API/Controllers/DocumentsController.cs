using AiKnowledgeCopilot.API.Models;
using AiKnowledgeCopilot.Application.Documents;
using AiKnowledgeCopilot.Application.Security;
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

    private readonly IDocumentStatusService
        _documentStatusService;

    private readonly IFileStorageService
        _fileStorageService;

    private readonly IDocumentUploadValidator
        _uploadValidator;

    private readonly ICurrentUserContext
        _currentUserContext;

    public DocumentsController(
        IDocumentService documentService,
        IDocumentStatusService documentStatusService,
        IFileStorageService fileStorageService,
        IDocumentUploadValidator uploadValidator,
        ICurrentUserContext currentUserContext)
    {
        _documentService =
            documentService;

        _documentStatusService =
            documentStatusService;

        _fileStorageService =
            fileStorageService;

        _uploadValidator =
            uploadValidator;

        _currentUserContext =
            currentUserContext;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(
        [FromForm] UploadDocumentForm form,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var requestedByUserId))
        {
            return CreateUnauthorizedProblem();
        }

        var validationResult =
            _uploadValidator.Validate(
                new DocumentUploadValidationRequest
                {
                    FileName = form.File?.FileName,
                    FileSizeInBytes = form.File?.Length ?? 0,
                    UploadedByUserId = requestedByUserId
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
                    requestedByUserId
            };

        var documentId =
            await _documentService
                .UploadAsync(
                    command,
                    cancellationToken);

        var response =
            new UploadDocumentResponse
            {
                DocumentId = documentId,

                Status = "Uploaded",

                StatusUrl =
                    Url.Action(
                        nameof(GetById),
                        new { documentId })
                    ?? $"/documents/{documentId}"
            };

        return AcceptedAtAction(
            nameof(GetById),
            new { documentId },
            response);
    }

    [HttpGet("{documentId:guid}")]
    public async Task<ActionResult<DocumentStatusDto>> GetById(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var requestedByUserId))
        {
            return CreateUnauthorizedProblem();
        }

        var documentStatus =
            await _documentStatusService
                .GetByIdAsync(
                    new DocumentStatusQuery
                    {
                        DocumentId = documentId,
                        RequestedByUserId = requestedByUserId
                    },
                    cancellationToken);

        if (documentStatus is null)
        {
            return CreateNotFoundProblem(
                documentId);
        }

        return Ok(documentStatus);
    }

    private bool TryGetCurrentUserId(
        out string requestedByUserId)
    {
        requestedByUserId = string.Empty;

        if (!_currentUserContext.IsAuthenticated)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(
            _currentUserContext.UserId))
        {
            return false;
        }

        requestedByUserId =
            _currentUserContext.UserId;

        return true;
    }

    private UnauthorizedObjectResult CreateUnauthorizedProblem()
    {
        var problemDetails =
            new ProblemDetails
            {
                Title = "Authentication is required.",
                Detail = "The X-User-Id header is required for this development version of the API.",
                Status = StatusCodes.Status401Unauthorized
            };

        problemDetails.Extensions["requiredHeader"] =
            "X-User-Id";

        return Unauthorized(problemDetails);
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

    private NotFoundObjectResult CreateNotFoundProblem(
        Guid documentId)
    {
        var problemDetails =
            new ProblemDetails
            {
                Title = "Document was not found.",
                Detail = $"Document '{documentId}' does not exist.",
                Status = StatusCodes.Status404NotFound
            };

        problemDetails.Extensions["documentId"] =
            documentId;

        return NotFound(problemDetails);
    }
}