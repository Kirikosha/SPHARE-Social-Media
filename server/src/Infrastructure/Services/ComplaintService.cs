using Application.Core;
using Application.DTOs.ComplaintDTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities.Complaints;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ComplaintService(ApplicationDbContext context, IMapper mapper, ISpamRepository spamRepository) 
    : IComplaintService
{
    private const double BaseComplaintValue = 1.0;
    private const double NewAuthorMultiplier = 2.0;
    
    public async Task<Result<List<PublicationComplaintDto>>> GetPublicationComplaintsAsync(CancellationToken ct)
    {
        var publicationComplaints = await context.PublicationComplaints.Include(a => a.Publication)
            .ThenInclude(a => a.Author).ThenInclude(a => a.ProfileImage).ToListAsync(ct);

        return Result<List<PublicationComplaintDto>>
            .Success(mapper.Map<List<PublicationComplaintDto>>(publicationComplaints));
    }

    public async Task<Result<List<GroupPublicationComplaintDto>>> GetGroupedComplaintsAsync(CancellationToken ct)
    {
        var result = await context.PublicationComplaints
            .GroupBy(c => c.PublicationId)
            .Select(g => new GroupPublicationComplaintDto
            {
                PublicationId = g.Key,
                TotalComplaints = g.Count(),
                Reasons = g.GroupBy(c => c.Reason)
                    .Select(r => new ReasonDto
                    {
                        Reason = r.Key,
                        Count = r.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(100)
                    .ToList()
            })
            .OrderByDescending(x => x.TotalComplaints)
            .ToListAsync(ct);

        return Result<List<GroupPublicationComplaintDto>>.Success(result);
    }

    public async Task<Result<List<CommentComplaintDto>>> GetCommentComplaintsAsync(CancellationToken ct)
    {
        var commentComplaints = await context.CommentComplaints.Include(a => a.Comment).ThenInclude(a => a.Author)
            .ToListAsync(ct);

        return Result<List<CommentComplaintDto>>
            .Success(mapper.Map<List<CommentComplaintDto>>(commentComplaints));
    }

    public async Task<Result<bool>> CreatePublicationComplaint(string userId, CreateComplaintDto complaintDto, 
        CancellationToken ct)
    {
        // Check for existing complaint -- Start
        var existingComplaint = await context.PublicationComplaints
            .Where(a => a.PublicationId == complaintDto.TargetId &&
                        a.ComplainerId == userId)
            .Select(c => new { c.ComplainedAt })
            .FirstOrDefaultAsync(ct);

        if (existingComplaint != null)
        {
            return Result<bool>.Failure(
                "Complain on that publication was already sent at " + existingComplaint.ComplainedAt.Date, 400);
        }
            
        var res = await spamRepository.MakeComplaint(userId, ct);
        if (!res)
        {
            return Result<bool>.Failure(
                "You cannot complain for today due to our antispam rules", 400);
        }

        bool isNewAuthor = await IsNewAuthor(complaintDto.TargetId, ct);
        PublicationComplaint complaint = new PublicationComplaint
        {
            Reason = complaintDto.Reason,
            Explanation = complaintDto.Explanation ?? string.Empty,
            ComplainedAt = DateTime.UtcNow,
            ComplainerId = userId,
            PublicationId = complaintDto.TargetId,
            ComplaintValue = isNewAuthor ?  (BaseComplaintValue * NewAuthorMultiplier) : BaseComplaintValue
        };

        await context.PublicationComplaints.AddAsync(complaint, ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> CreateCommentComplaint(string userId, CreateComplaintDto complaintDto, 
        CancellationToken ct)
    {
        var res = await spamRepository.MakeComplaint(userId, ct);
        if (!res)
        {
            return Result<bool>.Failure(
                "You cannot complain for today due to our antispam rules", 400);
        }
            
        CommentComplaint complaint = new CommentComplaint
        {
            Reason = complaintDto.Reason,
            Explanation = complaintDto.Explanation ?? string.Empty,
            ComplainedAt = DateTime.UtcNow,
            ComplainerId = userId,
            CommentId = complaintDto.TargetId
        };


        await context.CommentComplaints.AddAsync(complaint, ct);

        return Result<bool>.Success(true);
    }

    private async Task<bool> IsNewAuthor(string publicationId, CancellationToken ct)
    {
        var authorCreationDate = await context.Publications
            .Where(p => p.Id == publicationId)
            .Select(p => p.Author.DateOfCreation)
            .FirstOrDefaultAsync(ct);

        if (authorCreationDate == default)
            throw new Exception("Author cannot have default creation date");

        var cutOff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        return authorCreationDate >= cutOff;
    }
}