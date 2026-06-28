using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AiKnowledgeCopilot.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace AiKnowledgeCopilot.API.Security;

public class DevelopmentJwtTokenService
    : IDevelopmentJwtTokenService
{
    private readonly JwtOptions _jwtOptions;

    public DevelopmentJwtTokenService(
        JwtOptions jwtOptions)
    {
        _jwtOptions = jwtOptions;
    }

    public DevelopmentTokenResponse CreateToken(
        DevelopmentTokenRequest request)
    {
        ValidateRequest(request);

        var userId =
            request.UserId.Trim();

        var role =
            request.Role.Trim();

        var now =
            DateTime.UtcNow;

        var expiresAtUtc =
            now.AddMinutes(
                _jwtOptions.ExpirationMinutes);

        var claims =
            new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new("sub", userId),
                new(ClaimTypes.Name, request.DisplayName.Trim()),
                new(ClaimTypes.Role, role)
            };

        var signingKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _jwtOptions.SigningKey));

        var signingCredentials =
            new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: now,
                expires: expiresAtUtc,
                signingCredentials: signingCredentials);

        var accessToken =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return new DevelopmentTokenResponse
        {
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAtUtc,
            UserId = userId,
            Role = role
        };
    }

    private static void ValidateRequest(
        DevelopmentTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new ArgumentException(
                "UserId is required.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            throw new ArgumentException(
                "Role is required.",
                nameof(request));
        }
    }
}