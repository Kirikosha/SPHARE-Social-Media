using Application.Core;
using AutoMapper;
using Domain.DTOs.ComplaintDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Complaints.Queries;
public class GetCommentComplaints
{
    public class Query : IRequest<Result<List<CommentComplaintDto>>>
    {
    }

    public class Handler(ApplicationDbContext context, IMapper mapper)
        : IRequestHandler<Query, Result<List<CommentComplaintDto>>>
    {
        public async Task<Result<List<CommentComplaintDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var commentComplaints = await context.CommentComplaints.Include(a => a.Comment).ThenInclude(a => a.Author).ToListAsync();

            return Result<List<CommentComplaintDto>>
                .Success(mapper.Map<List<CommentComplaintDto>>(commentComplaints));
        }
    }
}
