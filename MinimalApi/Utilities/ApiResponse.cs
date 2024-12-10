using System.Net;

namespace MinimalApi.Utilities
{
    public class ApiResponse
    {
        public bool IsSuccessful { get; set; }
        public object? Result { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
