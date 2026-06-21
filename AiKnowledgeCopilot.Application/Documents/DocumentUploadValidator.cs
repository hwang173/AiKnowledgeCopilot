namespace AiKnowledgeCopilot.Application.Documents;

public class DocumentUploadValidator : IDocumentUploadValidator
{
    private readonly DocumentUploadOptions _options;

    private readonly ISupportedDocumentTypesProvider
        _supportedDocumentTypesProvider;

    public DocumentUploadValidator(
        DocumentUploadOptions options,
        ISupportedDocumentTypesProvider supportedDocumentTypesProvider)
    {
        _options = options;

        _supportedDocumentTypesProvider =
            supportedDocumentTypesProvider;
    }

    public DocumentUploadValidationResult Validate(
        DocumentUploadValidationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return DocumentUploadValidationResult.Failure(
                "FileRequired",
                "A document file is required.");
        }

        var sanitizedFileName =
            Path.GetFileName(request.FileName);

        if (string.IsNullOrWhiteSpace(sanitizedFileName))
        {
            return DocumentUploadValidationResult.Failure(
                "InvalidFileName",
                "The uploaded file name is invalid.");
        }

        if (sanitizedFileName.Length > _options.MaxFileNameLength)
        {
            return DocumentUploadValidationResult.Failure(
                "FileNameTooLong",
                $"File name must be {_options.MaxFileNameLength} characters or fewer.");
        }

        if (request.FileSizeInBytes <= 0)
        {
            return DocumentUploadValidationResult.Failure(
                "EmptyFile",
                "The uploaded file is empty.");
        }

        if (request.FileSizeInBytes > _options.MaxFileSizeInBytes)
        {
            return DocumentUploadValidationResult.Failure(
                "FileTooLarge",
                $"File size must be {_options.MaxFileSizeInBytes} bytes or fewer.");
        }

        if (string.IsNullOrWhiteSpace(request.UploadedByUserId))
        {
            return DocumentUploadValidationResult.Failure(
                "UploadedByUserIdRequired",
                "UploadedByUserId is required.");
        }

        if (request.UploadedByUserId.Length >
            _options.MaxUploadedByUserIdLength)
        {
            return DocumentUploadValidationResult.Failure(
                "UploadedByUserIdTooLong",
                $"UploadedByUserId must be {_options.MaxUploadedByUserIdLength} characters or fewer.");
        }

        var extension =
            Path.GetExtension(sanitizedFileName);

        if (string.IsNullOrWhiteSpace(extension))
        {
            return DocumentUploadValidationResult.Failure(
                "MissingFileExtension",
                "The uploaded file must have a supported file extension.");
        }

        var supportedExtensions =
            _supportedDocumentTypesProvider
                .GetSupportedExtensions();

        if (!supportedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase))
        {
            return DocumentUploadValidationResult.Failure(
                "UnsupportedFileType",
                $"Unsupported file type '{extension}'. Supported file types: {string.Join(", ", supportedExtensions)}.");
        }

        return DocumentUploadValidationResult.Success(
            sanitizedFileName);
    }
}