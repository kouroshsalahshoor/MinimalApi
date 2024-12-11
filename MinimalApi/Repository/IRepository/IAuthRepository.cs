using MinimalApi.Models.Dtos;
using MinimalApi.Models.Dtos.Auth;

namespace MinimalApi.Repository.IRepository
{
    public interface IAuthRepository
    {
        Task<bool> IsUnique(string userName);
        Task<ResponseDto> Login(LoginDto dto);
        Task<ResponseDto> Register(RegisterDto dto);
    }
}
