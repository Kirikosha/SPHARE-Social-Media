using Application.Features.Users.Commands;
using FluentValidation;

namespace Application.Validators;

public class UpdateUserMainInfoCommandValidator : AbstractValidator<UpdateUserMainInfo.Command>
{
    public UpdateUserMainInfoCommandValidator()
    {
        RuleFor(x => x.MainInfo.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.MainInfo.UniqueNameIdentifier)
            .NotEmpty().WithMessage("Unique name identifier is required.");
    }
}