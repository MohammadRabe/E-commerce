using CleanArch.Core.Bases;
using CleanArch.Core.Features.User_s.Commands.Models;
using CleanArch.Data.Entities.Identity;
using CleanArch.Data.Localization;
using CleanArch.Service.Abstract;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.User_s.Commands.Handlers
{
    public class ChangeUserPasswordCommandHandler : UserHandlerBase, IRequestHandler<ChangeUserPasswordCommand, Response<string>>
    {
        public ChangeUserPasswordCommandHandler(UserManager<User> userManager, IStringLocalizer<SharedResource> localizer, IAuthenticationService userService) : base(userManager, localizer, userService)
        {
        }

        public async Task<Response<string>> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null) return BadRequest<string>(null,new List<string>() { SharedResourcesKeys.UserNotFound });

            if (!await _userManager.CheckPasswordAsync(user, request.OldPassword)) 
                return BadRequest<string>(null, new() { _localizer[SharedResourcesKeys.WrongPassword] });

            var result = await _userManager.ChangePasswordAsync(user,request.OldPassword,request.NewPassword);
            if (!result.Succeeded)
                return InternalServerError<string>(string.Join(", ", result.Errors.Select(e => e.Description)));
            return Created($"Your new Password is {request.NewPassword}");
        }
    }
}
