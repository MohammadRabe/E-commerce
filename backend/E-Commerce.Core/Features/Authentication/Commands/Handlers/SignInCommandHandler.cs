using CleanArch.Core.Bases;
using CleanArch.Core.Features.Authentication.Commands.Models;
using CleanArch.Data.Entities.Identity;
using CleanArch.Data.Localization;
using CleanArch.Data.ResultModels;
using CleanArch.Service.Abstract;
using FluentValidation.Validators;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CleanArch.Core.Features.Authentication.Commands.Handlers
{
    public class SignInCommandHandler : SignInHandlerBase, IRequestHandler<SignInCommand, Response<SignInResultModel>>
    {
        public SignInCommandHandler(IStringLocalizer<SharedResource> localizer, SignInManager<User> signInManager, UserManager<User> userManager, IAuthenticationService authenticationService) : base(localizer, signInManager, userManager, authenticationService)
        {
        }

        public async Task<Response<SignInResultModel>> Handle(SignInCommand request, CancellationToken cancellationToken)
        {
            var user = new EmailAddressAttribute().IsValid(request.UserName)?
                  await _userManager.FindByEmailAsync(request.UserName) 
                : await _userManager.FindByNameAsync(request.UserName);

            if (user == null) 
                return BadRequest<SignInResultModel>(null, new() { _localizer[SharedResourcesKeys.InvalidCredentials] });
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if(!result.Succeeded)
                return BadRequest<SignInResultModel>(null, new() { _localizer[SharedResourcesKeys.InvalidCredentials] });

            // token 
            var resultModel = new SignInResultModel
            {
                UserName = user.UserName,
                AccessToken = await _authenticationService.GetAccessToken(user),
                RefreshToken = await _authenticationService.GetRefreshToken(user)
            };
            return Success(resultModel);
        }
    }
}
