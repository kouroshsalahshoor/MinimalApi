namespace MinimalApi.Models.Dtos.Auth
{
    public class RegisterDto
    {
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Password { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
}
