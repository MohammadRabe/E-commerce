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
    public class EditRoleCommandValidator : AbstractValidator<EditRoleCommand>
    {
        protected readonly IAuthorizationServices _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public EditRoleCommandValidator(IAuthorizationServices authService, IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _localizer = localizer;

            RuleFor(req => req.RoleId)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.Required]);

            RuleFor(req => req.RoleName)
                .NotEmpty().WithMessage(_localizer["RoleNameRequired"])
                .MaximumLength(50).WithMessage(_localizer["RoleNameMaxLength"])
                .MustAsync(async (role,roleName, cancellation) => !await _authService.IsRoleNameExistForEdit(roleName, role.RoleId))
                .WithMessage(_localizer[SharedResourcesKeys.RoleNameAlreadyExists]);
        }
    }
}
