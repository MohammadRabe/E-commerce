using E_commerce.Core.Features.Authentication.Models;
using E_commerce.Data.Entities;
using E_commerce.Service.Abstraction;
using Microsoft.AspNetCore.Identity;

namespace E_commerce.Core.Features.Authentication.Handlers;

internal static class AuthTokenIssuer
{
    public static async Task<TokenResult> Issue(User user, UserManager<User> users, ITokenService tokens)
    {
        var refreshToken = tokens.CreateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(tokens.RefreshTokenDays);
        await users.UpdateAsync(user);
        return new(await tokens.CreateAccessTokenAsync(user), refreshToken, user.UserName!, user.FullName);
    }
}
