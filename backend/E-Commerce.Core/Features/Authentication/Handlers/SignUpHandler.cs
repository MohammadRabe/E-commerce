using E_commerce.Core.Bases;
using E_commerce.Core.Features.Authentication.Models;
using E_commerce.Data.Entities;
using E_commerce.Service.Abstraction;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace E_commerce.Core.Features.Authentication.Handlers;

public sealed class SignUpHandler(
    UserManager<User> users,
    ITokenService tokens,
    ILogger<SignUpHandler> logger)
    : ResponseHandler, IRequestHandler<SignUpCommand, Response<TokenResult>>
{
    public async Task<Response<TokenResult>> Handle(SignUpCommand request, CancellationToken cancellationToken)
    {
        var fullName = string.Join(' ', request.FullName.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        var user = new User { UserName = request.UserName, FullName = fullName, Email = request.Email };
        var created = await users.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            logger.LogWarning("Account registration failed. Identity errors: {IdentityErrors}",
                created.Errors.Select(error => error.Code).ToArray());
            return BadRequest<TokenResult>(errors: created.Errors.Select(error => error.Description));
        }
        var role = await users.AddToRoleAsync(user, "Customer");
        if (!role.Succeeded)
        {
            logger.LogError("Account {UserId} was created but customer role assignment failed. Identity errors: {IdentityErrors}",
                user.Id, role.Errors.Select(error => error.Code).ToArray());
            return BadRequest<TokenResult>(errors: role.Errors.Select(error => error.Description));
        }
        logger.LogInformation("Registered user {UserId}", user.Id);
        return Success(await AuthTokenIssuer.Issue(user, users, tokens));
    }
}
