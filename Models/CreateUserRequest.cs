namespace JwtTesting.Models
{
    public class CreateUserRequest
    {
        public string  Username { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; }
        public List<int> Roles { get; set; }
    }
}
