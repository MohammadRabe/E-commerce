using E_commerce.Core.Bases;
using E_commerce.Core.Features.Authentication.Models;
using E_commerce.Data.Entities;
using E_commerce.Service.Abstraction;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Core.Features.Authentication.Handlers;

public sealed class RefreshTokenHandler(UserManager<User> users, ITokenService tokens)
    : ResponseHandler, IRequestHandler<RefreshTokenCommand, Response<TokenResult>>
{
    public async Task<Response<TokenResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await users.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken && u.RefreshTokenExpiryTime > DateTime.UtcNow, cancellationToken);
        if (user is null) return Unauthorized<TokenResult>(message: "Invalid or expired refresh token.");
        return Success(await AuthTokenIssuer.Issue(user, users, tokens));
    }
}
