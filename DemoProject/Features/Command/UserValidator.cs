using FluentValidation;
using DemoProject.Models;

namespace DemoProject.Features.Command;

public class UserValidator :AbstractValidator<CreateUserCommand>
{
    public UserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("User Name is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}