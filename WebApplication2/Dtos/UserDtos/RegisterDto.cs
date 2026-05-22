using FluentValidation;

namespace WebApplication2.Dtos.UserDtos;

public class RegisterDto
{
    public string FullName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("Username is required");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid email is required");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters long");
    }
}