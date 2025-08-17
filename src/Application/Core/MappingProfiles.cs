namespace Application.Core;

using AutoMapper;
using Domain.DTOs;
using Domain.DTOs.UserDTOs;
using Domain.DTOs.ViolationDTOs;
using Domain.Entities;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<User, AdminUserDto>()
            .ForMember(a => a.AmountOfViolations, src => src.MapFrom(a => a.Violations.Count));
        CreateMap<Image, ImageDto>();
        CreateMap<Violation, ViolationDto>();
        CreateMap<User, PublicUserDto>()
            .ForMember(u => u.JoinedAt, opt => opt.MapFrom(u => u.DateOfCreation.ToString()));
    }
}
