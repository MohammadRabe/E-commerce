using E_commerce.Core.Bases;
using E_commerce.Core.Features.Authentication.Models;
using E_commerce.Data.Entities;
using E_commerce.Service.Abstraction;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Core.Features.Authentication.Handlers;

public sealed class SignInHandler(UserManager<User> users, SignInManager<User> signIn, ITokenService tokens)
    : ResponseHandler, IRequestHandler<SignInCommand, Response<TokenResult>>
{
    public async Task<Response<TokenResult>> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var user = await users.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName || u.Email == request.UserName, cancellationToken);
        if (user is null || !(await signIn.CheckPasswordSignInAsync(user, request.Password, false)).Succeeded)
            return Unauthorized<TokenResult>(message: "Invalid username or password.");
        return Success(await AuthTokenIssuer.Issue(user, users, tokens));
    }
}
