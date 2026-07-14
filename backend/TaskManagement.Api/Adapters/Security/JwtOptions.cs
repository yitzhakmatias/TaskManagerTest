namespace TaskManagement.Api.Adapters.Security;

/// <summary>
/// Config-bound JWT signing settings. SigningKey is read from appsettings for
/// dev convenience only - see README trade-offs: a real deployment should
/// pull this from an environment variable / secret manager, never source control.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public required string SigningKey { get; init; }

    public int ExpiryMinutes { get; init; } = 60;
}
