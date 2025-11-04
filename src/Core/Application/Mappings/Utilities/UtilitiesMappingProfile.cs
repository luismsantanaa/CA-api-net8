using Application.Features.Utilities.UploadFiles.Queries.Vm;
using AutoMapper;
using Domain.Entities.Shared;

namespace Application.Mappings.Utilities
{
    /// <summary>
    /// AutoMapper profile for Utilities entities mapping
    /// </summary>
    public class UtilitiesMappingProfile : Profile
    {
        public UtilitiesMappingProfile()
        {
            // UploadedFile to UploadedFileVm - Mapeo a record con conversión de Guid a string
            CreateMap<UploadedFile, UploadedFileVm>()
                .ConstructUsing(src => new UploadedFileVm(
                    src.Id.ToString(),
                    src.Name,
                    src.Type,
                    src.Extension,
                    src.Size ?? 0,
                    src.Path,
                    src.Reference,
                    src.Comment
                ));
        }
    }
}

