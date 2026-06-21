namespace AiKnowledgeCopilot.Application.Documents;

public interface IDocumentUploadValidator
{
    DocumentUploadValidationResult Validate(
        DocumentUploadValidationRequest request);
}