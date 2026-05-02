using Application.DTOs.UserDTOs;
using Domain.Enums;

namespace Application.DTOs.PublicationDTOs;

using System;
using System.Collections.Generic;

public class PublicationDto
{
    public string Id { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime PostedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string> Images { get; set; } = [];
    public PublicUserBriefDto Author { get; set; } = null!;
    public int LikesAmount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public int CommentAmount { get; set; }
    public PublicationTypes PublicationType { get; set; }
    public int ViewCount { get; set; }
    public bool IsDeleted { get; set; }

    // Planned-specific — null for other types
    public DateTime? PublishAt { get; set; }

    // Conditional-specific — null for other types
    public ConditionType? ConditionType { get; set; }
    public int? ConditionTarget { get; set; }
    public ComparisonOperator? ComparisonOperator { get; set; }
}
