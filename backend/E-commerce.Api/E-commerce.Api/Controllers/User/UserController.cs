using CleanArch.Api.Controllers.Base;
using CleanArch.Core.Features.User_s.Commands.Models;
using CleanArch.Core.Features.User_s.Queries;
using CleanArch.Core.Features.User_s.Queries.Models;
using CleanArch.Data.Routing;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.Api.Controllers.User
{
    [ApiController]
    public class UserController : AppControllerBase
    {
        public UserController(IMediator mediator) : base(mediator) { }

        [HttpPost(Router.Version1.User.AddUser)]
        public async Task<IActionResult> AddUser([FromBody] AddUserCommand userModel) =>
            NewResult(await _mediator.Send(userModel));

        [HttpGet(Router.Version1.User.GetPagedUsers)]
        public async Task<IActionResult> GetPagedUsers([FromQuery] int pageNumber, [FromQuery] int pageSize) =>
            NewResult(await _mediator.Send(new GetPagedUsersQuery { PageNumber = pageNumber, PageSize = pageSize }));

        [HttpGet(Router.Version1.User.GetById)]
        public async Task<IActionResult> GetUserById(int id) =>
            NewResult(await _mediator.Send(new GetUserByIdQuery { Id = id }));
    }
}
