using Application.Core;
using AutoMapper;
using Domain.DTOs.ComplaintDTOs;
using Domain.Entities.Complaints;
using Infrastructure;

namespace Application.Features.Complaints.Commands;
public class CreatePublicationComplaint
{
    // Изменить возвращаемый тип на дтошку
    public class Command : IRequest<Result<bool>>
    {
        public required int UserId { get; set; }
        public required CreateComplaintDto Complaint { get; set; }
    }

    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            PublicationComplaint complaint = new PublicationComplaint
            {
                Reason = request.Complaint.Reason,
                Explanation = request.Complaint.Explanation ?? string.Empty,
                ComplainedAt = DateTime.UtcNow,
                ComplainerId = request.UserId,
                PublicationId = request.Complaint.TargetId
            };

            await context.PublicationComplaints.AddAsync(complaint);

            var mappedComplaint = mapper.Map<PublicationComplaintDto>(complaint);

            return Result<bool>.Success(true);
        }
    }
}
