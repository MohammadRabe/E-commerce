using E_commerce.Core.Features.Products.Queries.Models;
using FluentValidation;

namespace E_commerce.Core.Features.Products.Queries.Validators;

public sealed class GetProductsQueryValidator : AbstractValidator<GetPagedProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(query => query.PageNumber).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
