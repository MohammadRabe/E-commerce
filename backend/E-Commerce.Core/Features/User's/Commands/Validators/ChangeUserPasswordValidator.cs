using CleanArch.Core.Features.Student_s.Commands.Models;
using CleanArch.Core.Features.User_s.Commands.Models;
using CleanArch.Data.Localization;
using CleanArch.Service.Abstract;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.User_s.Commands.Validators
{
    public class ChangeUserPasswordValidator : AbstractValidator<ChangeUserPasswordCommand>
    {
        
        private readonly IStringLocalizer<SharedResource> _localizer;
        public ChangeUserPasswordValidator( IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;

            RuleFor(user => user.Id)
                .NotNull()
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage(_localizer[SharedResourcesKeys.Required]);

            RuleFor(user => user.OldPassword)
                .NotNull()
                .NotEmpty()
                .WithMessage(_localizer[SharedResourcesKeys.Required]);

            RuleFor(user => user.NewPassword)
            .NotNull()
            .NotEmpty()
            .Must((user,p)=> p != user.OldPassword)
            .WithMessage(_localizer[SharedResourcesKeys.ChooseDifferentPassword]);
         }
    }
}
