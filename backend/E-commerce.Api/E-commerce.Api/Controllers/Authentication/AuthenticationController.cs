using CleanArch.Api.Controllers.Base;
using E_commerce.Core.Features.Authentication.Models;
using CleanArch.Data.Routing;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace E_commerce.Api.Controllers.Authentication;

[ApiController]
public sealed class AuthenticationController(IMediator mediator) : AppControllerBase(mediator)
{
    public sealed record SignUpRequest([Required] string UserName, [Required] string FullName, [Required, EmailAddress] string Email, [Required, MinLength(6)] string Password);
    public sealed record SignInRequest([Required] string UserName, [Required] string Password);
    public sealed record RefreshTokenRequest([Required] string RefreshToken);

    [HttpPost(Router.Version1.Authentication.SignUp)]
    public async Task<IActionResult> SignUp(SignUpRequest request, CancellationToken cancellationToken) =>
        NewResult(await _mediator.Send(new SignUpCommand(request.UserName, request.FullName, request.Email, request.Password), cancellationToken));

    [HttpPost(Router.Version1.Authentication.SignIn)]
    public async Task<IActionResult> SignIn(SignInRequest request, CancellationToken cancellationToken) =>
        NewResult(await _mediator.Send(new SignInCommand(request.UserName, request.Password), cancellationToken));

    [HttpPost(Router.Version1.Authentication.RefreshToken)]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken) =>
        NewResult(await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken));
}
