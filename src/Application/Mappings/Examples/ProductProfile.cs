using Application.DTOs.Examples;
using AutoMapper;
using Domain.Entities.Examples;

namespace Application.Mappings.Examples
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<TestProduct, ProductVm>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ReverseMap();
            
            CreateMap<TestProduct, ProductDto>()
                .ReverseMap();
        }
    }
}

