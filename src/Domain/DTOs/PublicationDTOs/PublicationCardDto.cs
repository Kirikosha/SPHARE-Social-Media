using Domain.DTOs.UserDTOs;
using Domain.Enums;

namespace Domain.DTOs.PublicationDTOs;

public class PublicationCardDto
{
    public string Id { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime PostedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string> ImageUrls { get; set; } = [];
    public PublicUserBriefDto Author { get; set; } = null!;
    public int LikesAmount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public int CommentAmount { get; set; }
    public PublicationTypes PublicationType { get; set; }
    public int ViewCount { get; set; }
    public bool IsDeleted { get; set; }
}