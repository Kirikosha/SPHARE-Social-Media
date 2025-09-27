namespace Application.Features.Comments.Queries;

using Application.Core;
using AutoMapper;
using Domain.DTOs.CommentDTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class GetCommentsByPublicationId
{
    public class Query : IRequest<Result<List<CommentDto>>>
    {
        public required int PublicationId { get; set; }
    }
    public class Handler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<Query, Result<List<CommentDto>>>
    {
        public async Task<Result<List<CommentDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            Publication? publication = await context.Publications.Include(a => a.Comments)
                .FirstOrDefaultAsync(a => a.Id == request.PublicationId, cancellationToken);
            if (publication == null) 
                return Result<List<CommentDto>>.Failure("Publication you are trying to access does not exist", 404);

            return Result<List<CommentDto>>.Success(mapper.Map<List<CommentDto>>(publication.Comments));
        }
    }
}
