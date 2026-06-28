using AiKnowledgeCopilot.API.Models;

namespace AiKnowledgeCopilot.API.Security;

public interface IDevelopmentJwtTokenService
{
    DevelopmentTokenResponse CreateToken(
        DevelopmentTokenRequest request);
}