namespace JwtTesting.Dto
{
    public class UserLoginDto
    {
        public int Id { get; set; }

        public string? Username { get; set; }

        public string? PasswordHash { get; set; }

        public bool IsLoggedIn { get; set; }

        public bool? IsActive { get; set; }
    }
}
