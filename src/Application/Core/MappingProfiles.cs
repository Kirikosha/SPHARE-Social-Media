using Domain.DTOs.MessagingDTOs;

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
            .ForMember(u => u.JoinedAt, opt => opt.MapFrom(u => u.DateOfCreation))
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

        CreateMap<Chat, ChatDto>()
            .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.Participants))
            .ForMember(dest => dest.LastMessage, opt => opt.Ignore())
            .ForMember(dest => dest.UnreadCount, opt => opt.Ignore());

        CreateMap<Chat, ChatWithMessagesDto>()
            .IncludeBase<Chat, ChatDto>()
            .ForMember(dest => dest.Messages, opt =>
                opt.MapFrom(src => src.Messages.OrderBy(m => m.SentAt)));
        
        CreateMap<ChatUser, ChatUserDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Username : null))
            .ForMember(dest => dest.UniqueNameIdentifier, opt => opt.MapFrom(src => src.User != null ? src.User.UniqueNameIdentifier : null))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => 
                src.User != null && src.User.ProfileImage != null 
                    ? src.User.ProfileImage.ImageUrl : null))
            .ForMember(dest => dest.IsOnline, opt => opt.Ignore()); 
        
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.SendersUsername, opt => opt.MapFrom(src => 
                src.Sender != null ? src.Sender.Username : null))
            .ForMember(dest => dest.IsRead, opt => opt.Ignore());

        CreateMap<Publication, PublicationCalendarDto>();

    }
}
