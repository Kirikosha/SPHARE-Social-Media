using System.Numerics;

namespace Domain.Entities;

using Domain.Entities.Complaints;
using Domain.Enums;
using System;
using System.Collections.Generic;

public class Publication
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public List<Like> Likes { get; set; } = [];
    public DateTime PostedAt { get; set; } = DateTime.UtcNow; // Creation date
    public PublicationTypes PublicationType { get; set; } = PublicationTypes.ordinary;
    public DateTime? RemindAt { get; set; }
    public bool WasSent { get; set; } = false;
    public DateTime? UpdatedAt { get; set; }
    public List<Image>? Images { get; set; }
    public List<Comment>? Comments { get; set; }
    public List<PublicationComplaint> PublicationComplaints { get; set; } = [];
    
    // Condition module
    public ConditionType? ConditionType { get; set; }
    public int? ConditionTarget { get; set; }
    public ComparisonOperator? ConditionOperator { get; set; }
}
