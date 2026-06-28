using AiKnowledgeCopilot.API.Models;
using AiKnowledgeCopilot.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiKnowledgeCopilot.API.Controllers;

[ApiController]
[Route("dev/auth")]
public class DevelopmentAuthController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    private readonly IDevelopmentJwtTokenService
        _tokenService;

    public DevelopmentAuthController(
        IWebHostEnvironment environment,
        IDevelopmentJwtTokenService tokenService)
    {
        _environment =
            environment;

        _tokenService =
            tokenService;
    }

    [AllowAnonymous]
    [HttpPost("token")]
    public ActionResult<DevelopmentTokenResponse> CreateToken(
        DevelopmentTokenRequest request)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var response =
            _tokenService.CreateToken(request);

        return Ok(response);
    }
}