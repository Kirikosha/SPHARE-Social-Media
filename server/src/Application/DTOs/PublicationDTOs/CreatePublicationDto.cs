using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.PublicationDTOs;
using Domain.Enums;

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

public class CreatePublicationDto : IValidatableObject
{
    public string? Content { get; set; }
    public PublicationTypes PublicationType { get; set; } = PublicationTypes.ordinary;
    public List<IFormFile>? Images { get; set; }
    
    public DateTime? PublishAt { get; set; }
    
    public ConditionType? ConditionType { get; set; }
    public int? ConditionTarget { get; set; }
    public ComparisonOperator? ConditionOperator { get; set; }


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Content) && (Images == null || Images.Count == 0))
            yield return new ValidationResult(
                "Publication must have either content or at least one image.");

        if (PublicationType == PublicationTypes.planned)
        {
            if (PublishAt == null)
                yield return new ValidationResult(
                    "PublishAt is required for planned publications.",
                    [nameof(PublishAt)]);

            if (PublishAt <= DateTime.UtcNow)
                yield return new ValidationResult(
                    "PublishAt must be in the future.",
                    [nameof(PublishAt)]);
        }

        if (PublicationType == PublicationTypes.conditional)
        {
            if (ConditionType == null)
                yield return new ValidationResult(
                    "ConditionType is required for conditional publications.",
                    [nameof(ConditionType)]);

            if (ConditionTarget == null)
                yield return new ValidationResult(
                    "ConditionTarget is required for conditional publications.",
                    [nameof(ConditionTarget)]);

            if (ConditionOperator == null)
                yield return new ValidationResult(
                    "ConditionOperator is required for conditional publications.",
                    [nameof(ConditionOperator)]);
        }
    }
}
