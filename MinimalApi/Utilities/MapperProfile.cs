using AutoMapper;
using MinimalApi.Models;
using MinimalApi.Models.Auth;
using MinimalApi.Models.Dtos;
using MinimalApi.Models.Dtos.Auth;

namespace MinimalApi.Utilities
{    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Category, CategoryCreateDto>().ReverseMap();
            CreateMap<Category, CategoryUpdateDto>().ReverseMap();
            CreateMap<UserDto, ApplicationUser>().ReverseMap();
        }
    }
}
