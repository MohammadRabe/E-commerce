using CleanArch.Core.Bases;
using CleanArch.Data.Entities.Identity;
using CleanArch.Data.Localization;
using CleanArch.Service.Abstract;
using CleanArch.Service.AuthenticationServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.User_s.Commands.Handlers
{
    public class UserHandlerBase : ResponseHandler
    {
        protected readonly UserManager<User> _userManager;
        protected readonly IAuthenticationService _userService;

        public UserHandlerBase(UserManager<User> userManager, IStringLocalizer<SharedResource> localizer, IAuthenticationService userService) : base(localizer)
        {
            _userManager = userManager;
            _userService = userService;
        }
    }
}
