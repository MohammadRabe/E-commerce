using CleanArch.Core.Bases;
using CleanArch.Core.Features.Authorization.Queries.Models;
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

namespace CleanArch.Core.Features.Authorization.Queries.Handlers
{
    public class ManageUserClaimsQueryHandler : AuthorizationHandlerBase, IRequestHandler<ManageUserClaimsQuery, Response<ManageUserClaimsResultModel>>
    {
        public ManageUserClaimsQueryHandler(IAuthorizationServices authService, UserManager<User> userManager, RoleManager<Role> roleManager, IStringLocalizer<SharedResource> localizer) : base(authService, localizer, userManager, roleManager)
        {
        }

        public async Task<Response<ManageUserClaimsResultModel>> Handle(ManageUserClaimsQuery request, CancellationToken cancellationToken)
        {
            var result = await _authService.GetManageUserClaimsAsync(request.UserId);
            if (!result.IsSuccess)
                return NotFound<ManageUserClaimsResultModel>(null, new() { result.Error});

            return Success(result);
        }
    }
}
