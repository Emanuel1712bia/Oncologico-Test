using FluentValidation;
using OrderProcessing.Application.Orders.Dtos;

namespace OrderProcessing.Application.Orders.Validation;

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(request => request.Items)
            .NotEmpty()
            .WithMessage("The order must contain at least one item.");

        RuleForEach(request => request.Items).ChildRules(item =>
        {
            item.RuleFor(orderItem => orderItem.ProductId)
                .NotEqual(Guid.Empty)
                .WithMessage("productId must be a valid identifier.");

            item.RuleFor(orderItem => orderItem.Quantity)
                .GreaterThan(0)
                .WithMessage("quantity must be greater than zero.");
        });
    }
}
