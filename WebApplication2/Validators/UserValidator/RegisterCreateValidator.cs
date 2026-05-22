using FluentValidation;
using WebApplication2.Dtos.UserDtos;

namespace WebApplication2.Validators.UserValidator;

public class RegisterCreateValidator:AbstractValidator<RegisterDto> 
{
    public RegisterCreateValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters");
        RuleFor(x => x.FullName).NotEmpty().WithMessage("FullName is required")
            .MinimumLength(3).WithMessage("FullName must be at least 3 characters long")
            .MaximumLength(100).WithMessage("FullName must not exceed 100 characters");;
    }
}