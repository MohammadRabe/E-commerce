using E_commerce.Core.Bases;
using MediatR;

namespace E_commerce.Core.Features.Authentication.Models;

public sealed record SignUpCommand(string UserName, string Email, string Password) : IRequest<Response<TokenResult>>;
