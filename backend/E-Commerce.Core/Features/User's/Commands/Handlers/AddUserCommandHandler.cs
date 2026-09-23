
using CleanArch.Core.Bases;
using CleanArch.Core.Features.User_s.Commands.Models;
using CleanArch.Data.Entities.Identity;
using CleanArch.Data.Localization;
using CleanArch.Data.ResultModels;
using CleanArch.Service.Abstract;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.User_s.Commands.Handlers
{
    public class AddUserCommandHandler : UserHandlerBase, IRequestHandler<AddUserCommand, Response<CreateUserResultModel>>
    {
        public AddUserCommandHandler(UserManager<User> userManager, IStringLocalizer<SharedResource> localizer, IAuthenticationService userService) : base(userManager, localizer, userService)
        {
        }

        public async Task<Response<CreateUserResultModel>> Handle(AddUserCommand request, CancellationToken cancellationToken)
        {
            
            var result = await _userService.AddUserAsync(request.Adapt<User>(), request.Password);

            if (!result.IsSuccess)
            {
                return BadRequest<CreateUserResultModel>(null,new() { result.Error });
            }
            return Created(result);
        }
    }
}
