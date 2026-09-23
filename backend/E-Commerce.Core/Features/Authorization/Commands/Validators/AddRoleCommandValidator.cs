using CleanArch.Core.Features.Authorization.Commands.Models;
using CleanArch.Data.Localization;
using CleanArch.Service.Abstract;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authorization.Commands.Validators
{
    public class AddRoleCommandValidator : AbstractValidator<AddRoleCommand>
    {
        protected readonly IAuthorizationServices _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public AddRoleCommandValidator(IAuthorizationServices authService, IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _localizer = localizer;

            RuleFor(req => req.RoleName)
                .NotEmpty().WithMessage(_localizer["RoleNameRequired"])
                .MaximumLength(50).WithMessage(_localizer["RoleNameMaxLength"])
                .MustAsync(async (roleName, cancellation) => !await _authService.IsRoleNameExist(roleName))
                .WithMessage(_localizer[SharedResourcesKeys.RoleNameAlreadyExists]);
        }
    }
}
