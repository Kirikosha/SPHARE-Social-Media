using Application.Interfaces.Services;

namespace Application.Features.Complaints.Commands;
using Core;
using DTOs.ComplaintDTOs;
public class CreatePublicationComplaint
{
    // Изменить возвращаемый тип на дтошку
    public class Command : IRequest<Result<bool>>
    {
        public required string UserId { get; set; }
        public required CreateComplaintDto Complaint { get; set; }
    }

    public class Handler(IComplaintService complaintService) : IRequestHandler<Command, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await complaintService.CreatePublicationComplaint(request.UserId, request.Complaint,
                cancellationToken);
        }
    }
}
