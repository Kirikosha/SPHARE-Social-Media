using Domain.Enums;

namespace Domain.DTOs.PublicationDTOs;

using Domain.DTOs.UserDTOs;
using System;
using System.Collections.Generic;

public class PublicationDto
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public DateTime PostedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? RemindAt { get; set; }
    public List<ImageDto> Images { get; set; }
    public PublicUserDto Author { get; set; }
    public int LikesAmount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public int CommentAmount { get; set; }
    public ConditionType? ConditionType { get; set; }
    public int? ConditionTarget { get; set; }
    public ComparisonOperator? ComparisonOperator { get; set; }
    public int ViewCount { get; set; }
    public bool IsDeleted { get; set; }
}
