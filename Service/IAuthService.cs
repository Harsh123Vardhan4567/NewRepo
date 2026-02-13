using JwtTesting.Dto;
using JwtTesting.Models;
using System.Runtime.CompilerServices;

namespace JwtTesting.Service
{
    public interface IAuthService
    {
        Task<UserLoginDto?> LoginAsync(string username);
        Task<List<string>> GetUserRolesAsync(int userId);
        Task InsertRefreshTokenAsync(int userId, string token,  DateTime expiryDate);
        
        Task LogoutAsync(string refreshToken);

        Task<int> InsertUser(CreateUserRequest request);
        Task<bool> ValidateRefreshTokenAsync(string token);
        Task<bool> RevokeRefreshTokenAsync(string token);
    }
}
