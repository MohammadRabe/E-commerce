using E_commerce.Data.Entities;

namespace E_commerce.Service.Abstraction;

public interface ITokenService
{
    double RefreshTokenDays { get; }
    Task<string> CreateAccessTokenAsync(User user);
    string CreateRefreshToken();
}
