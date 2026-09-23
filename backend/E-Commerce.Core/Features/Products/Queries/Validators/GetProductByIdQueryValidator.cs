using E_commerce.Core.Features.Products.Queries.Models;
using FluentValidation;

namespace E_commerce.Core.Features.Products.Queries.Validators;

public sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator() => RuleFor(query => query.Id).GreaterThan(0);
}
