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
    public class ConfirmCodeQueryHandler : SignInHandlerBase, IRequestHandler<ConfirmCodeQuery, Response<ConfirmCodeResultModel>>
    {
        public ConfirmCodeQueryHandler(IStringLocalizer<SharedResource> localizer, SignInManager<User> signInManager, UserManager<User> userManager, IAuthenticationService authenticationService) : base(localizer, null, userManager, authenticationService)
        {
        }

        public async Task<Response<ConfirmCodeResultModel>> Handle(ConfirmCodeQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email.ToString());
            if (user == null)
                return BadRequest<ConfirmCodeResultModel>(null, new() { "user id is not valid" });

            var result = await _authenticationService.ConfirmCodeAsync(user, request.Code);
            if (!result.CodeMatched)
                return BadRequest<ConfirmCodeResultModel>(null, new() { result.Error});

            return Success(result);
        }
    }
}
