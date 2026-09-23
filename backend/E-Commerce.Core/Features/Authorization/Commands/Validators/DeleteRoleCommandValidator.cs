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
    public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
    {
        protected readonly IAuthorizationServices _authService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public DeleteRoleCommandValidator(IAuthorizationServices authService, IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _localizer = localizer;

            RuleFor(req => req.RoleId)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.Required])
                .MustAsync(async (roleId, cancellation) => await _authService.IsRoleIdExist(roleId))
                .WithMessage(_localizer[SharedResourcesKeys.NotFound]);
        }
    }
}
