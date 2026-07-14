using Microsoft.Extensions.Options;
using TaskManagement.Api.Core.Ports;

namespace TaskManagement.Api.Core.Application;

public class AuthService : IAuthService
{
    private readonly DemoUserOptions _demoUser;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(IOptions<DemoUserOptions> demoUser, IJwtTokenGenerator tokenGenerator)
    {
        _demoUser = demoUser.Value;
        _tokenGenerator = tokenGenerator;
    }

    public AuthToken? Authenticate(string username, string password)
    {
        var isValid = string.Equals(username, _demoUser.Username, StringComparison.Ordinal)
            && string.Equals(password, _demoUser.Password, StringComparison.Ordinal);

        return isValid ? _tokenGenerator.GenerateToken(username) : null;
    }
}
