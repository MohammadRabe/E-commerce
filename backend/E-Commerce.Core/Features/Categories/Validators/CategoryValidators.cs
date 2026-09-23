using E_commerce.Core.Features.Categories.Models;
using FluentValidation;

namespace E_commerce.Core.Features.Categories.Validators;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
}

public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public sealed class CategoryIdValidator : AbstractValidator<GetCategoryQuery>
{
    public CategoryIdValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
