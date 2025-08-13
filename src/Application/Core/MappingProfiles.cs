namespace Application.Core;

using AutoMapper;
using Domain.DTOs;
using Domain.DTOs.UserDTOs;
using Domain.Entities;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<User, UserDto>()
            .ForMember(a => a.AmountOfViolations, src => src.MapFrom(a => a.Violations.Count));
        CreateMap<Image, ImageDto>();
    }
}
