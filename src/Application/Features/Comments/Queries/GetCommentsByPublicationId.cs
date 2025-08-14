namespace Application.Features.Comments.Queries;

using AutoMapper;
using Domain.DTOs.CommentDTOs;
using Domain.Entities;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetCommentsByPublicationId
{
    public class Query : IRequest<List<CommentDto>>
    {
        public required int PublicationId { get; set; }
    }
    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Query, List<CommentDto>>
    {
        public async Task<List<CommentDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            Publication? publication = await context.Publications.Include(a => a.Comments)
                .FirstOrDefaultAsync(a => a.Id == request.PublicationId, cancellationToken);
            if (publication == null) throw new Exception("Publication was not found");

            return mapper.Map<List<CommentDto>>(publication.Comments);
        }
    }
}
