using Application.DTOs.Examples;
using AutoMapper;
using Domain.Entities.Examples;

namespace Application.Mappings.Examples
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<TestCategory, CategoryVm>()
                .ReverseMap();
            
            CreateMap<TestCategory, CategoryDto>()
                .ReverseMap();
        }
    }
}

