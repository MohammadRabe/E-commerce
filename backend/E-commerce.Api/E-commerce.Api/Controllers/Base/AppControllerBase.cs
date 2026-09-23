using E_commerce.Core.Bases;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CleanArch.Api.Controllers.Base
{
    public class AppControllerBase : ControllerBase
    {
        protected readonly IMediator _mediator;

        public AppControllerBase(IMediator mediator) => _mediator = mediator;

        protected ObjectResult NewResult<T>(Response<T> response) => response.StatusCode switch
        {
            HttpStatusCode.OK => Ok(response),
            HttpStatusCode.Created => Created(string.Empty, response),
            HttpStatusCode.BadRequest => BadRequest(response),
            HttpStatusCode.NotFound => NotFound(response),
            HttpStatusCode.InternalServerError => StatusCode(500, response),
            HttpStatusCode.UnprocessableEntity => UnprocessableEntity(response),
            _ => StatusCode((int)response.StatusCode, response)
        };
    }
}
