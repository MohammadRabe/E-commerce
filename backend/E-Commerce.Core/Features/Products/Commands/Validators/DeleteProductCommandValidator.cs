using E_commerce.Core.Features.Products.Commands.Models;
using FluentValidation;

namespace E_commerce.Core.Features.Products.Commands.Validators;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator() => RuleFor(command => command.Id).GreaterThan(0);
}
