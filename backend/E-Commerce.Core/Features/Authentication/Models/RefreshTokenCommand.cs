using E_commerce.Core.Bases;
using MediatR;

namespace E_commerce.Core.Features.Authentication.Models;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Response<TokenResult>>;
