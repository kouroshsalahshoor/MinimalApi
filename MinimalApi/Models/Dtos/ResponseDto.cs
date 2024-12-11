using MinimalApi.Models.Dtos.Auth;

namespace MinimalApi.Models.Dtos
{
    public class ResponseDto
    {
        public bool IsSuccessful { get; set; }
        public UserDto? User { get; set; }
        public string? Token { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
