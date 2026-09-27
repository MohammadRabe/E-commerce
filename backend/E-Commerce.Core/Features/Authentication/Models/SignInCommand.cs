using E_commerce.Core.Bases;
using MediatR;

namespace E_commerce.Core.Features.Authentication.Models;

public sealed record SignInCommand(string UserName, string Password, bool AdminOnly = false) : IRequest<Response<TokenResult>>;
