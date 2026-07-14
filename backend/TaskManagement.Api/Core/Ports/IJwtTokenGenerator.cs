namespace TaskManagement.Api.Core.Ports;

/// <summary>Issued bearer token plus its expiry, so callers can tell the client when to re-authenticate.</summary>
public record AuthToken(string Value, DateTime ExpiresAtUtc);

/// <summary>
/// Secondary (driven) port for minting bearer tokens. Kept separate from
/// IAuthService so the credential check (business rule) and the token
/// format/signing (an infrastructure concern - JWT today, could be anything
/// else tomorrow) can vary independently.
/// </summary>
public interface IJwtTokenGenerator
{
    AuthToken GenerateToken(string username);
}
