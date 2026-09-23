using CleanArch.Core.Features.Authentication.Commands.Models;
using CleanArch.Core.Features.Student_s.Commands.Models;
using CleanArch.Data.Entities;
using CleanArch.Data.Localization;
using CleanArch.Service.Abstract;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace CleanArch.Core.Features.Authentication.Commands.Validators
{
    public class SignInValidator : AbstractValidator<SignInCommand>
    {
        private readonly IStringLocalizer<SharedResource> _localizer;
        public SignInValidator(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;

            RuleFor(user => user.UserName)
                .NotNull()
                .NotEmpty()
                .WithMessage(_localizer[SharedResourcesKeys.Required]);

            RuleFor(user => user.Password)
                .NotNull()
                .NotEmpty()
                .WithMessage(_localizer[SharedResourcesKeys.Required]);
        }
    }
}
