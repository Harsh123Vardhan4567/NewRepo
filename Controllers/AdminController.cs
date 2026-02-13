using Azure;
using JwtTesting.Dto;
using JwtTesting.HelperMethod;
using JwtTesting.HelperMethod.GenerateToken;
using JwtTesting.Models;
using JwtTesting.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JwtTesting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly Tokens _tokens;


        public AdminController(IAuthService authService,  Tokens tokens)
        {

            _authService = authService;
            _tokens = tokens;
        }


        [HttpPost("Login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Email and Password are required"
                });
            }

            var user = await _authService.LoginAsync(email);

            if (user == null)
            {
                return Unauthorized(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Invalid email"
                });
            }

            bool isPasswordValid = Passwordhasher.VerifyPassword(password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Invalid password"
                });
            }

            var roles = await _authService.GetUserRolesAsync(user.Id);

            var accessToken = _tokens.GenerateJwtToken(user.Id, user.Username, roles);
            var refreshToken = _tokens.GenerateRefreshToken();
            if (refreshToken != null)
            {
                await _authService.InsertRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow);
            }

            var response = new AuthResponse
            {
                UserID=user.Id,
                AccessToken = accessToken,
                RefreshToken = refreshToken

            };

            return Ok(new ApiResponse<AuthResponse>
            {
                Success = true,
                Message = "Login successful",
                Data = response
            });
        }

        [HttpPost("Register")]
        public async Task<ActionResult<ApiResponse<int>>> Register([FromBody] CreateUserRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<int>
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            try
            {
                int userId = await _authService.InsertUser(request);

                if (userId <= 0)
                {
                    return BadRequest(new ApiResponse<int>
                    {
                        Success = false,
                        Message = "User registration failed"
                    });
                }

                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Message = "User registered successfully",
                    Data = userId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<int>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("Refresh")]
        public async Task<ActionResult<ApiResponse<string>>> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.RefreshToken))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid request",
                    Data = null
                });
            }


            var user = await _authService.ValidateRefreshTokenAsync(model.RefreshToken);

            if (user == null)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid or expired refresh token",
                    Data = null
                });
            }

            // Get roles
            var roles = await _authService.GetUserRolesAsync(model.UserID);

            // Generate new access token
            var newAccessToken = _tokens.GenerateJwtToken(model.UserID, model.UserName!, roles);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Token refreshed successfully",
                Data = newAccessToken
            });
        }


        [HttpPost("Logout")]
        public async Task<ActionResult<ApiResponse<string>>> Logout([FromBody] RefreshTokenRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.RefreshToken))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Refresh token is required",
                    Data = null
                });
            }

            var result = await _authService.RevokeRefreshTokenAsync(model.RefreshToken);

            if (!result)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Refresh token not found or already revoked",
                    Data = null
                });
            }

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Logged out successfully",
                Data = null
            });
        }


    }
}

