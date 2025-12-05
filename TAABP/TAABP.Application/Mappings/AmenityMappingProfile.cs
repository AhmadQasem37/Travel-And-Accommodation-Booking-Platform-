using AutoMapper;
using TAABP.Application.DTOs.Amenities;
using TAABP.Domain.Entities;

namespace TAABP.Application.Mappings;

public sealed class AmenityMappingProfile : Profile
{
    public AmenityMappingProfile()
    {
        CreateMap<Amenity, AmenityDto>();
    }
}
