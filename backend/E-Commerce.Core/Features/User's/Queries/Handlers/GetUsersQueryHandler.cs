using CleanArch.Core.Bases;
using CleanArch.Core.Features.User_s.Commands.Handlers;
using CleanArch.Core.Wrappers;
using CleanArch.Data.Dtos.Users;
using CleanArch.Data.Entities.Identity;
using CleanArch.Data.Localization;
using CleanArch.Service.Abstract;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.User_s.Queries.Handlers
{
    public class GetUsersQueryHandler : UserHandlerBase, IRequestHandler<GetPagedUsersQuery, Response<PagedList<GetPagedUsersQueryResponseDto>>>
    {
        public GetUsersQueryHandler(UserManager<User> userManager, IStringLocalizer<SharedResource> localizer, IAuthenticationService userService) : base(userManager, localizer, userService)
        {
        }

        public async Task<Response<PagedList<GetPagedUsersQueryResponseDto>>> Handle(GetPagedUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userManager.Users
                .ProjectToType<GetPagedUsersQueryResponseDto>()
                .ToPagedList(request.PageNumber,request.PageSize);
            Log.Warning($"Someone is looking up the users {DateTime.UtcNow}");
            return Success(users);
        }
    }
}
