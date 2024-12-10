using AutoMapper;
using MinimalApi.Models;
using MinimalApi.Models.Dtos;

namespace MinimalApi.Utilities
{    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Category, CategoryCreateDto>().ReverseMap();
        }
    }
}
