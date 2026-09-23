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
    public class AddUserValidator : AbstractValidator<AddUserCommand>
    {
        protected readonly IStudentService _stdService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public AddUserValidator(IStudentService stdService, IStringLocalizer<SharedResource> localizer)
        {
            _stdService = stdService;
            _localizer = localizer;

            RuleFor(student => student.UserName)
                .NotEmpty()
                .WithMessage(_localizer[SharedResourcesKeys.Required]);

            RuleFor(student => student.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                ;

            RuleFor(user => user.Password)
                .Must((user, password) => password == user.ConfirmPassword)
                .WithMessage("Password doesn't match");
        }
    }
}
