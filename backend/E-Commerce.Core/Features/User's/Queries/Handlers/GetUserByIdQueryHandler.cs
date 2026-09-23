using CleanArch.Core.Bases;
using CleanArch.Core.Features.User_s.Commands.Handlers;
using CleanArch.Core.Features.User_s.Queries.Models;
using CleanArch.Data.Dtos.Users;
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

namespace CleanArch.Core.Features.User_s.Queries.Handlers
{
    public class GetUserByIdQueryHandler : UserHandlerBase, IRequestHandler<GetUserByIdQuery, Response<GetUserByIdDto>>
    {
        public GetUserByIdQueryHandler(UserManager<User> userManager, IStringLocalizer<SharedResource> localizer, IAuthenticationService userService) : base(userManager, localizer, userService)
        {
        }

        public async Task<Response<GetUserByIdDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null) return BadRequest<GetUserByIdDto>(null, new List<string>() { _localizer[SharedResourcesKeys.UserNotFound] });
            return Success(user.Adapt<GetUserByIdDto>());
        }
    }
}
