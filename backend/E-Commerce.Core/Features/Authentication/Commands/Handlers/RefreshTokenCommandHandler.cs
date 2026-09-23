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
    public class RefreshTokenCommandHandler : SignInHandlerBase, IRequestHandler<RefreshTokenCommand, Response<JwtResultModel>>
    {


        public RefreshTokenCommandHandler(IStringLocalizer<SharedResource> localizer, SignInManager<User> signInManager, UserManager<User> userManager, IAuthenticationService authenticationService) : base(localizer, signInManager, userManager, authenticationService)
        {
        }

        public async Task<Response<JwtResultModel>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var result = await _authenticationService.RotateRefreshToken(request.RefreshToken);
            if (!result.IsValidToken)
                return BadRequest<JwtResultModel>(null, new() { _localizer[SharedResourcesKeys.InvalidToken] });
            return Success(result);
        }
    }
}
