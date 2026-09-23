
using CleanArch.Core.Bases;
using CleanArch.Core.Features.Authorization.Commands.Models;
using CleanArch.Data.Entities.Identity;
using CleanArch.Data.Localization;
using CleanArch.Service.Abstract;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace CleanArch.Core.Features.Authorization.Commands.Handlers
{
    public class AddCommandHandler : AuthorizationHandlerBase, IRequestHandler<AddRoleCommand, Response<string>>
    {
        public AddCommandHandler(IAuthorizationServices authService, UserManager<User> userManager, RoleManager<Role> roleManager, IStringLocalizer<SharedResource> localizer) : base(authService, localizer, userManager, roleManager)
        {
        }

        public async Task<Response<string>> Handle(AddRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _authService.AddRoleAsync(request.RoleName);
            if (!result.IsSuccess)
                return BadRequest<string>(null, new() { result.Error!});

            return Created(result.Message);
        }
    }
}
