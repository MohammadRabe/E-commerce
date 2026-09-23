using CleanArch.Core.Bases;
using CleanArch.Core.Features.Authorization.Commands.Models;
using CleanArch.Data.Entities.Identity;
using CleanArch.Data.Localization;
using CleanArch.Data.ResultModels;
using CleanArch.Service.Abstract;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authorization.Commands.Handlers
{
    public class DeleteRoleCommandHandler : AuthorizationHandlerBase, IRequestHandler<DeleteRoleCommand, Response<DeleteRoleResultModel>>
    {
        public DeleteRoleCommandHandler(IAuthorizationServices authService, UserManager<User> userManager, RoleManager<Role> roleManager, IStringLocalizer<SharedResource> localizer) : base(authService, localizer, userManager, roleManager)
        {
        }

        public async Task<Response<DeleteRoleResultModel>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.DeleteRoleAsync(request.RoleId);
            if (!result.IsDeleted)
                return BadRequest(result,new(){result.Errors});
            return Deleted(result,$"{result.Role} is deleted");
        }
    }
}
