using CleanArch.Core.Features.Authentication.Commands.Models;
using CleanArch.Data.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authentication.Commands.Validators
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
       
            private readonly IStringLocalizer<SharedResource> _localizer;
            public ResetPasswordCommandValidator(IStringLocalizer<SharedResource> localizer)
            {
                _localizer = localizer;

                RuleFor(user => user.Email)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage(_localizer[SharedResourcesKeys.Required]);

               
            }

}
}
