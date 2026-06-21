namespace AiKnowledgeCopilot.Application.Documents;

public class DocumentUploadValidationResult
{
    private DocumentUploadValidationResult(
        bool isValid,
        string? sanitizedFileName,
        string? errorCode,
        string? errorMessage)
    {
        IsValid = isValid;
        SanitizedFileName = sanitizedFileName;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsValid { get; }

    public string? SanitizedFileName { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public static DocumentUploadValidationResult Success(
        string sanitizedFileName)
    {
        return new DocumentUploadValidationResult(
            true,
            sanitizedFileName,
            null,
            null);
    }

    public static DocumentUploadValidationResult Failure(
        string errorCode,
        string errorMessage)
    {
        return new DocumentUploadValidationResult(
            false,
            null,
            errorCode,
            errorMessage);
    }
}