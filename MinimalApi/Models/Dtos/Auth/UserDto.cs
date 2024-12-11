namespace MinimalApi.Models.Dtos.Auth
{
    public class UserDto
    {
        public string Id { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Role { get; set; } = default!;
    }
}
