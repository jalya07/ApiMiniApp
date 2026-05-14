using FluentValidation;
using WebApplication2.Dtos.TicketDto;

namespace WebApplication2.Validators;

public class TicketCreateValidator:AbstractValidator<TicketCreateDto>
{
    public TicketCreateValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Ticket type is required.")
            .MaximumLength(50).WithMessage("Type must not exceed 50 characters.");
 
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be a positive value.");
 
        RuleFor(x => x.QuantityAvailable)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be non-negative.");
    }
}