using E_commerce.Core.Bases;
using E_commerce.Core.Features.Authentication.Models;
using E_commerce.Data.Entities;
using E_commerce.Service.Abstraction;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace E_commerce.Core.Features.Authentication.Handlers;

public sealed class SignUpHandler(UserManager<User> users, ITokenService tokens)
    : ResponseHandler, IRequestHandler<SignUpCommand, Response<TokenResult>>
{
    public async Task<Response<TokenResult>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        var user = new User { UserName = request.UserName, Email = request.Email };
        var created = await users.CreateAsync(user, request.Password);
        if (!created.Succeeded) return BadRequest<TokenResult>(errors: created.Errors.Select(error => error.Description));
        var role = await users.AddToRoleAsync(user, "Customer");
        if (!role.Succeeded) return BadRequest<TokenResult>(errors: role.Errors.Select(error => error.Description));
        return Success(await AuthTokenIssuer.Issue(user, users, tokens));
    }
}
