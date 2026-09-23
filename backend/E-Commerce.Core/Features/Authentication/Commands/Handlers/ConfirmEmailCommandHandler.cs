using CleanArch.Core.Bases;
using CleanArch.Core.Features.Authentication.Commands.Models;
using CleanArch.Core.Features.Email_s;
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
    public class ConfirmEmailCommandHandler : EmailHandlerBase, IRequestHandler<ConfirmEmailCommand,Response<ConfirmEmailResultModel>>
    {
        public ConfirmEmailCommandHandler(IStringLocalizer<SharedResource> localizer, IEmailService emailService, UserManager<User> userManager) : base(localizer, emailService, userManager)
        {
        }

        public async Task<Response<ConfirmEmailResultModel>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return BadRequest<ConfirmEmailResultModel>( null, new() { "user id is not valid"});

            var result = await _userManager.ConfirmEmailAsync(user,request.Token);
            if (!result.Succeeded)
                return BadRequest<ConfirmEmailResultModel>(null, new() { string.Join(',', result.Errors.Select(e => e.Description)) });

            return Success(new ConfirmEmailResultModel()
            {
                IsConfirmed = true,
                Email = user.Email,
            });

        }
    }
}
