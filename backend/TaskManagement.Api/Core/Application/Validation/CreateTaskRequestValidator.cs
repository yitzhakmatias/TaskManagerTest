using FluentValidation;
using TaskManagement.Api.Core.Application.Dtos;

namespace TaskManagement.Api.Core.Application.Validation;

/// <summary>
/// Validates the transport-layer request shape. This lives at the application
/// boundary rather than inside TaskService: the service assumes it is only ever
/// given already-valid input, which keeps the core free of HTTP/DTO concerns.
/// </summary>
public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be 200 characters or fewer.");
    }
}
