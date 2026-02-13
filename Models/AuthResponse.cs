namespace JwtTesting.Models
{
    public class AuthResponse
    {
        public int  UserID { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
