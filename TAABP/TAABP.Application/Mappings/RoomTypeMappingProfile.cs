using AutoMapper;
using TAABP.Application.DTOs.RoomTypes;
using TAABP.Domain.Entities;

namespace TAABP.Application.Mappings;

public sealed class RoomTypeMappingProfile : Profile
{
    public RoomTypeMappingProfile()
    {
        CreateMap<RoomType, RoomTypeDto>();
    }
}
