namespace Application.Core;

using AutoMapper;
using Domain.DTOs;
using Domain.DTOs.CommentDTOs;
using Domain.DTOs.ComplaintDTOs;
using Domain.DTOs.DetailedUserInfoDTOs;
using Domain.DTOs.PublicationDTOs;
using Domain.DTOs.UserDTOs;
using Domain.DTOs.ViolationDTOs;
using Domain.Entities;
using Domain.Entities.Complaints;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<User, AdminUserDto>()
            .ForMember(a => a.AmountOfViolations, src => src.MapFrom(a => a.Violations.Count));
        CreateMap<Image, ImageDto>();
        CreateMap<Violation, ViolationDto>();
        CreateMap<User, PublicUserDto>()
            .ForMember(u => u.JoinedAt, opt => opt.MapFrom(u => u.DateOfCreation.ToString()))
            .ForMember(u => u.UserProfileDetails, opt => opt.MapFrom(u => u.ProfileDetails));
        CreateMap<UserProfileDetails, UserProfileDetailsDto>();
        CreateMap<Address, AddressDto>();
        CreateMap<UpdatePublicUserDto, User>();
        CreateMap<SetAddressDto, Address>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore());

        CreateMap<SetUserProfileDetailsDto, UserProfileDetails>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore());

        CreateMap<Address, Address>();
        CreateMap<UserProfileDetails, UserProfileDetails>();
        CreateMap<PublicationComplaint, PublicationComplaintDto>();
        CreateMap<CommentComplaint, CommentComplaintDto>();
        CreateMap<CreatePublicationDto, Publication>();
        CreateMap<Publication, PublicationDto>()
            .ForMember(dest => dest.LikesAmount, opt => opt.MapFrom(u => u.Likes.Count));
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.RepliesAmount, opt => opt.MapFrom(src => src.Replies.Count));
    }
}
