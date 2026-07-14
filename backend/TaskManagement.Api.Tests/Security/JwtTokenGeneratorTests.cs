using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using TaskManagement.Api.Adapters.Security;

namespace TaskManagement.Api.Tests.Security;

[TestFixture]
public class JwtTokenGeneratorTests
{
    private static readonly JwtOptions Options = new()
    {
        Issuer = "TaskManagement.Api.Tests",
        Audience = "TaskManagement.Client.Tests",
        SigningKey = "test-only-signing-key-not-for-real-use-1234567890",
        ExpiryMinutes = 30
    };

    private JwtTokenGenerator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new JwtTokenGenerator(Microsoft.Extensions.Options.Options.Create(Options));
    }

    [Test]
    public void GenerateToken_ProducesAValidatable_CorrectlyClaimedToken()
    {
        var token = _sut.GenerateToken("admin");

        Assert.That(token.Value, Is.Not.Null.And.Not.Empty);
        Assert.That(token.ExpiresAtUtc, Is.GreaterThan(DateTime.UtcNow));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Options.Issuer,
            ValidateAudience = true,
            ValidAudience = Options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.SigningKey)),
            ValidateLifetime = true,
        };

        var principal = new JwtSecurityTokenHandler().ValidateToken(token.Value, validationParameters, out _);

        Assert.That(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), Is.EqualTo("admin"));
    }

    [Test]
    public void GenerateToken_FailsValidation_WithTheWrongSigningKey()
    {
        var token = _sut.GenerateToken("admin");

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Options.Issuer,
            ValidateAudience = true,
            ValidAudience = Options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("a-completely-different-signing-key-0987654321")),
            ValidateLifetime = true,
        };

        Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(token.Value, validationParameters, out _));
    }
}
