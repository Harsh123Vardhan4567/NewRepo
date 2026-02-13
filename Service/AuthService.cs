using JwtTesting.Dto;
using JwtTesting.HelperMethod;
using JwtTesting.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace JwtTesting.Service
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public AuthService(IConfiguration  configuration)
        {
            _connectionString = configuration.GetConnectionString("Dbconn");
            _configuration = configuration;
        }


        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            List<string> roles = new List<string>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetUserWithRoles", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", userId);

                await con.OpenAsync();

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        roles.Add(reader["Name"].ToString());
                    }
                }
            }

            return roles;
        }

        public async Task InsertRefreshTokenAsync(int userId, string token, DateTime date)
        {
            date =DateTime.UtcNow;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_InsertRefreshToken", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@Token", SqlDbType.NVarChar, 500).Value = token;
                    cmd.Parameters.Add("@CreatedDate", SqlDbType.DateTime).Value = date;

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public Task InsertUserWithRolesAsync(CreateUserRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<UserLoginDto?> LoginAsync(string username)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetUserByEmail", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", username);

                await con.OpenAsync();

                using SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new UserLoginDto
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                        IsLoggedIn = reader.GetBoolean(reader.GetOrdinal("IsLoggedIn")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    };
                }

                return null;
            }
        }

        public Task LogoutAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }
        public async Task<int> InsertUser(CreateUserRequest request)
        {
            request.PasswordHash = Passwordhasher.HashPassword(request.PasswordHash);
            string userJson = JsonSerializer.Serialize(request);

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_InsertUserWithRoles_FromJson", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UserJson", SqlDbType.NVarChar).Value = userJson;

                await conn.OpenAsync();

                var result = await cmd.ExecuteScalarAsync();

                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token)
        {
            bool isValid = false;

            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_ValidateRefreshToken", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Token", token);

                await con.OpenAsync();

                var result = await cmd.ExecuteScalarAsync();

                if (result != null && Convert.ToInt32(result) == 1)
                {
                    isValid = true;
                }
            }

            return isValid;
        }
        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_RevokeRefreshToken", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Token", token);

                await con.OpenAsync();

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }


    }
}
