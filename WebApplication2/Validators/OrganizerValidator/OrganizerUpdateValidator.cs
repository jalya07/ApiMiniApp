using FluentValidation;
using WebApplication2.Dtos.OrganizerDto;

namespace WebApplication2.Validators;

public class OrganizerUpdateValidator:AbstractValidator<OrganizerUpdateDto>
{
    public OrganizerUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
 
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
 
        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.")
            .When(x => x.Phone != null);
    }
}