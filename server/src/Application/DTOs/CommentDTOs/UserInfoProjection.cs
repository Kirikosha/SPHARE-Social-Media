namespace Application.DTOs.CommentDTOs;

public sealed class UserInfoProjection
{
    public string Id { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string UniqueNameIdentifier { get; init; } = null!;
    public bool Blocked { get; init; }
    public string? ImageUrl { get; init; }
    public DateTime LastCommentDate { get; init; }
    public bool PublicationExists { get; init; }
}