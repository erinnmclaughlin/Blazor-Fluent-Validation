using BlazorFluentValidation.Models;
using FluentValidation;

namespace BlazorFluentValidation.Validators
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            // Require name field
            RuleFor(x => x.Name).NotEmpty().WithMessage("You must provide a name for the order.");

            // Require at least one item in the list
            RuleFor(x => x.Items.Count).GreaterThan(0).WithMessage("You must add at least one item to the order.");

            // Each item in the list should also be validated with the OrderItemValidator
            RuleForEach(x => x.Items).SetValidator(new OrderItemValidator());
        }
    }
}
