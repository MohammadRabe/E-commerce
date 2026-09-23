using E_commerce.Core.Features.Products.Commands.Models;
using FluentValidation;

namespace E_commerce.Core.Features.Products.Commands.Validators;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Title).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Price).GreaterThan(0);
        RuleFor(command => command.CategoryId).GreaterThan(0);
        RuleFor(command => command.Images).NotNull();
        RuleFor(command => command.Rating).InclusiveBetween(0, 5);
        RuleFor(command => command.RatingCount).GreaterThanOrEqualTo(0);
        RuleFor(command => command.StockQuantity).GreaterThanOrEqualTo(0);
    }
}
