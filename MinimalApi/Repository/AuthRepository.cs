using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinimalApi.Data;
using MinimalApi.Models.Auth;
using MinimalApi.Models.Dtos;
using MinimalApi.Models.Dtos.Auth;
using MinimalApi.Repository.IRepository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MinimalApi.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly string _secret;

    public AuthRepository(IMapper mapper, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _mapper = mapper;
        _userManager = userManager;
        _configuration = configuration;
        _secret = _configuration.GetValue<string>("ApiSettings:Secret")!;
    }
    public async Task<bool> IsUnique(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user == null;
    }

    public async Task<ResponseDto> Login(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
        {
            return new ResponseDto()
            {
                IsSuccessful = false,
                Errors = new List<string>() { "Invalid Login" }
            };
        }

        List<Claim> claims = new();
        claims!.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
        claims.Add(new Claim(ClaimTypes.Name, user.UserName!));
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role!));
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secret);
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Role = roles.FirstOrDefault()!;

        return new ResponseDto()
        {
            IsSuccessful = true,
            User = userDto,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
        };
    }

    public async Task<ResponseDto> Register(RegisterDto dto)
    {
        ApplicationUser user = new()
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (result.Succeeded == false)
        {
            return new ResponseDto() { IsSuccessful = false, Errors = result.Errors.Select(x => x.Description).ToList() };
        }

        result = await _userManager.AddToRoleAsync(user, dto.Role);
        if (result.Succeeded == false)
        {
            return new ResponseDto() { IsSuccessful = false, Errors = result.Errors.Select(x => x.Description).ToList() };
        }

        var userDto = _mapper.Map<UserDto>(user);
        var roles = await _userManager.GetRolesAsync(user);
        userDto.Role = roles.FirstOrDefault()!;

        return new ResponseDto() { IsSuccessful = true, User = userDto };
    }
}
