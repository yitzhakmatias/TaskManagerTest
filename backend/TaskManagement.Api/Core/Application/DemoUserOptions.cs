namespace TaskManagement.Api.Core.Application;

/// <summary>
/// Config-bound stand-in for a real user store. There is deliberately no
/// Users table / password hashing / registration flow here - see the README
/// trade-offs section. Swapping this for a real store means implementing
/// IAuthService differently; nothing else in the app needs to change.
/// </summary>
public class DemoUserOptions
{
    public const string SectionName = "DemoUser";

    public required string Username { get; init; }

    public required string Password { get; init; }
}
