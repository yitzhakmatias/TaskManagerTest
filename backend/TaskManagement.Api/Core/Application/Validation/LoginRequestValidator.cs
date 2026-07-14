using FluentValidation;
using TaskManagement.Api.Core.Application.Dtos;

namespace TaskManagement.Api.Core.Application.Validation;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
