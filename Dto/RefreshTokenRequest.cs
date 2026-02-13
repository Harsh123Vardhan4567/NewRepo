namespace JwtTesting.Dto
{
    public class RefreshTokenRequest
    {
        public int  UserID { get; set; }

        public string ?UserName { get; set; }

        public string? RefreshToken { get; set; }

        
    }
}
