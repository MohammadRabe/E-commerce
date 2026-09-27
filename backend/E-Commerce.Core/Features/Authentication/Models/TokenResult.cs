namespace E_commerce.Core.Features.Authentication.Models;

public sealed record TokenResult(string AccessToken, string RefreshToken, string UserName, string FullName);
