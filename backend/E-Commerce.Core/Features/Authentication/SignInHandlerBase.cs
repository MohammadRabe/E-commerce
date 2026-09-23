using CleanArch.Core.Bases;
using CleanArch.Data.Entities.Identity;
using CleanArch.Data.Localization;
using CleanArch.Service.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authentication
{
    public class SignInHandlerBase : ResponseHandler
    {
        protected readonly SignInManager<User> _signInManager;
        protected readonly UserManager<User> _userManager;
        protected readonly IAuthenticationService _authenticationService;
        public SignInHandlerBase(IStringLocalizer<SharedResource> localizer, SignInManager<User> signInManager, UserManager<User> userManager, IAuthenticationService authenticationService) : base(localizer)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _authenticationService = authenticationService;
        }
    }
}
