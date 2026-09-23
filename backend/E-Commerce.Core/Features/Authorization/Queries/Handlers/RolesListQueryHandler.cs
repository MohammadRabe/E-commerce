using CleanArch.Core.Bases;
using CleanArch.Core.Features.Authorization.Queries.Models;
using CleanArch.Data.Dtos.Role_s;
using CleanArch.Data.Entities.Identity;
using CleanArch.Data.Localization;
using CleanArch.Service.Abstract;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authorization.Queries.Handlers
{
    public class RolesListQueryHandler : AuthorizationHandlerBase, IRequestHandler<RolesListQuery, Response<List<GetRolesListDto>>>
    {
        public RolesListQueryHandler(IAuthorizationServices authService, UserManager<User> userManager, RoleManager<Role> roleManager, IStringLocalizer<SharedResource> localizer) : base(authService, localizer, userManager, roleManager)
        {
        }

        public async Task<Response<List<GetRolesListDto>>> Handle(RolesListQuery request, CancellationToken cancellationToken)
        {
            return Success(await _authService.GetAllRoleAsQueryable().ProjectToType<GetRolesListDto>().ToListAsync());
        }
    }
}
