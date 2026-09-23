using CleanArch.Core.Bases;
using CleanArch.Core.Features.Authorization.Queries.Models;
using CleanArch.Data.Dtos.Role_s;
using CleanArch.Data.Entities.Identity;
using CleanArch.Data.Localization;
using CleanArch.Service.Abstract;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authorization.Queries.Handlers
{
    public class RoleByIdQueryHandler : AuthorizationHandlerBase, IRequestHandler<RoleByIdQuery, Response<GetRoleByIdDto>>

    {
        public RoleByIdQueryHandler(IAuthorizationServices authService, UserManager<User> userManager, RoleManager<Role> roleManager, IStringLocalizer<SharedResource> localizer) : base(authService, localizer, userManager, roleManager)
        {
        }

        public async Task<Response<GetRoleByIdDto>> Handle(RoleByIdQuery request, CancellationToken cancellationToken)
        {
            var role = await _authService.GetRoleByIdAsync(request.Id);
            return role is not null ?
                Success(role.Adapt<GetRoleByIdDto>()) :
                NotFound<GetRoleByIdDto>(null, new List<string>(){_localizer[SharedResourcesKeys.NotFound]});
        }
    }
}
