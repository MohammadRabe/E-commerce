using CleanArch.Core.Bases;
using CleanArch.Data.Entities.Identity;
using CleanArch.Data.Localization;
using CleanArch.Service.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Features.Authorization
{
    public class AuthorizationHandlerBase : ResponseHandler
    {
        protected readonly IAuthorizationServices _authService;
        protected readonly UserManager<User> _userManager;
        protected readonly RoleManager<Role> _roleManager;

        public AuthorizationHandlerBase(IAuthorizationServices authService, IStringLocalizer<SharedResource> localizer, UserManager<User> userManager, RoleManager<Role> roleManager) : base(localizer)
        {
            _authService = authService;
            _userManager = userManager;
            _roleManager = roleManager;
        }
    }
}
