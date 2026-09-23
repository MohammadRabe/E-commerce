using CleanArch.Api.Controllers.Base;
using CleanArch.Core.Features.Authorization.Commands.Models;
using CleanArch.Core.Features.Authorization.Queries.Models;
using CleanArch.Data.Dtos.Claims_;
using CleanArch.Data.Routing;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.Api.Controllers.Authorization
{
    [ApiController]
    public class AuthorizationController : AppControllerBase
    {
        public AuthorizationController(IMediator mediator) : base(mediator) { }

        [HttpPost(Router.Version1.Authorization.AddRole)]
        public async Task<IActionResult> AddRole(string name) =>
            NewResult(await _mediator.Send(new AddRoleCommand { RoleName = name }));

        [HttpPost(Router.Version1.Authorization.EditRole)]
        public async Task<IActionResult> EditRole([FromBody] EditRoleCommand request) =>
            NewResult(await _mediator.Send(request));

        [HttpPost(Router.Version1.Authorization.DeleteRole)]
        public async Task<IActionResult> DeleteRole(int roleId) =>
            NewResult(await _mediator.Send(new DeleteRoleCommand { RoleId = roleId }));

        [HttpGet(Router.Version1.Authorization.GetAllRoles)]
        public async Task<IActionResult> GetAllRoles() => NewResult(await _mediator.Send(new RolesListQuery()));

        [HttpGet(Router.Version1.Authorization.GetRoleById)]
        public async Task<IActionResult> GetRoleById(int roleId) =>
            NewResult(await _mediator.Send(new RoleByIdQuery { Id = roleId }));

        [HttpGet(Router.Version1.Authorization.GetManageUserRoleList)]
        public async Task<IActionResult> GetManageUserRoleList(int userId) =>
            NewResult(await _mediator.Send(new ManageUserRoleListQuery { UserId = userId }));

        [HttpGet(Router.Version1.Authorization.GetManageUserClaimsList)]
        public async Task<IActionResult> GetManageUserClaimsList(int userId) =>
            NewResult(await _mediator.Send(new ManageUserClaimsQuery { UserId = userId }));

        [HttpPost(Router.Version1.Authorization.UpdateUserClaims)]
        public async Task<IActionResult> UpdateUserClaims(int userId, List<UserClaimDto> claims) =>
            NewResult(await _mediator.Send(new UpdateUserClaimsCommand { UserId = userId, Claims = claims }));

        [HttpPost(Router.Version1.Authentication.AddUserToRole)]
        public async Task<IActionResult> AddUserToRole(int userId, int roleId) =>
            NewResult(await _mediator.Send(new AddUserToRoleCommand { RoleId = roleId, UserId = userId }));

        [HttpDelete(Router.Version1.Authentication.DeleteUserFromRole)]
        public async Task<IActionResult> DeleteUserFromRole(int userId, int roleId) =>
            NewResult(await _mediator.Send(new DeleteUserFromRoleCommand { RoleId = roleId, UserId = userId }));
    }
}
