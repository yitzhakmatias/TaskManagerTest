using FluentValidation;
using TaskManagement.Api.Core.Application.Dtos;
using TaskManagement.Api.Core.Ports;

namespace TaskManagement.Api.Adapters.Http;

/// <summary>
/// Primary (driving) adapter for authentication. Thin, same shape as
/// TaskFunctions: validate the request DTO, delegate to the port, shape the response.
/// </summary>
public static class AuthFunctions
{
    public static RouteGroupBuilder MapAuthFunctions(this RouteGroupBuilder group)
    {
        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Authenticates with a username/password and returns a JWT bearer token.")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem()
            .AllowAnonymous();

        return group;
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IAuthService authService,
        IValidator<LoginRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var token = authService.Authenticate(request.Username, request.Password);
        if (token is null)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(new LoginResponse(token.Value, token.ExpiresAtUtc));
    }
}
