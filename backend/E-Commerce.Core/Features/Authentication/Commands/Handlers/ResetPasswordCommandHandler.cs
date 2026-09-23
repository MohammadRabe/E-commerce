using CleanArch.Core.Bases;
using CleanArch.Core.Features.Authentication.Commands.Models;
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

namespace CleanArch.Core.Features.Authentication.Commands.Handlers
{
    public class ResetPasswordCommandHandler : SignInHandlerBase, IRequestHandler<ResetPasswordCommand, Response<ResetPasswordResultModel>>
    {
        public ResetPasswordCommandHandler(IStringLocalizer<SharedResource> localizer, SignInManager<User> signInManager, UserManager<User> userManager, IAuthenticationService authenticationService) : base(localizer, null, userManager, authenticationService)
        {
        }

        public async Task<Response<ResetPasswordResultModel>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return BadRequest(new ResetPasswordResultModel()
                {
                    IsCodeSent = false,
                    Error = "User is not found"
                });
            var result = await _authenticationService.SendResetPasswordCodeAsync(user);
            if (!result.IsCodeSent)
                return BadRequest(new ResetPasswordResultModel()
                {
                    IsCodeSent = false,
                    Error = result.Error
                });

            return Success(result);
        }
    }
}
