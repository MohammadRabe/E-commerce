using CleanArch.Core.Bases;
using CleanArch.Core.Features.Authorization.Commands.Models;
using CleanArch.Core.Features.User_s.Commands.Models;
using CleanArch.Data.Entities.Identity;
using CleanArch.Data.Localization;
using CleanArch.Data.ResultModels;
using CleanArch.Service.Abstract;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authorization.Commands.Handlers
{
    public class AddUserToRoleCommandHandler : AuthorizationHandlerBase, IRequestHandler<AddUserToRoleCommand, Response<EditUserRoleResultModel>>
    {
        public AddUserToRoleCommandHandler(IAuthorizationServices authService,UserManager<User> userManager,RoleManager<Role> roleManager, IStringLocalizer<SharedResource> localizer) : base(authService, localizer, userManager, roleManager )
        {
        }

        public async Task<Response<EditUserRoleResultModel>> Handle(AddUserToRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return BadRequest<EditUserRoleResultModel>(null, new() { "user id is not valid" });

            var role = await _roleManager.Roles.FirstOrDefaultAsync(role => role.Id.Equals(request.RoleId));
            if(role == null)
                return BadRequest<EditUserRoleResultModel>(null, new() { "role id is not valid" });

            if(await _userManager.IsInRoleAsync(user,role.Name))
                return BadRequest<EditUserRoleResultModel>(null, new() { "user already has the role" });

            var result =await _userManager.AddToRoleAsync(user, role.Name);
            if(!result.Succeeded)
                return BadRequest<EditUserRoleResultModel>(null, new() { string.Join(',',result.Errors.Select(er => er.Description)) });

            return Success(new EditUserRoleResultModel()
            {
                UserRolesUpdated = true,
                EditedRole = role.Name,
                UserId = user.Id.ToString()
            });
        }
    }
}
