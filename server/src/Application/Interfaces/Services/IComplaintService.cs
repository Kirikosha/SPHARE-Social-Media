using Application.Core;
using Application.DTOs.ComplaintDTOs;

namespace Application.Interfaces.Services;

public interface IComplaintService
{
    Task<Result<List<PublicationComplaintDto>>> GetPublicationComplaintsAsync(CancellationToken ct);
    Task<Result<List<GroupPublicationComplaintDto>>> GetGroupedComplaintsAsync(CancellationToken ct);
    Task<Result<List<CommentComplaintDto>>> GetCommentComplaintsAsync(CancellationToken ct);
    Task<Result<bool>> CreatePublicationComplaint(string userId, CreateComplaintDto complaintDto, CancellationToken ct);
    Task<Result<bool>> CreateCommentComplaint(string userId, CreateComplaintDto complaintDto, CancellationToken ct);
}