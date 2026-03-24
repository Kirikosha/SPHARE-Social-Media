using Domain.DTOs.PublicationDTOs;
using Domain.Enums;

namespace Application.Features.Publications.Queries;

using Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetPublicationsToRemind
{
    public class Query : IRequest<Result<List<PublicationNotificationDto>>>
    {
        public DateTime PostedAt { get; set; }
        public int BatchSize { get; set; }
    }
    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<List<PublicationNotificationDto>>>
    {
        public async Task<Result<List<PublicationNotificationDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            return Result<List<PublicationNotificationDto>>.Success(
                await context.Publications
                    .AsNoTracking()
                    .Where(p =>
                        ((p.RemindAt != null && p.RemindAt <= now) ||
                         (p.ConditionType != null &&
                          p.ComparisonOperator == ComparisonOperator.GreaterThanOrEqual &&
                          p.Author.SubscriberNumber >= p.ConditionTarget))
                        && !p.WasSent
                        && p.PostedAt > request.PostedAt)
                    .OrderBy(p => p.PostedAt)
                    .Select(p => new PublicationNotificationDto
                    {
                        Id = p.Id,
                        Content = p.Content,
                        PostedAt = p.PostedAt,
                        WasSent = p.WasSent,
                        AuthorId = p.AuthorId,

                        AuthorUsername = p.Author.Username,
                        AuthorImageUrl = p.Author.ProfileImage != null
                            ? p.Author.ProfileImage.ImageUrl
                            : null,

                        FirstImageUrl = p.Images!
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault()
                    })
                    .Take(request.BatchSize)
                    .ToListAsync(cancellationToken)
            );
        }
    }
}
