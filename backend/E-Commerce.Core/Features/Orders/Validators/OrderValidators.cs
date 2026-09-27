using E_commerce.Core.Features.Orders.Models;
using E_commerce.Data.Enum;
using FluentValidation;

namespace E_commerce.Core.Features.Orders.Validators;

public sealed class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Order.ShippingAddress).NotEmpty().MaximumLength(500);
        RuleFor(command => command.Order.ShippingPhoneNumber)
            .Must(phone => !string.IsNullOrWhiteSpace(phone)).WithMessage("Shipping phone number is required.")
            .MaximumLength(30);
        RuleFor(command => command.Order.Items).NotNull().Must(items => items.Count > 0)
            .WithMessage("An order must contain at least one item.");
        RuleForEach(command => command.Order.Items).ChildRules(item =>
        {
            item.RuleFor(line => line.ProductId).GreaterThan(0);
            item.RuleFor(line => line.Quantity).GreaterThan(0);
        });
        RuleFor(command => command.Order.Items).Must(items => items.Select(item => item.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Each product can only appear once in an order.");
    }
}

public sealed class DeleteOrderValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderValidator()
    {
        RuleFor(command => command.OrderId).GreaterThan(0);
        RuleFor(command => command.UserId).NotEmpty();
    }
}

public sealed class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(command => command.OrderId).GreaterThan(0);
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Status).IsInEnum();
    }
}
