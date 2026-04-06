namespace Application.DTOs.PublicationDTOs;
using Domain.Enums;

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

public class CreatePublicationDto
{
    public string? Content { get; set; }
    public PublicationTypes PublicationType { get; set; } = PublicationTypes.ordinary;
    public DateTime? RemindAt { get; set; }
    public List<IFormFile>? Images { get; set; }
    public ConditionType? ConditionType { get; set; }
    public int? ConditionTarget { get; set; }
    public ComparisonOperator? ConditionOperator { get; set; }
}
