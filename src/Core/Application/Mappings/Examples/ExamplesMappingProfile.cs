using Application.Features.Examples.Categories.Commands;
using Application.Features.Examples.Categories.VMs;
using Application.Features.Examples.Products.Commands;
using Application.Features.Examples.Products.VMs;
using AutoMapper;
using Domain.Entities.Examples;

namespace Application.Mappings.Examples
{
    public class ExamplesMappingProfile : Profile
    {
        public ExamplesMappingProfile()
        {
            // TestCategory
            CreateMap<TestCategory, CategoryVm>();
            CreateMap<TestCategory, CreateCategoryCommand>();
            CreateMap<TestCategory, UpdateCategoryCommand>();
            CreateMap<CreateCategoryCommand, TestCategory>();
            CreateMap<UpdateCategoryCommand, TestCategory>();

            // TestProduct - Mapeo a records usando ConstructUsing para campos calculados
            CreateMap<TestProduct, ProductVm>()
                .ConstructUsing(src => new ProductVm(
                    src.Id,
                    src.Name, // Name de TestProduct se mapea a Title en ProductVm
                    src.Description,
                    src.Image,
                    src.Price,
                    src.CategoryId,
                    src.Category != null ? (src.Category.Name ?? string.Empty) : string.Empty
                ));
            
            CreateMap<TestProduct, ProductWithCategoryVm>()
                .ConstructUsing(src => new ProductWithCategoryVm(
                    src.Id,
                    src.Name,
                    src.Description,
                    src.Image,
                    src.Price,
                    src.CategoryId,
                    src.Category != null ? (src.Category.Name ?? string.Empty) : string.Empty,
                    src.Category != null ? new CategoryVm(src.Category.Id, src.Category.Name ?? string.Empty, src.Category.Image) : null
                ));
            CreateMap<TestProduct, CreateProductCommand>();
            CreateMap<TestProduct, UpdateProductCommand>();
            CreateMap<CreateProductCommand, TestProduct>();
            CreateMap<UpdateProductCommand, TestProduct>();
        }
    }
}
