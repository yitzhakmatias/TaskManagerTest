namespace TaskManagement.Api.Core.Ports;

/// <summary>
/// Primary (driving) port for authentication. Deliberately synchronous - the
/// only current implementation checks against a config-bound demo credential,
/// no I/O involved. A real user store (DB-backed, password-hashed) would most
/// likely make this async; that's an interface change, not an architectural one.
/// </summary>
public interface IAuthService
{
    /// <summary>Returns a token if the credentials are valid, otherwise null.</summary>
    AuthToken? Authenticate(string username, string password);
}
