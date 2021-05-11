using BlazorFluentValidation.Models;
using FluentValidation;

namespace BlazorFluentValidation.Validators
{
    public class OrderItemValidator : AbstractValidator<OrderItem>
    {
        public OrderItemValidator()
        {
            // Quantity is required and must be greater than 0
            RuleFor(x => x.Quantity).NotNull().WithMessage("You must enter a quantity.")
                .GreaterThan(0).WithMessage("Invalid quantity.");

            // Description is required and must be between 3 and 50 characters
            RuleFor(x => x.Description).NotEmpty().WithMessage("You must provide a description for the item.")
                .MinimumLength(3).WithMessage("Item description must be at least 3 characaters.")
                .MaximumLength(50).WithMessage("Item description must be less than 50 characters.");
        }
    }
}
